namespace Microsoft.EntityFrameworkCore.Query;

public class ObjectCollectionContainsNpgsqlTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "ObjectCollectionContainsTest";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual async Task Contains_on_object_array_with_element_value_converter()
    {
        var contextFactory = await InitializeNonSharedTest<BlogContext>(seed: SeedAsync);

        using var context = contextFactory.CreateDbContext();
        var ids = new object[] { 1, 2 };

        var result = await context.Blogs.Where(b => ids.Contains(b.Id)).OrderBy(b => b.Id).Select(b => b.Id).ToListAsync();

        Assert.Equal([1, 2], result);

        AssertSql(
            """
@ids={ '1'
'2' } (DbType = Object)

SELECT b."Id"
FROM "Blogs" AS b
WHERE b."Id" = ANY (@ids)
ORDER BY b."Id" NULLS FIRST
""");
    }

    [ConditionalFact]
    public virtual async Task Contains_on_object_list_with_element_value_converter()
    {
        var contextFactory = await InitializeNonSharedTest<BlogContext>(seed: SeedAsync);

        using var context = contextFactory.CreateDbContext();
        var ids = new List<object> { 1, 2 };

        var result = await context.Blogs.Where(b => ids.Contains(b.Id)).OrderBy(b => b.Id).Select(b => b.Id).ToListAsync();

        Assert.Equal([1, 2], result);

        AssertSql(
            """
@ids={ '1'
'2' } (DbType = Object)

SELECT b."Id"
FROM "Blogs" AS b
WHERE b."Id" = ANY (@ids)
ORDER BY b."Id" NULLS FIRST
""");
    }

    private static async Task SeedAsync(BlogContext context)
    {
        context.Blogs.AddRange(
            new Blog { Id = 1 },
            new Blog { Id = 2 },
            new Blog { Id = 3 });
        await context.SaveChangesAsync();
    }

    protected void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);

    protected class BlogContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs => Set<Blog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Blog>().Property(b => b.Id)
                .HasConversion(new ValueConverter<int, long>(v => v, v => checked((int)v)))
                .HasColumnType("bigint");
    }

    protected class Blog
    {
        public int Id { get; set; }
    }
}
