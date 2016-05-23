using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class NpgsqlDatabaseCreatorTest
    {
        #region Npgsql-specific

        [Fact]
        public void Postgres_extension_is_created()
        {
            var connString = NpgsqlTestStore.NextScratchConnectionString();
            using (var context = new ExtensionContext(connString))
            {
                var creator = context.GetService<IRelationalDatabaseCreator>();
                creator.EnsureDeleted();
                creator.EnsureCreated();

                try
                {
                    var cmd = context.GetService<IRawSqlCommandBuilder>().Build(
                        "SELECT EXISTS (SELECT * FROM pg_available_extensions WHERE name='hstore' AND installed_version IS NOT NULL);"
                    );
                    var conn = context.GetService<IRelationalConnection>();
                    Assert.True((bool) cmd.ExecuteScalar(conn));
                }
                finally
                {
                    creator.Delete();
                }
            }
        }

        class ExtensionContext : DbContext
        {
            readonly string _connectionString;

            public ExtensionContext(string connectionString)
            {
                _connectionString = connectionString;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseNpgsql(_connectionString);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.HasPostgresExtension("hstore");
        }

        #endregion
    }
}
