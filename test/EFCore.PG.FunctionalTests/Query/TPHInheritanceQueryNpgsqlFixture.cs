using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

using Microsoft.EntityFrameworkCore.Query.Inheritance;
namespace Microsoft.EntityFrameworkCore.Query;

public class TPHInheritanceQueryNpgsqlFixture : TPHInheritanceQueryFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<AnimalQuery>().HasNoKey().ToSqlQuery("""
            SELECT * FROM "Animals"
            """);
    }
}
