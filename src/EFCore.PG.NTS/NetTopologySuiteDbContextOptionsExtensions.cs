using GeoAPI;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NetTopologySuiteDbContextOptionsExtensions
    {
        public static NpgsqlDbContextOptionsBuilder UseNetTopologySuite(
            [NotNull] this NpgsqlDbContextOptionsBuilder optionsBuilder,
            ICoordinateSequenceFactory coordinateSequenceFactory = null,
            IPrecisionModel precisionModel = null,
            Ordinates handleOrdinates = Ordinates.None)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            // TODO: Global-only setup at the ADO.NET level for now, optionally allow per-connection?
            NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite(coordinateSequenceFactory, precisionModel, handleOrdinates);

            optionsBuilder.UsePlugin(new NetTopologySuitePlugin());

            return optionsBuilder;
        }
    }
}
