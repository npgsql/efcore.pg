namespace Microsoft.EntityFrameworkCore;

public class DataAnnotationNpgsqlTest(DataAnnotationNpgsqlTest.DataAnnotationNpgsqlFixture fixture)
    : DataAnnotationRelationalTestBase<DataAnnotationNpgsqlTest.DataAnnotationNpgsqlFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override TestHelpers TestHelpers
        => NpgsqlTestHelpers.Instance;

    public override Task StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        => Task.CompletedTask; // Npgsql does not support length

    public override Task TimestampAttribute_throws_if_value_in_database_changed()
        => Task.CompletedTask; // Npgsql does not support length

    public override Task MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        => Task.CompletedTask; // Npgsql does not support length

    public class DataAnnotationNpgsqlFixture : DataAnnotationRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
    }
}
