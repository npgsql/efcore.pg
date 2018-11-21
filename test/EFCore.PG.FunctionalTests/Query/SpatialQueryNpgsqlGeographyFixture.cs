using System.Threading;
using GeoAPI;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class SpatialQueryNpgsqlGeographyFixture : SpatialQueryRelationalFixture
    {
        IGeometryServices _geometryServices;
        IGeometryFactory _geometryFactory;

        public IGeometryServices GeometryServices
            => LazyInitializer.EnsureInitialized(
                ref _geometryServices,
                () => new NtsGeometryServices(
                    NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory,
                    NtsGeometryServices.Instance.DefaultPrecisionModel,
                    4326));

        public override IGeometryFactory GeometryFactory
            => LazyInitializer.EnsureInitialized(
                ref _geometryFactory,
                () => GeometryServices.CreateGeometryFactory());

        protected override string StoreName
            => "SpatialQueryGeographyTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        {
            NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();

            return base.AddServices(serviceCollection)
                       .AddEntityFrameworkNpgsqlNetTopologySuite();
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);
            new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite(null, null, Ordinates.None, true);

            return optionsBuilder;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.HasPostgresExtension("postgis");

            modelBuilder.Entity<PointEntity>()
                        .Property(e => e.Point)
                        .ForNpgsqlHasSpatialType("POINT");

            modelBuilder.Entity<PointEntity>()
                        .Property(e => e.ConcretePoint)
                        .ForNpgsqlHasSpatialType("POINT")
                        .ForNpgsqlHasSrid(4326);

            modelBuilder.Entity<LineStringEntity>()
                        .Property(e => e.LineString)
                        .ForNpgsqlHasSpatialType("LINESTRING");

            modelBuilder.Entity<MultiLineStringEntity>()
                        .Property(e => e.MultiLineString)
                        .ForNpgsqlHasSpatialType("MULTILINESTRING")
                        .ForNpgsqlHasSrid(4326);
        }
    }
}
