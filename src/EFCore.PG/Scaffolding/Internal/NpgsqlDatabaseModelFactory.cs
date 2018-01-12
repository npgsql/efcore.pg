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
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class NpgsqlDatabaseModelFactory : IDatabaseModelFactory
    {
        private const string NamePartRegex
            = @"(?:(?:\[(?<part{0}>(?:(?:\]\])|[^\]])+)\])|(?<part{0}>[^\.\[\]]+))";

        private static readonly Regex _partExtractor
            = new Regex(
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"^{0}(?:\.{1})?$",
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)),
                RegexOptions.Compiled,
                TimeSpan.FromMilliseconds(1000.0));

        private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;

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

        public virtual DatabaseModel Create(DbConnection connection, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            var connectionNpgsql = (NpgsqlConnection)connection;
            var connectionStartedOpen = connectionNpgsql.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                connectionNpgsql.Open();
            }

            try
            {
                var databaseModel = new DatabaseModel
                {
                    DatabaseName = connectionNpgsql.Database,
                    DefaultSchema = "public"
                };

                var schemaList = schemas.ToList();
                var schemaFilter = GenerateSchemaFilter(schemaList);
                var tableList = tables.ToList();
                var tableFilter = GenerateTableFilter(tableList.Select(Parse).ToList(), schemaFilter);

                foreach (var table in GetTables(connectionNpgsql, tableFilter))
                {
                    table.Database = databaseModel;
                    databaseModel.Tables.Add(table);
                }

                foreach (var table in databaseModel.Tables)
                {
                    while (table.Columns.Remove(null)) { }
                }

                foreach (var sequence in GetSequences(connectionNpgsql, databaseModel.Tables, schemaFilter))
                {
                    sequence.Database = databaseModel;
                    databaseModel.Sequences.Add(sequence);
                }

                GetExtensions(connectionNpgsql, databaseModel);

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

        private IEnumerable<DatabaseTable> GetTables(
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

            var filter = $"AND cls.relname <> '{HistoryRepository.DefaultTableName}' {(tableFilter != null ? $" AND {tableFilter("nspname", "relname")}" : "")}";

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
                        {
                            table[NpgsqlAnnotationNames.Comment] = comment;
                        }
                        tables.Add(table);
                    }
                }

                GetColumns(connection, tables, filter);
                GetIndexes(connection, tables, filter);
                GetKeys(connection, tables, filter);
                GetConstraints(connection, tables, filter);

                return tables;
            }
        }

        private void GetColumns(
            NpgsqlConnection connection,
            IReadOnlyList<DatabaseTable> tables,
            string tableFilter)
        {
            using (var command = connection.CreateCommand())
            {
                var commandText = @"
SELECT
    nspname, relname, attisdropped, attname, typ.typname, atttypmod, description, basetyp.typname AS domtypname,
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
    nspname NOT IN ('pg_catalog', 'information_schema')
AND
    attnum > 0 " + tableFilter + " ORDER BY attnum";

                command.CommandText = commandText;

                using (var reader = command.ExecuteReader())
                {
                    var tableColumnGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                            ddr => (tableSchema: ddr.GetValueOrDefault<string>("nspname"),
                                tableName: ddr.GetValueOrDefault<string>("relname")));

                    foreach (var tableColumnGroup in tableColumnGroups)
                    {
                        var tableSchema = tableColumnGroup.Key.tableSchema;
                        var tableName = tableColumnGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        foreach (var dataRecord in tableColumnGroup)
                        {
                            var typeModifier = dataRecord.GetValueOrDefault<int>("atttypmod");
                            var columnName = dataRecord.GetValueOrDefault<string>("attname");
                            var dataTypeName = dataRecord.GetValueOrDefault<string>("typname");
                            var nullable = dataRecord.GetValueOrDefault<bool>("nullable");
                            var defaultValue = dataRecord.GetValueOrDefault<string>("default");
                            string computedValue = null;
                            var comment = dataRecord.GetValueOrDefault<string>("description");

                            var storeType = GetStoreType(dataTypeName, typeModifier);

                            if (dataTypeName == "bpchar")
                            {
                                dataTypeName = "char";
                            }

                            var column = new DatabaseColumn
                            {
                                Table = table,
                                Name = columnName,
                                StoreType = storeType,
                                IsNullable = nullable,
                                DefaultValueSql = defaultValue,
                                ComputedColumnSql = computedValue
                            };

                            if (defaultValue != null)
                            {
                                // Somewhat hacky... We identify serial columns by examining their default expression,
                                // and reverse-engineer these as ValueGenerated.OnAdd
                                if (defaultValue == $"nextval('{tableName}_{columnName}_seq'::regclass)" ||
                                    defaultValue == $"nextval('\"{tableName}_{columnName}_seq\"'::regclass)")
                                {
                                    // TODO: Scaffold as serial, bigserial, not int...
                                    // But in normal code-first I don't have to set the column type...!
                                    // TODO: Think about composite keys. Do serial magic only for non-composite.
                                    column.ValueGenerated = ValueGenerated.OnAdd;
                                    column.DefaultValueSql = null;
                                }
                                else
                                {
                                    column.ValueGenerated = default(ValueGenerated?);
                                }
                            }
                            table.Columns.Add(column);
                        }
                    }
                }
            }
        }

        private void GetIndexes(
            NpgsqlConnection connection,
            IReadOnlyList<DatabaseTable> tables,
            string tableFilter)
        {
            using (var command = connection.CreateCommand())
            {
                var getColumnsQuery = @"
SELECT
    nspname, cls.relname AS cls_relname, idxcls.relname AS idx_relname, indisunique, indkey, amname,
    CASE WHEN indexprs IS NULL THEN NULL ELSE pg_get_expr(indexprs, cls.oid) END AS expr
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
                    var tableIndexGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                            ddr => (tableSchema: ddr.GetValueOrDefault<string>("nspname"),
                                tableName: ddr.GetValueOrDefault<string>("cls_relname")));

                    foreach (var tableIndexGroup in tableIndexGroups)
                    {
                        var tableSchema = tableIndexGroup.Key.tableSchema;
                        var tableName = tableIndexGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        var indexGroups = tableIndexGroup
                            .GroupBy(
                                ddr =>
                                    (Name: ddr.GetValueOrDefault<string>("idx_relname"),
                                    IsUnique: ddr.GetValueOrDefault<bool>("indisunique")))
                            .ToArray();

                        foreach (var indexGroup in indexGroups)
                        {
                            var index = new DatabaseIndex
                            {
                                Table = table,
                                Name = indexGroup.Key.Name,
                                IsUnique = indexGroup.Key.IsUnique
                            };

                            foreach (var dataRecord in indexGroup)
                            {
                                var columnIndices = dataRecord.GetValueOrDefault<short[]>("indkey");
                                if (columnIndices.Any(i => i == 0))
                                {
                                    if (dataRecord.IsDBNull(dataRecord.GetOrdinal("expr")))
                                        throw new Exception($"Seen 0 in indkey for index {index.Name} but indexprs is null");
                                    index[NpgsqlAnnotationNames.IndexExpression] = dataRecord.GetValueOrDefault<string>("expr");
                                }
                                else
                                {
                                    var columns = (List<DatabaseColumn>)table.Columns;
                                    for (var ordinal = 0; ordinal < columnIndices.Length; ordinal++)
                                    {
                                        var columnIndex = columnIndices[ordinal] - 1;
                                        index.Columns.Add(columns[columnIndex]);
                                    }
                                }

                                index[NpgsqlAnnotationNames.IndexMethod] = dataRecord.GetValueOrDefault<string>("amname");
                            }

                            table.Indexes.Add(index);
                        }
                    }
                }
            }
        }

        private void GetKeys(
            NpgsqlConnection connection,
            IReadOnlyList<DatabaseTable> tables,
            string tableFilter)
        {

            var getKeys = @"
SELECT
    ns.nspname, cls.relname, conname, contype, conkey, frnns.nspname AS fr_nspname, frncls.relname AS fr_relname, confkey, confdeltype
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
JOIN pg_constraint as con ON con.conrelid = cls.oid
LEFT OUTER JOIN pg_class AS frncls ON frncls.oid = con.confrelid
LEFT OUTER JOIN pg_namespace as frnns ON frnns.oid = frncls.relnamespace
WHERE
    cls.relkind = 'r' AND
    ns.nspname NOT IN ('pg_catalog', 'information_schema')
AND
    con.contype = 'p'
"+ tableFilter;

            var command = connection.CreateCommand();
            command.CommandText = getKeys;

            using (var reader = command.ExecuteReader())
            {
                var tableKeyGroups = reader.Cast<DbDataRecord>()
                    .GroupBy(
                        ddr => (tableSchema: ddr.GetValueOrDefault<string>("nspname"),
                            tableName: ddr.GetValueOrDefault<string>("relname")));

                foreach (var tableKeyGroup in tableKeyGroups)
                {
                    var tableSchema = tableKeyGroup.Key.tableSchema;
                    var tableName = tableKeyGroup.Key.tableName;

                    var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                    var primaryKeyGroups = tableKeyGroup
                        .GroupBy(
                            c => (Name: c.GetValueOrDefault<string>("conname"),
                                PrincipalTableSchema: c.GetValueOrDefault<string>("nspname"),
                                PrincipalTableName: c.GetValueOrDefault<string>("relname")))
                                .ToArray();


                    if (primaryKeyGroups.Length == 1)
                    {
                        var primaryKeyGroup = primaryKeyGroups[0];
                        var primaryKey = new DatabasePrimaryKey
                        {
                            Table = table,
                            Name = primaryKeyGroup.Key.Name
                        };

                        foreach (var dataRecord in primaryKeyGroup)
                        {
                            var pkColumnIndices = dataRecord.GetValueOrDefault<short[]>("conkey");
                            foreach (var pkColumnIndex in pkColumnIndices)
                            {
                                primaryKey.Columns.Add(table.Columns[pkColumnIndex - 1]);
                            }
                        }
                        table.PrimaryKey = primaryKey;
                    }
                }
            }
        }

        private void GetConstraints(
            NpgsqlConnection connection,
            IReadOnlyList<DatabaseTable> tables,
            string tableFilter)
        {

            var getConstraints = @"
SELECT
    ns.nspname, cls.relname, conname, contype, conkey, frnns.nspname AS fr_nspname, frncls.relname AS fr_relname, confkey, confdeltype
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
JOIN pg_constraint as con ON con.conrelid = cls.oid
LEFT OUTER JOIN pg_class AS frncls ON frncls.oid = con.confrelid
LEFT OUTER JOIN pg_namespace as frnns ON frnns.oid = frncls.relnamespace
WHERE
    cls.relkind = 'r' AND
    ns.nspname NOT IN ('pg_catalog', 'information_schema')
AND
    con.contype = 'f'
" + tableFilter;

            var command = connection.CreateCommand();
            command.CommandText = getConstraints;

            using (var reader = command.ExecuteReader())
            {
                var tableForeignKeyGroups = reader.Cast<DbDataRecord>()
                    .GroupBy(
                        ddr => (tableSchema: ddr.GetValueOrDefault<string>("nspname"),
                            tableName: ddr.GetValueOrDefault<string>("relname")));

                foreach (var tableForeignKeyGroup in tableForeignKeyGroups)
                {
                    var tableSchema = tableForeignKeyGroup.Key.tableSchema;
                    var tableName = tableForeignKeyGroup.Key.tableName;

                    var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                    var foreignKeyGroups = tableForeignKeyGroup
                        .GroupBy(
                            c => (Name: c.GetValueOrDefault<string>("conname"),
                                PrincipalTableSchema: c.GetValueOrDefault<string>("fr_nspname"),
                                PrincipalTableName: c.GetValueOrDefault<string>("fr_relname"),
                                OnDeleteAction: c.GetValueOrDefault<char>("confdeltype"),
                                ConstraintType: c.GetValueOrDefault<char>("contype")));

                    foreach (var foreignKeyGroup in foreignKeyGroups)
                    {
                        var fkName = foreignKeyGroup.Key.Name;
                        var principalTableSchema = foreignKeyGroup.Key.PrincipalTableSchema;
                        var principalTableName = foreignKeyGroup.Key.PrincipalTableName;
                        var onDeleteAction = foreignKeyGroup.Key.OnDeleteAction;
                        var constraintType = foreignKeyGroup.Key.ConstraintType;

                        var principalTable = tables.FirstOrDefault(
                                                 t => t.Schema == principalTableSchema
                                                      && t.Name == principalTableName)
                                             ?? tables.FirstOrDefault(
                                                 t => t.Schema.Equals(principalTableSchema, StringComparison.OrdinalIgnoreCase)
                                                      && t.Name.Equals(principalTableName, StringComparison.OrdinalIgnoreCase));

                        if (principalTable == null)
                        {
                            continue;
                        }

                        var foreignKey = new DatabaseForeignKey
                        {
                            Name = fkName,
                            Table = table,
                            PrincipalTable = principalTable,
                            OnDelete = ConvertToReferentialAction(onDeleteAction)
                        };

                        foreach (var dataRecord in foreignKeyGroup)
                        {
                            var columnIndices = dataRecord.GetValueOrDefault<short[]>("conkey");
                            var principalColumnIndices = dataRecord.GetValueOrDefault<short[]>("confkey");
                            if (columnIndices.Length != principalColumnIndices.Length)
                                throw new Exception("Got varying lengths for column and principal column indices");

                            var principalColumns = (List<DatabaseColumn>)principalTable.Columns;

                            for (var i = 0; i < columnIndices.Length; i++)
                            {
                                foreignKey.Columns.Add(table.Columns[columnIndices[i] - 1]);
                                foreignKey.PrincipalColumns.Add(principalColumns[principalColumnIndices[i] - 1]);
                            }
                        }

                        table.ForeignKeys.Add(foreignKey);
                    }
                }
            }
        }

        private IEnumerable<DatabaseSequence> GetSequences(
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

                        if (sequence.StoreType == "bigint")
                        {
                            long defaultStart, defaultMin, defaultMax;
                            if (sequence.IncrementBy > 0)
                            {
                                defaultMin = 1;
                                defaultMax = long.MaxValue;
                                Debug.Assert(sequence.MinValue.HasValue);
                                defaultStart = sequence.MinValue.Value;
                            }
                            else
                            {
                                // PostgreSQL 10 changed the default minvalue for a descending sequence, see #264
                                defaultMin = connection.PostgreSqlVersion >= new Version(10, 0)
                                    ? long.MinValue
                                    : long.MinValue + 1;
                                defaultMax = -1;
                                Debug.Assert(sequence.MaxValue.HasValue);
                                defaultStart = sequence.MaxValue.Value;
                            }
                            if (sequence.StartValue == defaultStart)
                                sequence.StartValue = null;
                            if (sequence.MinValue == defaultMin)
                                sequence.MinValue = null;
                            if (sequence.MaxValue == defaultMax)
                                sequence.MaxValue = null;
                        }
                        else
                        {
                            _logger.Logger.LogWarning($"Sequence with datatype {sequence.StoreType} which isn't the expected bigint.");
                        }

                        yield return sequence;
                    }
                }
            }
        }

        private void GetExtensions(NpgsqlConnection connection, DatabaseModel databaseModel)
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

        private static ReferentialAction? ConvertToReferentialAction(char onDeleteAction)
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

        private static string GetStoreType(string dataTypeName, int typeModifier)
        {
            if (typeModifier == -1)
                return dataTypeName;

            switch (dataTypeName)
            {
                case "bpchar":
                case "char":
                case "varchar":
                    return $"{dataTypeName}({typeModifier - 4})";  // Max length
                case "numeric":
                case "decimal":
                    // See http://stackoverflow.com/questions/3350148/where-are-numeric-precision-and-scale-for-a-field-found-in-the-pg-catalog-tables
                    var precision = ((typeModifier - 4) >> 16) & 65535;
                    var scale = (typeModifier - 4) & 65535;
                    return $"{dataTypeName}({precision}, {scale})";
                // TODO: Support for precision-only for timestamp, time, interval
                default:
                    //Logger.Logger.LogWarning($"Don't know how to interpret type modifier {typeModifier} for datatype {dataTypeName}'");
                    return dataTypeName;
            }
        }

        private static Func<string, string> GenerateSchemaFilter(IReadOnlyList<string> schemas)
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

        private static (string Schema, string Table) Parse(string table)
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

        private static Func<string, string, string> GenerateTableFilter(
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
                            tableFilterBuilder.Append(") AND CONCAT(");
                            tableFilterBuilder.Append(s);
                            tableFilterBuilder.Append(", N'.', ");
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

        private static string EscapeLiteral(string s) => $"N'{s}'";
    }
}
