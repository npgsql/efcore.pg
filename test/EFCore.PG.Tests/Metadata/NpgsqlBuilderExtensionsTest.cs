using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Tests.Metadata
{
    public class NpgsqlBuilderExtensionsTest
    {
        [Fact]
        public void CockroachDbInterleaveInParent()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.Entity<Customer>()
                .ToTable("customers", "my_schema")
                .ForCockroachDbInterleaveInParent(typeof(Customer), new List<string> { "col_a", "col_b" });

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
            var interleaveInParent = entityType.Npgsql().CockroachDbInterleaveInParent;

            Assert.Equal("my_schema", interleaveInParent.ParentTableSchema);
            Assert.Equal("customers", interleaveInParent.ParentTableName);
            var interleavePrefix = interleaveInParent.InterleavePrefix;
            Assert.Equal(2, interleavePrefix.Count);
            Assert.Equal("col_a", interleavePrefix[0]);
            Assert.Equal("col_b", interleavePrefix[1]);
        }

        protected virtual ModelBuilder CreateConventionModelBuilder()
        {
            return NpgsqlTestHelpers.Instance.CreateConventionBuilder();
        }
    }
}
