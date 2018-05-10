using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class DataAnnotationNpgsqlTest : DataAnnotationTestBase<DataAnnotationNpgsqlTest.DataAnnotationNpgsqlFixture>
    {
        public DataAnnotationNpgsqlTest(DataAnnotationNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        // Need to override some tests because the base class defines their string properties with type
        // nvarchar(128) which is SQL Server-specific
        [Fact]
        public override ModelBuilder Field_annotations_are_enabled()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<FieldAnnotationClass>()
                .Property<string>("_personFirstName")
                .HasColumnType("varchar(128)");

            Validate(modelBuilder);

            Assert.True(GetProperty<FieldAnnotationClass>(modelBuilder, "_personFirstName").IsPrimaryKey());

            return modelBuilder;
        }

        [Fact]
        public override ModelBuilder Non_public_annotations_are_enabled()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<PrivateMemberAnnotationClass>().Property(
                PrivateMemberAnnotationClass.PersonFirstNameExpr)
                .HasColumnType("varchar(128)");

            Validate(modelBuilder);

            Assert.True(GetProperty<PrivateMemberAnnotationClass>(modelBuilder, "PersonFirstName").IsPrimaryKey());

            return modelBuilder;
        }

        [Fact]
        public override ModelBuilder Key_and_column_work_together()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ColumnKeyAnnotationClass1>()
                .Property(c => c.PersonFirstName)
                .HasColumnType("varchar(128)");

            Validate(modelBuilder);

            Assert.True(GetProperty<ColumnKeyAnnotationClass1>(modelBuilder, "PersonFirstName").IsPrimaryKey());

            return modelBuilder;
        }

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

        public class DataAnnotationNpgsqlFixture : DataAnnotationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }
    }
}
