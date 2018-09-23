using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal
{
    /// <summary>
    /// Represents options for Npgsql NetTopologySuite that can only be set at the <see cref="IServiceProvider"/> singleton level.
    /// </summary>
    public interface INpgsqlNetTopologySuiteOptions : ISingletonOptions
    {
        /// <summary>
        /// True if geography is to be used by default instead of geometry
        /// </summary>
        bool IsGeographyDefault { get; }
    }
}
