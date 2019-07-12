using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Xunit;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlRelationalConnectionTest
    {
        #region Tests

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
                    "Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password",
                    b => b.UseAdminDatabase("template0"))
                .Options;

            using var connection = new NpgsqlRelationalConnection(CreateDependencies(options));
            using var master = connection.CreateMasterConnection();
            Assert.Equal(@"Host=localhost;Database=template0;Username=some_user;Password=some_password;Pooling=False", master.ConnectionString);
        }

        [Fact]
        public void Warning_is_logged_if_auto_prepare_is_disabled()
        {
            var options = new DbContextOptionsBuilder()
                .UseNpgsql("Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password;MaxAutoPrepare=0")
                .Options;

            using var conn1 = new NpgsqlRelationalConnection(CreateDependencies(options));
            using var conn2 = new NpgsqlRelationalConnection(CreateDependencies(options));

            Assert.NotNull(conn1.DbConnection);
            Assert.NotNull(conn2.DbConnection);

            var (level, _, message, _, _) = Assert.Single(TestLoggerFactory.Log.Where(x => x.Id == NpgsqlEventId.AutoPrepareDisabledWarning));

            Assert.Equal(level, LogLevel.Warning);
            Assert.Equal(
                "A connection to database 'NpgsqlConnectionTest' on server 'tcp://localhost:5432' is being made without automatic preparation. Consider enabling this feature to significantly improve performance. See https://www.npgsql.org/doc/prepare for more details.",
                message);
        }

        #endregion

        #region Support

        static readonly ListLoggerFactory TestLoggerFactory = new ListLoggerFactory();

        public NpgsqlRelationalConnectionTest() => TestLoggerFactory.Clear();

        static RelationalConnectionDependencies CreateDependencies(DbContextOptions? options = null)
        {
            options ??= new DbContextOptionsBuilder()
                .UseNpgsql(@"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password")
                .Options;

            return new RelationalConnectionDependencies(
                options,
                new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                    TestLoggerFactory,
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener"),
                    new NpgsqlLoggingDefinitions()),
                new DiagnosticsLogger<DbLoggerCategory.Database.Connection>(
                    TestLoggerFactory,
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener"),
                    new NpgsqlLoggingDefinitions()),
                new NamedConnectionStringResolver(options),
                new RelationalTransactionFactory(new RelationalTransactionFactoryDependencies()));
        }

        #endregion
    }
}
