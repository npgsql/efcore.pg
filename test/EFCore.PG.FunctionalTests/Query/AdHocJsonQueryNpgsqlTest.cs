using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocJsonQueryNpgsqlTest(NonSharedFixture fixture) : AdHocJsonQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override async Task Seed29219(DbContext ctx)
    {
        var entity1 = new Context29219.MyEntity
        {
            Id = 1,
            Reference = new Context29219.MyJsonEntity { NonNullableScalar = 10, NullableScalar = 11 },
            Collection =
            [
                new() { NonNullableScalar = 100, NullableScalar = 101 },
                new() { NonNullableScalar = 200, NullableScalar = 201 },
                new() { NonNullableScalar = 300, NullableScalar = null }
            ]
        };

        var entity2 = new Context29219.MyEntity
        {
            Id = 2,
            Reference = new Context29219.MyJsonEntity { NonNullableScalar = 20, NullableScalar = null },
            Collection = [new() { NonNullableScalar = 1001, NullableScalar = null }]
        };

        ctx.AddRange(entity1, entity2);
        await ctx.SaveChangesAsync();

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Id", "Reference", "Collection")
VALUES(3, '{ "NonNullableScalar" : 30 }', '[{ "NonNullableScalar" : 10001 }]')
""");
    }

    protected override async Task Seed30028(DbContext ctx)
    {
        // complete
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
1,
'{"RootName":"e1","Collection":[{"BranchName":"e1 c1","Nested":{"LeafName":"e1 c1 l"}},{"BranchName":"e1 c2","Nested":{"LeafName":"e1 c2 l"}}],"OptionalReference":{"BranchName":"e1 or","Nested":{"LeafName":"e1 or l"}},"RequiredReference":{"BranchName":"e1 rr","Nested":{"LeafName":"e1 rr l"}}}')
""");

        // missing collection
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
2,
'{"RootName":"e2","OptionalReference":{"BranchName":"e2 or","Nested":{"LeafName":"e2 or l"}},"RequiredReference":{"BranchName":"e2 rr","Nested":{"LeafName":"e2 rr l"}}}')
""");

        // missing optional reference
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
3,
'{"RootName":"e3","Collection":[{"BranchName":"e3 c1","Nested":{"LeafName":"e3 c1 l"}},{"BranchName":"e3 c2","Nested":{"LeafName":"e3 c2 l"}}],"RequiredReference":{"BranchName":"e3 rr","Nested":{"LeafName":"e3 rr l"}}}')
""");

        // missing required reference
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
4,
'{"RootName":"e4","Collection":[{"BranchName":"e4 c1","Nested":{"LeafName":"e4 c1 l"}},{"BranchName":"e4 c2","Nested":{"LeafName":"e4 c2 l"}}],"OptionalReference":{"BranchName":"e4 or","Nested":{"LeafName":"e4 or l"}}}')
""");
    }

    protected override async Task Seed33046(DbContext ctx)
        => await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Reviews" ("Rounds", "Id")
VALUES('[{"RoundNumber":11,"SubRounds":[{"SubRoundNumber":111},{"SubRoundNumber":112}]}]', 1)
""");

    protected override async Task SeedJunkInJson(DbContext ctx)
        => await ctx.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO "Entities" ("Collection", "CollectionWithCtor", "Reference", "ReferenceWithCtor", "Id")
VALUES(
'[{"JunkReference":{"Something":"SomeValue" },"Name":"c11","JunkProperty1":50,"Number":11.5,"JunkCollection1":[],"JunkCollection2":[{"Foo":"junk value"}],"NestedCollection":[{"DoB":"2002-04-01T00:00:00","DummyProp":"Dummy value"},{"DoB":"2002-04-02T00:00:00","DummyReference":{"Foo":5}}],"NestedReference":{"DoB":"2002-03-01T00:00:00"}},{"Name":"c12","Number":12.5,"NestedCollection":[{"DoB":"2002-06-01T00:00:00"},{"DoB":"2002-06-02T00:00:00"}],"NestedDummy":59,"NestedReference":{"DoB":"2002-05-01T00:00:00"}}]',
'[{"MyBool":true,"Name":"c11 ctor","JunkReference":{"Something":"SomeValue","JunkCollection":[{"Foo":"junk value"}]},"NestedCollection":[{"DoB":"2002-08-01T00:00:00"},{"DoB":"2002-08-02T00:00:00"}],"NestedReference":{"DoB":"2002-07-01T00:00:00"}},{"MyBool":false,"Name":"c12 ctor","NestedCollection":[{"DoB":"2002-10-01T00:00:00"},{"DoB":"2002-10-02T00:00:00"}],"JunkCollection":[{"Foo":"junk value"}],"NestedReference":{"DoB":"2002-09-01T00:00:00"}}]',
'{"Name":"r1","JunkCollection":[{"Foo":"junk value"}],"JunkReference":{"Something":"SomeValue" },"Number":1.5,"NestedCollection":[{"DoB":"2000-02-01T00:00:00","JunkReference":{"Something":"SomeValue"}},{"DoB":"2000-02-02T00:00:00"}],"NestedReference":{"DoB":"2000-01-01T00:00:00"}}',
'{"MyBool":true,"JunkCollection":[{"Foo":"junk value"}],"Name":"r1 ctor","JunkReference":{"Something":"SomeValue" },"NestedCollection":[{"DoB":"2001-02-01T00:00:00"},{"DoB":"2001-02-02T00:00:00"}],"NestedReference":{"JunkCollection":[{"Foo":"junk value"}],"DoB":"2001-01-01T00:00:00"}}',
1)
""");

    protected override async Task SeedTrickyBuffering(DbContext ctx)
        => await ctx.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO "Entities" ("Reference", "Id")
VALUES(
'{"Name": "r1", "Number": 7, "JunkReference":{"Something": "SomeValue" }, "JunkCollection": [{"Foo": "junk value"}], "NestedReference": {"DoB": "2000-01-01T00:00:00Z"}, "NestedCollection": [{"DoB": "2000-02-01T00:00:00Z", "JunkReference": {"Something": "SomeValue"}}, {"DoB": "2000-02-02T00:00:00Z"}]}',1)
""");

    protected override async Task SeedShadowProperties(DbContext ctx)
        => await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Collection", "CollectionWithCtor", "Reference", "ReferenceWithCtor", "Id", "Name")
VALUES(
'[{"Name":"e1_c1","ShadowDouble":5.5},{"ShadowDouble":20.5,"Name":"e1_c2"}]',
'[{"Name":"e1_c1 ctor","ShadowNullableByte":6},{"ShadowNullableByte":null,"Name":"e1_c2 ctor"}]',
'{"Name":"e1_r", "ShadowString":"Foo"}',
'{"ShadowInt":143,"Name":"e1_r ctor"}',
1,
'e1')
""");

    protected override async Task SeedNotICollection(DbContext ctx)
    {
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Json", "Id")
VALUES(
'{"Collection":[{"Bar":11,"Foo":"c11"},{"Bar":12,"Foo":"c12"},{"Bar":13,"Foo":"c13"}]}',
1)
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Json", "Id")
VALUES(
'{"Collection":[{"Bar":21,"Foo":"c21"},{"Bar":22,"Foo":"c22"}]}',
2)
""");
    }

    #region BadJsonProperties

    // PostgreSQL stores JSON as jsonb, which doesn't allow badly-formed JSON; so the following tests are irrelevant.

    public override async Task Bad_json_properties_duplicated_navigations(bool noTracking)
    {
        if (noTracking)
        {
            await Assert.ThrowsAsync<NotSupportedException>(() => base.Bad_json_properties_duplicated_navigations(noTracking: true));
        }
        else
        {
            await base.Bad_json_properties_duplicated_navigations(noTracking: false);
        }
    }

    public override Task Bad_json_properties_duplicated_scalars(bool noTracking)
        => Assert.ThrowsAsync<NotSupportedException>(() => base.Bad_json_properties_duplicated_scalars(noTracking));

    public override Task Bad_json_properties_empty_navigations(bool noTracking)
        => Assert.ThrowsAsync<NotSupportedException>(() => base.Bad_json_properties_empty_navigations(noTracking));

    public override Task Bad_json_properties_empty_scalars(bool noTracking)
        => Assert.ThrowsAsync<NotSupportedException>(() => base.Bad_json_properties_empty_scalars(noTracking));

    public override Task Bad_json_properties_null_navigations(bool noTracking)
        => Assert.ThrowsAsync<ThrowsAnyException>(() => base.Bad_json_properties_null_navigations(noTracking));

    public override Task Bad_json_properties_null_scalars(bool noTracking)
        => Assert.ThrowsAsync<ThrowsAnyException>(() => base.Bad_json_properties_null_scalars(noTracking));

    protected override Task SeedBadJsonProperties(ContextBadJsonProperties ctx)
        => throw new NotSupportedException("PostgreSQL stores JSON as jsonb, which doesn't allow badly-formed JSON");

    #endregion

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_predicate_on_bytea(bool async)
    {
        var contextFactory = await InitializeAsync<TypesDbContext>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Bytea = [1, 2, 3] } },
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Bytea = [1, 2, 4] } });
                await context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Where(x => x.JsonEntity.Bytea == new byte[] { 1, 2, 4 });

            var result = async
                ? await query.SingleAsync()
                : query.Single();

            Assert.Equal(2, result.Id);

            AssertSql(
                """
SELECT e."Id", e."JsonEntity"
FROM "Entities" AS e
WHERE (decode(e."JsonEntity" ->> 'Bytea', 'base64')) = BYTEA E'\\x010204'
LIMIT 2
""");
        }
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_predicate_on_interval(bool async)
    {
        var contextFactory = await InitializeAsync<TypesDbContext>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Interval = new TimeSpan(1, 2, 3, 4, 123, 456) } },
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Interval = new TimeSpan(2, 2, 3, 4, 123, 456) } });
                await context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Where(x => x.JsonEntity.Interval == new TimeSpan(2, 2, 3, 4, 123, 456));

            var result = async
                ? await query.SingleAsync()
                : query.Single();

            Assert.Equal(2, result.Id);

            AssertSql(
                """
SELECT e."Id", e."JsonEntity"
FROM "Entities" AS e
WHERE (CAST(e."JsonEntity" ->> 'Interval' AS interval)) = INTERVAL '2 02:03:04.123456'
LIMIT 2
""");
        }
    }

    protected class TypesDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TypesContainerEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TypesContainerEntity>().OwnsOne(b => b.JsonEntity).ToJson();
    }

    public class TypesContainerEntity
    {
        public int Id { get; set; }
        public TypesJsonEntity JsonEntity { get; set; }
    }

    public class TypesJsonEntity
    {
        public byte[] Bytea { get; set; }
        public TimeSpan Interval { get; set; }
    }

    #region Problematc tests (Unspecified DateTime)

    // These tests use a model with a non-UTC DateTime, which isn't supported in PG's timestamp with time zone

    public override Task Project_entity_with_json_null_values()
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_entity_with_json_null_values());

    public override Task Try_project_collection_but_JSON_is_entity()
        => Assert.ThrowsAsync<ThrowsException>(() => base.Try_project_collection_but_JSON_is_entity());

    public override Task Try_project_reference_but_JSON_is_collection()
        => Assert.ThrowsAsync<ThrowsException>(() => base.Try_project_reference_but_JSON_is_collection());

    public override Task Project_entity_with_optional_json_entity_owned_by_required_json()
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_entity_with_optional_json_entity_owned_by_required_json());

    public override Task Project_required_json_entity()
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_required_json_entity());

    public override Task Project_optional_json_entity_owned_by_required_json_entity()
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_optional_json_entity_owned_by_required_json_entity());

    public override Task Project_missing_required_scalar(bool async)
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_missing_required_scalar(async));

    public override Task Project_nested_json_entity_with_missing_scalars(bool async)
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_nested_json_entity_with_missing_scalars(async));

    public override Task Project_null_required_scalar(bool async)
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_null_required_scalar(async));

    public override Task Project_root_entity_with_missing_required_navigation(bool async)
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_root_entity_with_missing_required_navigation(async));

    public override Task Project_root_entity_with_null_required_navigation(bool async)
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_root_entity_with_null_required_navigation(async));

    public override Task Project_root_with_missing_scalars(bool async)
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_root_with_missing_scalars(async));

    public override Task Project_top_level_json_entity_with_missing_scalars(bool async)
        => Assert.ThrowsAsync<ArgumentException>(() => base.Project_top_level_json_entity_with_missing_scalars(async));

    public override Task Project_missing_required_navigation(bool async)
        => Task.CompletedTask; // Different exception expected in the base implementation

    public override Task Project_null_required_navigation(bool async)
        => Task.CompletedTask; // Different exception expected in the base implementation

    public override Task Project_top_level_entity_with_null_value_required_scalars(bool async)
        => Task.CompletedTask; // Different exception expected in the base implementation

    #endregion Problematc tests (Unspecified DateTime)

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}
