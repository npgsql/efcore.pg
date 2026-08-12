namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedEntityQueryNpgsqlTest(NonSharedFixture fixture) : OwnedEntityQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    // The base test corrupts the seeded data with raw SQL using unquoted identifiers, which PostgreSQL
    // folds to lowercase; reimplement with quoted identifiers.
    public override async Task Inconsistent_owned_entity_data_logs_warning_and_does_not_cause_identity_conflict()
    {
        var contextFactory = await InitializeNonSharedTest<Context38223>(
            shouldLogCategory: c => c == DbLoggerCategory.Query.Name,
            onConfiguring: b => b.ConfigureWarnings(c => c.Log(CoreEventId.InconsistentOwnedDataWarning)),
            seed: async c =>
            {
                // Insert a valid entity via EF Core, then corrupt Outer's required property to NULL to
                // create inconsistent data: Inner appears present but Outer's required property is null.
                var rootEntity = new Context38223.RootEntity
                {
                    Id = Guid.NewGuid(),
                    Outer = new Context38223.Outer
                    {
                        RequiredProperty = 1,
                        Inner = new Context38223.Inner { InnerProperty = 42 }
                    }
                };
                c.Add(rootEntity);
                await c.SaveChangesAsync();

                await c.Database.ExecuteSqlRawAsync(
                    """UPDATE "RootEntity" SET "Outer_RequiredProperty" = NULL WHERE "Id" = {0}""", rootEntity.Id);
            });

        using var context = contextFactory.CreateDbContext();

        ListLoggerFactory.Clear();

        var root = await context.Set<Context38223.RootEntity>().SingleAsync();

        Assert.NotNull(root);
        Assert.Null(root.Outer);

        Assert.Contains(
            ListLoggerFactory.Log,
            l => l.Id == CoreEventId.InconsistentOwnedDataWarning && l.Level == LogLevel.Warning);

        // Replacing the owned entity should not throw an identity conflict exception
        root.Outer = new Context38223.Outer
        {
            RequiredProperty = 1,
            Inner = new Context38223.Inner { InnerProperty = 2 }
        };

        await context.SaveChangesAsync();
    }
}
