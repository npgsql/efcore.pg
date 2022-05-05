using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

/// <inheritdoc />
public class NpgsqlSingletonOptions : INpgsqlSingletonOptions
{
    public static readonly Version DefaultPostgresVersion = new(12, 0);

    /// <inheritdoc />
    public virtual Version PostgresVersion { get; private set; } = null!;

    /// <inheritdoc />
    public virtual Version? PostgresVersionWithoutDefault { get; private set; }

    /// <inheritdoc />
    public virtual bool UseRedshift { get; private set; }

    /// <inheritdoc />
    public virtual bool ReverseNullOrderingEnabled { get; private set; }

    /// <inheritdoc />
    public virtual IReadOnlyList<UserRangeDefinition> UserRangeDefinitions { get; private set; }

    public NpgsqlSingletonOptions()
        => UserRangeDefinitions = Array.Empty<UserRangeDefinition>();

    /// <inheritdoc />
    public virtual void Initialize(IDbContextOptions options)
    {
        var npgsqlOptions = options.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

        PostgresVersionWithoutDefault = npgsqlOptions.PostgresVersion;
        PostgresVersion = npgsqlOptions.PostgresVersion ?? DefaultPostgresVersion;
        UseRedshift = npgsqlOptions.UseRedshift;
        ReverseNullOrderingEnabled = npgsqlOptions.ReverseNullOrdering;
        UserRangeDefinitions = npgsqlOptions.UserRangeDefinitions;
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        var npgsqlOptions = options.FindExtension<NpgsqlOptionsExtension>() ?? new NpgsqlOptionsExtension();

        if (PostgresVersionWithoutDefault != npgsqlOptions.PostgresVersion)
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

        if (UserRangeDefinitions.Count != npgsqlOptions.UserRangeDefinitions.Count
            || UserRangeDefinitions.Zip(npgsqlOptions.UserRangeDefinitions).Any(t => t.First != t.Second))
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(NpgsqlDbContextOptionsBuilder.MapRange),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
    }
}