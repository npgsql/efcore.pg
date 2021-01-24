using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal
{
    public class NpgsqlUpdateSqlGenerator : UpdateSqlGenerator
    {
        public NpgsqlUpdateSqlGenerator([NotNull] UpdateSqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override ResultSetMapping AppendInsertOperation(
            StringBuilder commandStringBuilder,
            ModificationCommand command,
            int commandPosition)
            => AppendInsertOperation(commandStringBuilder, command, commandPosition, false);

        public virtual ResultSetMapping AppendInsertOperation(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] ModificationCommand command,
            int commandPosition,
            bool overridingSystemValue)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            AppendInsertCommandHeader(commandStringBuilder, command.TableName, command.Schema, writeOperations);
            if (overridingSystemValue)
                commandStringBuilder.AppendLine().Append("OVERRIDING SYSTEM VALUE");
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, name, schema, writeOperations);
            if (readOperations.Length > 0)
                AppendReturningClause(commandStringBuilder, readOperations);

            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();

            return ResultSetMapping.NoResultSet;
        }

        public override ResultSetMapping AppendUpdateOperation(
            StringBuilder commandStringBuilder,
            ModificationCommand command,
            int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var tableName = command.TableName;
            var schemaName = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            AppendUpdateCommandHeader(commandStringBuilder, tableName, schemaName, writeOperations);
            AppendWhereClause(commandStringBuilder, conditionOperations);
            if (readOperations.Length > 0)
            {
                AppendReturningClause(commandStringBuilder, readOperations);
            }
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
            return ResultSetMapping.NoResultSet;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private void AppendReturningClause(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations)
        {
            commandStringBuilder
                .AppendLine()
                .Append("RETURNING ")
                .AppendJoin(operations.Select(c => SqlGenerationHelper.DelimitIdentifier(c.ColumnName)));
        }

        public override void AppendNextSequenceValueOperation(StringBuilder commandStringBuilder, string name, string schema)
        {
            commandStringBuilder.Append("SELECT nextval('");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, Check.NotNull(name, nameof(name)), schema);
            commandStringBuilder.Append("')");
        }

        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));

            // TODO: Npgsql
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            throw new NotImplementedException();
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        {
            throw new NotImplementedException();
        }

        public enum ResultsGrouping
        {
            OneResultSet,
            OneCommandPerResultSet
        }
    }
}
