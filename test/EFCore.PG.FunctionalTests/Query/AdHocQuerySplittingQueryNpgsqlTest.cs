using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocQuerySplittingQueryNpgsqlTest(NonSharedFixture fixture) : AdHocQuerySplittingQueryTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    private static readonly FieldInfo _querySplittingBehaviorFieldInfo =
        typeof(RelationalOptionsExtension).GetField("_querySplittingBehavior", BindingFlags.NonPublic | BindingFlags.Instance);

    protected override DbContextOptionsBuilder SetQuerySplittingBehavior(
        DbContextOptionsBuilder optionsBuilder,
        QuerySplittingBehavior splittingBehavior)
    {
        new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseQuerySplittingBehavior(splittingBehavior);

        return optionsBuilder;
    }

    protected override DbContextOptionsBuilder ClearQuerySplittingBehavior(DbContextOptionsBuilder optionsBuilder)
    {
        var extension = optionsBuilder.Options.FindExtension<NpgsqlOptionsExtension>();
        if (extension == null)
        {
            extension = new NpgsqlOptionsExtension();
        }
        else
        {
            _querySplittingBehaviorFieldInfo.SetValue(extension, null);
        }

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    protected override TestStore CreateTestStore25225()
    {
        var testStore = NpgsqlTestStore.Create(StoreName);
        testStore.UseConnectionString = true;
        return testStore;
    }
}
