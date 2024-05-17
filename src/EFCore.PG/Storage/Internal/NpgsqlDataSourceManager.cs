using System.Data.Common;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     Manages resolving and creating <see cref="NpgsqlDataSource" /> instances.
/// </summary>
/// <remarks>
///     <para>
///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///         the same compatibility standards as public APIs. It may be changed or removed without notice in
///         any release. You should only use it directly in your code with extreme caution and knowing that
///         doing so can result in application failures when updating to a new Entity Framework Core release.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class NpgsqlDataSourceManager : IDisposable, IAsyncDisposable
{
    private bool _isInitialized;
    private string? _connectionString;
    private readonly IEnumerable<INpgsqlDataSourceConfigurationPlugin> _plugins;
    private NpgsqlDataSource? _dataSource;
    private readonly object _lock = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlDataSourceManager(IEnumerable<INpgsqlDataSourceConfigurationPlugin> plugins)
        => _plugins = plugins.ToArray();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbDataSource? GetDataSource(NpgsqlOptionsExtension? npgsqlOptionsExtension, IServiceProvider? applicationServiceProvider)
        => npgsqlOptionsExtension switch
        {
            // If the user has explicitly passed in a data source via UseNpgsql(), use that.
            // Note that in this case, the data source is scoped (not singleton), and so can change between different
            // DbContext instances using the same internal service provider.
            { DataSource: DbDataSource dataSource } => dataSource,

            // If the user has passed in a DbConnection, never use a data source - even if e.g. MapEnum() was called.
            // This is to avoid blocking and allow continuing using enums in conjunction with DbConnections (which
            // must be manually set up by the user for the enum, of course).
            { Connection: not null } => null,

            // If the user hasn't configured anything in UseNpgsql (no data source, no connection, no connection string), check the
            // application service provider to see if a data source is registered there, and return that.
            { ConnectionString: null } when applicationServiceProvider?.GetService<NpgsqlDataSource>() is DbDataSource dataSource
                => dataSource,

            // Otherwise if there's no connection string, abort: a connection string is required to create a data source in any case.
            { ConnectionString: null } or null => null,

            // The following are features which require an NpgsqlDataSource, since they require configuration on NpgsqlDataSourceBuilder.
            { EnumDefinitions.Count: > 0 } => GetSingletonDataSource(npgsqlOptionsExtension, "MapEnum"),
            _ when _plugins.Any() => GetSingletonDataSource(npgsqlOptionsExtension, _plugins.First().GetType().Name),

            // If there's no configured feature which requires us to use a data source internally, don't use one; this causes
            // NpgsqlRelationalConnection to use the connection string as before (no data source), allowing switching connection strings
            // with the same service provider etc.
            _ => null
        };

    private DbDataSource GetSingletonDataSource(NpgsqlOptionsExtension npgsqlOptionsExtension, string dataSourceFeature)
    {
        if (!_isInitialized)
        {
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    _dataSource = CreateSingletonDataSource(npgsqlOptionsExtension);
                    _connectionString = npgsqlOptionsExtension.ConnectionString;
                    _isInitialized = true;
                    return _dataSource;
                }
            }
        }

        Check.DebugAssert(_dataSource is not null, "_dataSource cannot be null at this point");

        if (_connectionString != npgsqlOptionsExtension.ConnectionString)
        {
            throw new InvalidOperationException(NpgsqlStrings.DataSourceWithMultipleConnectionStrings(dataSourceFeature));
        }

        return _dataSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual NpgsqlDataSource CreateSingletonDataSource(NpgsqlOptionsExtension npgsqlOptionsExtension)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(npgsqlOptionsExtension.ConnectionString);

        foreach (var enumDefinition in npgsqlOptionsExtension.EnumDefinitions)
        {
            dataSourceBuilder.MapEnum(
                enumDefinition.ClrType,
                enumDefinition.StoreTypeSchema is null
                    ? enumDefinition.StoreTypeName
                    : enumDefinition.StoreTypeSchema + "." + enumDefinition.StoreTypeName,
                enumDefinition.NameTranslator);
        }

        foreach (var plugin in _plugins)
        {
            plugin.Configure(dataSourceBuilder);
        }

        // Legacy authentication-related callbacks at the EF level; apply these when building a data source as well.
        if (npgsqlOptionsExtension.ProvideClientCertificatesCallback is not null)
        {
            dataSourceBuilder.UseClientCertificatesCallback(x => npgsqlOptionsExtension.ProvideClientCertificatesCallback(x));
        }

        if (npgsqlOptionsExtension.RemoteCertificateValidationCallback is not null)
        {
            dataSourceBuilder.UseUserCertificateValidationCallback(npgsqlOptionsExtension.RemoteCertificateValidationCallback);
        }

        return dataSourceBuilder.Build();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Dispose()
        => _dataSource?.Dispose();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_dataSource != null)
        {
            await _dataSource.DisposeAsync().ConfigureAwait(false);
        }
    }
}
