using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using Xunit;
using Npgsql;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Tests
{
    public class NpgsqlRelationalConnectionTest
    {
        [Fact]
        public void Creates_Npgsql_Server_connection_string()
        {
            using (var connection = new NpgsqlRelationalConnection(CreateDependencies()))
            {
                Assert.IsType<NpgsqlConnection>(connection.DbConnection);
            }
        }

        [Fact]
        public void Can_create_master_connection_string()
        {
            using (var connection = new NpgsqlRelationalConnection(CreateDependencies()))
            {
                using (var master = connection.CreateMasterConnection())
                {
                    Assert.Equal(@"Host=localhost;Database=postgres;Username=some_user;Password=some_password;Pooling=False", master.ConnectionString);
                }
            }
        }

        [Fact]
        public void Can_create_master_connection_string_with_alternate_admin_db()
        {
            var options = new DbContextOptionsBuilder()
                .UseNpgsql(
                    @"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password",
                    b => b.UseAdminDatabase("template0"))
                .Options;

            using (var connection = new NpgsqlRelationalConnection(CreateDependencies(options)))
            {
                using (var master = connection.CreateMasterConnection())
                {
                    Assert.Equal(@"Host=localhost;Database=template0;Username=some_user;Password=some_password;Pooling=False", master.ConnectionString);
                }
            }
        }

        public static RelationalConnectionDependencies CreateDependencies(DbContextOptions options = null)
        {
            options = options
                      ?? new DbContextOptionsBuilder()
                          .UseNpgsql(@"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password")
                          .Options;

            return new RelationalConnectionDependencies(
                options,
                new InterceptingLogger<LoggerCategory.Database.Transaction>(new LoggerFactory(), new LoggingOptions()),
                new InterceptingLogger<LoggerCategory.Database.Connection>(new LoggerFactory(), new LoggingOptions()),
                new DiagnosticListener("Fake"));
        }
    }
}
