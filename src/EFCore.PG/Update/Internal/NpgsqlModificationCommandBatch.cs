using System.Data;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;

/// <summary>
/// The Npgsql-specific implementation for <see cref="ModificationCommandBatch" />.
/// </summary>
/// <remarks>
/// The usual ModificationCommandBatch implementation is <see cref="AffectedCountModificationCommandBatch"/>,
/// which selects the number of rows modified via a SQL query.
///
/// PostgreSQL actually has no way of selecting the modified row count.
/// SQL defines GET DIAGNOSTICS which should provide this, but in PostgreSQL it's only available
/// in PL/pgSQL. See http://www.postgresql.org/docs/9.4/static/unsupported-features-sql-standard.html,
/// identifier F121-01.
///
/// Instead, the affected row count can be accessed in the PostgreSQL protocol itself, which seems
/// cleaner and more efficient anyway (no additional query).
/// </remarks>
public class NpgsqlModificationCommandBatch : ReaderModificationCommandBatch
{
    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlModificationCommandBatch"/> class.
    /// </summary>
    public NpgsqlModificationCommandBatch(
        ModificationCommandBatchFactoryDependencies dependencies,
        int maxBatchSize)
        : base(dependencies)
        => MaxBatchSize = maxBatchSize;

    /// <summary>
    ///     The maximum number of <see cref="ModificationCommand"/> instances that can be added to a single batch; defaults to 1000.
    /// </summary>
    protected override int MaxBatchSize { get; }


    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void AddParameter(IColumnModification columnModification)
    {
        // PostgreSQL stored procedures cannot return a regular result set, and output parameter values are simply sent back as the
        // result set; this is very different from SQL Server, where output parameter values can be sent back in addition to result
        // sets. So we avoid adding NpgsqlParameters for output parameters - we'll just retrieve and propagate the values below when
        // consuming the result set.
        if (columnModification.Column is IStoreStoredProcedureParameter { Direction: ParameterDirection.Output })
        {
            return;
        }

        base.AddParameter(columnModification);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void Consume(RelationalDataReader reader)
        => Consume(reader, async: false).GetAwaiter().GetResult();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Task ConsumeAsync(RelationalDataReader reader, CancellationToken cancellationToken = default)
        => Consume(reader, async: true, cancellationToken);

    private async Task Consume(RelationalDataReader reader, bool async, CancellationToken cancellationToken = default)
    {
        var npgsqlReader = (NpgsqlDataReader)reader.DbDataReader;

#pragma warning disable 618
        Debug.Assert(npgsqlReader.Statements.Count == ModificationCommands.Count, $"Reader has {npgsqlReader.Statements.Count} statements, expected {ModificationCommands.Count}");
#pragma warning restore 618

        var commandIndex = 0;

        try
        {
            bool? onResultSet = null;
            while (commandIndex < ModificationCommands.Count)
            {
                var command = ModificationCommands[commandIndex];

                // Note that in the PG provider, we never transmit rows affected via the result set (except with stored procedures);
                // it's transmitted separately via the PG wire protocol and exposed on the reader (see below).
                // As a result, if there's a result set we know that it contains values to be propagated back into the entity instance.
                if (ResultSetMappings[commandIndex].HasFlag(ResultSetMapping.HasResultRow))
                {
                    if (async)
                    {
                        if (!(await reader.ReadAsync(cancellationToken).ConfigureAwait(false)))
                        {
                            await ThrowAggregateUpdateConcurrencyExceptionAsync(reader, commandIndex, 1, 0, cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        if (!reader.Read())
                        {
                            ThrowAggregateUpdateConcurrencyException(reader, commandIndex, 1, 0);
                        }
                    }


                    if (command.RowsAffectedColumn is { } rowsAffectedColumn)
                    {
                        // Only stored procedures have rows affected, as an output parameter. For regular commands, the rows affected gets
                        // transferred via the Post
                        Debug.Assert(command.StoreStoredProcedure is not null);

                        var rowsAffectedParameter = (IStoreStoredProcedureParameter)rowsAffectedColumn;
                        Debug.Assert(rowsAffectedParameter.Direction == ParameterDirection.Output);

                        var readerIndex = -1;

                        for (var i = 0; i < command.ColumnModifications.Count; i++)
                        {
                            var columnModification = command.ColumnModifications[i];
                            if (columnModification.Column is IStoreStoredProcedureParameter
                                {
                                    Direction: ParameterDirection.Output or ParameterDirection.InputOutput
                                })
                            {
                                readerIndex++;
                            }

                            if (columnModification.Column == rowsAffectedColumn)
                            {
                                break;
                            }
                        }

                        if (reader.DbDataReader.GetInt32(readerIndex) != 1)
                        {
                            await ThrowAggregateUpdateConcurrencyExceptionAsync(reader, commandIndex, 1, 0, cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }

                    command.PropagateResults(reader);

                    commandIndex++;

                    onResultSet = async
                        ? await npgsqlReader.NextResultAsync(cancellationToken).ConfigureAwait(false)
                        : npgsqlReader.NextResult();

                }
                else
                {
                    Debug.Assert(ResultSetMappings[commandIndex] == ResultSetMapping.NoResults);

                    // With stored procedures, either the rows affected is returned via an output parameter (and is therefore a result
                    // column and handled above), or there's no rows affected and we skip the check entirely.
                    // Without stored procedures, PG transmits the rows affected for each statement in the CommandComplete message of the
                    // protocol, and we always just check that.
                    // TODO: when EF Core adds support for DbBatch (https://github.com/dotnet/efcore/issues/18990), we can start using that
                    // standardized API for fetching the rows affected by an individual command in a batch.
#pragma warning disable 618
                    if (npgsqlReader.Statements[commandIndex].Rows != 1 && command.StoreStoredProcedure is null)
                    {
                        if (async)
                        {
                            await ThrowAggregateUpdateConcurrencyExceptionAsync(reader, commandIndex, 1, 0, cancellationToken)
                                .ConfigureAwait(false);
                        }
                        else
                        {
                            ThrowAggregateUpdateConcurrencyException(reader, commandIndex, 1, 0);
                        }
                    }
#pragma warning restore 618
                    commandIndex++;
                }
            }

            if (onResultSet == true)
            {
                Dependencies.UpdateLogger.UnexpectedTrailingResultSetWhenSaving();
            }
        }
        catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands[commandIndex].Entries);
        }
    }

    private IReadOnlyList<IUpdateEntry> AggregateEntries(int endIndex, int commandCount)
    {
        var entries = new List<IUpdateEntry>();
        for (var i = endIndex - commandCount; i < endIndex; i++)
        {
            entries.AddRange(ModificationCommands[i].Entries);
        }

        return entries;
    }

    /// <summary>
    ///     Throws an exception indicating the command affected an unexpected number of rows.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="commandIndex">The ordinal of the command.</param>
    /// <param name="expectedRowsAffected">The expected number of rows affected.</param>
    /// <param name="rowsAffected">The actual number of rows affected.</param>
    protected virtual void ThrowAggregateUpdateConcurrencyException(
        RelationalDataReader reader,
        int commandIndex,
        int expectedRowsAffected,
        int rowsAffected)
    {
        var entries = AggregateEntries(commandIndex + 1, expectedRowsAffected);
        var exception = new DbUpdateConcurrencyException(
            RelationalStrings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
            entries);

        if (!Dependencies.UpdateLogger.OptimisticConcurrencyException(
                Dependencies.CurrentContext.Context,
                entries,
                exception,
                (c, ex, e, d) => CreateConcurrencyExceptionEventData(c, reader, ex, e, d)).IsSuppressed)
        {
            throw exception;
        }
    }

    /// <summary>
    ///     Throws an exception indicating the command affected an unexpected number of rows.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="commandIndex">The ordinal of the command.</param>
    /// <param name="expectedRowsAffected">The expected number of rows affected.</param>
    /// <param name="rowsAffected">The actual number of rows affected.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns> A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected virtual async Task ThrowAggregateUpdateConcurrencyExceptionAsync(
        RelationalDataReader reader,
        int commandIndex,
        int expectedRowsAffected,
        int rowsAffected,
        CancellationToken cancellationToken)
    {
        var entries = AggregateEntries(commandIndex + 1, expectedRowsAffected);
        var exception = new DbUpdateConcurrencyException(
            RelationalStrings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
            entries);

        if (!(await Dependencies.UpdateLogger.OptimisticConcurrencyExceptionAsync(
                    Dependencies.CurrentContext.Context,
                    entries,
                    exception,
                    (c, ex, e, d) => CreateConcurrencyExceptionEventData(c, reader, ex, e, d),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)).IsSuppressed)
        {
            throw exception;
        }
    }

    private static RelationalConcurrencyExceptionEventData CreateConcurrencyExceptionEventData(
        DbContext context,
        RelationalDataReader reader,
        DbUpdateConcurrencyException exception,
        IReadOnlyList<IUpdateEntry> entries,
        EventDefinition<Exception> definition)
        => new(
            definition,
            (definition1, payload)
                => ((EventDefinition<Exception>)definition1).GenerateMessage(((ConcurrencyExceptionEventData)payload).Exception),
            context,
            reader.RelationalConnection.DbConnection,
            reader.DbCommand,
            reader.DbDataReader,
            reader.CommandId,
            reader.RelationalConnection.ConnectionId,
            entries,
            exception);
}
