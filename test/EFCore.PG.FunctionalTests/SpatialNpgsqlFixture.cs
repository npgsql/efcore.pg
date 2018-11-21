using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class SpatialNpgsqlFixture : SpatialFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                   .AddEntityFrameworkNpgsqlNetTopologySuite();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);
            new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite();

            return optionsBuilder;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.HasPostgresExtension("postgis")
                        .HasPostgresExtension("uuid-ossp");

            modelBuilder.Entity<PointEntity>()
                        .Property(e => e.Point)
                        .ForNpgsqlHasSpatialType("POINT");

            modelBuilder.Entity<PointEntity>()
                        .Property(e => e.ConcretePoint)
                        .ForNpgsqlHasSpatialType("POINT")
                        .ForNpgsqlHasSrid(0);

            modelBuilder.Entity<LineStringEntity>()
                        .Property(e => e.LineString)
                        .ForNpgsqlHasSpatialType("LINESTRING");

            modelBuilder.Entity<MultiLineStringEntity>()
                        .Property(e => e.MultiLineString)
                        .ForNpgsqlHasSpatialType("MULTILINESTRING")
                        .ForNpgsqlHasSrid(0);
        }
    }
}
