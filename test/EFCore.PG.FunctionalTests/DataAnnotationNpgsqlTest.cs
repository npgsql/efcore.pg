using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class DataAnnotationNpgsqlTest : DataAnnotationRelationalTestBase<DataAnnotationNpgsqlTest.DataAnnotationNpgsqlFixture>
{
    public DataAnnotationNpgsqlTest(DataAnnotationNpgsqlFixture fixture)
        : base(fixture)
    {
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override TestHelpers TestHelpers
        => NpgsqlTestHelpers.Instance;

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

    public class DataAnnotationNpgsqlFixture : DataAnnotationRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
    }
}
