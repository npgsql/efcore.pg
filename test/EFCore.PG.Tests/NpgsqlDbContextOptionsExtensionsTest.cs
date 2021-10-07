using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlDbContextOptionsExtensionsTest
    {
        [ConditionalFact]
        public void Can_add_extension_with_max_batch_size()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql("Database=Crunchie", b => b.MaxBatchSize(123));

            var extension = optionsBuilder.Options.Extensions.OfType<NpgsqlOptionsExtension>().Single();

            Assert.Equal(123, extension.MaxBatchSize);
        }

        [ConditionalFact]
        public void Can_add_extension_with_command_timeout()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql("Database=Crunchie", b => b.CommandTimeout(30));

            var extension = optionsBuilder.Options.Extensions.OfType<NpgsqlOptionsExtension>().Single();

            Assert.Equal(30, extension.CommandTimeout);
        }

        [ConditionalFact]
        public void Can_add_extension_with_connection_string()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql("Database=Crunchie");

            var extension = optionsBuilder.Options.Extensions.OfType<NpgsqlOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [ConditionalFact]
        public void Can_add_extension_with_connection_string_using_generic_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseNpgsql("Database=Whisper");

            var extension = optionsBuilder.Options.Extensions.OfType<NpgsqlOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [ConditionalFact]
        public void Can_add_extension_with_connection()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            var connection = new NpgsqlConnection();

            optionsBuilder.UseNpgsql(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<NpgsqlOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [ConditionalFact]
        public void Can_add_extension_with_connection_using_generic_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            var connection = new NpgsqlConnection();

            optionsBuilder.UseNpgsql(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<NpgsqlOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [ConditionalFact]
        public void Service_collection_extension_method_can_configure_npgsql_options()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddNpgsql<ApplicationDbContext>(
                "Database=Crunchie",
                NpgsqlOption =>
                {
                    NpgsqlOption.MaxBatchSize(123);
                    NpgsqlOption.CommandTimeout(30);
                },
                dbContextOption =>
                {
                    dbContextOption.EnableDetailedErrors(true);
                });

            var services = serviceCollection.BuildServiceProvider();

            using (var serviceScope = services
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var coreOptions = serviceScope.ServiceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>().GetExtension<CoreOptionsExtension>();
                Assert.True(coreOptions.DetailedErrorsEnabled);

                var NpgsqlOptions = serviceScope.ServiceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>().GetExtension<NpgsqlOptionsExtension>();
                Assert.Equal(123, NpgsqlOptions.MaxBatchSize);
                Assert.Equal(30, NpgsqlOptions.CommandTimeout);
                Assert.Equal("Database=Crunchie", NpgsqlOptions.ConnectionString);
            }
        }

        private class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions options)
                   : base(options)
            {
            }
        }
    }
}
