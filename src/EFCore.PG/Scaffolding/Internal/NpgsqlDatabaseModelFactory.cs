using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;

/// <summary>
/// The default database model factory for Npgsql.
/// </summary>
public class NpgsqlDatabaseModelFactory : DatabaseModelFactory
{
    #region Fields

    /// <summary>
    /// The regular expression formatting string for schema and/or table names.
    /// </summary>
    private const string NamePartRegex = @"(?:(?:""(?<part{0}>(?:(?:"""")|[^""])+)"")|(?<part{0}>[^\.\[""]+))";

    /// <summary>
    /// The <see cref="Regex"/> to extract the schema and/or table names.
    /// </summary>
    private static readonly Regex SchemaTableNameExtractor =
        new(
            string.Format(
                CultureInfo.InvariantCulture,
                @"^{0}(?:\.{1})?$",
                string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)),
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(1000.0));

    /// <summary>
    /// The types used for serial columns.
    /// </summary>
    private static readonly string[] SerialTypes = { "int2", "int4", "int8" };

    /// <summary>
    /// The diagnostic logger instance.
    /// </summary>
    private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;

    #endregion

    #region Public surface

    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlDatabaseModelFactory"/> class.
    /// </summary>
    public NpgsqlDatabaseModelFactory(IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        => _logger = Check.NotNull(logger, nameof(logger));

    /// <inheritdoc />
    public override DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
    {
        Check.NotEmpty(connectionString, nameof(connectionString));
        Check.NotNull(options, nameof(options));

        using var connection = new NpgsqlConnection(connectionString);
        return Create(connection, options);
    }

    /// <inheritdoc />
    public override DatabaseModel Create(DbConnection dbConnection, DatabaseModelFactoryOptions options)
    {
        Check.NotNull(dbConnection, nameof(dbConnection));
        Check.NotNull(options, nameof(options));

        var databaseModel = new DatabaseModel();

        var connection = (NpgsqlConnection)dbConnection;

        var connectionStartedOpen = connection.State == ConnectionState.Open;

        if (!connectionStartedOpen)
        {
            connection.Open();
        }

        try
        {
            var internalSchemas = "'pg_catalog', 'information_schema'";
            using (var command = new NpgsqlCommand("SELECT version()", connection))
            {
                var longVersion = (string)command.ExecuteScalar()!;
                if (longVersion.Contains("CockroachDB"))
                {
                    internalSchemas += ", 'crdb_internal'";
                }
            }

            databaseModel.DatabaseName = connection.Database;
            databaseModel.DefaultSchema = "public";

            PopulateGlobalDatabaseInfo(connection, databaseModel);

            var schemaList = options.Schemas.ToList();
            var schemaFilter = GenerateSchemaFilter(schemaList);
            var tableList = options.Tables.ToList();
            var tableFilter = GenerateTableFilter(tableList.Select(Parse).ToList(), schemaFilter);

            var enums = GetEnums(connection, databaseModel);

            foreach (var table in GetTables(connection, databaseModel, tableFilter, internalSchemas, enums, _logger))
            {
                table.Database = databaseModel;
                databaseModel.Tables.Add(table);
            }

            foreach (var table in databaseModel.Tables)
            {
                while (table.Columns.Remove(null!)) {}
            }

            foreach (var sequence in GetSequences(connection, databaseModel, schemaFilter, _logger))
            {
                sequence.Database = databaseModel;
                databaseModel.Sequences.Add(sequence);
            }

            if (connection.PostgreSqlVersion >= new Version(9, 1))
            {
                GetExtensions(connection, databaseModel);
                GetCollations(connection, databaseModel, internalSchemas, _logger);
            }

            for (var i = 0; i < databaseModel.Tables.Count; i++)
            {
                var table = databaseModel.Tables[i];

                // We may have dropped or skipped columns. We load these because constraints take them into
                // account when referencing columns, but must now get rid of them before returning
                // the database model.
                while (table.Columns.Remove(null!)) {}
            }

            foreach (var schema in schemaList
                         .Except(databaseModel.Sequences.Select(s => s.Schema).Concat(databaseModel.Tables.Select(t => t.Schema))))
            {
                _logger.MissingSchemaWarning(schema);
            }

            foreach (var table in tableList)
            {
                var (schema, name) = Parse(table);
                if (!databaseModel.Tables.Any(t => !string.IsNullOrEmpty(schema) && t.Schema == schema || t.Name == name))
                {
                    _logger.MissingTableWarning(table);
                }
            }

            return databaseModel;
        }
        finally
        {
            if (!connectionStartedOpen)
            {
                connection.Close();
            }
        }
    }

    #endregion

    #region Type information queries

    private static void PopulateGlobalDatabaseInfo(NpgsqlConnection connection, DatabaseModel databaseModel)
    {
        if (connection.PostgreSqlVersion < new Version(8, 4))
        {
            return;
        }

        var commandText = @"
SELECT datcollate FROM pg_database WHERE datname=current_database() AND
        datcollate <> (SELECT datcollate FROM pg_database WHERE datname='template1')";
        using var command = new NpgsqlCommand(commandText, connection);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            databaseModel.Collation = reader.GetString(0);
        }
    }

    /// <summary>
    /// Queries the database for defined tables and registers them with the model.
    /// </summary>
    private static IEnumerable<DatabaseTable> GetTables(
        NpgsqlConnection connection,
        DatabaseModel databaseModel,
        Func<string, string, string>? tableFilter,
        string internalSchemas,
        HashSet<string> enums,
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
    {
        var filter = tableFilter is not null ? $"AND {tableFilter("ns.nspname", "cls.relname")}" : null;
        var commandText = $@"
SELECT nspname, relname, relkind, description
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
LEFT OUTER JOIN pg_description AS des ON des.objoid = cls.oid AND des.objsubid=0
WHERE
  cls.relkind IN ('r', 'v', 'm', 'f') AND
  ns.nspname NOT IN ({internalSchemas}) AND
  cls.relname <> '{HistoryRepository.DefaultTableName}' AND
  -- Exclude tables which are members of PG extensions
  NOT EXISTS (
    SELECT 1 FROM pg_depend WHERE
      classid=(
        SELECT cls.oid
        FROM pg_class AS cls
        JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
        WHERE relname='pg_class' AND ns.nspname='pg_catalog'
      ) AND
      objid=cls.oid AND
      deptype IN ('e', 'x')
  )
  {filter}";

        var tables = new List<DatabaseTable>();

        using (var command = new NpgsqlCommand(commandText, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var schema = reader.GetValueOrDefault<string>("nspname");
                var name = reader.GetString("relname");
                var type = reader.GetChar("relkind");
                var comment = reader.GetValueOrDefault<string>("description");

                var table = type switch
                {
                    'r' => new DatabaseTable(),
                    'f' => new DatabaseTable(),
                    'v' => new DatabaseView(),
                    'm' => new DatabaseView(),
                    _ => throw new ArgumentOutOfRangeException($"Unknown relkind '{type}' when scaffolding {DisplayName(schema, name)}")
                };

                table.Database = databaseModel;
                table.Name = name;
                table.Schema = schema;
                table.Comment = comment;

                tables.Add(table);
            }
        }

        GetColumns(connection, tables, filter, internalSchemas, enums, logger);
        GetConstraints(connection, tables, filter, internalSchemas, out var constraintIndexes, logger);
        GetIndexes(connection, tables, filter, internalSchemas, constraintIndexes, logger);
        return tables;
    }

    /// <summary>
    /// Queries the database for defined columns and registers them with the model.
    /// </summary>
    private static void GetColumns(
        NpgsqlConnection connection,
        IReadOnlyList<DatabaseTable> tables,
        string? tableFilter,
        string internalSchemas,
        HashSet<string> enums,
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
    {
        var commandText = $@"
SELECT
  nspname,
  cls.relname,
  typ.typname,
  basetyp.typname AS basetypname,
  attname,
  description,
  {(connection.PostgreSqlVersion >= new Version(9, 1) ? "collname" : "NULL::text as collname")},
  attisdropped,
  {(connection.PostgreSqlVersion >= new Version(10, 0) ? "attidentity::text" : "' '::text as attidentity")},
  {(connection.PostgreSqlVersion >= new Version(12, 0) ? "attgenerated::text" : "' '::text as attgenerated")},
  {(connection.PostgreSqlVersion >= new Version(14, 0) ? "attcompression::text" : "''::text as attcompression")},
  format_type(typ.oid, atttypmod) AS formatted_typname,
  format_type(basetyp.oid, typ.typtypmod) AS formatted_basetypname,
  CASE
    WHEN pg_proc.proname = 'array_recv' THEN 'a'
    ELSE typ.typtype
  END AS typtype,
  CASE WHEN pg_proc.proname='array_recv' THEN elemtyp.typname END AS elemtypname,
  NOT (attnotnull OR typ.typnotnull) AS nullable,
  CASE
    WHEN atthasdef THEN (SELECT pg_get_expr(adbin, cls.oid) FROM pg_attrdef WHERE adrelid = cls.oid AND adnum = attr.attnum)
  END AS default,

  -- Sequence options for identity columns
  {(connection.PostgreSqlVersion >= new Version(10, 0) ?
      "format_type(seqtypid, 0) AS seqtype, seqstart, seqmin, seqmax, seqincrement, seqcycle, seqcache" :
      "NULL AS seqtype, NULL AS seqstart, NULL AS seqmin, NULL AS seqmax, NULL AS seqincrement, NULL AS seqcycle, NULL AS seqcache")}

FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
LEFT JOIN pg_attribute AS attr ON attrelid = cls.oid
LEFT JOIN pg_type AS typ ON attr.atttypid = typ.oid
LEFT JOIN pg_proc ON pg_proc.oid = typ.typreceive
LEFT JOIN pg_type AS elemtyp ON (elemtyp.oid = typ.typelem)
LEFT JOIN pg_type AS basetyp ON (basetyp.oid = typ.typbasetype)
LEFT JOIN pg_description AS des ON des.objoid = cls.oid AND des.objsubid = attnum
{(connection.PostgreSqlVersion >= new Version(9, 1) ? "LEFT JOIN pg_collation as coll ON coll.oid = attr.attcollation" : "")}
-- Bring in identity sequences the depend on this column
LEFT JOIN pg_depend AS dep ON dep.refobjid = cls.oid AND dep.refobjsubid = attr.attnum AND dep.deptype = 'i'
{(connection.PostgreSqlVersion >= new Version(10, 0) ? "LEFT JOIN pg_sequence AS seq ON seq.seqrelid = dep.objid" : "")}
WHERE
  cls.relkind IN ('r', 'v', 'm', 'f') AND
  nspname NOT IN ({internalSchemas}) AND
  attnum > 0 AND
  cls.relname <> '{HistoryRepository.DefaultTableName}' AND
  -- Exclude tables which are members of PG extensions
  NOT EXISTS (
    SELECT 1 FROM pg_depend WHERE
      classid=(
        SELECT cls.oid
        FROM pg_class AS cls
        JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
        WHERE relname='pg_class' AND ns.nspname='pg_catalog'
      ) AND
      objid=cls.oid AND
      deptype IN ('e', 'x')
  )
  {tableFilter}
ORDER BY attnum";

        using var command = new NpgsqlCommand(commandText, connection);
        using var reader = command.ExecuteReader();

        var tableGroups = reader.Cast<DbDataRecord>().GroupBy(ddr => (
            tableSchema: ddr.GetFieldValue<string>("nspname"),
            tableName: ddr.GetFieldValue<string>("relname")));

        foreach (var tableGroup in tableGroups)
        {
            var tableSchema = tableGroup.Key.tableSchema;
            var tableName = tableGroup.Key.tableName;

            var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

            foreach (var record in tableGroup)
            {
                var columnName = record.GetFieldValue<string>("attname");

                // We need to know about dropped columns because constraints take them into
                // account when referencing columns. We'll get rid of them before returning the model.
                if (record.GetValueOrDefault<bool>("attisdropped"))
                {
                    table.Columns.Add(null!);
                    continue;
                }

                var formattedTypeName = AdjustFormattedTypeName(record.GetFieldValue<string>("formatted_typname"));
                var formattedBaseTypeName = record.GetValueOrDefault<string>("formatted_basetypname");
                var (storeType, systemTypeName) = formattedBaseTypeName is null
                    ? (formattedTypeName, record.GetFieldValue<string>("typname"))
                    : (formattedBaseTypeName, record.GetFieldValue<string>("basetypname")); // domain type

                var column = new DatabaseColumn
                {
                    Table = table,
                    Name = columnName,
                    StoreType = storeType,
                    IsNullable = record.GetValueOrDefault<bool>("nullable"),
                };

                // Enum types cannot be scaffolded for now (nor can domains of enum types),
                // skip with an informative message
                if (enums.Contains(formattedTypeName) ||
                    formattedBaseTypeName is not null && enums.Contains(formattedBaseTypeName))
                {
                    logger.EnumColumnSkippedWarning($"{DisplayName(tableSchema, tableName)}.{column.Name}");
                    // We need to know about skipped columns because constraints take them into
                    // account when referencing columns. We'll get rid of them before returning the model.
                    table.Columns.Add(null!);
                    continue;
                }

                // Default values and PostgreSQL 12 generated columns
                if (record.GetFieldValue<string>("attgenerated") == "s")
                {
                    column.ComputedColumnSql = record.GetValueOrDefault<string>("default");
                    column.IsStored = true;
                }
                else
                {
                    column.DefaultValueSql = record.GetValueOrDefault<string>("default");
                    AdjustDefaults(column, systemTypeName);
                }

                // Identify IDENTITY columns, as well as SERIAL ones.
                var isIdentity = false;
                switch (record.GetFieldValue<string>("attidentity"))
                {
                    case "a":
                        column[NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityAlwaysColumn;
                        isIdentity = true;
                        break;
                    case "d":
                        column[NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn;
                        isIdentity = true;
                        break;
                    default:
                        // Hacky but necessary...
                        // We identify serial columns by examining their default expression, and reverse-engineer these as ValueGenerated.OnAdd.
                        // We can't actually parse this since the table and column names are concatenated and may contain arbitrary underscores,
                        // so we construct various possibilities and compare against them.
                        // TODO: Think about composite keys? Do serial magic only for non-composite.
                        if (SerialTypes.Contains(systemTypeName))
                        {
                            var seqName = $"{column.Table.Name}_{column.Name}_seq";
                            if (column.Table.Schema == "public" &&
                                (column.DefaultValueSql == $"nextval('{seqName}'::regclass)" ||
                                    column.DefaultValueSql == $"nextval('\"{seqName}\"'::regclass)")
                                ||  // non-public schema
                                column.DefaultValueSql == $"nextval('{column.Table.Schema}.{seqName}'::regclass)" ||
                                column.DefaultValueSql == $"nextval('{column.Table.Schema}.\"{seqName}\"'::regclass)" ||
                                column.DefaultValueSql == $"nextval('\"{column.Table.Schema}\".{seqName}'::regclass)" ||
                                column.DefaultValueSql == $"nextval('\"{column.Table.Schema}\".\"{seqName}\"'::regclass)")
                            {
                                column.DefaultValueSql = null;
                                // Serial is the default value generation strategy, so NpgsqlAnnotationCodeGenerator
                                // makes sure it isn't actually rendered
                                column[NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn;
                            }
                        }

                        break;
                }

                if (column[NpgsqlAnnotationNames.ValueGenerationStrategy] is not null)
                {
                    column.ValueGenerated = ValueGenerated.OnAdd;
                }

                if (isIdentity)
                {
                    // Get the options for the associated sequence
                    var seqInfo = ReadSequenceInfo(record, connection.PostgreSqlVersion);
                    var sequenceData = new IdentitySequenceOptionsData
                    {
                        StartValue = seqInfo.StartValue,
                        MinValue = seqInfo.MinValue,
                        MaxValue = seqInfo.MaxValue,
                        IncrementBy = (int)(seqInfo.IncrementBy ?? 1),
                        IsCyclic = seqInfo.IsCyclic ?? false,
                        NumbersToCache = seqInfo.NumbersToCache ?? 1
                    };

                    if (!sequenceData.Equals(IdentitySequenceOptionsData.Empty))
                    {
                        column[NpgsqlAnnotationNames.IdentityOptions] = sequenceData.Serialize();
                    }
                }

                if (record.GetValueOrDefault<string>("description") is { } comment)
                {
                    column.Comment = comment;
                }

                if (record.GetValueOrDefault<string>("collname") is { } collation && collation != "default")
                {
                    column.Collation = collation;
                }

                if (record.GetValueOrDefault<string>("attcompression") is { } compressionMethodChar)
                {
                    column[NpgsqlAnnotationNames.CompressionMethod] = compressionMethodChar switch
                    {
                        "p" => "pglz",
                        "l" => "lz4",
                        _ => null
                    };
                }

                logger.ColumnFound(
                    DisplayName(tableSchema, tableName),
                    column.Name,
                    formattedTypeName,
                    column.IsNullable,
                    isIdentity,
                    column.DefaultValueSql,
                    column.ComputedColumnSql);

                table.Columns.Add(column);
            }
        }
    }

    /// <summary>
    /// Queries the database for defined indexes and registers them with the model.
    /// </summary>
    private static void GetIndexes(
        NpgsqlConnection connection,
        IReadOnlyList<DatabaseTable> tables,
        string? tableFilter,
        string internalSchemas,
        List<uint> constraintIndexes,
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
    {
        // Load the pg_opclass table (https://www.postgresql.org/docs/current/catalog-pg-opclass.html),
        // which is referenced by the indices we'll load below
        var opClasses = new Dictionary<uint, (string Name, bool IsDefault)>();
        try
        {
            using var command = new NpgsqlCommand("SELECT oid, opcname, opcdefault FROM pg_opclass", connection);
            using var reader = command.ExecuteReader();

            foreach (var opClass in reader.Cast<DbDataRecord>())
            {
                opClasses[opClass.GetFieldValue<uint>("oid")] = (
                    opClass.GetFieldValue<string>("opcname"),
                    opClass.GetFieldValue<bool>("opcdefault"));
            }
        }
        catch (PostgresException e)
        {
            logger.Logger.LogWarning(e,
                "Could not load index operator classes from pg_opclass. Operator classes will not be scaffolded");
        }

        var collations = new Dictionary<uint, string>();

        if (connection.PostgreSqlVersion >= new Version(9, 1))
        {
            using (var command = new NpgsqlCommand("SELECT oid, collname FROM pg_collation", connection))
            using (var reader = command.ExecuteReader())
            {
                foreach (var collation in reader.Cast<DbDataRecord>())
                {
                    collations[collation.GetFieldValue<uint>("oid")] = collation.GetFieldValue<string>("collname");
                }
            }
        }

        var commandText = $@"
SELECT
  idxcls.oid AS idx_oid,
  nspname,
  cls.relname AS cls_relname,
  idxcls.relname AS idx_relname,
  indisunique,
  {(connection.PostgreSqlVersion >= new Version(11, 0) ? "indnkeyatts" : "indnatts AS indnkeyatts")},
  {(connection.PostgreSqlVersion >= new Version(9, 6) ? "pg_indexam_has_property(am.oid, 'can_order') as amcanorder" : "amcanorder")},
  indkey,
  amname,
  indclass,
  indoption,
  {(connection.PostgreSqlVersion >= new Version(9, 1) ? "indcollation" : "''::oidvector AS indcollation")},
  CASE
    WHEN indexprs IS NULL THEN NULL
    ELSE pg_get_expr(indexprs, cls.oid)
  END AS exprs,
  CASE
    WHEN indpred IS NULL THEN NULL
    ELSE pg_get_expr(indpred, cls.oid)
  END AS pred
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
JOIN pg_index AS idx ON indrelid = cls.oid
JOIN pg_class AS idxcls ON idxcls.oid = indexrelid
JOIN pg_am AS am ON am.oid = idxcls.relam
WHERE
  cls.relkind = 'r' AND
  nspname NOT IN ({internalSchemas}) AND
  NOT indisprimary AND
  cls.relname <> '{HistoryRepository.DefaultTableName}' AND
  -- Exclude tables which are members of PG extensions
  NOT EXISTS (
    SELECT 1 FROM pg_depend WHERE
      classid=(
        SELECT cls.oid
        FROM pg_class AS cls
        JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
        WHERE relname='pg_class' AND ns.nspname='pg_catalog'
      ) AND
      objid=cls.oid AND
      deptype IN ('e', 'x')
  )
  {tableFilter}";

        using (var command = new NpgsqlCommand(commandText, connection))
        using (var reader = command.ExecuteReader())
        {
            var tableGroups = reader.Cast<DbDataRecord>().GroupBy(ddr => (
                tableSchema: ddr.GetFieldValue<string>("nspname"),
                tableName: ddr.GetFieldValue<string>("cls_relname")));

            foreach (var tableGroup in tableGroups)
            {
                var tableSchema = tableGroup.Key.tableSchema;
                var tableName = tableGroup.Key.tableName;

                var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                foreach (var record in tableGroup)
                {
                    // Constraints are detected separately (see GetConstraints), and we don't want their
                    // supporting indexes to appear independently.
                    if (constraintIndexes.Contains(record.GetFieldValue<uint>("idx_oid")))
                    {
                        continue;
                    }

                    var indexName = record.GetFieldValue<string>("idx_relname");
                    var index = new DatabaseIndex
                    {
                        Table = table,
                        Name = indexName,
                        IsUnique = record.GetFieldValue<bool>("indisunique")
                    };

                    var numKeyColumns = record.GetFieldValue<short>("indnkeyatts");
                    var columnIndices = record.GetFieldValue<short[]>("indkey");
                    var tableColumns = (List<DatabaseColumn>)table.Columns;

                    if (columnIndices.Any(i => i == 0))
                    {
                        // Expression index, not supported
                        logger.ExpressionIndexSkippedWarning(index.Name, DisplayName(tableSchema, tableName));
                        continue;

                        /*
                        var expressions = record.GetValueOrDefault<string>("exprs");
                        if (expressions is null)
                            throw new Exception($"Seen 0 in indkey for index {index.Name} but indexprs is null");
                        index[NpgsqlAnnotationNames.IndexExpression] = expressions;
                        */
                    }

                    // Key columns come before non-key (included) columns, process them first
                    foreach (var i in columnIndices.Take(numKeyColumns))
                    {
                        if (tableColumns[i - 1] is { } indexKeyColumn)
                        {
                            index.Columns.Add(indexKeyColumn);
                        }
                        else
                        {
                            logger.UnsupportedColumnIndexSkippedWarning(index.Name, DisplayName(tableSchema, tableName));
                            goto IndexEnd;
                        }
                    }

                    // Now go over non-key (included columns) if any are present
                    if (columnIndices.Length > numKeyColumns)
                    {
                        var nonKeyColumns = new List<string>();
                        foreach (var i in columnIndices.Skip(numKeyColumns))
                        {
                            if (tableColumns[i - 1] is { } indexKeyColumn)
                            {
                                nonKeyColumns.Add(indexKeyColumn.Name);
                            }
                            else
                            {
                                logger.UnsupportedColumnIndexSkippedWarning(index.Name, DisplayName(tableSchema, tableName));
                                goto IndexEnd;
                            }
                        }

                        // Scaffolding included/covered properties is currently blocked, see #2194
                        // index[NpgsqlAnnotationNames.IndexInclude] = nonKeyColumns.ToArray();
                    }

                    if (record.GetValueOrDefault<string>("pred") is { } predicate)
                    {
                        index.Filter = predicate;
                    }

                    // It's cleaner to always output the index method on the database model,
                    // even when it's btree (the default);
                    // NpgsqlAnnotationCodeGenerator can then omit it as by-convention.
                    // However, because of https://github.com/aspnet/EntityFrameworkCore/issues/11846 we omit
                    // the annotation from the model entirely.
                    if (record.GetValueOrDefault<string>("amname") is { } indexMethod && indexMethod != "btree")
                    {
                        index[NpgsqlAnnotationNames.IndexMethod] = indexMethod;
                    }

                    // Handle index operator classes, which we pre-loaded
                    var opClassNames = record
                        .GetFieldValue<uint[]>("indclass")
                        .Select(oid => opClasses.TryGetValue(oid, out var opc) && !opc.IsDefault ? opc.Name : null)
                        .ToArray();

                    if (opClassNames.Any(op => op is not null))
                    {
                        index[NpgsqlAnnotationNames.IndexOperators] = opClassNames;
                    }

                    var columnCollations = record
                        .GetFieldValue<uint[]>("indcollation")
                        .Select(oid => collations.TryGetValue(oid, out var collation) && collation != "default" ? collation : null)
                        .ToArray();

                    if (columnCollations.Any(coll => coll is not null))
                    {
                        index[RelationalAnnotationNames.Collation] = columnCollations;
                    }

                    if (record.GetValueOrDefault<bool>("amcanorder"))
                    {
                        var options = record.GetFieldValue<ushort[]>("indoption");

                        // The first bit in indoption specifies whether values are sorted in descending order, the second whether
                        // NULLs are sorted first instead of last.
                        var isDescending = options.Select(val => (val & 0x0001) != 0).ToList();
                        var nullSortOrders = options
                            .Select(val => (val & 0x0002) != 0 ? NullSortOrder.NullsFirst : NullSortOrder.NullsLast)
                            .ToArray();

                        index.IsDescending = isDescending;

                        if (!SortOrderHelper.IsDefaultNullSortOrder(nullSortOrders, isDescending))
                        {
                            index[NpgsqlAnnotationNames.IndexNullSortOrder] = nullSortOrders;
                        }
                    }

                    table.Indexes.Add(index);

                    IndexEnd: ;
                }
            }
        }
    }

    /// <summary>
    /// Queries the database for defined constraints and registers them with the model.
    /// </summary>
    private static void GetConstraints(
        NpgsqlConnection connection,
        IReadOnlyList<DatabaseTable> tables,
        string? tableFilter,
        string internalSchemas,
        out List<uint> constraintIndexes,
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
    {
        var commandText = $@"
SELECT
  ns.nspname,
  cls.relname,
  conname,
  contype::text,
  conkey,
  conindid,
  frnns.nspname AS fr_nspname,
  frncls.relname AS fr_relname,
  confkey,
  confdeltype::text
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
JOIN pg_constraint as con ON con.conrelid = cls.oid
LEFT OUTER JOIN pg_class AS frncls ON frncls.oid = con.confrelid
LEFT OUTER JOIN pg_namespace as frnns ON frnns.oid = frncls.relnamespace
WHERE
  cls.relkind = 'r' AND
  ns.nspname NOT IN ({internalSchemas}) AND
  con.contype IN ('p', 'f', 'u') AND
  cls.relname <> '{HistoryRepository.DefaultTableName}' AND
  -- Exclude tables which are members of PG extensions
  NOT EXISTS (
    SELECT 1 FROM pg_depend WHERE
      classid=(
        SELECT cls.oid
        FROM pg_class AS cls
        JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
        WHERE relname='pg_class' AND ns.nspname='pg_catalog'
      ) AND
      objid=cls.oid AND
      deptype IN ('e', 'x')
  )
  {tableFilter}";

        using var command = new NpgsqlCommand(commandText, connection);
        using var reader = command.ExecuteReader();

        constraintIndexes = new List<uint>();
        var tableGroups = reader.Cast<DbDataRecord>().GroupBy(ddr => (
            tableSchema: ddr.GetFieldValue<string>("nspname"),
            tableName: ddr.GetFieldValue<string>("relname")));

        foreach (var tableGroup in tableGroups)
        {
            var tableSchema = tableGroup.Key.tableSchema;
            var tableName = tableGroup.Key.tableName;

            var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

            // Primary keys
            foreach (var primaryKeyRecord in tableGroup.Where(ddr => ddr.GetFieldValue<string>("contype") == "p"))
            {
                var pkName = primaryKeyRecord.GetValueOrDefault<string>("conname");
                var primaryKey = new DatabasePrimaryKey
                {
                    Table = table,
                    Name = pkName
                };

                foreach (var pkColumnIndex in primaryKeyRecord.GetFieldValue<short[]>("conkey"))
                {
                    if (table.Columns[pkColumnIndex - 1] is { } pkColumn)
                    {
                        primaryKey.Columns.Add(pkColumn);
                    }
                    else
                    {
                        logger.UnsupportedColumnConstraintSkippedWarning(primaryKey.Name, DisplayName(tableSchema, tableName));
                        goto PkEnd;
                    }
                }

                table.PrimaryKey = primaryKey;
                PkEnd: ;
            }

            // Foreign keys
            foreach (var foreignKeyRecord in tableGroup.Where(ddr => ddr.GetFieldValue<string>("contype") == "f"))
            {
                var fkName = foreignKeyRecord.GetFieldValue<string>("conname");
                var principalTableSchema = foreignKeyRecord.GetFieldValue<string>("fr_nspname");
                var principalTableName = foreignKeyRecord.GetFieldValue<string>("fr_relname");
                var onDeleteAction = foreignKeyRecord.GetFieldValue<string>("confdeltype");

                var principalTable =
                    tables.FirstOrDefault(t =>
                        principalTableSchema == t.Schema && principalTableName == t.Name)
                    ?? tables.FirstOrDefault(t =>
                        principalTableSchema.Equals(t.Schema, StringComparison.OrdinalIgnoreCase) &&
                        principalTableName.Equals(t.Name, StringComparison.OrdinalIgnoreCase));

                if (principalTable is null)
                {
                    logger.ForeignKeyReferencesMissingPrincipalTableWarning(
                        fkName,
                        DisplayName(table.Schema, table.Name),
                        DisplayName(principalTableSchema, principalTableName));

                    continue;
                }

                var foreignKey = new DatabaseForeignKey
                {
                    Table = table,
                    Name = fkName,
                    PrincipalTable = principalTable,
                    OnDelete = ConvertToReferentialAction(onDeleteAction)
                };

                var columnIndices = foreignKeyRecord.GetFieldValue<short[]>("conkey");
                var principalColumnIndices = foreignKeyRecord.GetFieldValue<short[]>("confkey");

                if (columnIndices.Length != principalColumnIndices.Length)
                {
                    throw new InvalidOperationException("Found varying lengths for column and principal column indices.");
                }

                var principalColumns = (List<DatabaseColumn>)principalTable.Columns;

                for (var i = 0; i < columnIndices.Length; i++)
                {
                    var foreignKeyColumn = table.Columns[columnIndices[i] - 1];
                    var foreignKeyPrincipalColumn = principalColumns[principalColumnIndices[i] - 1];
                    if (foreignKeyColumn is null || foreignKeyPrincipalColumn is null)
                    {
                        logger.UnsupportedColumnConstraintSkippedWarning(foreignKey.Name, DisplayName(tableSchema, tableName));
                        goto ForeignKeyEnd;
                    }

                    foreignKey.Columns.Add(foreignKeyColumn);
                    foreignKey.PrincipalColumns.Add(foreignKeyPrincipalColumn);
                }

                table.ForeignKeys.Add(foreignKey);
                ForeignKeyEnd: ;
            }

            // Unique constraints
            foreach (var record in tableGroup.Where(ddr => ddr.GetValueOrDefault<string>("contype") == "u"))
            {
                var name = record.GetValueOrDefault<string>("conname");

                logger.UniqueConstraintFound(name, DisplayName(tableSchema, tableName));

                var uniqueConstraint = new DatabaseUniqueConstraint
                {
                    Table = table,
                    Name = name
                };

                foreach (var columnIndex in record.GetFieldValue<short[]>("conkey"))
                {
                    var constraintColumn = table.Columns[columnIndex - 1];
                    if (constraintColumn is null)
                    {
                        logger.UnsupportedColumnConstraintSkippedWarning(uniqueConstraint.Name, DisplayName(tableSchema, tableName));
                        goto UniqueConstraintEnd;
                    }

                    uniqueConstraint.Columns.Add(constraintColumn);
                }

                table.UniqueConstraints.Add(uniqueConstraint);
                constraintIndexes.Add(record.GetValueOrDefault<uint>("conindid"));

                UniqueConstraintEnd: ;
            }
        }
    }

    /// <summary>
    /// Queries the database for defined sequences and registers them with the model.
    /// </summary>
    private static IEnumerable<DatabaseSequence> GetSequences(
        NpgsqlConnection connection,
        DatabaseModel databaseModel,
        Func<string, string>? schemaFilter,
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
    {
        // Note: we consult information_schema.sequences instead of pg_sequence but the latter was only introduced in PG 10
        var commandText = $@"
SELECT
  sequence_schema, sequence_name,
  data_type AS seqtype,
  {(connection.PostgreSqlVersion >= new Version(9, 1) ? "start_value" : "1")}::bigint AS seqstart,
  minimum_value::bigint AS seqmin,
  maximum_value::bigint AS seqmax,
  increment::bigint AS seqincrement,
  1::bigint AS seqcache,
  CASE
    WHEN cycle_option = 'YES' THEN TRUE
    ELSE FALSE
  END AS seqcycle
FROM information_schema.sequences
JOIN pg_namespace AS ns ON ns.nspname = sequence_schema
JOIN pg_class AS cls ON cls.relnamespace = ns.oid AND cls.relname = sequence_name
WHERE
  cls.relkind = 'S'
  /* AND seqtype IN ('integer', 'bigint', 'smallint') */
  /* Filter out owned serial and identity sequences */
  AND NOT EXISTS (SELECT * FROM pg_depend AS dep WHERE dep.objid = cls.oid AND dep.deptype IN ('i', 'I', 'a'))
  {(schemaFilter is not null ? $"AND {schemaFilter("nspname")}" : null)}";

        using var command = new NpgsqlCommand(commandText, connection);
        using var reader = command.ExecuteReader();

        foreach (var record in reader.Cast<DbDataRecord>())
        {
            var sequenceName = reader.GetFieldValue<string>("sequence_name");
            var sequenceSchema = reader.GetFieldValue<string>("sequence_schema");

            var seqInfo = ReadSequenceInfo(record, connection.PostgreSqlVersion);
            var sequence = new DatabaseSequence
            {
                Database = databaseModel,
                Name = sequenceName,
                Schema = sequenceSchema,
                StoreType = seqInfo.StoreType,
                StartValue = seqInfo.StartValue,
                MinValue = seqInfo.MinValue,
                MaxValue = seqInfo.MaxValue,
                IncrementBy = (int?)seqInfo.IncrementBy,
                IsCyclic = seqInfo.IsCyclic
            };

            yield return sequence;
        }
    }

    /// <summary>
    /// Queries the database for defined enums and registers them with the model.
    /// </summary>
    private static HashSet<string> GetEnums(NpgsqlConnection connection, DatabaseModel databaseModel)
    {
        var enums = new HashSet<string>();

        // pg_enum doesn't exist on Redshift
        if (connection.PostgreSqlVersion < new Version(8, 3))
        {
            return enums;
        }

        var commandText = $@"
SELECT
  nspname,
  typname,
  array_agg(enumlabel{(connection.PostgreSqlVersion >= new Version(9, 1) ? " ORDER BY enumsortorder" : "")}) AS labels
FROM pg_enum
JOIN pg_type ON pg_type.oid = enumtypid
JOIN pg_namespace ON pg_namespace.oid = pg_type.typnamespace
GROUP BY nspname, typname";

        using var command = new NpgsqlCommand(commandText, connection);
        using var reader = command.ExecuteReader();

        // TODO: just return a collection and make this a static utility method.
        while (reader.Read())
        {
            var schema = reader.GetFieldValue<string?>("nspname");
            var name = reader.GetFieldValue<string>("typname");
            var labels = reader.GetFieldValue<string[]>("labels");

            if (schema == "public")
            {
                schema = null;
            }

            PostgresEnum.GetOrAddPostgresEnum(databaseModel, schema, name, labels);
            enums.Add(name);
        }

        return enums;
    }

    /// <summary>
    /// Queries the installed database extensions and registers them with the model.
    /// </summary>
    private static void GetExtensions(NpgsqlConnection connection, DatabaseModel databaseModel)
    {
        const string commandText = @"
SELECT ns.nspname, extname, extversion FROM pg_extension
JOIN pg_namespace ns ON ns.oid=extnamespace";
        using var command = new NpgsqlCommand(commandText, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var schema = reader.GetFieldValue<string?>("nspname");
            var name = reader.GetString(reader.GetOrdinal("extname"));
            var version = reader.GetValueOrDefault<string>("extversion");

            if (name == "plpgsql") // Implicitly installed in all PG databases
            {
                continue;
            }

            databaseModel.GetOrAddPostgresExtension(schema, name, version);
        }
    }

    private static void GetCollations(
        NpgsqlConnection connection,
        DatabaseModel databaseModel,
        string internalSchemas,
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
    {
        var commandText = @$"
SELECT
    nspname, collname, collprovider, collcollate, collctype,
    {(connection.PostgreSqlVersion >= new Version(15, 0) ? "colliculocale" : "NULL AS colliculocale")},
    {(connection.PostgreSqlVersion >= new Version(12, 0) ? "collisdeterministic" : "true AS collisdeterministic")}
FROM pg_collation coll
    JOIN pg_namespace ns ON ns.oid=coll.collnamespace
WHERE
    nspname NOT IN ({internalSchemas})";

        try
        {
            using var command = new NpgsqlCommand(commandText, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var schema = reader.GetString(reader.GetOrdinal("nspname"));
                var name = reader.GetString(reader.GetOrdinal("collname"));
                var icuLocale = reader.GetValueOrDefault<string>("colliculocale");
                var lcCollate = reader.GetValueOrDefault<string>("collcollate");
                var lcCtype = reader.GetValueOrDefault<string>("collctype");
                var providerCode = reader.GetChar(reader.GetOrdinal("collprovider"));
                var isDeterministic = reader.GetBoolean(reader.GetOrdinal("collisdeterministic"));

                string? provider;
                switch (providerCode)
                {
                    case 'c':
                        provider = "libc";
                        break;
                    case 'i':
                        provider = "icu";
                        break;
                    case 'd':
                        provider = null;
                        break;
                    default:
                        logger.Logger.LogWarning(
                            $"Unknown collation provider code {providerCode} for collation {name}, skipping.");
                        continue;
                }

                // Starting with PG15, ICU collations only have colliculocale populated.
                if (lcCollate is null || lcCtype is null)
                {
                    Debug.Assert(lcCollate is null && lcCtype is null);
                    Debug.Assert(icuLocale is not null);
                    lcCollate = icuLocale;
                    lcCtype = icuLocale;
                }

                logger.CollationFound(schema, name, lcCollate, lcCtype, provider, isDeterministic);

                PostgresCollation.GetOrAddCollation(
                    databaseModel, schema, name, lcCollate!, lcCtype, provider, isDeterministic);
            }
        }
        catch (PostgresException e)
        {
            logger.Logger.LogWarning(e, "Could not load database collations.");
        }
    }

    #endregion

    #region Configure default values

    /// <summary>
    /// Configures the default value for a column.
    /// </summary>
    /// <param name="column">The column to configure.</param>
    /// <param name="systemTypeName">The type name of the column.</param>
    private static void AdjustDefaults(DatabaseColumn column, string systemTypeName)
    {
        var defaultValue = column.DefaultValueSql;
        if (defaultValue is null || defaultValue == "(NULL)")
        {
            column.DefaultValueSql = null;
            return;
        }

        if (column.IsNullable)
        {
            return;
        }

        if (defaultValue == "0")
        {
            if (systemTypeName == "float4" ||
                systemTypeName == "float8" ||
                systemTypeName == "int2" ||
                systemTypeName == "int4" ||
                systemTypeName == "int8" ||
                systemTypeName == "money" ||
                systemTypeName == "numeric")
            {
                column.DefaultValueSql = null;
                return;
            }
        }

        if (defaultValue == "0.0" || defaultValue == "'0'::numeric")
        {
            if (systemTypeName == "numeric" ||
                systemTypeName == "float4" ||
                systemTypeName == "float8" ||
                systemTypeName == "money")
            {
                column.DefaultValueSql = null;
                return;
            }
        }

        if (systemTypeName == "bool" && defaultValue == "false" ||
            systemTypeName == "date" && defaultValue == "'0001-01-01'::date" ||
            systemTypeName == "timestamp" && defaultValue == "'1900-01-01 00:00:00'::timestamp without time zone" ||
            systemTypeName == "time" && defaultValue == "'00:00:00'::time without time zone" ||
            systemTypeName == "interval" && defaultValue == "'00:00:00'::interval" ||
            systemTypeName == "uuid" && defaultValue == "'00000000-0000-0000-0000-000000000000'::uuid")
        {
            column.DefaultValueSql = null;
        }
    }

    private static SequenceInfo ReadSequenceInfo(DbDataRecord record, Version postgresVersion)
    {
        var storeType = record.GetFieldValue<string>("seqtype");
        var startValue = record.GetValueOrDefault<long>("seqstart");
        var minValue = record.GetValueOrDefault<long>("seqmin");
        var maxValue = record.GetValueOrDefault<long>("seqmax");
        var incrementBy = record.GetValueOrDefault<long>("seqincrement");
        var isCyclic = record.GetValueOrDefault<bool>("seqcycle");
        var numbersToCache = (int)record.GetValueOrDefault<long>("seqcache");

        long defaultStart, defaultMin, defaultMax;

        switch (storeType)
        {
            case "smallint" when incrementBy > 0:
                defaultMin = 1;
                defaultMax = short.MaxValue;
                defaultStart = minValue;
                break;

            case "smallint":
                // PostgreSQL 10 changed the default minvalue for a descending sequence, see #264
                defaultMin = postgresVersion >= new Version(10, 0)
                    ? short.MinValue
                    : short.MinValue + 1;
                defaultMax = -1;
                defaultStart = maxValue;
                break;

            case "integer" when incrementBy > 0:
                defaultMin = 1;
                defaultMax = int.MaxValue;
                defaultStart = minValue;
                break;

            case "integer":
                // PostgreSQL 10 changed the default minvalue for a descending sequence, see #264
                defaultMin = postgresVersion >= new Version(10, 0)
                    ? int.MinValue
                    : int.MinValue + 1;
                defaultMax = -1;
                defaultStart = maxValue;
                break;

            case "bigint" when incrementBy > 0:
                defaultMin = 1;
                defaultMax = long.MaxValue;
                defaultStart = minValue;
                break;

            case "bigint":
                // PostgreSQL 10 changed the default minvalue for a descending sequence, see #264
                defaultMin = postgresVersion >= new Version(10, 0)
                    ? long.MinValue
                    : long.MinValue + 1;
                defaultMax = -1;
                defaultStart = maxValue;
                break;

            default:
                throw new NotSupportedException($"Sequence has datatype {storeType} which isn't an expected sequence type.");
        }

        return new SequenceInfo(storeType)
        {
            StartValue = startValue == defaultStart ? null : startValue,
            MinValue = minValue == defaultMin ? null : minValue,
            MaxValue = maxValue == defaultMax ? null : maxValue,
            IncrementBy = incrementBy == 1 ? null : incrementBy,
            IsCyclic = isCyclic == false ? null : true,
            NumbersToCache = numbersToCache == 1 ? null : numbersToCache
        };
    }

    private sealed class SequenceInfo
    {
        public SequenceInfo(string storeType) => StoreType = storeType;
        public string StoreType { get; set; }
        public long? StartValue { get; set; }
        public long? MinValue { get; set; }
        public long? MaxValue { get; set; }
        public long? IncrementBy { get; set; }
        public bool? IsCyclic { get; set; }
        public long? NumbersToCache { get; set; }
    }

    #endregion

    #region Filter fragment generators

    /// <summary>
    /// Builds a delegate to generate a schema filter fragment.
    /// </summary>
    private static Func<string, string>? GenerateSchemaFilter(IReadOnlyList<string> schemas)
        => schemas.Any()
            ? s => $"{s} IN ({string.Join(", ", schemas.Select(EscapeLiteral))})"
            : null;

    /// <summary>
    /// Builds a delegate to generate a table filter fragment.
    /// </summary>
    private static Func<string, string, string>? GenerateTableFilter(
        IReadOnlyList<(string? Schema, string Table)> tables,
        Func<string, string>? schemaFilter)
        => schemaFilter is not null || tables.Any()
            ? (s, t) =>
            {
                var tableFilterBuilder = new StringBuilder();

                var openBracket = false;
                if (schemaFilter is not null)
                {
                    tableFilterBuilder
                        .Append("(")
                        .Append(schemaFilter(s));
                    openBracket = true;
                }

                if (tables.Any())
                {
                    if (openBracket)
                    {
                        tableFilterBuilder
                            .AppendLine()
                            .Append("OR ");
                    }
                    else
                    {
                        tableFilterBuilder.Append("(");
                        openBracket = true;
                    }

                    var tablesWithoutSchema = tables.Where(e => string.IsNullOrEmpty(e.Schema)).ToList();
                    if (tablesWithoutSchema.Any())
                    {
                        tableFilterBuilder.Append(t);
                        tableFilterBuilder.Append(" IN (");
                        tableFilterBuilder.Append(string.Join(", ", tablesWithoutSchema.Select(e => EscapeLiteral(e.Table))));
                        tableFilterBuilder.Append(")");
                    }

                    var tablesWithSchema = tables.Where(e => !string.IsNullOrEmpty(e.Schema)).ToList();
                    if (tablesWithSchema.Any())
                    {
                        if (tablesWithoutSchema.Any())
                        {
                            tableFilterBuilder.Append(" OR ");
                        }

                        tableFilterBuilder.Append(t);
                        tableFilterBuilder.Append(" IN (");
                        tableFilterBuilder.Append(string.Join(", ", tablesWithSchema.Select(e => EscapeLiteral(e.Table))));
                        tableFilterBuilder.Append(") AND (");
                        tableFilterBuilder.Append(s);
                        tableFilterBuilder.Append(" || '.' || ");
                        tableFilterBuilder.Append(t);
                        tableFilterBuilder.Append(") IN (");
                        tableFilterBuilder.Append(string.Join(", ", tablesWithSchema.Select(e => EscapeLiteral($"{e.Schema}.{e.Table}"))));
                        tableFilterBuilder.Append(")");
                    }
                }

                if (openBracket)
                {
                    tableFilterBuilder.Append(")");
                }

                return tableFilterBuilder.ToString();
            }
            : null;

    #endregion

    #region Utilities

    /// <summary>
    /// Type names as returned by PostgreSQL's format_type need to be cleaned up a bit
    /// </summary>
    private static string AdjustFormattedTypeName(string formattedTypeName)
    {
        // User-defined types (e.g. enums) with capital letters get formatted with quotes, remove.
        if (formattedTypeName[0] == '"')
        {
            formattedTypeName = formattedTypeName.Substring(1, formattedTypeName.Length - 2);
        }

        if (formattedTypeName == "bpchar")
        {
            formattedTypeName = "char";
        }

        return formattedTypeName;
    }

    /// <summary>
    /// Maps a character to a <see cref="ReferentialAction"/>.
    /// </summary>
    private static ReferentialAction ConvertToReferentialAction(string onDeleteAction)
        => onDeleteAction switch
        {
            "a" => ReferentialAction.NoAction,
            "r" => ReferentialAction.Restrict,
            "c" => ReferentialAction.Cascade,
            "n" => ReferentialAction.SetNull,
            "d" => ReferentialAction.SetDefault,
            _ => throw new ArgumentOutOfRangeException(
                $"Unknown value {onDeleteAction} for foreign key deletion action code.")
        };

    /// <summary>
    /// Constructs the display name given a schema and table name.
    /// </summary>
    // TODO: should this default to/screen out the public schema?
    private static string DisplayName(string? schema, string name)
        => string.IsNullOrEmpty(schema) ? name : $"{schema}.{name}";

    /// <summary>
    /// Parses the table name into a tuple of schema name and table name where the schema may be null.
    /// </summary>
    private static (string? Schema, string Table) Parse(string table)
    {
        var match = SchemaTableNameExtractor.Match(table.Trim());

        if (!match.Success)
        {
            throw new InvalidOperationException("The table name could not be parsed.");
        }

        var part1 = match.Groups["part1"].Value;
        var part2 = match.Groups["part2"].Value;

        return string.IsNullOrEmpty(part2) ? (null, part1) : (part1, part2);
    }

    /// <summary>
    /// Wraps a string literal in single quotes.
    /// </summary>
    private static string EscapeLiteral(string? s) => $"'{s}'";

    #endregion
}