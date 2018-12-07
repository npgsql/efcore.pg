using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal
{
    /// <inheritdoc />
    public class NpgsqlNetTopologySuiteOptions : INpgsqlNetTopologySuiteOptions
    {
        /// <inheritdoc />
        public bool IsGeographyDefault { get; set; }

        /// <inheritdoc />
        public Ordinates Ordinates { get; set; }

        /// <inheritdoc />
        public void Initialize(IDbContextOptions options)
        {
            var npgsqlNtsOptions = options.FindExtension<NpgsqlNetTopologySuiteOptionsExtension>() ?? new NpgsqlNetTopologySuiteOptionsExtension();

            IsGeographyDefault = npgsqlNtsOptions.IsGeographyDefault;
            Ordinates = npgsqlNtsOptions.Ordinates;
        }

        /// <inheritdoc />
        public void Validate(IDbContextOptions options) {}
    }
}
