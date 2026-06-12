namespace Microsoft.EntityFrameworkCore;

public class WithConstructorsNpgsqlTest(WithConstructorsNpgsqlTest.WithConstructorsNpgsqlFixture fixture)
    : WithConstructorsTestBase<WithConstructorsNpgsqlTest.WithConstructorsNpgsqlFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class WithConstructorsNpgsqlFixture : WithConstructorsFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<BlogQuery>().HasNoKey().ToSqlQuery("""
                SELECT * FROM "Blog"
                """);
        }
    }
}
