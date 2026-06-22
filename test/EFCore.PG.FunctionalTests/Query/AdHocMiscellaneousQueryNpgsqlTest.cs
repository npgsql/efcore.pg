using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocMiscellaneousQueryNpgsqlTest(NonSharedFixture fixture) : AdHocMiscellaneousQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
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

    public override async Task StoreType_for_UDF_used(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context27954Npgsql>();
        using var context = contextFactory.CreateDbContext();

        var date = new DateTime(2012, 12, 12);
        var query1 = context.Set<Context27954Npgsql.MyEntity>().Where(x => x.SomeDate == date);
        var query2 = context.Set<Context27954Npgsql.MyEntity>().Where(x => Context27954Npgsql.MyEntity.Modify(x.SomeDate) == date);

        if (async)
        {
            await query1.ToListAsync();
            await Assert.ThrowsAnyAsync<Exception>(() => query2.ToListAsync());
        }
        else
        {
            query1.ToList();
            Assert.ThrowsAny<Exception>(() => query2.ToList());
        }
    }

    protected class Context27954Npgsql(DbContextOptions options) : DbContext(options)
    {
        public DbSet<MyEntity> MyEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasDbFunction(typeof(MyEntity).GetMethod(nameof(MyEntity.Modify))!)
                .HasName("ModifyDate")
                .HasStoreType("timestamp without time zone");

            modelBuilder.Entity<MyEntity>()
                .Property(e => e.SomeDate)
                .HasColumnType("timestamp without time zone");
        }

        public class MyEntity
        {
            public int Id { get; set; }

            public DateTime SomeDate { get; set; }

            public static DateTime Modify(DateTime date)
                => throw new NotSupportedException();
        }
    }
}
