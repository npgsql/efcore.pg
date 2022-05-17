namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

/// <summary>
/// Represents options for Npgsql that can only be set at the <see cref="IServiceProvider"/> singleton level.
/// </summary>
public interface INpgsqlSingletonOptions : ISingletonOptions
{
    /// <summary>
    /// The backend version to target.
    /// </summary>
    Version PostgresVersion { get; }

    /// <summary>
    /// The backend version to target, but returns <see langword="null" /> unless the user explicitly specified a version.
    /// </summary>
    Version? PostgresVersionWithoutDefault { get; }

    /// <summary>
    /// Whether to target Redshift.
    /// </summary>
    bool UseRedshift { get; }

    /// <summary>
    /// True if reverse null ordering is enabled; otherwise, false.
    /// </summary>
    bool ReverseNullOrderingEnabled { get; }

    /// <summary>
    /// The collection of range mappings.
    /// </summary>
    IReadOnlyList<UserRangeDefinition> UserRangeDefinitions { get; }
}