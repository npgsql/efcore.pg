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
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class NpgsqlDatabaseModelFactory : IDatabaseModelFactory
    {
        private NpgsqlConnection _connection;
        private TableSelectionSet _tableSelectionSet;
        private DatabaseModel _databaseModel;
        private Dictionary<string, TableModel> _tables;
        private Dictionary<string, ColumnModel> _tableColumns;

        private static string TableKey(TableModel table) => TableKey(table.Name, table.SchemaName);
        private static string TableKey(string name, string schema) => $"\"{schema}\".\"{name}\"";
        private static string ColumnKey(TableModel table, string columnName) => $"{TableKey(table)}.\"{columnName}\"";

        public NpgsqlDatabaseModelFactory([NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            Logger = loggerFactory.CreateCommandsLogger();
        }

        public virtual ILogger Logger { get; }

        private void ResetState()
        {
            _connection = null;
            _tableSelectionSet = null;
            _databaseModel = new DatabaseModel();
            _tables = new Dictionary<string, TableModel>();
            _tableColumns = new Dictionary<string, ColumnModel>(StringComparer.OrdinalIgnoreCase);
        }

        public DatabaseModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tableSelectionSet, nameof(tableSelectionSet));

            ResetState();

            using (_connection = new NpgsqlConnection(connectionString))
            {
                _connection.Open();
                _tableSelectionSet = tableSelectionSet;

                _databaseModel.DatabaseName = _connection.Database;
                _databaseModel.DefaultSchemaName = "public";

                GetTables();
                GetColumns();
                GetIndexes();
                GetConstraints();
                GetSequences();
                return _databaseModel;
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
                    var table = new TableModel
                    {
                        SchemaName = reader.GetString(0),
                        Name = reader.GetString(1)
                    };

                    if (_tableSelectionSet.Allows(table.SchemaName, table.Name))
                    {
                        _databaseModel.Tables.Add(table);
                        _tables[TableKey(table)] = table;
                    }
                }
            }
        }

        const string GetColumnsQuery = @"
            SELECT
                nspname, relname, attname, typname, attnum, atttypmod,
                (NOT attnotnull) AS nullable,
                CASE WHEN atthasdef THEN (SELECT pg_get_expr(adbin, cls.oid) FROM pg_attrdef WHERE adrelid = cls.oid AND adnum = attr.attnum) ELSE NULL END AS default
            FROM pg_class AS cls
            JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
            LEFT OUTER JOIN pg_attribute AS attr ON attrelid = cls.oid
            LEFT OUTER JOIN pg_type AS typ ON attr.atttypid = typ.oid
            WHERE
              atttypid <> 0 AND
              relkind = 'r' AND
              nspname NOT IN ('pg_catalog', 'information_schema') AND
              relname <> '" + HistoryRepository.DefaultTableName + @"' AND
              attnum > 0
            ORDER BY attnum";

        private void GetColumns()
        {
            using (var command = new NpgsqlCommand(GetColumnsQuery, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var schemaName = reader.GetString(0);
                    var tableName = reader.GetString(1);
                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                    {
                        continue;
                    }

                    var columnName = reader.GetString(2);
                    var dataType = reader.GetString(3);
                    var ordinal = reader.GetInt32(4) - 1;
                    var typeModifier = reader.GetInt32(5);
                    var isNullable = reader.GetBoolean(6);
                    int? maxLength = null;
                    int? precision = null;
                    int? scale = null;
                    var defaultValue = reader.IsDBNull(7) ? null : reader.GetString(7);

                    if (typeModifier != -1)
                    {
                        switch (dataType)
                        {
                        case "bpchar":
                        case "char":
                        case "varchar":
                            maxLength = typeModifier - 4;
                            break;
                        case "numeric":
                        case "decimal":
                            // See http://stackoverflow.com/questions/3350148/where-are-numeric-precision-and-scale-for-a-field-found-in-the-pg-catalog-tables
                            precision = ((typeModifier - 4) >> 16) & 65535;
                            scale = (typeModifier - 4) & 65535;
                            break;
                        }
                    }

                    var table = _tables[TableKey(tableName, schemaName)];
                    var column = new ColumnModel
                    {
                        Table          = table,
                        Name           = columnName,
                        DataType       = dataType,
                        Ordinal        = ordinal,
                        IsNullable     = isNullable,
                        MaxLength      = maxLength,
                        Precision      = precision,
                        Scale          = scale,
                        DefaultValue   = defaultValue
                    };

                    // Somewhat hacky... We identify serial columns by examining their default expression,
                    // and reverse-engineer these as ValueGenerated.OnAdd
                    if (defaultValue != null && (
                          defaultValue == $"nextval('{tableName}_{columnName}_seq'::regclass)" ||
                          defaultValue == $"nextval('\"{tableName}_{columnName}_seq\"'::regclass)")
                       )
                    {
                        column.Npgsql().IsSerial = true;
                        column.ValueGenerated = ValueGenerated.OnAdd;
                        column.DefaultValue = null;
                    }

                    table.Columns.Add(column);
                    _tableColumns.Add(ColumnKey(table, column.Name), column);
                }
            }
        }

        const string GetIndexesQuery = @"
            SELECT
                nspname, cls.relname, idxcls.relname, indisunique, indkey,
                CASE WHEN indexprs IS NULL THEN NULL ELSE pg_get_expr(indexprs, cls.oid) END
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
                    var schemaName = reader.GetString(0);
                    var tableName = reader.GetString(1);
                    var indexName = reader.GetString(2);

                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                        continue;

                    TableModel table;
                    if (!_tables.TryGetValue(TableKey(tableName, schemaName), out table))
                        continue;

                    var index = new IndexModel
                    {
                        Table = table,
                        Name = indexName,
                        IsUnique = reader.GetBoolean(3)
                    };

                    table.Indexes.Add(index);

                    var columnIndices = reader.GetFieldValue<short[]>(4);
                    if (columnIndices.Any(i => i == 0))
                    {
                        if (reader.IsDBNull(5))
                            throw new Exception($"Seen 0 in indkey for index {indexName} but indexprs is null");
                        index.Npgsql().Expression = reader.GetString(5);
                    }
                    else
                    {
                        var columns = (List<ColumnModel>)table.Columns;
                        for (var ordinal = 0; ordinal < columnIndices.Length; ordinal++)
                        {
                            var columnIndex = columnIndices[ordinal] - 1;
                            index.IndexColumns.Add(new IndexColumnModel
                            {
                                Index = index,
                                Column = columns[columnIndex],
                                Ordinal = ordinal  // TODO: One-based or zero-based?
                            });
                        }
                    }
                }
            }
        }

        const string GetConstraintsQuery = @"
            SELECT
                ns.nspname, cls.relname, conname, contype, conkey, frnns.nspname, frncls.relname, confkey, confdeltype
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
                    var schemaName = reader.GetString(0);
                    var tableName = reader.GetString(1);

                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                        continue;
                    var table = _tables[TableKey(tableName, schemaName)];
                    var columns = (List<ColumnModel>)table.Columns;

                    var constraintName = reader.GetString(2);
                    var constraintType = reader.GetChar(3);
                    switch (constraintType)
                    {
                    case 'p':
                        var pkColumnIndices = reader.GetFieldValue<short[]>(4);
                        for (var i = 0; i < pkColumnIndices.Length; i++)
                            columns[pkColumnIndices[i] - 1].PrimaryKeyOrdinal = i + 1;
                        continue;

                    case 'f':
                        var foreignSchemaName = reader.GetString(5);
                        var foreignTableName = reader.GetString(6);
                        TableModel principalTable;
                        if (!_tables.TryGetValue(TableKey(foreignTableName, foreignSchemaName), out principalTable))
                            continue;

                        var fkInfo = new ForeignKeyModel
                        {
                            Name = constraintName,
                            Table = table,
                            PrincipalTable = principalTable,
                            OnDelete = ConvertToReferentialAction(reader.GetChar(8))
                        };

                        var columnIndices = reader.GetFieldValue<short[]>(4);
                        var principalColumnIndices = reader.GetFieldValue<short[]>(7);
                        if (columnIndices.Length != principalColumnIndices.Length)
                            throw new Exception("Got varying lengths for column and principal column indices");

                        for (var ordinal = 0; ordinal < columnIndices.Length; ordinal++)
                            fkInfo.Columns.Add(new ForeignKeyColumnModel {
                                Column = columns[columnIndices[ordinal] - 1],
                                PrincipalColumn = columns[principalColumnIndices[ordinal] - 1],
                                Ordinal = ordinal
                            });

                        table.ForeignKeys.Add(fkInfo);
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
                CASE WHEN cycle_option = 'YES' THEN TRUE ELSE FALSE END,
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
                        var ownerSchema = reader.GetString(8);
                        var ownerTable = reader.GetString(9);
                        var ownerColumn = reader.GetString(10);

                        TableModel ownerTableModel;
                        ColumnModel ownerColumnModel;
                        if (_tables.TryGetValue(TableKey(ownerTable, ownerSchema), out ownerTableModel) &&
                            _tableColumns.TryGetValue(ColumnKey(ownerTableModel, ownerColumn), out ownerColumnModel) &&
                            ownerColumnModel.Npgsql().IsSerial)
                        {
                            continue;
                        }
                    }

                    var sequence = new SequenceModel
                    {
                        SchemaName = reader.GetString(0),
                        Name = reader.GetString(1),
                        DataType = reader.GetString(2),
                        Start = reader.GetInt64(3),
                        Min = reader.GetInt64(4),
                        Max = reader.GetInt64(5),
                        IncrementBy = reader.GetInt32(6),
                        IsCyclic = reader.GetBoolean(7)
                    };

                    if (sequence.DataType == "bigint")
                    {
                        long defaultStart, defaultMin, defaultMax;
                        if (sequence.IncrementBy > 0)
                        {
                            defaultMin = 1;
                            defaultMax = long.MaxValue;
                            Debug.Assert(sequence.Min.HasValue);
                            defaultStart = sequence.Min.Value;
                        } else {
                            defaultMin = long.MinValue + 1;
                            defaultMax = -1;
                            Debug.Assert(sequence.Max.HasValue);
                            defaultStart = sequence.Max.Value;
                        }
                        if (sequence.Start == defaultStart) {
                            sequence.Start = null;
                        }
                        if (sequence.Min == defaultMin) {
                            sequence.Min = null;
                        }
                        if (sequence.Max == defaultMax) {
                            sequence.Max = null;
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"Sequence with datatype {sequence.DataType} which isn't the expected bigint.");
                    }

                    _databaseModel.Sequences.Add(sequence);
                }
            }
        }
    }
}
