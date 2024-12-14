using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlSingletonOptions : INpgsqlSingletonOptions
{
    /// <inheritdoc />
    public virtual Version PostgresVersion { get; private set; } = null!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsPostgresVersionSet { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool UseRedshift { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ReverseNullOrderingEnabled { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<EnumDefinition> EnumDefinitions { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<UserRangeDefinition> UserRangeDefinitions { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlSingletonOptions()
    {
        EnumDefinitions = [];
        UserRangeDefinitions = [];
    }

    /// <inheritdoc />
    public virtual void Initialize(IDbContextOptions options)
    {
        var npgsqlOptions = options.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

        PostgresVersion = npgsqlOptions.PostgresVersion;
        IsPostgresVersionSet = npgsqlOptions.IsPostgresVersionSet;
        UseRedshift = npgsqlOptions.UseRedshift;
        ReverseNullOrderingEnabled = npgsqlOptions.ReverseNullOrdering;
        EnumDefinitions = npgsqlOptions.EnumDefinitions;
        UserRangeDefinitions = npgsqlOptions.UserRangeDefinitions;
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        var npgsqlOptions = options.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

        if (PostgresVersion != npgsqlOptions.PostgresVersion)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(NpgsqlDbContextOptionsBuilder.SetPostgresVersion),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (UseRedshift != npgsqlOptions.UseRedshift)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(NpgsqlDbContextOptionsBuilder.UseRedshift),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (ReverseNullOrderingEnabled != npgsqlOptions.ReverseNullOrdering)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(NpgsqlDbContextOptionsBuilder.ReverseNullOrdering),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (!EnumDefinitions.SequenceEqual(npgsqlOptions.EnumDefinitions))
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(NpgsqlDbContextOptionsBuilder.MapEnum),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (!UserRangeDefinitions.SequenceEqual(npgsqlOptions.UserRangeDefinitions))
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(NpgsqlDbContextOptionsBuilder.MapRange),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
    }
}
