using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Relational.Tests.Migrations.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Tests.Migrations
{
    public class NpgsqlModelDifferTest : MigrationsModelDifferTestBase
    {
        [Fact]
        public void Create_extension_comes_after_ensure_schema()
        {
            Execute(
                _ => { },
                modelBuilder =>
                {
                    modelBuilder.HasDefaultSchema("some_schema");
                    modelBuilder.HasPostgresExtension("some_extension");
                    modelBuilder.Entity("some_table", x => x.Property<int>("Id"));
                },
                operations =>
                {
                    Assert.Equal(3, operations.Count);

                    var createSchemaOperation = Assert.IsType<EnsureSchemaOperation>(operations[0]);
                    Assert.Equal("some_schema", createSchemaOperation.Name);

                    var createPostgresExtensionOperation = Assert.IsType<NpgsqlCreatePostgresExtensionOperation>(operations[1]);
                    Assert.Equal("some_schema", createPostgresExtensionOperation.Schema);
                    Assert.Equal("some_extension", createPostgresExtensionOperation.Name);
                });
        }
    }
}
