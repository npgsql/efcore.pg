using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

/// <inheritdoc />
public class NpgsqlNetTopologySuiteOptions : INpgsqlNetTopologySuiteOptions
{
    /// <inheritdoc />
    public virtual bool IsGeographyDefault { get; set; }

    /// <inheritdoc />
    public virtual void Initialize(IDbContextOptions options)
    {
        var npgsqlNtsOptions = options.FindExtension<NpgsqlNetTopologySuiteOptionsExtension>() ?? new NpgsqlNetTopologySuiteOptionsExtension();

        IsGeographyDefault = npgsqlNtsOptions.IsGeographyDefault;
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options) {}
}