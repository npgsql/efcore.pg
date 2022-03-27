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

    /// <inheritdoc />
    protected override bool IsValid()
        => ParameterValues.Count <= ushort.MaxValue;

    protected override void Consume(RelationalDataReader reader)
    {
        var npgsqlReader = (NpgsqlDataReader)reader.DbDataReader;

#pragma warning disable 618
        Debug.Assert(npgsqlReader.Statements.Count == ModificationCommands.Count, $"Reader has {npgsqlReader.Statements.Count} statements, expected {ModificationCommands.Count}");
#pragma warning restore 618

        var commandIndex = 0;

        try
        {
            while (true)
            {
                // Find the next propagating command, if any
                int nextPropagating;
                for (nextPropagating = commandIndex;
                     nextPropagating < ModificationCommands.Count &&
                     !ModificationCommands[nextPropagating].RequiresResultPropagation;
                     nextPropagating++)
                {
                }

                // Go over all non-propagating commands before the next propagating one,
                // make sure they executed
                for (; commandIndex < nextPropagating; commandIndex++)
                {
#pragma warning disable 618
                    if (npgsqlReader.Statements[commandIndex].Rows == 0)
                    {
                        throw new DbUpdateConcurrencyException(
                            RelationalStrings.UpdateConcurrencyException(1, 0),
                            ModificationCommands[commandIndex].Entries
                        );
                    }
#pragma warning restore 618
                }

                if (nextPropagating == ModificationCommands.Count)
                {
                    Debug.Assert(!npgsqlReader.NextResult(), "Expected less resultsets");
                    break;
                }

                // Propagate to results from the reader to the ModificationCommand

                var modificationCommand = ModificationCommands[commandIndex++];

                if (!reader.Read())
                {
                    throw new DbUpdateConcurrencyException(
                        RelationalStrings.UpdateConcurrencyException(1, 0),
                        modificationCommand.Entries);
                }

                var valueBufferFactory = CreateValueBufferFactory(modificationCommand.ColumnModifications);
                modificationCommand.PropagateResults(valueBufferFactory.Create(npgsqlReader));

                npgsqlReader.NextResult();
            }
        }
        catch (DbUpdateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands[commandIndex].Entries);
        }
    }

    protected override async Task ConsumeAsync(
        RelationalDataReader reader,
        CancellationToken cancellationToken = default)
    {
        var npgsqlReader = (NpgsqlDataReader)reader.DbDataReader;

#pragma warning disable 618
        Debug.Assert(npgsqlReader.Statements.Count == ModificationCommands.Count, $"Reader has {npgsqlReader.Statements.Count} statements, expected {ModificationCommands.Count}");
#pragma warning restore 618

        var commandIndex = 0;

        try
        {
            while (true)
            {
                // Find the next propagating command, if any
                int nextPropagating;
                for (nextPropagating = commandIndex;
                     nextPropagating < ModificationCommands.Count &&
                     !ModificationCommands[nextPropagating].RequiresResultPropagation;
                     nextPropagating++)
                {
                }

                // Go over all non-propagating commands before the next propagating one,
                // make sure they executed
                for (; commandIndex < nextPropagating; commandIndex++)
                {
#pragma warning disable 618
                    if (npgsqlReader.Statements[commandIndex].Rows == 0)
                    {
                        throw new DbUpdateConcurrencyException(
                            RelationalStrings.UpdateConcurrencyException(1, 0),
                            ModificationCommands[commandIndex].Entries
                        );
                    }
#pragma warning restore 618
                }

                if (nextPropagating == ModificationCommands.Count)
                {
                    Debug.Assert(!(await npgsqlReader.NextResultAsync(cancellationToken).ConfigureAwait(false)), "Expected less resultsets");
                    break;
                }

                // Extract result from the command and propagate it

                var modificationCommand = ModificationCommands[commandIndex++];

                if (!(await reader.ReadAsync(cancellationToken).ConfigureAwait(false)))
                {
                    throw new DbUpdateConcurrencyException(
                        RelationalStrings.UpdateConcurrencyException(1, 0),
                        modificationCommand.Entries
                    );
                }

                var valueBufferFactory = CreateValueBufferFactory(modificationCommand.ColumnModifications);
                modificationCommand.PropagateResults(valueBufferFactory.Create(npgsqlReader));

                await npgsqlReader.NextResultAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (DbUpdateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands[commandIndex].Entries);
        }
    }
}
