#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

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
    public class NpgsqlDatabaseModelFactory : IDatabaseModelFactory
    {
        const string NamePartRegex
            = @"(?:(?:""(?<part{0}>(?:(?:"""")|[^""])+)"")|(?<part{0}>[^\.\[""]+))";

        static readonly Regex _partExtractor
            = new Regex(
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"^{0}(?:\.{1})?$",
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)),
                RegexOptions.Compiled,
                TimeSpan.FromMilliseconds(1000.0));

        readonly HashSet<string> _enums = new HashSet<string>();

        readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;

        public NpgsqlDatabaseModelFactory([NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
        }

        public virtual DatabaseModel Create(string connectionString, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tables, nameof(tables));
            Check.NotNull(schemas, nameof(schemas));

            using (var connection = new NpgsqlConnection(connectionString))
            {
                return Create(connection, tables, schemas);
            }
        }

        public virtual DatabaseModel Create(DbConnection dbConnection, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            var connection = (NpgsqlConnection)dbConnection;
            var connectionStartedOpen = connection.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                connection.Open();
            }

            try
            {
                var databaseModel = new DatabaseModel
                {
                    DatabaseName = connection.Database,
                    DefaultSchema = "public"
                };

                var schemaList = schemas.ToList();
                var schemaFilter = GenerateSchemaFilter(schemaList);
                var tableList = tables.ToList();
                var tableFilter = GenerateTableFilter(tableList.Select(Parse).ToList(), schemaFilter);

                GetEnums(connection, databaseModel);

                foreach (var table in GetTables(connection, tableFilter))
                {
                    table.Database = databaseModel;
                    databaseModel.Tables.Add(table);
                }

                foreach (var table in databaseModel.Tables)
                {
                    while (table.Columns.Remove(null)) { }
                }

                foreach (var sequence in GetSequences(connection, databaseModel.Tables, schemaFilter))
                {
                    sequence.Database = databaseModel;
                    databaseModel.Sequences.Add(sequence);
                }

                GetExtensions(connection, databaseModel);

                // We may have dropped columns. We load these because constraints take them into
                // account when referencing columns, but must now get rid of them before returning
                // the database model.
                foreach (var table in databaseModel.Tables)
                    while (table.Columns.Remove(null)) {}

                foreach (var schema in schemaList
                    .Except(
                        databaseModel.Sequences.Select(s => s.Schema)
                            .Concat(databaseModel.Tables.Select(t => t.Schema))))
                {
                    _logger.MissingSchemaWarning(schema);
                }

                foreach (var table in tableList)
                {
                    var (Schema, Table) = Parse(table);
                    if (!databaseModel.Tables.Any(
                        t => !string.IsNullOrEmpty(Schema)
                             && t.Schema == Schema
                             || t.Name == Table))
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

        IEnumerable<DatabaseTable> GetTables(
            NpgsqlConnection connection,
            Func<string, string, string> tableFilter)
        {
            var commandText = @"
SELECT nspname, relname, description
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
LEFT OUTER JOIN pg_description AS des ON des.objoid = cls.oid AND des.objsubid=0
WHERE
    cls.relkind = 'r'
AND
    ns.nspname NOT IN ('pg_catalog', 'information_schema') ";

            var filter = $"AND cls.relname <> '{HistoryRepository.DefaultTableName}' {(tableFilter != null ? $"AND {tableFilter("ns.nspname", "cls.relname")}" : "")}";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText + filter;

                var tables = new List<DatabaseTable>();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var table = new DatabaseTable
                        {
                            Schema = reader.GetValueOrDefault<string>("nspname"),
                            Name = reader.GetValueOrDefault<string>("relname")
                        };

                        var comment = reader.GetValueOrDefault<string>("description");
                        if (comment != null)
                            table[NpgsqlAnnotationNames.Comment] = comment;
                        tables.Add(table);
                    }
                }

                GetColumns(connection, tables, filter);
                GetConstraints(connection, tables, filter, out var constraintIndexes);
                GetIndexes(connection, tables, filter, constraintIndexes);

                return tables;
            }
        }

        void GetColumns(
            NpgsqlConnection connection,
            IReadOnlyList<DatabaseTable> tables,
            string tableFilter)
        {
            using (var command = connection.CreateCommand())
            {
                var commandText = $@"
SELECT
    nspname, relname, typ.typname, attname, description, attisdropped,
    {(connection.PostgreSqlVersion >= new Version(10, 0) ? "attidentity" : "''::\"char\" as attidentity")},
    format_type(typ.oid, atttypmod) AS formatted_typname, basetyp.typname AS basetypname,
    CASE WHEN pg_proc.proname='array_recv' THEN 'a' ELSE typ.typtype END AS typtype,
    CASE
        WHEN pg_proc.proname='array_recv' THEN elemtyp.typname
        ELSE NULL
    END AS elemtypname,
    (NOT attnotnull) AS nullable,
    CASE WHEN atthasdef THEN (SELECT pg_get_expr(adbin, cls.oid) FROM pg_attrdef WHERE adrelid = cls.oid AND adnum = attr.attnum) ELSE NULL END AS default
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
LEFT OUTER JOIN pg_attribute AS attr ON attrelid = cls.oid
LEFT OUTER JOIN pg_type AS typ ON attr.atttypid = typ.oid
LEFT OUTER JOIN pg_proc ON pg_proc.oid = typ.typreceive
LEFT OUTER JOIN pg_type AS elemtyp ON (elemtyp.oid = typ.typelem)
LEFT OUTER JOIN pg_type AS basetyp ON (basetyp.oid = typ.typbasetype)
LEFT OUTER JOIN pg_description AS des ON des.objoid = cls.oid AND des.objsubid = attnum
WHERE
    relkind = 'r' AND
    nspname NOT IN ('pg_catalog', 'information_schema') AND
    attnum > 0 {tableFilter} ORDER BY attnum";

                command.CommandText = commandText;

                using (var reader = command.ExecuteReader())
                {
                    var tableGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                            ddr => (tableSchema: ddr.GetValueOrDefault<string>("nspname"),
                                tableName: ddr.GetValueOrDefault<string>("relname")));

                    foreach (var tableGroup in tableGroups)
                    {
                        var tableSchema = tableGroup.Key.tableSchema;
                        var tableName = tableGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        foreach (var record in tableGroup)
                        {
                            var columnName = record.GetValueOrDefault<string>("attname");
                            var typeName = record.GetValueOrDefault<string>("typname");
                            var formattedTypeName = record.GetValueOrDefault<string>("formatted_typname");
                            var nullable = record.GetValueOrDefault<bool>("nullable");
                            var defaultValue = record.GetValueOrDefault<string>("default");

                            // We need to know about dropped columns because constraints take them into
                            // account when referencing columns. We'll get rid of them before returning the model.
                            var isDropped = record.GetValueOrDefault<bool>("attisdropped");
                            if (isDropped)
                            {
                                table.Columns.Add(null);
                                continue;
                            }

                            // User-defined types (e.g. enums) with capital letters get formatted with quotes, remove.
                            if (formattedTypeName[0] == '"')
                                formattedTypeName = formattedTypeName.Substring(1, formattedTypeName.Length - 2);
                            if (_enums.Contains(formattedTypeName))
                            {
                                _logger.EnumColumnSkippedWarning(DisplayName(tableSchema, tableName) + '.' + columnName);
                                continue;
                            }

                            _logger.ColumnFound(
                                DisplayName(tableSchema, tableName),
                                columnName,
                                formattedTypeName,
                                nullable,
                                defaultValue);

                            var column = new DatabaseColumn
                            {
                                Table = table,
                                Name = columnName,
                                IsNullable = nullable,
                                DefaultValueSql = defaultValue,
                                ComputedColumnSql = null
                            };

                            if (formattedTypeName == "bpchar")
                                formattedTypeName = "char";

                            string systemTypeName;
                            var domainBaseTypeName = record.GetValueOrDefault<string>("basetypname");
                            if (domainBaseTypeName == null)
                            {
                                column.StoreType = formattedTypeName;
                                systemTypeName = typeName;
                            }
                            else
                            {
                                // This is a domain type
                                column.StoreType = typeName;
                                column.SetUnderlyingStoreType(domainBaseTypeName);
                                systemTypeName = domainBaseTypeName;
                            }

                            var identityChar = record.GetValueOrDefault<char>("attidentity");
                            if (identityChar == 'a')
                                column[NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityAlwaysColumn;
                            else if (identityChar == 'd')
                                column[NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn;
                            else if (SerialTypes.Contains(systemTypeName) &&
                                defaultValue == $"nextval('{column.Table.Name}_{column.Name}_seq'::regclass)" ||
                                defaultValue == $"nextval('\"{column.Table.Name}_{column.Name}_seq\"'::regclass)")
                            {
                                // Hacky but necessary...
                                // We identify serial columns by examining their default expression,
                                // and reverse-engineer these as ValueGenerated.OnAdd
                                // TODO: Think about composite keys? Do serial magic only for non-composite.
                                column.DefaultValueSql = null;
                                // Serial is the default value generation strategy, so NpgsqlAnnotationCodeGenerator
                                // makes sure it isn't actually rendered
                                column[NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn;
                            }

                            if (column[NpgsqlAnnotationNames.ValueGenerationStrategy] != null)
                                column.ValueGenerated = ValueGenerated.OnAdd;

                            AdjustDefaults(column, systemTypeName);

                            var comment = record.GetValueOrDefault<string>("description");
                            if (comment != null)
                                column[NpgsqlAnnotationNames.Comment] = comment;

                            table.Columns.Add(column);
                        }
                    }
                }
            }
        }

        static readonly string[] SerialTypes = { "int2", "int4", "int8" };

        static void AdjustDefaults(DatabaseColumn column, string systemTypeName)
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
                    systemTypeName == "int2"   ||
                    systemTypeName == "int4"   ||
                    systemTypeName == "int8"   ||
                    systemTypeName == "money"  ||
                    systemTypeName == "numeric")
                {
                    column.DefaultValueSql = null;
                    return;
                }
            }

            if (defaultValue == "0.0" || defaultValue == "'0'::numeric")
            {
                if (systemTypeName == "numeric" ||
                    systemTypeName == "float4"  ||
                    systemTypeName == "float8"  ||
                    systemTypeName == "money")
                {
                    column.DefaultValueSql = null;
                    return;
                }
            }

            if ((systemTypeName == "bool"      && defaultValue == "false") ||
                (systemTypeName == "date"      && defaultValue == "'0001-01-01'::date") ||
                (systemTypeName == "timestamp" && defaultValue == "'1900-01-01 00:00:00'::timestamp without time zone") ||
                (systemTypeName == "time"      && defaultValue == "'00:00:00'::time without time zone") ||
                (systemTypeName == "interval"  && defaultValue == "'00:00:00'::interval") ||
                (systemTypeName == "uuid"      && defaultValue == "'00000000-0000-0000-0000-000000000000'::uuid"))
            {
                column.DefaultValueSql = null;
                return;
            }
        }

        void GetIndexes(
            NpgsqlConnection connection,
            IReadOnlyList<DatabaseTable> tables,
            string tableFilter,
            List<uint> constraintIndexes)
        {
            using (var command = connection.CreateCommand())
            {
                var getColumnsQuery = @"
SELECT
    idxcls.oid AS idx_oid, nspname, cls.relname AS cls_relname, idxcls.relname AS idx_relname, indisunique, indkey, amname,
    CASE WHEN indexprs IS NULL THEN NULL ELSE pg_get_expr(indexprs, cls.oid) END AS exprs,
    CASE WHEN indpred IS NULL THEN NULL ELSE pg_get_expr(indpred, cls.oid) END AS pred
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
JOIN pg_index AS idx ON indrelid = cls.oid
JOIN pg_class AS idxcls ON idxcls.oid = indexrelid
JOIN pg_am AS am ON am.oid = idxcls.relam
WHERE
    cls.relkind = 'r' AND
    nspname NOT IN ('pg_catalog', 'information_schema')
AND
    NOT indisprimary
" + tableFilter;

                command.CommandText = getColumnsQuery;

                using (var reader = command.ExecuteReader())
                {
                    var tableGroups = reader.Cast<DbDataRecord>().GroupBy(ddr => (
                        tableSchema: ddr.GetValueOrDefault<string>("nspname"),
                        tableName: ddr.GetValueOrDefault<string>("cls_relname")
                    ));

                    foreach (var tableGroup in tableGroups)
                    {
                        var tableSchema = tableGroup.Key.tableSchema;
                        var tableName = tableGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        foreach (var record in tableGroup)
                        {
                            // Constraints are detected separately (see GetConstraints), and we don't want their
                            // supporting indexes to appear independently.
                            var indexOID = record.GetValueOrDefault<uint>("idx_oid");
                            if (constraintIndexes.Contains(indexOID))
                                continue;

                            var index = new DatabaseIndex
                            {
                                Table = table,
                                Name = record.GetValueOrDefault<string>("idx_relname"),
                                IsUnique = record.GetValueOrDefault<bool>("indisunique")
                            };

                            var columnIndices = record.GetValueOrDefault<short[]>("indkey");
                            if (columnIndices.Any(i => i == 0))
                            {
                                // Expression index, not supported
                                _logger.ExpressionIndexSkippedWarning(index.Name, DisplayName(tableSchema, tableName));
                                continue;

                                /*
                                var expressions = record.GetValueOrDefault<string>("exprs");
                                if (expressions == null)
                                    throw new Exception($"Seen 0 in indkey for index {index.Name} but indexprs is null");
                                index[NpgsqlAnnotationNames.IndexExpression] = expressions;
                                */
                            }
                            else
                            {
                                var columns = (List<DatabaseColumn>)table.Columns;
                                foreach (var i in columnIndices)
                                    index.Columns.Add(columns[i - 1]);
                            }

                            var predicate = record.GetValueOrDefault<string>("pred");
                            if (predicate != null)
                                index.Filter = predicate;

                            index[NpgsqlAnnotationNames.IndexMethod] = record.GetValueOrDefault<string>("amname");

                            table.Indexes.Add(index);
                        }
                    }
                }
            }
        }

        void GetConstraints(
            NpgsqlConnection connection,
            IReadOnlyList<DatabaseTable> tables,
            string tableFilter,
            out List<uint> constraintIndexes)
        {
            constraintIndexes = new List<uint>();

            var getConstraints = @"
SELECT
    ns.nspname, cls.relname, conname, contype, conkey, conindid,
    frnns.nspname AS fr_nspname, frncls.relname AS fr_relname, confkey, confdeltype
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
JOIN pg_constraint as con ON con.conrelid = cls.oid
LEFT OUTER JOIN pg_class AS frncls ON frncls.oid = con.confrelid
LEFT OUTER JOIN pg_namespace as frnns ON frnns.oid = frncls.relnamespace
WHERE
    cls.relkind = 'r' AND
    ns.nspname NOT IN ('pg_catalog', 'information_schema')
AND
    con.contype IN ('p', 'f', 'u')
" + tableFilter;

            var command = connection.CreateCommand();
            command.CommandText = getConstraints;

            using (var reader = command.ExecuteReader())
            {
                var tableGroups = reader.Cast<DbDataRecord>()
                    .GroupBy(
                        ddr => (tableSchema: ddr.GetValueOrDefault<string>("nspname"),
                            tableName: ddr.GetValueOrDefault<string>("relname")));

                foreach (var tableGroup in tableGroups)
                {
                    var tableSchema = tableGroup.Key.tableSchema;
                    var tableName = tableGroup.Key.tableName;

                    var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                    // Primary keys
                    foreach (var primaryKeyRecord in tableGroup
                        .Where(ddr => ddr.GetValueOrDefault<char>("contype") == 'p'))
                    {
                        var primaryKey = new DatabasePrimaryKey
                        {
                            Table = table,
                            Name = primaryKeyRecord.GetValueOrDefault<string>("conname")
                        };

                        var pkColumnIndices = primaryKeyRecord.GetValueOrDefault<short[]>("conkey");
                        foreach (var pkColumnIndex in pkColumnIndices)
                            primaryKey.Columns.Add(table.Columns[pkColumnIndex - 1]);
                        table.PrimaryKey = primaryKey;
                    }

                    // Foreign keys
                    foreach (var foreignKeyRecord in tableGroup
                        .Where(ddr => ddr.GetValueOrDefault<char>("contype") == 'f'))
                    {
                        var fkName = foreignKeyRecord.GetValueOrDefault<string>("conname");
                        var principalTableSchema = foreignKeyRecord.GetValueOrDefault<string>("fr_nspname");
                        var principalTableName = foreignKeyRecord.GetValueOrDefault<string>("fr_relname");
                        var onDeleteAction = foreignKeyRecord.GetValueOrDefault<char>("confdeltype");

                        var principalTable = tables.FirstOrDefault(
                                                 t => t.Schema == principalTableSchema
                                                      && t.Name == principalTableName)
                                             ?? tables.FirstOrDefault(
                                                 t => t.Schema.Equals(principalTableSchema, StringComparison.OrdinalIgnoreCase)
                                                      && t.Name.Equals(principalTableName, StringComparison.OrdinalIgnoreCase));

                        if (principalTable == null)
                        {
                            _logger.ForeignKeyReferencesMissingPrincipalTableWarning(
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
                            throw new Exception("Got varying lengths for column and principal column indices");

                        var principalColumns = (List<DatabaseColumn>)principalTable.Columns;

                        for (var i = 0; i < columnIndices.Length; i++)
                        {
                            foreignKey.Columns.Add(table.Columns[columnIndices[i] - 1]);
                            foreignKey.PrincipalColumns.Add(principalColumns[principalColumnIndices[i] - 1]);
                        }

                        table.ForeignKeys.Add(foreignKey);
                    }

                    // Unique constraints
                    foreach (var record in tableGroup
                        .Where(ddr => ddr.GetValueOrDefault<char>("contype") == 'u')
                        .ToArray())
                    {
                        var name = record.GetValueOrDefault<string>("conname");

                        _logger.UniqueConstraintFound(name, DisplayName(tableSchema, tableName));

                        var uniqueConstraint = new DatabaseUniqueConstraint
                        {
                            Table = table,
                            Name = name
                        };

                        var columnIndices = record.GetValueOrDefault<short[]>("conkey");
                        foreach (var t in columnIndices)
                            uniqueConstraint.Columns.Add(table.Columns[t-1]);

                        table.UniqueConstraints.Add(uniqueConstraint);
                        constraintIndexes.Add(record.GetValueOrDefault<uint>("conindid"));
                    }
                }
            }
        }

        IEnumerable<DatabaseSequence> GetSequences(
           NpgsqlConnection connection,
           IList<DatabaseTable> tables,
           Func<string, string> schemaFilter)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    sequence_schema, sequence_name, data_type, start_value::bigint, minimum_value::bigint, maximum_value::bigint, increment::int,
    CASE WHEN cycle_option = 'YES' THEN TRUE ELSE FALSE END AS is_cyclic,
    ownerns.nspname AS owner_schema,
    tblcls.relname AS owner_table,
    attname AS owner_column
FROM information_schema.sequences
JOIN pg_namespace AS seqns ON seqns.nspname = sequence_schema
JOIN pg_class AS seqcls ON seqcls.relnamespace = seqns.oid AND seqcls.relname = sequence_name AND seqcls.relkind = 'S'
LEFT OUTER JOIN pg_depend AS dep ON dep.objid = seqcls.oid AND deptype='a'
LEFT OUTER JOIN pg_class AS tblcls ON tblcls.oid = dep.refobjid
LEFT OUTER JOIN pg_attribute AS att ON attrelid = dep.refobjid AND attnum = dep.refobjsubid
LEFT OUTER JOIN pg_namespace AS ownerns ON ownerns.oid = tblcls.relnamespace";

                if (schemaFilter != null)
                    command.CommandText += "\nWHERE " + schemaFilter("sequence_schema");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // If the sequence is OWNED BY a column which is a serial, we skip it. The sequence will be created implicitly.
                        if (!reader.IsDBNull(10))
                        {
                            var ownerSchema = reader.GetValueOrDefault<string>("owner_schema");
                            var ownerTable = reader.GetValueOrDefault<string>("owner_table");
                            var ownerColumn = reader.GetValueOrDefault<string>("owner_column");

                            var ownerDatabaseTable = tables
                                .FirstOrDefault(t => t.Name == ownerTable && t.Schema == ownerSchema);

                            var ownerDatabaseColumn = ownerDatabaseTable
                                .Columns
                                .FirstOrDefault(t => t.Name == ownerColumn);

                            if (ownerDatabaseTable != null && ownerDatabaseColumn?.ValueGenerated == ValueGenerated.OnAdd)
                            {
                                // Don't reverse-engineer sequences which drive serial columns, these are implicitly
                                // reverse-engineered by the serial column.
                                continue;
                            }
                        }

                        var sequence = new DatabaseSequence
                        {
                            Schema = reader.GetValueOrDefault<string>("sequence_schema"),
                            Name = reader.GetValueOrDefault<string>("sequence_name"),
                            StoreType = reader.GetValueOrDefault<string>("data_type"),
                            StartValue = reader.GetValueOrDefault<long>("start_value"),
                            MinValue = reader.GetValueOrDefault<long>("minimum_value"),
                            MaxValue = reader.GetValueOrDefault<long>("maximum_value"),
                            IncrementBy = reader.GetValueOrDefault<int>("increment"),
                            IsCyclic = reader.GetValueOrDefault<bool>("is_cyclic")
                        };

                        SetSequenceStartMinMax(sequence, connection.PostgreSqlVersion);
                        yield return sequence;
                    }
                }
            }
        }

        void SetSequenceStartMinMax(DatabaseSequence sequence, Version postgresVersion)
        {
            long defaultStart, defaultMin, defaultMax;

            if (sequence.StoreType == "smallint")
            {
                if (sequence.IncrementBy > 0)
                {
                    defaultMin = 1;
                    defaultMax = short.MaxValue;
                    defaultStart = sequence.MinValue.Value;
                }
                else
                {
                    // PostgreSQL 10 changed the default minvalue for a descending sequence, see #264
                    defaultMin = postgresVersion >= new Version(10, 0)
                        ? short.MinValue
                        : short.MinValue + 1;
                    defaultMax = -1;
                    defaultStart = sequence.MaxValue.Value;
                }
            }
            else if (sequence.StoreType == "integer")
            {
                if (sequence.IncrementBy > 0)
                {
                    defaultMin = 1;
                    defaultMax = int.MaxValue;
                    defaultStart = sequence.MinValue.Value;
                }
                else
                {
                    // PostgreSQL 10 changed the default minvalue for a descending sequence, see #264
                    defaultMin = postgresVersion >= new Version(10, 0)
                        ? int.MinValue
                        : int.MinValue + 1;
                    defaultMax = -1;
                    defaultStart = sequence.MaxValue.Value;
                }
            }
            else if (sequence.StoreType == "bigint")
            {
                if (sequence.IncrementBy > 0)
                {
                    defaultMin = 1;
                    defaultMax = long.MaxValue;
                    defaultStart = sequence.MinValue.Value;
                }
                else
                {
                    // PostgreSQL 10 changed the default minvalue for a descending sequence, see #264
                    defaultMin = postgresVersion >= new Version(10, 0)
                        ? long.MinValue
                        : long.MinValue + 1;
                    defaultMax = -1;
                    Debug.Assert(sequence.MaxValue.HasValue);
                    defaultStart = sequence.MaxValue.Value;
                }
            }
            else
            {
                _logger.Logger.LogWarning($"Sequence with datatype {sequence.StoreType} which isn't the expected bigint.");
                return;
            }

            if (sequence.StartValue == defaultStart)
                sequence.StartValue = null;
            if (sequence.MinValue == defaultMin)
                sequence.MinValue = null;
            if (sequence.MaxValue == defaultMax)
                sequence.MaxValue = null;
        }

        void GetEnums(NpgsqlConnection connection, DatabaseModel databaseModel)
        {
            _enums.Clear();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT nspname, typname, array_agg(enumlabel ORDER BY enumsortorder) AS labels
FROM pg_enum
JOIN pg_type ON pg_type.oid=enumtypid
JOIN pg_namespace ON pg_namespace.oid=pg_type.typnamespace
GROUP BY nspname, typname";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var schema = reader.GetValueOrDefault<string>("nspname");
                        var name = reader.GetValueOrDefault<string>("typname");
                        var labels = reader.GetValueOrDefault<string[]>("labels");

                        if (schema == "public")
                            schema = null;
                        PostgresEnum.GetOrAddPostgresEnum(databaseModel, schema, name, labels);
                        _enums.Add(name);
                    }
                }
            }
        }

        void GetExtensions(NpgsqlConnection connection, DatabaseModel databaseModel)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT name,default_version,installed_version FROM pg_available_extensions";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(reader.GetOrdinal("name"));
                        var defaultVersion = reader.GetValueOrDefault<string>("default_version");
                        var installedVersion = reader.GetValueOrDefault<string>("installed_version");

                        if (installedVersion == null)
                            continue;

                        if (name == "plpgsql")   // Implicitly installed in all PG databases
                            continue;

                        PostgresExtension.GetOrAddPostgresExtension(databaseModel, name);
                    }
                }
            }
        }

        static string DisplayName(string schema, string name)
            => (!string.IsNullOrEmpty(schema) ? schema + "." : "") + name;

        static ReferentialAction? ConvertToReferentialAction(char onDeleteAction)
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
                    throw new ArgumentOutOfRangeException($"Unknown value {onDeleteAction} for foreign key deletion action code");
            }
        }

        static Func<string, string> GenerateSchemaFilter(IReadOnlyList<string> schemas)
        {
            if (schemas.Any())
            {
                return s =>
                {
                    var schemaFilterBuilder = new StringBuilder();
                    schemaFilterBuilder.Append(s);
                    schemaFilterBuilder.Append(" IN (");
                    schemaFilterBuilder.Append(string.Join(", ", schemas.Select(EscapeLiteral)));
                    schemaFilterBuilder.Append(")");
                    return schemaFilterBuilder.ToString();
                };
            }

            return null;
        }

        static (string Schema, string Table) Parse(string table)
        {
            var match = _partExtractor.Match(table.Trim());

            if (!match.Success)
            {
                throw new InvalidOperationException();
            }

            var part1 = match.Groups["part1"].Value;
            var part2 = match.Groups["part2"].Value;

            return string.IsNullOrEmpty(part2) ? (null, part1) : (part1, part2);
        }

        static Func<string, string, string> GenerateTableFilter(
            IReadOnlyList<(string Schema, string Table)> tables,
            Func<string, string> schemaFilter)
        {
            if (schemaFilter != null
                || tables.Any())
            {
                return (s, t) =>
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
                };
            }

            return null;
        }

        static string EscapeLiteral(string s) => $"'{s}'";
    }
}
