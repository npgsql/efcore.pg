using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using static Npgsql.EntityFrameworkCore.PostgreSQL.NodaTimeTest;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class BuiltInDataTypesWithNodaTimePluginEnabledTest : BuiltInDataTypesNpgsqlTestBase<BuiltInDataTypesWithNodaTimePluginEnabledTest.BuiltInDataTypesWithNodaTimePluginEnabledFixture>
    {
        public BuiltInDataTypesWithNodaTimePluginEnabledTest(BuiltInDataTypesWithNodaTimePluginEnabledFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        public class BuiltInDataTypesWithNodaTimePluginEnabledFixture : BuiltInDataTypesNpgsqlTestBaseFixture
        {
            protected override string StoreName => "BuiltInDataTypesWithNodaTimeEnabled";

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection).AddEntityFrameworkNpgsqlNodaTime();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var optionsBuilder = base.AddOptions(builder);
                new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNodaTime();

                return optionsBuilder;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);
                modelBuilder.Entity<NodaTimeTypes>();
            }
        }
    }
}
