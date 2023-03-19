using System.Data.Common;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

/// <summary>
///     Represents options for Npgsql that can only be set at the <see cref="IServiceProvider"/> singleton level.
/// </summary>
public interface INpgsqlSingletonOptions : ISingletonOptions
{
    /// <summary>
    ///     The backend version to target.
    /// </summary>
    Version PostgresVersion { get; }

    /// <summary>
    ///     Whether the user has explicitly set the backend version to target.
    /// </summary>
    bool IsPostgresVersionSet { get; }

    /// <summary>
    ///     Whether to target Redshift.
    /// </summary>
    bool UseRedshift { get; }

    /// <summary>
    ///     Whether reverse null ordering is enabled.
    /// </summary>
    bool ReverseNullOrderingEnabled { get; }

    /// <summary>
    ///     The data source being used, or <see langword="null" /> if a connection string or connection was provided directly.
    /// </summary>
    DbDataSource? DataSource { get; }

    /// <summary>
    ///     The collection of range mappings.
    /// </summary>
    IReadOnlyList<UserRangeDefinition> UserRangeDefinitions { get; }

    /// <summary>
    ///     The root service provider for the application, if available. />.
    /// </summary>
    IServiceProvider? ApplicationServiceProvider { get; }
}
