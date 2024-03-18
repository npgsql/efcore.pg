using System.Data;
using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlUpdateSqlGenerator : UpdateSqlGenerator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlUpdateSqlGenerator(UpdateSqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ResultSetMapping AppendInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
        => AppendInsertOperation(commandStringBuilder, command, commandPosition, overridingSystemValue: false, out requiresTransaction);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ResultSetMapping AppendInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        bool overridingSystemValue,
        out bool requiresTransaction)
    {
        var name = command.TableName;
        var schema = command.Schema;
        var operations = command.ColumnModifications;

        var writeOperations = operations.Where(o => o.IsWrite).ToList();
        var readOperations = operations.Where(o => o.IsRead).ToList();

        AppendInsertCommand(commandStringBuilder, name, schema, writeOperations, readOperations, overridingSystemValue);

        requiresTransaction = false;

        return readOperations.Count > 0 ? ResultSetMapping.LastInResultSet : ResultSetMapping.NoResults;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void AppendInsertCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        IReadOnlyList<IColumnModification> writeOperations,
        IReadOnlyList<IColumnModification> readOperations,
        bool overridingSystemValue)
    {
        AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);

        if (overridingSystemValue)
        {
            commandStringBuilder.AppendLine().Append("OVERRIDING SYSTEM VALUE");
        }

        AppendValuesHeader(commandStringBuilder, writeOperations);
        AppendValues(commandStringBuilder, name, schema, writeOperations);
        AppendReturningClause(commandStringBuilder, readOperations);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ResultSetMapping AppendUpdateOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        // The default implementation adds RETURNING 1 to do concurrency check (was the row actually updated), but in PostgreSQL we check
        // the per-statement row-affected value exposed by Npgsql in the batch; so no need for RETURNING 1.
        var name = command.TableName;
        var schema = command.Schema;
        var operations = command.ColumnModifications;

        var writeOperations = operations.Where(o => o.IsWrite).ToList();
        var conditionOperations = operations.Where(o => o.IsCondition).ToList();
        var readOperations = operations.Where(o => o.IsRead).ToList();

        requiresTransaction = false;

        AppendUpdateCommand(commandStringBuilder, name, schema, writeOperations, readOperations, conditionOperations);

        return readOperations.Count > 0 ? ResultSetMapping.LastInResultSet : ResultSetMapping.NoResults;
    }

    /// <inheritdoc />
    protected override void AppendUpdateColumnValue(
        ISqlGenerationHelper updateSqlGeneratorHelper,
        IColumnModification columnModification,
        StringBuilder stringBuilder,
        string name,
        string? schema)
    {
        if (columnModification.JsonPath is not (null or "$"))
        {
            Check.DebugAssert(
                columnModification.TypeMapping is NpgsqlOwnedJsonTypeMapping,
                "ColumnModification with JsonPath but non-NpgsqlOwnedJsonTypeMapping");

            if (columnModification.TypeMapping.StoreType is "json")
            {
                throw new NotSupportedException(
                    "Cannot perform partial update because the PostgreSQL 'json' type has no json_set method. Use 'jsonb' instead.");
            }

            Check.DebugAssert(columnModification.TypeMapping.StoreType is "jsonb", "Non-jsonb type mapping in JSON partial update");

            // TODO: Lax or not?
            stringBuilder
                .Append("jsonb_set(")
                .Append(updateSqlGeneratorHelper.DelimitIdentifier(columnModification.ColumnName))
                .Append(", '{");

            // TODO: Unfortunately JsonPath is provided as a JSONPATH string, but PG's jsonb_set requires the path as an array.
            // Parse the components back out (https://github.com/dotnet/efcore/issues/32185)
            var components = columnModification.JsonPath.Split(".");
            var needsComma = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (needsComma)
                {
                    stringBuilder.Append(',');
                }

                var component = components[i];
                var bracketOpen = component.IndexOf('[');
                if (bracketOpen == -1)
                {
                    if (i > 0) // The first component is $, representing the root
                    {
                        stringBuilder.Append(component);
                        needsComma = true;
                    }

                    continue;
                }

                var propertyName = component[..bracketOpen];
                if (i > 0) // The first component is $, representing the root
                {
                    stringBuilder
                        .Append(propertyName)
                        .Append(',');
                }

                stringBuilder.Append(component[(bracketOpen + 1)..^1]);
                needsComma = true;
            }

            stringBuilder.Append("}', ");

            // TODO: Hack around
            if (columnModification.Value is null)
            {
                _columnModificationValueField ??= typeof(ColumnModification).GetField(
                    "_value", BindingFlags.Instance | BindingFlags.NonPublic)!;
                _columnModificationValueField.SetValue(columnModification, "null");
            }

            base.AppendUpdateColumnValue(updateSqlGeneratorHelper, columnModification, stringBuilder, name, schema);

            stringBuilder.Append(")");
        }
        else
        {
            base.AppendUpdateColumnValue(updateSqlGeneratorHelper, columnModification, stringBuilder, name, schema);
        }
    }

    private FieldInfo? _columnModificationValueField;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ResultSetMapping AppendDeleteOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        // The default implementation adds RETURNING 1 to do concurrency check (was the row actually deleted), but in PostgreSQL we check
        // the per-statement row-affected value exposed by Npgsql in the batch; so no need for RETURNING 1.
        var name = command.TableName;
        var schema = command.Schema;
        var conditionOperations = command.ColumnModifications.Where(o => o.IsCondition).ToList();

        requiresTransaction = false;

        AppendDeleteCommand(commandStringBuilder, name, schema, [], conditionOperations);

        return ResultSetMapping.NoResults;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ResultSetMapping AppendStoredProcedureCall(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        Check.DebugAssert(command.StoreStoredProcedure is not null, "command.StoreStoredProcedure is not null");

        var storedProcedure = command.StoreStoredProcedure;

        Check.DebugAssert(
            storedProcedure.Parameters.Any() || storedProcedure.ResultColumns.Any(),
            "Stored procedure call with neither parameters nor result columns");

        var resultSetMapping = ResultSetMapping.NoResults;

        commandStringBuilder.Append("CALL ");

        // PostgreSQL supports neither a return value nor a result set with stored procedures, only output parameters.
        Check.DebugAssert(storedProcedure.ReturnValue is null, "storedProcedure.Return is null");
        Check.DebugAssert(!storedProcedure.ResultColumns.Any(), "!storedProcedure.ResultColumns.Any()");

        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, storedProcedure.Name, storedProcedure.Schema);

        commandStringBuilder.Append('(');

        var first = true;

        // Only positional parameter style supported for now, see https://github.com/dotnet/efcore/issues/28439

        // Note: the column modifications are already ordered according to the sproc parameter ordering
        // (see ModificationCommand.GenerateColumnModifications)
        for (var i = 0; i < command.ColumnModifications.Count; i++)
        {
            var columnModification = command.ColumnModifications[i];
            var parameter = (IStoreStoredProcedureParameter)columnModification.Column!;

            if (first)
            {
                first = false;
            }
            else
            {
                commandStringBuilder.Append(", ");
            }

            Check.DebugAssert(columnModification.UseParameter, "Column modification matched a parameter, but UseParameter is false");

            if (parameter.Direction == ParameterDirection.Output)
            {
                // Recommended PG practice is to pass NULL for output parameters
                commandStringBuilder.Append("NULL");
            }
            else
            {
                SqlGenerationHelper.GenerateParameterNamePlaceholder(
                    commandStringBuilder, columnModification.UseOriginalValueParameter
                        ? columnModification.OriginalParameterName!
                        : columnModification.ParameterName!);
            }

            // PostgreSQL stored procedures cannot return a regular result set, and output parameter values are simply sent back as the
            // result set; this is very different from SQL Server, where output parameter values can be sent back in addition to result
            // sets.
            if (parameter.Direction.HasFlag(ParameterDirection.Output))
            {
                // The distinction between having only a rows affected output parameter and having other non-rows affected parameters
                // is important later on (i.e. whether we need to propagate or not).
                resultSetMapping = parameter == command.RowsAffectedColumn && resultSetMapping == ResultSetMapping.NoResults
                    ? ResultSetMapping.ResultSetWithRowsAffectedOnly
                    : ResultSetMapping.LastInResultSet;
            }
        }

        commandStringBuilder.Append(')');

        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

        requiresTransaction = true;

        return resultSetMapping;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void AppendObtainNextSequenceValueOperation(StringBuilder commandStringBuilder, string name, string? schema)
    {
        commandStringBuilder.Append("nextval('");
        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, Check.NotNull(name, nameof(name)), schema);
        commandStringBuilder.Append("')");
    }
}
