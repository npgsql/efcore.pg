using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocMiscellaneousQueryNpgsqlTest(NonSharedFixture fixture) : AdHocMiscellaneousQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder SetParameterizedCollectionMode(DbContextOptionsBuilder optionsBuilder, ParameterTranslationMode parameterizedCollectionMode)
    {
        new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseParameterizedCollectionMode(parameterizedCollectionMode);

        return optionsBuilder;
    }

    // Unlike the other providers, EFCore.PG does actually support mapping JsonElement
    public override Task Mapping_JsonElement_property_throws_a_meaningful_exception()
        => Task.CompletedTask;

    protected override Task Seed2951(Context2951 context)
        => context.Database.ExecuteSqlRawAsync(
            """
CREATE TABLE "ZeroKey" ("Id" int);
INSERT INTO "ZeroKey" VALUES (NULL)
""");

    // Writes DateTime with Kind=Unspecified to timestamptz
    public override Task SelectMany_where_Select(bool async)
        => Task.CompletedTask;

    // Writes DateTime with Kind=Unspecified to timestamptz
    public override Task Subquery_first_member_compared_to_null(bool async)
        => Task.CompletedTask;

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/27995/files#r874038747")]
    public override Task StoreType_for_UDF_used(bool async)
        => base.StoreType_for_UDF_used(async);

    [ConditionalFact]
    public virtual async Task Like_with_implicit_escape_does_not_apply_value_converter()
    {
        var contextFactory = await InitializeNonSharedTest<Context3888>(
            seed: async context =>
            {
                context.Entities.Add(new Context3888.Entity { Value = "ABC" });
                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var pattern = "%ABC%";

        Assert.Equal(1, await context.Entities.CountAsync(e => EF.Functions.Like(e.Value!, pattern)));
        Assert.Equal(1, await context.Entities.CountAsync(e => EF.Functions.ILike(e.Value!, pattern)));

        AssertSql(
            """
@pattern='%ABC%'

SELECT count(*)::int
FROM "Entities" AS e
WHERE e."Value" LIKE @pattern ESCAPE ''
""",
            //
            """
@pattern='%ABC%'

SELECT count(*)::int
FROM "Entities" AS e
WHERE e."Value" ILIKE @pattern ESCAPE ''
""");
    }

    protected class Context3888(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>()
                .Property(e => e.Value)
                .HasConversion(
                    value => string.IsNullOrWhiteSpace(value) ? null : value,
                    value => value ?? string.Empty);

        public class Entity
        {
            public int Id { get; set; }
            public string? Value { get; set; }
        }
    }
}
