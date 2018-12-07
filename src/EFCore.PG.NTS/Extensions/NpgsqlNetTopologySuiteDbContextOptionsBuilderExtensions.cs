using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// NetTopologySuite specific extension methods for <see cref="NpgsqlDbContextOptionsBuilder"/>.
    /// </summary>
    public static class NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Use NetTopologySuite to access SQL Server spatial data.
        /// </summary>
        /// <returns>
        /// The options builder so that further configuration can be chained.
        /// </returns>
        public static NpgsqlDbContextOptionsBuilder UseNetTopologySuite(
            [NotNull] this NpgsqlDbContextOptionsBuilder optionsBuilder,
            ICoordinateSequenceFactory coordinateSequenceFactory = null,
            IPrecisionModel precisionModel = null,
            Ordinates handleOrdinates = Ordinates.XY,
            bool geographyAsDefault = false)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            // TODO: Global-only setup at the ADO.NET level for now, optionally allow per-connection?
            NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite(coordinateSequenceFactory, precisionModel, handleOrdinates, geographyAsDefault);

            var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

            var extension = coreOptionsBuilder.Options.FindExtension<NpgsqlNetTopologySuiteOptionsExtension>()
                            ?? new NpgsqlNetTopologySuiteOptionsExtension();

            if (geographyAsDefault)
                extension = extension.WithGeographyDefault();

            extension = extension.WithOrdinates(handleOrdinates);

            ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}
