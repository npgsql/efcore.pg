using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal
{
    /// <summary>
    /// The default database model factory for Npgsql.
    /// </summary>
    public class NpgsqlDatabaseModelFactory : DatabaseModelFactory
    {
        #region Fields

        /// <summary>
        /// The regular expression formatting string for schema and/or table names.
        /// </summary>
        [NotNull] const string NamePartRegex = @"(?:(?:""(?<part{0}>(?:(?:"""")|[^""])+)"")|(?<part{0}>[^\.\[""]+))";

        /// <summary>
        /// The <see cref="Regex"/> to extract the schema and/or table names.
        /// </summary>
        [NotNull] static readonly Regex SchemaTableNameExtractor =
            new Regex(
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"^{0}(?:\.{1})?$",
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)),
                RegexOptions.Compiled,
                TimeSpan.FromMilliseconds(1000.0));

        /// <summary>
        /// Tables and views which are considered to be system tables and should not get scaffolded, e.g. the support table
        /// created by the PostGIS extension.
        /// </summary>
        [NotNull] [ItemNotNull] static readonly string[] SystemTablesAndViews =
        {
            "spatial_ref_sys", "geography_columns", "geometry_columns", "raster_columns", "raster_overviews"
        };

        /// <summary>
        /// The types used for serial columns.
        /// </summary>
        [NotNull] [ItemNotNull] static readonly string[] SerialTypes = { "int2", "int4", "int8" };

        /// <summary>
        /// The diagnostic logger instance.
        /// </summary>
        [NotNull] readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;

        #endregion

        #region Public surface

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlDatabaseModelFactory"/> class.
        /// </summary>
        /// <param name="logger">The diagnostic logger instance.</param>
        public NpgsqlDatabaseModelFactory([NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
            => _logger = Check.NotNull(logger, nameof(logger));

        /// <inheritdoc />
        public override DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(options, nameof(options));

            using (var connection = new NpgsqlConnection(connectionString))
            {
                return Create(connection, options);
            }
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
                connection.Open();

            try
            {
                databaseModel.DatabaseName = connection.Database;
                databaseModel.DefaultSchema = "public";

                var schemaList = options.Schemas.ToList();
                var schemaFilter = GenerateSchemaFilter(schemaList);
                var tableList = options.Tables.ToList();
                var tableFilter = GenerateTableFilter(tableList.Select(ParseSchemaTable).ToList(), schemaFilter);

                var enums = GetEnums(connection, databaseModel);

                foreach (var table in GetTables(connection, tableFilter, enums, _logger))
                {
                    table.Database = databaseModel;
                    databaseModel.Tables.Add(table);
                }

                foreach (var table in databaseModel.Tables)
                {
                    while (table.Columns.Remove(null)) {}
                }

                foreach (var sequence in GetSequences(connection, schemaFilter, _logger))
                {
                    sequence.Database = databaseModel;
                    databaseModel.Sequences.Add(sequence);
                }

                GetExtensions(connection, databaseModel);

                for (var i = 0; i < databaseModel.Tables.Count; i++)
                {
                    var table = databaseModel.Tables[i];

                    // Remove some tables which shouldn't get scaffolded, unless they're explicitly mentioned
                    // in the table list
                    if (SystemTablesAndViews.Contains(table.Name) && !tableList.Contains(table.Name))
                    {
                        databaseModel.Tables.RemoveAt(i--);
                        continue;
                    }

                    // We may have dropped or skipped columns. We load these because constraints take them into
                    // account when referencing columns, but must now get rid of them before returning
                    // the database model.
                    while (table.Columns.Remove(null)) {}
                }

                foreach (var schema in schemaList
                    .Except(databaseModel.Sequences.Select(s => s.Schema).Concat(databaseModel.Tables.Select(t => t.Schema))))
                {
                    _logger.MissingSchemaWarning(schema);
                }

                foreach (var table in tableList)
                {
                    var (schema, name) = ParseSchemaTable(table);
                    if (!databaseModel.Tables.Any(t => !string.IsNullOrEmpty(schema) && t.Schema == schema || t.Name == name))
                        _logger.MissingTableWarning(table);
                }

                return databaseModel;
            }
            finally
            {
                if (!connectionStartedOpen)
                    connection.Close();
            }
        }

        #endregion

        #region Type information queries

        /// <summary>
        /// Queries the database for defined tables and registers them with the model.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="tableFilter">The table filter fragment.</param>
        /// <param name="enums">The collection of discovered enums.</param>
        /// <param name="logger">The diagnostic logger.</param>
        /// <returns>
        /// A collection of tables defined in the database.
        /// </returns>
        [NotNull]
        static IEnumerable<DatabaseTable> GetTables(
            [NotNull] NpgsqlConnection connection,
            [CanBeNull] Func<string, string, string> tableFilter,
            [NotNull] [ItemNotNull] HashSet<string> enums,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            var filter = tableFilter != null ? $"AND {tableFilter("ns.nspname", "cls.relname")}" : null;
            var commandText = $@"
SELECT nspname, relname, description
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
LEFT OUTER JOIN pg_description AS des ON des.objoid = cls.oid AND des.objsubid=0
WHERE
  cls.relkind IN ('r', 'v', 'm') AND
  ns.nspname NOT IN ('pg_catalog', 'information_schema') AND
  cls.relname <> '{HistoryRepository.DefaultTableName}'
  {filter}";

            var tables = new List<DatabaseTable>();

            using (var command = new NpgsqlCommand(commandText, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var table = new DatabaseTable
                    {
                        Schema = reader.GetValueOrDefault<string>("nspname"),
                        Name = reader.GetValueOrDefault<string>("relname")
                    };

                    if (reader.GetValueOrDefault<string>("description") is string comment)
                        table[NpgsqlAnnotationNames.Comment] = comment;

                    tables.Add(table);
                }
            }

            GetColumns(connection, tables, filter, enums, logger);
            GetConstraints(connection, tables, filter, out var constraintIndexes, logger);
            GetIndexes(connection, tables, filter, constraintIndexes, logger);
            return tables;
        }

        /// <summary>
        /// Queries the database for defined columns and registers them with the model.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="tables">The database tables.</param>
        /// <param name="tableFilter">The table filter fragment.</param>
        /// <param name="enums">The collection of discovered enums.</param>
        /// <param name="logger">The diagnostic logger.</param>
        static void GetColumns(
            [NotNull] NpgsqlConnection connection,
            [NotNull] IReadOnlyList<DatabaseTable> tables,
            [CanBeNull] string tableFilter,
            [NotNull] HashSet<string> enums,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            var commandText = $@"
SELECT
  nspname,
  cls.relname,
  typ.typname,
  basetyp.typname AS basetypname,
  attname,
  description,
  attisdropped,
  {(connection.PostgreSqlVersion >= new Version(10, 0) ? "attidentity" : "''::\"char\" as attidentity")},
  {(connection.PostgreSqlVersion >= new Version(12, 0) ? "attgenerated" : "''::\"char\" as attgenerated")},
  format_type(typ.oid, atttypmod) AS formatted_typname,
  format_type(basetyp.oid, typ.typtypmod) AS formatted_basetypname,
  CASE
    WHEN pg_proc.proname = 'array_recv' THEN 'a'
    ELSE typ.typtype
  END AS typtype,
  CASE
    WHEN pg_proc.proname='array_recv' THEN elemtyp.typname
    ELSE NULL
  END AS elemtypname,
  (NOT attnotnull) AS nullable,
  CASE
    WHEN atthasdef THEN (SELECT pg_get_expr(adbin, cls.oid) FROM pg_attrdef WHERE adrelid = cls.oid AND adnum = attr.attnum)
    ELSE NULL
  END AS default,

  -- Sequence options for identity columns
  format_type(seqtypid, 0) AS seqtype, seqstart, seqmin, seqmax, seqincrement, seqcycle, seqcache

FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
LEFT JOIN pg_attribute AS attr ON attrelid = cls.oid
LEFT JOIN pg_type AS typ ON attr.atttypid = typ.oid
LEFT JOIN pg_proc ON pg_proc.oid = typ.typreceive
LEFT JOIN pg_type AS elemtyp ON (elemtyp.oid = typ.typelem)
LEFT JOIN pg_type AS basetyp ON (basetyp.oid = typ.typbasetype)
LEFT JOIN pg_description AS des ON des.objoid = cls.oid AND des.objsubid = attnum
-- Bring in identity sequences the depend on this column
LEFT JOIN pg_depend AS dep ON dep.refobjid = cls.oid AND dep.refobjsubid = attr.attnum AND dep.deptype = 'i'
LEFT JOIN pg_sequence AS seq ON seq.seqrelid = dep.objid
WHERE
  cls.relkind IN ('r', 'v', 'm') AND
  nspname NOT IN ('pg_catalog', 'information_schema') AND
  attnum > 0 AND
  cls.relname <> '{HistoryRepository.DefaultTableName}'
  {tableFilter}
ORDER BY attnum";

            using (var command = new NpgsqlCommand(commandText, connection))
            using (var reader = command.ExecuteReader())
            {
                var tableGroups = reader.Cast<DbDataRecord>().GroupBy(ddr => (
                    tableSchema: ddr.GetValueOrDefault<string>("nspname"),
                    tableName: ddr.GetValueOrDefault<string>("relname")));

                foreach (var tableGroup in tableGroups)
                {
                    var tableSchema = tableGroup.Key.tableSchema;
                    var tableName = tableGroup.Key.tableName;

                    var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                    foreach (var record in tableGroup)
                    {
                        var column = new DatabaseColumn
                        {
                            Table = table,
                            Name = record.GetValueOrDefault<string>("attname"),
                            IsNullable = record.GetValueOrDefault<bool>("nullable"),
                        };

                        // We need to know about dropped columns because constraints take them into
                        // account when referencing columns. We'll get rid of them before returning the model.
                        if (record.GetValueOrDefault<bool>("attisdropped"))
                        {
                            table.Columns.Add(null);
                            continue;
                        }

                        string systemTypeName;
                        var formattedTypeName = AdjustFormattedTypeName(record.GetValueOrDefault<string>("formatted_typname"));
                        var formattedBaseTypeName = record.GetValueOrDefault<string>("formatted_basetypname");
                        if (formattedBaseTypeName == null)
                        {
                            column.StoreType = formattedTypeName;
                            systemTypeName = record.GetValueOrDefault<string>("typname");
                        }
                        else
                        {
                            // This is a domain type
                            column.StoreType = formattedBaseTypeName;
                            systemTypeName = record.GetValueOrDefault<string>("basetypname");
                        }

                        // Enum types cannot be scaffolded for now (nor can domains of enum types),
                        // skip with an informative message
                        if (enums.Contains(formattedTypeName) || enums.Contains(formattedBaseTypeName))
                        {
                            logger.EnumColumnSkippedWarning($"{DisplayName(tableSchema, tableName)}.{column.Name}");
                            // We need to know about skipped columns because constraints take them into
                            // account when referencing columns. We'll get rid of them before returning the model.
                            table.Columns.Add(null);
                            continue;
                        }

                        logger.ColumnFound(
                            DisplayName(tableSchema, tableName),
                            column.Name,
                            formattedTypeName,
                            column.IsNullable,
                            column.DefaultValueSql);

                        // Default values and PostgreSQL 12 generated columns
                        if (record.GetValueOrDefault<char>("attgenerated") == 's')
                            column.ComputedColumnSql = record.GetValueOrDefault<string>("default");
                        else
                        {
                            column.DefaultValueSql = record.GetValueOrDefault<string>("default");
                            AdjustDefaults(column, systemTypeName);
                        }

                        // Identify IDENTITY columns, as well as SERIAL ones.
                        var isIdentity = false;
                        switch (record.GetValueOrDefault<char>("attidentity"))
                        {
                        case 'a':
                            column[NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityAlwaysColumn;
                            isIdentity = true;
                            break;
                        case 'd':
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

                        if (column[NpgsqlAnnotationNames.ValueGenerationStrategy] != null)
                            column.ValueGenerated = ValueGenerated.OnAdd;

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
                                column[NpgsqlAnnotationNames.IdentityOptions] = sequenceData.Serialize();
                        }

                        if (record.GetValueOrDefault<string>("description") is string comment)
                            column[NpgsqlAnnotationNames.Comment] = comment;

                        table.Columns.Add(column);
                    }
                }
            }
        }

        /// <summary>
        /// Queries the database for defined indexes and registers them with the model.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="tables">The database tables.</param>
        /// <param name="tableFilter">The table filter fragment.</param>
        /// <param name="constraintIndexes">The constraint indexes.</param>
        /// <param name="logger">The diagnostic logger.</param>
        static void GetIndexes(
            [NotNull] NpgsqlConnection connection,
            [NotNull] IReadOnlyList<DatabaseTable> tables,
            [CanBeNull] string tableFilter,
            [NotNull] List<uint> constraintIndexes,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            // Load the pg_opclass table (https://www.postgresql.org/docs/current/catalog-pg-opclass.html),
            // which is referenced by the indices we'll load below
            var opClasses = new Dictionary<uint, (string Name, bool IsDefault)>();
            using (var command = new NpgsqlCommand("SELECT oid, opcname, opcdefault FROM pg_opclass", connection))
            using (var reader = command.ExecuteReader())
                foreach (var opClass in reader.Cast<DbDataRecord>())
                    opClasses[opClass.GetValueOrDefault<uint>("oid")] = (opClass.GetValueOrDefault<string>("opcname"), opClass.GetValueOrDefault<bool>("opcdefault"));

            var collations = new Dictionary<uint, string>();
            using (var command = new NpgsqlCommand("SELECT oid, collname FROM pg_collation", connection))
            using (var reader = command.ExecuteReader())
                foreach (var collation in reader.Cast<DbDataRecord>())
                    collations[collation.GetValueOrDefault<uint>("oid")] = collation.GetValueOrDefault<string>("collname");

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
  indcollation,
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
  nspname NOT IN ('pg_catalog', 'information_schema') AND
  NOT indisprimary AND
  cls.relname <> '{HistoryRepository.DefaultTableName}'
  {tableFilter}";

            using (var command = new NpgsqlCommand(commandText, connection))
            using (var reader = command.ExecuteReader())
            {
                var tableGroups = reader.Cast<DbDataRecord>().GroupBy(ddr => (
                    tableSchema: ddr.GetValueOrDefault<string>("nspname"),
                    tableName: ddr.GetValueOrDefault<string>("cls_relname")));

                foreach (var tableGroup in tableGroups)
                {
                    var tableSchema = tableGroup.Key.tableSchema;
                    var tableName = tableGroup.Key.tableName;

                    var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                    foreach (var record in tableGroup)
                    {
                        // Constraints are detected separately (see GetConstraints), and we don't want their
                        // supporting indexes to appear independently.
                        if (constraintIndexes.Contains(record.GetValueOrDefault<uint>("idx_oid")))
                            continue;

                        var index = new DatabaseIndex
                        {
                            Table = table,
                            Name = record.GetValueOrDefault<string>("idx_relname"),
                            IsUnique = record.GetValueOrDefault<bool>("indisunique")
                        };

                        var numKeyColumns = record.GetValueOrDefault<short>("indnkeyatts");
                        var columnIndices = record.GetValueOrDefault<short[]>("indkey");
                        var tableColumns = (List<DatabaseColumn>)table.Columns;

                        if (columnIndices.Any(i => i == 0))
                        {
                            // Expression index, not supported
                            logger.ExpressionIndexSkippedWarning(index.Name, DisplayName(tableSchema, tableName));
                            continue;

                            /*
                            var expressions = record.GetValueOrDefault<string>("exprs");
                            if (expressions == null)
                                throw new Exception($"Seen 0 in indkey for index {index.Name} but indexprs is null");
                            index[NpgsqlAnnotationNames.IndexExpression] = expressions;
                            */
                        }

                        // Key columns come before non-key (included) columns, process them first
                        foreach (var i in columnIndices.Take(numKeyColumns))
                        {
                            if (tableColumns[i - 1] is DatabaseColumn indexKeyColumn)
                                index.Columns.Add(indexKeyColumn);
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
                                if (tableColumns[i - 1] is DatabaseColumn indexKeyColumn)
                                    nonKeyColumns.Add(indexKeyColumn.Name);
                                else
                                {
                                    logger.UnsupportedColumnIndexSkippedWarning(index.Name, DisplayName(tableSchema, tableName));
                                    goto IndexEnd;
                                }
                            }

                            index[NpgsqlAnnotationNames.IndexInclude] = nonKeyColumns.ToArray();
                        }

                        if (record.GetValueOrDefault<string>("pred") is string predicate)
                            index.Filter = predicate;

                        // It's cleaner to always output the index method on the database model,
                        // even when it's btree (the default);
                        // NpgsqlAnnotationCodeGenerator can then omit it as by-convention.
                        // However, because of https://github.com/aspnet/EntityFrameworkCore/issues/11846 we omit
                        // the annotation from the model entirely.
                        if (record.GetValueOrDefault<string>("amname") is string indexMethod && indexMethod != "btree")
                            index[NpgsqlAnnotationNames.IndexMethod] = indexMethod;

                        // Handle index operator classes, which we pre-loaded
                        var opClassNames = record
                            .GetValueOrDefault<uint[]>("indclass")
                            .Select(oid => opClasses.TryGetValue(oid, out var opc) && !opc.IsDefault ? opc.Name : null)
                            .ToArray();

                        if (opClassNames.Any(op => op != null))
                            index[NpgsqlAnnotationNames.IndexOperators] = opClassNames;

                        var columnCollations = record
                            .GetValueOrDefault<uint[]>("indcollation")
                            .Select(oid => collations.TryGetValue(oid, out var collation) && !string.Equals(collation, "default") ? collation : null)
                            .ToArray();

                        if (columnCollations.Any(coll => coll != null))
                            index[NpgsqlAnnotationNames.IndexCollation] = columnCollations;

                        if (record.GetValueOrDefault<bool>("amcanorder"))
                        {
                            var options = record.GetValueOrDefault<ushort[]>("indoption");

                            // The first bit specifies whether values are sorted in descending order.
                            const ushort indoptionDescFlag = 0x0001;

                            var sortOrders = options
                                .Select(val => (val & indoptionDescFlag) != 0 ? SortOrder.Descending : SortOrder.Ascending)
                                .ToArray();

                            if (!SortOrderHelper.IsDefaultSortOrder(sortOrders))
                                index[NpgsqlAnnotationNames.IndexSortOrder] = sortOrders;

                            // The second bit specifies whether NULLs are sorted first instead of last.
                            const ushort indoptionNullsFirstFlag = 0x0002;

                            var nullSortOrders = options
                                .Select(val => (val & indoptionNullsFirstFlag) != 0 ? NullSortOrder.NullsFirst : NullSortOrder.NullsLast)
                                .ToArray();

                            if (!SortOrderHelper.IsDefaultNullSortOrder(nullSortOrders, sortOrders))
                                index[NpgsqlAnnotationNames.IndexNullSortOrder] = nullSortOrders;
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
        /// <param name="connection">The database connection.</param>
        /// <param name="tables">The database tables.</param>
        /// <param name="tableFilter">The table filter fragment.</param>
        /// <param name="constraintIndexes">The constraint indexes.</param>
        /// <param name="logger">The diagnostic logger.</param>
        /// <exception cref="InvalidOperationException">Found varying lengths for column and principal column indices.</exception>
        static void GetConstraints(
            [NotNull] NpgsqlConnection connection,
            [NotNull] IReadOnlyList<DatabaseTable> tables,
            [CanBeNull] string tableFilter,
            [NotNull] out List<uint> constraintIndexes,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            var commandText = $@"
SELECT
  ns.nspname,
  cls.relname,
  conname,
  contype,
  conkey,
  conindid,
  frnns.nspname AS fr_nspname,
  frncls.relname AS fr_relname,
  confkey,
  confdeltype
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
JOIN pg_constraint as con ON con.conrelid = cls.oid
LEFT OUTER JOIN pg_class AS frncls ON frncls.oid = con.confrelid
LEFT OUTER JOIN pg_namespace as frnns ON frnns.oid = frncls.relnamespace
WHERE
  cls.relkind = 'r' AND
  ns.nspname NOT IN ('pg_catalog', 'information_schema') AND
  con.contype IN ('p', 'f', 'u') AND
  cls.relname <> '{HistoryRepository.DefaultTableName}'
  {tableFilter}";

            using (var command = new NpgsqlCommand(commandText, connection))
            using (var reader = command.ExecuteReader())
            {
                constraintIndexes = new List<uint>();
                var tableGroups = reader.Cast<DbDataRecord>().GroupBy(ddr => (
                    tableSchema: ddr.GetValueOrDefault<string>("nspname"),
                    tableName: ddr.GetValueOrDefault<string>("relname")));

                foreach (var tableGroup in tableGroups)
                {
                    var tableSchema = tableGroup.Key.tableSchema;
                    var tableName = tableGroup.Key.tableName;

                    var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                    // Primary keys
                    foreach (var primaryKeyRecord in tableGroup.Where(ddr => ddr.GetValueOrDefault<char>("contype") == 'p'))
                    {
                        var primaryKey = new DatabasePrimaryKey
                        {
                            Table = table,
                            Name = primaryKeyRecord.GetValueOrDefault<string>("conname")
                        };

                        foreach (var pkColumnIndex in primaryKeyRecord.GetValueOrDefault<short[]>("conkey"))
                        {
                            if (table.Columns[pkColumnIndex - 1] is DatabaseColumn pkColumn)
                                primaryKey.Columns.Add(pkColumn);
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
                    foreach (var foreignKeyRecord in tableGroup.Where(ddr => ddr.GetValueOrDefault<char>("contype") == 'f'))
                    {
                        var fkName = foreignKeyRecord.GetValueOrDefault<string>("conname");
                        var principalTableSchema = foreignKeyRecord.GetValueOrDefault<string>("fr_nspname");
                        var principalTableName = foreignKeyRecord.GetValueOrDefault<string>("fr_relname");
                        var onDeleteAction = foreignKeyRecord.GetValueOrDefault<char>("confdeltype");

                        var principalTable =
                            tables.FirstOrDefault(t =>
                                t.Schema == principalTableSchema &&
                                t.Name == principalTableName)
                            ?? tables.FirstOrDefault(t =>
                                t.Schema.Equals(principalTableSchema, StringComparison.OrdinalIgnoreCase) &&
                                t.Name.Equals(principalTableName, StringComparison.OrdinalIgnoreCase));

                        if (principalTable == null)
                        {
                            logger.ForeignKeyReferencesMissingPrincipalTableWarning(
                                fkName,
                                DisplayName(table.Schema, table.Name),
                                DisplayName(principalTableSchema, principalTableName));

                            continue;
                        }

                        var foreignKey = new DatabaseForeignKey
                        {
                            Name = fkName,
                            Table = table,
                            PrincipalTable = principalTable,
                            OnDelete = ConvertToReferentialAction(onDeleteAction)
                        };

                        var columnIndices = foreignKeyRecord.GetValueOrDefault<short[]>("conkey");
                        var principalColumnIndices = foreignKeyRecord.GetValueOrDefault<short[]>("confkey");

                        if (columnIndices.Length != principalColumnIndices.Length)
                            throw new InvalidOperationException("Found varying lengths for column and principal column indices.");

                        var principalColumns = (List<DatabaseColumn>)principalTable.Columns;

                        for (var i = 0; i < columnIndices.Length; i++)
                        {
                            var foreignKeyColumn = table.Columns[columnIndices[i] - 1];
                            var foreignKeyPrincipalColumn = principalColumns[principalColumnIndices[i] - 1];
                            if (foreignKeyColumn == null || foreignKeyPrincipalColumn == null)
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
                    foreach (var record in tableGroup.Where(ddr => ddr.GetValueOrDefault<char>("contype") == 'u'))
                    {
                        var name = record.GetValueOrDefault<string>("conname");

                        logger.UniqueConstraintFound(name, DisplayName(tableSchema, tableName));

                        var uniqueConstraint = new DatabaseUniqueConstraint
                        {
                            Table = table,
                            Name = name
                        };

                        foreach (var columnIndex in record.GetValueOrDefault<short[]>("conkey"))
                        {
                            var constraintColumn = table.Columns[columnIndex - 1];
                            if (constraintColumn == null)
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
        }

        /// <summary>
        /// Queries the database for defined sequences and registers them with the model.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="schemaFilter">The schema filter.</param>
        /// <param name="logger">The diagnostic logger.</param>
        /// <returns>
        /// A collection of sequences defined in teh database.
        /// </returns>
        [NotNull]
        static IEnumerable<DatabaseSequence> GetSequences(
            [NotNull] NpgsqlConnection connection,
            [CanBeNull] Func<string, string> schemaFilter,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            var commandText = $@"
SELECT
    nspname, relname,
    format_type(typ.oid, 0) AS seqtype,
    seqstart, seqmin, seqmax, seqincrement, seqcycle, seqcache
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
JOIN pg_sequence AS seq ON seq.seqrelid = cls.oid
JOIN pg_type AS typ ON typ.oid = seq.seqtypid
WHERE
  cls.relkind = 'S'
  /* AND seqtype IN ('integer', 'bigint', 'smallint') */
  /* Filter out owned serial and identity sequences */
  AND NOT EXISTS (SELECT * FROM pg_depend AS dep WHERE dep.objid = cls.oid AND dep.deptype IN ('i', 'I', 'a'))
  {(schemaFilter != null ? $"AND {schemaFilter("nspname")}" : null)}";

            using (var command = new NpgsqlCommand(commandText, connection))
            using (var reader = command.ExecuteReader())
            {
                foreach (var record in reader.Cast<DbDataRecord>())
                {
                    var seqInfo = ReadSequenceInfo(record, connection.PostgreSqlVersion);
                    var sequence = new DatabaseSequence
                    {
                        Schema = reader.GetValueOrDefault<string>("nspname"),
                        Name = reader.GetValueOrDefault<string>("relname"),
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
        }

        /// <summary>
        /// Queries the database for defined enums and registers them with the model.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="databaseModel">The database model.</param>
        [NotNull]
        static HashSet<string> GetEnums([NotNull] NpgsqlConnection connection, [NotNull] DatabaseModel databaseModel)
        {
            const string commandText = @"
SELECT
  nspname,
  typname,
  array_agg(enumlabel ORDER BY enumsortorder) AS labels
FROM pg_enum
JOIN pg_type ON pg_type.oid = enumtypid
JOIN pg_namespace ON pg_namespace.oid = pg_type.typnamespace
GROUP BY nspname, typname";

            using (var command = new NpgsqlCommand(commandText, connection))
            using (var reader = command.ExecuteReader())
            {
                // TODO: just return a collection and make this a static utility method.
                var enums = new HashSet<string>();
                while (reader.Read())
                {
                    var schema = reader.GetValueOrDefault<string>("nspname");
                    var name = reader.GetValueOrDefault<string>("typname");
                    var labels = reader.GetValueOrDefault<string[]>("labels");

                    if (schema == "public")
                        schema = null;

                    PostgresEnum.GetOrAddPostgresEnum(databaseModel, schema, name, labels);
                    enums.Add(name);
                }

                return enums;
            }
        }

        /// <summary>
        /// Queries the installed database extensions and registers them with the model.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="databaseModel">The database model.</param>
        static void GetExtensions([NotNull] NpgsqlConnection connection, [NotNull] DatabaseModel databaseModel)
        {
            const string commandText = "SELECT name, default_version, installed_version FROM pg_available_extensions";
            using (var command = new NpgsqlCommand(commandText, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader.GetString(reader.GetOrdinal("name"));
                    var _ = reader.GetValueOrDefault<string>("default_version");
                    var installedVersion = reader.GetValueOrDefault<string>("installed_version");

                    if (installedVersion == null)
                        continue;

                    if (name == "plpgsql") // Implicitly installed in all PG databases
                        continue;

                    // TODO: how/should we query the schema?
                    databaseModel.GetOrAddPostgresExtension(null, name, installedVersion);
                }
            }
        }

        #endregion

        #region Configure default values

        /// <summary>
        /// Configures the default value for a column.
        /// </summary>
        /// <param name="column">The column to configure.</param>
        /// <param name="systemTypeName">The type name of the column.</param>
        static void AdjustDefaults([NotNull] DatabaseColumn column, [NotNull] string systemTypeName)
        {
            var defaultValue = column.DefaultValueSql;
            if (defaultValue == null || defaultValue == "(NULL)")
            {
                column.DefaultValueSql = null;
                return;
            }

            if (column.IsNullable)
                return;

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

        static SequenceInfo ReadSequenceInfo(DbDataRecord record, Version postgresVersion)
        {
            var storeType = record.GetValueOrDefault<string>("seqtype");
            var startValue = record.GetValueOrDefault<long>("seqstart");
            var minValue = record.GetValueOrDefault<long>("seqmin");
            var maxValue = record.GetValueOrDefault<long>("seqmax");
            var incrementBy = (int)record.GetValueOrDefault<long>("seqincrement");
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

            return new SequenceInfo
            {
                StoreType = storeType,
                StartValue = startValue == defaultStart ? null : (long?)startValue,
                MinValue = minValue == defaultMin ? null : (long?)minValue,
                MaxValue = maxValue == defaultMax ? null : (long?)maxValue,
                IncrementBy = incrementBy == 1 ? null : (long?)incrementBy,
                IsCyclic = isCyclic == false ? null : (bool?)true,
                NumbersToCache = numbersToCache == 1 ? null : (long?)numbersToCache
            };
        }

        class SequenceInfo
        {
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
        /// <param name="schemas">The list of schema names.</param>
        /// <returns>
        /// A delegate that generates a schema filter fragment.
        /// </returns>
        [CanBeNull]
        static Func<string, string> GenerateSchemaFilter([NotNull] IReadOnlyList<string> schemas)
            => schemas.Any()
                ? s => $"{s} IN ({string.Join(", ", schemas.Select(EscapeLiteral))})"
                : (Func<string, string>)null;

        /// <summary>
        /// Builds a delegate to generate a table filter fragment.
        /// </summary>
        /// <param name="tables">The list of tables parsed into tuples of schema name and table name.</param>
        /// <param name="schemaFilter">The delegate that generates a schema filter fragment.</param>
        /// <returns>
        /// A delegate that generates a table filter fragment.
        /// </returns>
        [CanBeNull]
        static Func<string, string, string> GenerateTableFilter(
            [NotNull] IReadOnlyList<(string Schema, string Table)> tables,
            [CanBeNull] Func<string, string> schemaFilter)
            => schemaFilter != null || tables.Any()
                ? (s, t) =>
                {
                    var tableFilterBuilder = new StringBuilder();

                    var openBracket = false;
                    if (schemaFilter != null)
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
                                tableFilterBuilder.Append(" OR ");

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
                        tableFilterBuilder.Append(")");

                    return tableFilterBuilder.ToString();
                }
                : (Func<string, string, string>)null;

        #endregion

        #region Utilities

        /// <summary>
        /// Type names as returned by PostgreSQL's format_type need to be cleaned up a bit
        /// </summary>
        /// <param name="formattedTypeName">The type name to adjust.</param>
        /// <returns>
        /// The adjusted type name or the original name if no adjustments were required.
        /// </returns>
        [NotNull]
        static string AdjustFormattedTypeName([NotNull] string formattedTypeName)
        {
            // User-defined types (e.g. enums) with capital letters get formatted with quotes, remove.
            if (formattedTypeName[0] == '"')
                formattedTypeName = formattedTypeName.Substring(1, formattedTypeName.Length - 2);

            if (formattedTypeName == "bpchar")
                formattedTypeName = "char";

            return formattedTypeName;
        }

        /// <summary>
        /// Maps a character to a <see cref="ReferentialAction"/>.
        /// </summary>
        /// <param name="onDeleteAction">The character to map.</param>
        /// <returns>
        /// A <see cref="ReferentialAction"/> associated with the <paramref name="onDeleteAction"/> character.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Unknown value <paramref name="onDeleteAction"/> for foreign key deletion action code.
        /// </exception>
        static ReferentialAction ConvertToReferentialAction(char onDeleteAction)
        {
            switch (onDeleteAction)
            {
            case 'a':
                return ReferentialAction.NoAction;
            case 'r':
                return ReferentialAction.Restrict;
            case 'c':
                return ReferentialAction.Cascade;
            case 'n':
                return ReferentialAction.SetNull;
            case 'd':
                return ReferentialAction.SetDefault;
            default:
                throw new ArgumentOutOfRangeException($"Unknown value {onDeleteAction} for foreign key deletion action code.");
            }
        }

        /// <summary>
        /// Constructs the display name given a schema and table name.
        /// </summary>
        /// <param name="schema">The schema name.</param>
        /// <param name="name">The table name.</param>
        /// <returns>
        /// A display name in the form of 'schema.name' or 'name'.
        /// </returns>
        // TODO: should this default to/screen out the public schema?
        [NotNull]
        static string DisplayName([CanBeNull] string schema, [NotNull] string name)
            => string.IsNullOrEmpty(schema) ? name : $"{schema}.{name}";

        /// <summary>
        /// Parses the table name into a tuple of schema name and table name where the schema may be null.
        /// </summary>
        /// <param name="table">The name to parse.</param>
        /// <returns>
        /// A tuple of schema name and table name where the schema may be null.
        /// </returns>
        /// <exception cref="InvalidOperationException">The table name could not be parsed.</exception>
        static (string Schema, string Table) ParseSchemaTable([NotNull] string table)
        {
            var match = SchemaTableNameExtractor.Match(table.Trim());

            if (!match.Success)
                throw new InvalidOperationException("The table name could not be parsed.");

            var part1 = match.Groups["part1"].Value;
            var part2 = match.Groups["part2"].Value;

            return string.IsNullOrEmpty(part2) ? (null, part1) : (part1, part2);
        }

        /// <summary>
        /// Wraps a string literal in single quotes.
        /// </summary>
        /// <param name="s">The string literal.</param>
        /// <returns>
        /// The string literal wrapped in single quotes.
        /// </returns>
        [NotNull]
        static string EscapeLiteral([CanBeNull] string s) => $"'{s}'";

        #endregion
    }
}
