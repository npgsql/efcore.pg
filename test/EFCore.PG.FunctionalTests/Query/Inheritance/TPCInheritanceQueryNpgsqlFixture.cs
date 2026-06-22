using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPCInheritanceQueryNpgsqlFixture : TPCInheritanceQueryFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override async Task SeedAsync(InheritanceContext context)
    {
        await base.SeedAsync(context);

        await context.Database.ExecuteSqlRawAsync(
            """
SELECT setval(
    '"AnimalSequence"',
    GREATEST(
        COALESCE((SELECT MAX("Id") FROM "Kiwi"), 1),
        COALESCE((SELECT MAX("Id") FROM "Eagle"), 1)),
    true);
""");
    }
}
