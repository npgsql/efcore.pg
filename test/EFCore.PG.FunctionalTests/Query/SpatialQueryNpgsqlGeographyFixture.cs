using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class SpatialQueryNpgsqlGeographyFixture : SpatialQueryRelationalFixture
    {
        NtsGeometryServices _geometryServices;
        GeometryFactory _geometryFactory;

        public NtsGeometryServices GeometryServices
            => LazyInitializer.EnsureInitialized(
                ref _geometryServices,
                () => new NtsGeometryServices(
                    NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory,
                    NtsGeometryServices.Instance.DefaultPrecisionModel,
                    4326));

        public override GeometryFactory GeometryFactory
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
        }
    }
}
