namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlDatabase(
    DatabaseDependencies dependencies,
    RelationalDatabaseDependencies relationalDependencies,
    ICurrentDbContext currentContext)
    : RelationalDatabase(dependencies, relationalDependencies)
{
    /// <inheritdoc />
    public override int SaveChanges(IList<IUpdateEntry> entries)
    {
        // TODO: Replace these overrides with an EF hook to normalize store exceptions directly:
        // https://github.com/dotnet/efcore/issues/33068
        try
        {
            return base.SaveChanges(entries);
        }
        catch (Exception ex) when (CreateConcurrencyException(ex, entries) is { } exception)
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            var affectedEntries = exception.Entries.Select(e => e.GetInfrastructure()).ToArray();
#pragma warning restore EF1001
            Dependencies.Logger.OptimisticConcurrencyException(
                currentContext.Context, affectedEntries, exception, createEventData: null);

            // Store failures abort the transaction and cannot be suppressed.
            throw exception;
        }
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(IList<IUpdateEntry> entries, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(entries, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (CreateConcurrencyException(ex, entries) is { } exception)
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            var affectedEntries = exception.Entries.Select(e => e.GetInfrastructure()).ToArray();
#pragma warning restore EF1001
            await Dependencies.Logger.OptimisticConcurrencyExceptionAsync(
                    currentContext.Context, affectedEntries, exception, createEventData: null, cancellationToken)
                .ConfigureAwait(false);

            // Store failures abort the transaction and cannot be suppressed.
            throw exception;
        }
    }

    private static DbUpdateConcurrencyException? CreateConcurrencyException(Exception exception, IList<IUpdateEntry> entries)
    {
        var postgresException = exception switch
        {
            PostgresException e => e,
            DbUpdateException { InnerException: PostgresException e } and not DbUpdateConcurrencyException => e,
            _ => null
        };

        if (postgresException?.SqlState is not (PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected))
        {
            return null;
        }

#pragma warning disable EF1001 // Internal EF Core API usage.
        return new DbUpdateConcurrencyException(
            exception is DbUpdateException ? exception.Message : RelationalStrings.UpdateStoreException,
            postgresException,
            exception is DbUpdateException updateException
                ? updateException.Entries.Select(e => e.GetInfrastructure()).ToArray()
                : entries.ToArray());
#pragma warning restore EF1001
    }
}
