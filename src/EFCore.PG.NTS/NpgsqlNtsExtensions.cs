using GeoAPI;
using JetBrains.Annotations;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlNtsDbContextOptionsExtensions
    {
        public static NpgsqlDbContextOptionsBuilder UseNetTopologySuite(
            [NotNull] this NpgsqlDbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            // TODO: This should probably be in the ADO.NET plugin?
            NetTopologySuiteBootstrapper.Bootstrap();

            // TODO: Global-only setup at the ADO.NET level for now, optionally allow per-connection?
            NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();

            optionsBuilder.UsePlugin(new NpgsqlNetTopologySuitePlugin());

            return optionsBuilder;
        }
    }
}
