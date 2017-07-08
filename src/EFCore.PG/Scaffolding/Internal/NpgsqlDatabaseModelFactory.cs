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
using System.Linq;
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
        NpgsqlConnection _connection;
        TableSelectionSet _tableSelectionSet;
        DatabaseModel _databaseModel;
        Dictionary<string, DatabaseTable> _tables;
        Dictionary<string, DatabaseColumn> _tableColumns;

        static string TableKey(DatabaseTable table) => TableKey(table.Name, table.Schema);
        static string TableKey(string name, string schema) => $"\"{schema}\".\"{name}\"";
        static string ColumnKey(DatabaseTable table, string columnName) => $"{TableKey(table)}.\"{columnName}\"";

        public NpgsqlDatabaseModelFactory([NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            Check.NotNull(logger, nameof(logger));

            Logger = logger;
        }

        public virtual IDiagnosticsLogger<DbLoggerCategory.Scaffolding> Logger { get; }

        void ResetState()
        {
            _connection = null;
            _tableSelectionSet = null;
            _databaseModel = new DatabaseModel();
            _tables = new Dictionary<string, DatabaseTable>();
            _tableColumns = new Dictionary<string, DatabaseColumn>(StringComparer.OrdinalIgnoreCase);
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
            ResetState();

            _connection = (NpgsqlConnection)connection;

            var connectionStartedOpen = _connection.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                _connection.Open();
            }

            try
            {
                _tableSelectionSet = new TableSelectionSet(tables, schemas);

                _databaseModel.DatabaseName = _connection.Database;
                _databaseModel.DefaultSchema = "public";

                GetTables();
                GetColumns();
                GetIndexes();
                GetConstraints();
                GetSequences();
                GetExtensions();

                // We may have dropped columns. We load these because constraints take them into
                // account when referencing columns, but must now get rid of them before returning
                // the database model.
                foreach (var table in _databaseModel.Tables)
                    while (table.Columns.Remove(null)) {}

                return _databaseModel;
            }
            finally
            {
                if (!connectionStartedOpen)
                {
                    _connection.Close();
                }
            }
        }

        const string GetTablesQuery = @"
SELECT nspname, relname
FROM pg_class AS cl
JOIN pg_namespace AS ns ON ns.oid = cl.relnamespace
WHERE
    cl.relkind = 'r' AND
    ns.nspname NOT IN ('pg_catalog', 'information_schema') AND
    relname <> '" + HistoryRepository.DefaultTableName + "'";

        void GetTables()
        {
            using (var command = new NpgsqlCommand(GetTablesQuery, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var table = new DatabaseTable
                    {
                        Schema = reader.GetValueOrDefault<string>("nspname"),
                        Name = reader.GetValueOrDefault<string>("relname")
                    };

                    if (_tableSelectionSet.Allows(table.Schema, table.Name))
                    {
                        _databaseModel.Tables.Add(table);
                        _tables[TableKey(table)] = table;
                    }
                }
            }
        }

        const string GetColumnsQuery = @"
SELECT
    nspname, relname, attisdropped, attname, typ.typname, atttypmod,
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
WHERE
    relkind = 'r' AND
    nspname NOT IN ('pg_catalog', 'information_schema') AND
    relname <> '" + HistoryRepository.DefaultTableName + @"' AND
    attnum > 0
ORDER BY attnum";

        void GetColumns()
        {
            using (var command = new NpgsqlCommand(GetColumnsQuery, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var schemaName = reader.GetValueOrDefault<string>("nspname");
                    var tableName = reader.GetValueOrDefault<string>("relname");
                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                        continue;

                    var table = _tables[TableKey(tableName, schemaName)];

                    // We need to know about dropped columns because constraints take them into
                    // account when referencing columns. We'll get rid of them before returning the model.
                    var isDropped = reader.GetValueOrDefault<bool>("attisdropped");
                    if (isDropped)
                    {
                        table.Columns.Add(null);
                        continue;
                    }

                    var columnName = reader.GetValueOrDefault<string>("attname");
                    var dataType = reader.GetValueOrDefault<string>("typname");
                    var typeModifier = reader.GetValueOrDefault<int>("atttypmod");
                    var typeChar = reader.GetValueOrDefault<char>("typtype");
                    var elemDataType = reader.GetValueOrDefault<string>("elemtypname");
                    var isNullable = reader.GetValueOrDefault<bool>("nullable");
                    var defaultValue = reader.GetValueOrDefault<string>("default");

                    // bpchar is just an internal name for char
                    if (dataType == "bpchar")
                        dataType = "char";

                    var column = new DatabaseColumn
                    {
                        Table               = table,
                        Name                = columnName,
                        StoreType           = GetStoreType(dataType, typeModifier),
                        IsNullable          = isNullable,
                        DefaultValueSql     = defaultValue
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
                    }

                    switch (typeChar)
                    {
                    case 'b':
                        // Base (regular), is the default
                        break;
                    case 'a':
                        // PG array types in pg_type start with underscores (_int for array of int), but the type name
                        // PG accepts when creating columns is int[], translate.
                        if (column.StoreType.StartsWith("_"))
                            column.StoreType = column.StoreType.Substring(1) + "[]";
                        break;
                    case 'r':
                        column[NpgsqlAnnotationNames.PostgresTypeType] = PostgresTypeType.Range;
                        break;
                    case 'e':
                        column[NpgsqlAnnotationNames.PostgresTypeType] = PostgresTypeType.Enum;
                        break;
                    default:
                        Logger.Logger.LogWarning($"Can't scaffold column '{columnName}' of type '{dataType}': unknown type char '{typeChar}'");
                        continue;
                    }

                    table.Columns.Add(column);
                    _tableColumns.Add(ColumnKey(table, column.Name), column);
                }
            }
        }

        string GetStoreType(string dataTypeName, int typeModifier)
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
                Logger.Logger.LogWarning($"Don't know how to interpret type modifier {typeModifier} for datatype {dataTypeName}'");
                return dataTypeName;
            }
        }

        const string GetIndexesQuery = @"
SELECT
    nspname, cls.relname AS cls_relname, idxcls.relname AS idx_relname, indisunique, indkey,
    CASE WHEN indexprs IS NULL THEN NULL ELSE pg_get_expr(indexprs, cls.oid) END AS expr
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
JOIN pg_index AS idx ON indrelid = cls.oid
JOIN pg_class AS idxcls ON idxcls.oid = indexrelid
WHERE
    cls.relkind = 'r' AND
    nspname NOT IN ('pg_catalog', 'information_schema') AND
    cls.relname <> '" + HistoryRepository.DefaultTableName + @"' AND
    NOT indisprimary";

        /// <remarks>
        /// Primary keys are handled as in <see cref="GetConstraints"/>, not here
        /// </remarks>
        void GetIndexes()
        {
            using (var command = new NpgsqlCommand(GetIndexesQuery, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var schemaName = reader.GetValueOrDefault<string>("nspname");
                    var tableName = reader.GetValueOrDefault<string>("cls_relname");
                    var indexName = reader.GetValueOrDefault<string>("idx_relname");

                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                        continue;

                    DatabaseTable table;
                    if (!_tables.TryGetValue(TableKey(tableName, schemaName), out table))
                        continue;

                    var index = new DatabaseIndex
                    {
                        Table = table,
                        Name = indexName,
                        IsUnique = reader.GetValueOrDefault<bool>("indisunique")
                    };

                    table.Indexes.Add(index);

                    var columnIndices = reader.GetValueOrDefault<short[]>("indkey");
                    if (columnIndices.Any(i => i == 0))
                    {
                        if (reader.IsDBNull(reader.GetOrdinal("expr")))
                            throw new Exception($"Seen 0 in indkey for index {indexName} but indexprs is null");
                        index[NpgsqlAnnotationNames.IndexExpression] = reader.GetValueOrDefault<string>("expr");
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
                }
            }
        }

        const string GetConstraintsQuery = @"
SELECT
    ns.nspname, cls.relname, conname, contype, conkey, frnns.nspname AS fr_nspname, frncls.relname AS fr_relname, confkey, confdeltype
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
JOIN pg_constraint as con ON con.conrelid = cls.oid
LEFT OUTER JOIN pg_class AS frncls ON frncls.oid = con.confrelid
LEFT OUTER JOIN pg_namespace as frnns ON frnns.oid = frncls.relnamespace
WHERE
    cls.relkind = 'r' AND
    ns.nspname NOT IN ('pg_catalog', 'information_schema') AND
    cls.relname <> '" + HistoryRepository.DefaultTableName + @"' AND
    con.contype IN ('p', 'f')";

        void GetConstraints()
        {
            using (var command = new NpgsqlCommand(GetConstraintsQuery, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var schemaName = reader.GetValueOrDefault<string>("nspname");
                    var tableName = reader.GetValueOrDefault<string>("relname");

                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                        continue;
                    var table = _tables[TableKey(tableName, schemaName)];
                    var columns = (List<DatabaseColumn>)table.Columns;

                    var constraintName = reader.GetValueOrDefault<string>("conname");
                    var constraintType = reader.GetValueOrDefault<char>("contype");
                    switch (constraintType)
                    {
                    case 'p':
                        var primaryKey = new DatabasePrimaryKey
                        {
                            Table = table,
                            Name = constraintName
                        };
                        var pkColumnIndices = reader.GetValueOrDefault<short[]>("conkey");
                        foreach (var pkColumnIndex in pkColumnIndices)
                                primaryKey.Columns.Add(columns[pkColumnIndex-1]);
                        Debug.Assert(table.PrimaryKey == null);
                        table.PrimaryKey = primaryKey;
                        continue;

                    case 'f':
                        var foreignSchemaName = reader.GetValueOrDefault<string>("fr_nspname");
                        var foreignTableName = reader.GetValueOrDefault<string>("fr_relname");
                        if (!_tables.TryGetValue(TableKey(foreignTableName, foreignSchemaName), out var principalTable))
                            continue;

                        var foreignKey = new DatabaseForeignKey
                        {
                            Name = constraintName,
                            Table = table,
                            PrincipalTable = principalTable,
                            OnDelete = ConvertToReferentialAction(reader.GetValueOrDefault<char>("confdeltype"))
                        };

                        var columnIndices = reader.GetValueOrDefault<short[]>("conkey");
                        var principalColumnIndices = reader.GetValueOrDefault<short[]>("confkey");
                        if (columnIndices.Length != principalColumnIndices.Length)
                            throw new Exception("Got varying lengths for column and principal column indices");

                        var principalColumns = (List<DatabaseColumn>)principalTable.Columns;

                        for (var i = 0; i < columnIndices.Length; i++)
                        {
                            foreignKey.Columns.Add(columns[columnIndices[i] - 1]);
                            foreignKey.PrincipalColumns.Add(principalColumns[principalColumnIndices[i] - 1]);
                        }

                        table.ForeignKeys.Add(foreignKey);
                        break;

                    default:
                        throw new NotSupportedException($"Unknown constraint type code {constraintType} for constraint {constraintName}");
                    }
                }
            }
        }

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

        const string GetSequencesQuery = @"
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

        void GetSequences()
        {
            using (var command = new NpgsqlCommand(GetSequencesQuery, _connection))
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

                        DatabaseTable ownerDatabaseTable;
                        DatabaseColumn ownerDatabaseColumn;
                        if (_tables.TryGetValue(TableKey(ownerTable, ownerSchema), out ownerDatabaseTable) &&
                            _tableColumns.TryGetValue(ColumnKey(ownerDatabaseTable, ownerColumn), out ownerDatabaseColumn) &&
                            ownerDatabaseColumn.ValueGenerated == ValueGenerated.OnAdd)
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

                    if (!_tableSelectionSet.Allows(sequence.Schema, ""))
                        continue;

                    if (sequence.StoreType == "bigint")
                    {
                        long defaultStart, defaultMin, defaultMax;
                        if (sequence.IncrementBy > 0)
                        {
                            defaultMin = 1;
                            defaultMax = long.MaxValue;
                            Debug.Assert(sequence.MinValue.HasValue);
                            defaultStart = sequence.MinValue.Value;
                        } else {
                            defaultMin = long.MinValue + 1;
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
                        Logger.Logger.LogWarning($"Sequence with datatype {sequence.StoreType} which isn't the expected bigint.");

                    _databaseModel.Sequences.Add(sequence);
                }
            }
        }

        void GetExtensions()
        {
            using (var command = new NpgsqlCommand("SELECT name,default_version,installed_version FROM pg_available_extensions", _connection))
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

                    PostgresExtension.GetOrAddPostgresExtension(_databaseModel, name);
                }
            }
        }
    }
}
