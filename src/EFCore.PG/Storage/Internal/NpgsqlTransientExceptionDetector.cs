using System;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     Detects the exceptions caused by PostgreSQL or network transient failures.
/// </summary>
public class NpgsqlTransientExceptionDetector
{
    public static bool ShouldRetryOn(Exception? ex)
        => (ex as NpgsqlException)?.IsTransient == true || ex is TimeoutException;
}