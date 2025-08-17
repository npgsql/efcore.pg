using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.JsonOwnedNavigations;

public class JsonOwnedTypeNpgsqlFixture : JsonOwnedNavigationsRelationalFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName => "JsonTypeRelationshipsQueryTest";

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRoot>().OwnsOne(x => x.RequiredReferenceTrunk).HasColumnType("json");
        modelBuilder.Entity<RelationshipsRoot>().OwnsOne(x => x.OptionalReferenceTrunk).HasColumnType("json");
        modelBuilder.Entity<RelationshipsRoot>().OwnsMany(x => x.CollectionTrunk).HasColumnType("json");
    }
}
