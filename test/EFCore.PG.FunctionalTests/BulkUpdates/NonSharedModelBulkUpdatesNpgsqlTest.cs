namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class NonSharedModelBulkUpdatesNpgsqlTest(NonSharedFixture fixture) : NonSharedModelBulkUpdatesRelationalTestBase(fixture)
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

    public override async Task Delete_with_owned_collection_and_non_natively_translatable_query(bool async)
    {
        await base.Delete_with_owned_collection_and_non_natively_translatable_query(async);

        AssertSql(
            """
@p='1'

DELETE FROM "Owner" AS o
WHERE o."Id" IN (
    SELECT o0."Id"
    FROM "Owner" AS o0
    ORDER BY o0."Title" NULLS FIRST
    OFFSET @p
)
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

    public override async Task Replace_ColumnExpression_in_column_setter(bool async)
    {
        await base.Replace_ColumnExpression_in_column_setter(async);

        AssertSql(
            """
@p='SomeValue'

UPDATE "OwnedCollection" AS o0
SET "Value" = @p
FROM "Owner" AS o
WHERE o."Id" = o0."OwnerId"
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
@p='SomeValue'

UPDATE "Owner" AS o
SET "Title" = @p
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

    public override async Task Update_non_owned_property_on_entity_with_owned_in_join(bool async)
    {
        await base.Update_non_owned_property_on_entity_with_owned_in_join(async);

        AssertSql(
            """
@p='NewValue'

UPDATE "Owner" AS o
SET "Title" = @p
FROM "Owner" AS o0
WHERE o."Id" = o0."Id"
""");
    }

    public override async Task Update_owned_and_non_owned_properties_with_table_sharing(bool async)
    {
        await base.Update_owned_and_non_owned_properties_with_table_sharing(async);

        AssertSql(
            """
UPDATE "Owner" AS o
SET "Title" = COALESCE(o."OwnedReference_Number"::text, ''),
    "OwnedReference_Number" = length(o."Title")::int
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
            seed: async context =>
            {
                context.Set<Blog>().Add(new Blog { Title = "SomeBlog" });
                await context.SaveChangesAsync();
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
SET "Title" = b0."Rating"::text,
    "Rating" = length(b0."Title")::int
FROM "Blogs" AS b
WHERE b."Id" = b0."Id"
""");
    }

    public override async Task Delete_entity_with_auto_include(bool async)
    {
        await base.Delete_entity_with_auto_include(async);

        AssertSql(
            """
DELETE FROM "Context30572_Principal" AS c
WHERE c."Id" IN (
    SELECT c0."Id"
    FROM "Context30572_Principal" AS c0
    LEFT JOIN "Context30572_Dependent" AS c1 ON c0."DependentId" = c1."Id"
)
""");
    }

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

    [ConditionalTheory] // #3001
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_with_primitive_collection_in_value_selector(bool async)
    {
        var contextFactory = await InitializeAsync<Context3001>(
            seed: async ctx =>
            {
                ctx.AddRange(new EntityWithPrimitiveCollection { Tags = ["tag1", "tag2"] });
                await ctx.SaveChangesAsync();
            });

        await AssertUpdate(
            async,
            contextFactory.CreateContext,
            ss => ss.EntitiesWithPrimitiveCollection,
            s => s.SetProperty(x => x.Tags, x => x.Tags.Append("another_tag")),
            rowsAffectedCount: 1);
    }

    protected class Context3001(DbContextOptions options) : DbContext(options)
    {
        public DbSet<EntityWithPrimitiveCollection> EntitiesWithPrimitiveCollection { get; set; }
    }

    protected class EntityWithPrimitiveCollection
    {
        public int Id { get; set; }
        public List<string> Tags { get; set; } = null!;
    }

    public override async Task Delete_with_view_mapping(bool async)
    {
        await base.Delete_with_view_mapping(async);

        AssertSql(
            """
DELETE FROM "Blogs" AS b
""");
    }

    public override async Task Update_with_view_mapping(bool async)
    {
        await base.Update_with_view_mapping(async);

        AssertSql(
            """
@p='Updated'

UPDATE "Blogs" AS b
SET "Data" = @p
""");
    }

    public override async Task Update_complex_type_with_view_mapping(bool async)
    {
        await base.Update_complex_type_with_view_mapping(async);

        // #34706
        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
