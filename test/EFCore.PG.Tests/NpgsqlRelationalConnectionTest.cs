using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Xunit;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlRelationalConnectionTest
    {
        [Fact]
        public void Creates_Npgsql_Server_connection_string()
        {
            using var connection = new NpgsqlRelationalConnection(CreateDependencies());

            Assert.IsType<NpgsqlConnection>(connection.DbConnection);
        }

        [Fact]
        public void Can_create_master_connection_string()
        {
            using var connection = new NpgsqlRelationalConnection(CreateDependencies());
            using var master = connection.CreateMasterConnection();

            Assert.Equal(@"Host=localhost;Database=postgres;Username=some_user;Password=some_password;Pooling=False", master.ConnectionString);
        }

        [Fact]
        public void Can_create_master_connection_string_with_alternate_admin_db()
        {
            var options = new DbContextOptionsBuilder()
                .UseNpgsql(
                    @"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password",
                    b => b.UseAdminDatabase("template0"))
                .Options;

            using var connection = new NpgsqlRelationalConnection(CreateDependencies(options));
            using var master = connection.CreateMasterConnection();

            Assert.Equal(@"Host=localhost;Database=template0;Username=some_user;Password=some_password;Pooling=False", master.ConnectionString);
        }

        public static RelationalConnectionDependencies CreateDependencies(DbContextOptions options = null)
        {
            options ??= new DbContextOptionsBuilder()
                .UseNpgsql(@"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password")
                .Options;

            return new RelationalConnectionDependencies(
                options,
                new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                    new LoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener"),
                    new NpgsqlLoggingDefinitions(),
                    new NullDbContextLogger()),
                new RelationalConnectionDiagnosticsLogger(
                    new LoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener"),
                    new NpgsqlLoggingDefinitions(),
                    new NullDbContextLogger(),
                    CreateOptions()),
                new NamedConnectionStringResolver(options),
                new RelationalTransactionFactory(
                    new RelationalTransactionFactoryDependencies(
                        new RelationalSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies()))),
                new CurrentDbContext(new FakeDbContext()),
                new RelationalCommandBuilderFactory(
                    new RelationalCommandBuilderDependencies(
                        new NpgsqlTypeMappingSource(
                            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
                            new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                            new NpgsqlOptions()))));
        }

        private const string ConnectionString = "Fake Connection String";

        private static IDbContextOptions CreateOptions(
            RelationalOptionsExtension optionsExtension = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(
                    optionsExtension
                    ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

            return optionsBuilder.Options;
        }

        private class FakeDbContext : DbContext
        {
        }
    }
}
