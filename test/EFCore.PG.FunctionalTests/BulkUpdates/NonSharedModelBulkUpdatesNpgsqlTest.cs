using Microsoft.EntityFrameworkCore.BulkUpdates;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update;

public class NonSharedModelBulkUpdatesNpgsqlTest : NonSharedModelBulkUpdatesTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_aggregate_root_when_eager_loaded_owned_collection(bool async)
    {
        await base.Delete_aggregate_root_when_eager_loaded_owned_collection(async);

        AssertSql(
            """
DELETE FROM "Owner" AS o
""");
    }

    public override async Task Delete_aggregate_root_when_table_sharing_with_owned(bool async)
    {
        await base.Delete_aggregate_root_when_table_sharing_with_owned(async);

        AssertSql(
            """
DELETE FROM "Owner" AS o
""");
    }

    public override async Task Delete_aggregate_root_when_table_sharing_with_non_owned_throws(bool async)
    {
        await base.Delete_aggregate_root_when_table_sharing_with_non_owned_throws(async);

        AssertSql();
    }

    public override async Task Update_non_owned_property_on_entity_with_owned(bool async)
    {
        await base.Update_non_owned_property_on_entity_with_owned(async);

        AssertSql(
            """
UPDATE "Owner" AS o
SET "Title" = 'SomeValue'
""");
    }

    public override async Task Delete_predicate_based_on_optional_navigation(bool async)
    {
        await base.Delete_predicate_based_on_optional_navigation(async);

        AssertSql(
            """
DELETE FROM "Posts" AS p
WHERE p."Id" IN (
    SELECT p0."Id"
    FROM "Posts" AS p0
    LEFT JOIN "Blogs" AS b ON p0."BlogId" = b."Id"
    WHERE b."Title" LIKE 'Arthur%'
)
""");
    }

    public override async Task Update_non_owned_property_on_entity_with_owned2(bool async)
    {
        await base.Update_non_owned_property_on_entity_with_owned2(async);

        AssertSql(
            """
UPDATE "Owner" AS o
SET "Title" = COALESCE(o."Title", '') || '_Suffix'
""");
    }

    public override async Task Update_owned_and_non_owned_properties_with_table_sharing(bool async)
    {
        await base.Update_owned_and_non_owned_properties_with_table_sharing(async);

        AssertSql(
            """
UPDATE "Owner" AS o
SET "OwnedReference_Number" = length(o."Title")::int,
    "Title" = o."OwnedReference_Number"::text
""");
    }

    public override async Task Update_main_table_in_entity_with_entity_splitting(bool async)
    {
        // Overridden/duplicated because we update DateTime, which Npgsql requires to be a UTC timestamp
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: mb => mb.Entity<Blog>()
                .ToTable("Blogs")
                .SplitToTable(
                    "BlogsPart1", tb =>
                    {
                        tb.Property(b => b.Title);
                        tb.Property(b => b.Rating);
                    }),
            seed: context =>
            {
                context.Set<Blog>().Add(new Blog { Title = "SomeBlog" });
                context.SaveChanges();
            });

        await AssertUpdate(
            async,
            contextFactory.CreateContext,
            ss => ss.Set<Blog>(),
            s => s.SetProperty(b => b.CreationTimestamp, b => new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            rowsAffectedCount: 1);

        AssertSql(
            """
UPDATE "Blogs" AS b
SET "CreationTimestamp" = TIMESTAMPTZ '2020-01-01T00:00:00Z'
""");
    }

    public override async Task Update_non_main_table_in_entity_with_entity_splitting(bool async)
    {
        await base.Update_non_main_table_in_entity_with_entity_splitting(async);

        AssertSql(
            """
UPDATE "BlogsPart1" AS b0
SET "Rating" = length(b0."Title")::int,
    "Title" = b0."Rating"::text
""");
    }

    public override Task Delete_entity_with_auto_include(bool async)
        => Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => base.Delete_entity_with_auto_include(async)); // #30577

    public override async Task Update_with_alias_uniquification_in_setter_subquery(bool async)
    {
        await base.Update_with_alias_uniquification_in_setter_subquery(async);

        AssertSql(
            """
UPDATE "Orders" AS o
SET "Total" = (
    SELECT COALESCE(sum(o0."Amount"), 0)::int
    FROM "OrderProduct" AS o0
    WHERE o."Id" = o0."OrderId")
WHERE o."Id" = 1
""");
    }

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
