using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class DataAnnotationNpgsqlTest : DataAnnotationTestBase<NpgsqlTestStore, DataAnnotationNpgsqlFixture>
    {
        public DataAnnotationNpgsqlTest(DataAnnotationNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            // Npgsql does not support length
        }

        public override void TimestampAttribute_throws_if_value_in_database_changed()
        {
            // Npgsql does not support length
        }

        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            // Npgsql does not support length            
        }

        [Fact]
        public override ModelBuilder DatabaseGeneratedOption_configures_the_property_correctly()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<GeneratedEntity>();

            var entity = modelBuilder.Model.FindEntityType(typeof(GeneratedEntity));

            var id = entity.FindProperty(nameof(GeneratedEntity.Id));
            Assert.Equal(ValueGenerated.Never, id.ValueGenerated);
            Assert.False(id.RequiresValueGenerator);

            var identity = entity.FindProperty(nameof(GeneratedEntity.Identity));
            Assert.Equal(ValueGenerated.OnAdd, identity.ValueGenerated);
            Assert.False(identity.RequiresValueGenerator);

            var version = entity.FindProperty(nameof(GeneratedEntity.Version));
            Assert.Equal(ValueGenerated.OnAddOrUpdate, version.ValueGenerated);
            Assert.False(version.RequiresValueGenerator);

            Validate(modelBuilder);

            return modelBuilder;
        }
    }
}
