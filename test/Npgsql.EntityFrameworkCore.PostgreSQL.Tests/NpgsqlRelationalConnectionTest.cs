using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using Xunit;
using Npgsql;

namespace Microsoft.EntityFrameworkCore.Relational.Tests
{
    public class NpgsqlRelationalConnectionTest
    {
        [Fact]
        public void Creates_Npgsql_Server_connection_string()
        {
            using (var connection = new NpgsqlRelationalConnection(CreateOptions(), new Logger<NpgsqlConnection>(new LoggerFactory())))
            {
                Assert.IsType<NpgsqlConnection>(connection.DbConnection);
            }
        }

        [Fact]
        public void Can_create_master_connection_string()
        {
            using (var connection = new NpgsqlRelationalConnection(CreateOptions(), new Logger<NpgsqlConnection>(new LoggerFactory())))
            {
                using (var master = connection.CreateMasterConnection())
                {
                    Assert.Equal(@"Host=localhost;Database=postgres;Username=some_user;Password=some_password;Pooling=False", master.ConnectionString);
                }
            }
        }

        public static IDbContextOptions CreateOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql(@"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password");

            return optionsBuilder.Options;
        }
    }
}
