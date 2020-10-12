using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class BuiltInDataTypesWithNodaTimePluginEnabledTest : BuiltInDataTypesNpgsqlTest,
        IClassFixture<BuiltInDataTypesWithNodaTimePluginEnabledTest.BuiltInDataTypesWithNodaTimePluginEnabledFixture>
    {
        public BuiltInDataTypesWithNodaTimePluginEnabledTest(BuiltInDataTypesWithNodaTimePluginEnabledFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        { }

        public class BuiltInDataTypesWithNodaTimePluginEnabledFixture : BuiltInDataTypesNpgsqlFixture
        {
            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection).AddEntityFrameworkNpgsqlNodaTime();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var optionsBuilder = base.AddOptions(builder);
                new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNodaTime();

                return optionsBuilder;
            }
        }
    }
}
