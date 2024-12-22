using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class BatchingTest(BatchingTest.BatchingTestFixture fixture) : IClassFixture<BatchingTest.BatchingTestFixture>
{
    protected BatchingTestFixture Fixture { get; } = fixture;

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    public async Task Inserts_are_batched_correctly(bool clientPk, bool clientFk, bool clientOrder)
    {
        var expectedBlogs = new List<Blog>();
        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner1 = new Owner();
                var owner2 = new Owner();
                context.Owners.Add(owner1);
                context.Owners.Add(owner2);

                for (var i = 1; i < 500; i++)
                {
                    var blog = new Blog();
                    if (clientPk)
                    {
                        blog.Id = Guid.NewGuid();
                    }

                    if (clientFk)
                    {
                        blog.Owner = i % 2 == 0 ? owner1 : owner2;
                    }

                    if (clientOrder)
                    {
                        blog.Order = i;
                    }

                    context.Set<Blog>().Add(blog);
                    expectedBlogs.Add(blog);
                }

                await context.SaveChangesAsync();
            },
            context => AssertDatabaseState(context, clientOrder, expectedBlogs));
    }

    [Fact]
    public async Task Inserts_and_updates_are_batched_correctly()
    {
        var expectedBlogs = new List<Blog>();

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner1 = new Owner { Name = "0" };
                var owner2 = new Owner { Name = "1" };
                context.Owners.Add(owner1);
                context.Owners.Add(owner2);

                var blog1 = new Blog
                {
                    Id = Guid.NewGuid(),
                    Owner = owner1,
                    Order = 1
                };

                context.Set<Blog>().Add(blog1);
                expectedBlogs.Add(blog1);

                context.SaveChanges();

                owner2.Name = "2";

                blog1.Order = 0;
                var blog2 = new Blog
                {
                    Id = Guid.NewGuid(),
                    Owner = owner1,
                    Order = 1
                };

                context.Set<Blog>().Add(blog2);
                expectedBlogs.Add(blog2);

                var blog3 = new Blog
                {
                    Id = Guid.NewGuid(),
                    Owner = owner2,
                    Order = 2
                };

                context.Set<Blog>().Add(blog3);
                expectedBlogs.Add(blog3);

                await context.SaveChangesAsync();
            },
            context => AssertDatabaseState(context, true, expectedBlogs));
    }

    [Fact]
    public Task Inserts_when_database_type_is_different()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner1 = new Owner { Id = "0", Name = "Zero" };
                var owner2 = new Owner { Id = "A", Name = string.Join("", Enumerable.Repeat('A', 900)) };
                context.Owners.Add(owner1);
                context.Owners.Add(owner2);

                await context.SaveChangesAsync();
            },
            async context => Assert.Equal(2, await context.Owners.CountAsync()));

    [ConditionalTheory]
    [InlineData(3)]
    [InlineData(4)]
    public Task Inserts_are_batched_only_when_necessary(int minBatchSize)
    {
        var expectedBlogs = new List<Blog>();
        return TestHelpers.ExecuteWithStrategyInTransactionAsync(
            () => (BloggingContext)Fixture.CreateContext(minBatchSize),
            UseTransaction,
            async context =>
            {
                var owner = new Owner();
                context.Owners.Add(owner);

                for (var i = 1; i < 3; i++)
                {
                    var blog = new Blog { Id = Guid.NewGuid(), Owner = owner };

                    context.Set<Blog>().Add(blog);
                    expectedBlogs.Add(blog);
                }

                Fixture.TestSqlLoggerFactory.Clear();

                await context.SaveChangesAsync();

                Assert.Contains(
                    minBatchSize == 3
                        ? RelationalResources.LogBatchReadyForExecution(new TestLogger<NpgsqlLoggingDefinitions>())
                            .GenerateMessage(3)
                        : RelationalResources.LogBatchSmallerThanMinBatchSize(new TestLogger<NpgsqlLoggingDefinitions>())
                            .GenerateMessage(3, 4),
                    Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));

                Assert.Equal(minBatchSize <= 3 ? 1 : 3, Fixture.TestSqlLoggerFactory.SqlStatements.Count);
            }, context => AssertDatabaseState(context, false, expectedBlogs));
    }

    private async Task AssertDatabaseState(DbContext context, bool clientOrder, List<Blog> expectedBlogs)
    {
        expectedBlogs = clientOrder
            ? expectedBlogs.OrderBy(b => b.Order).ToList()
            : expectedBlogs.OrderBy(b => b.Id).ToList();
        var actualBlogs = clientOrder
            ? await context.Set<Blog>().OrderBy(b => b.Order).ToListAsync()
            : expectedBlogs.OrderBy(b => b.Id).ToList();
        Assert.Equal(expectedBlogs.Count, actualBlogs.Count);

        for (var i = 0; i < actualBlogs.Count; i++)
        {
            var expected = expectedBlogs[i];
            var actual = actualBlogs[i];
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Order, actual.Order);
            Assert.Equal(expected.OwnerId, actual.OwnerId);
            Assert.Equal(expected.Version, actual.Version);
        }
    }

    private BloggingContext CreateContext()
        => (BloggingContext)Fixture.CreateContext();

    private Task ExecuteWithStrategyInTransactionAsync(
        Func<BloggingContext, Task> testOperation,
        Func<BloggingContext, Task> nestedTestOperation)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction, testOperation, nestedTestOperation);

    protected void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    private class BloggingContext(DbContextOptions options) : PoolableDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Owner>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedOnAdd();
                    b.Property(e => e.Version)
                        .HasColumnName("xmin")
                        .HasColumnType("xid")
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();
                });

            modelBuilder.Entity<Blog>().Property(b => b.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }

        // ReSharper disable once UnusedMember.Local
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Owner> Owners { get; set; }
    }

    private class Blog
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string OwnerId { get; set; }
        public Owner Owner { get; set; }
        public uint Version { get; set; }
    }

    private class Owner
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public uint Version { get; set; }
    }

    public class BatchingTestFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName { get; } = "BatchingTest";

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override Type ContextType { get; } = typeof(BloggingContext);

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Update.Name;

        protected override Task SeedAsync(PoolableDbContext context)
            => context.Database.EnsureCreatedResilientlyAsync();

        public DbContext CreateContext(int minBatchSize)
        {
            var optionsBuilder = new DbContextOptionsBuilder(CreateOptions());
            new NpgsqlDbContextOptionsBuilder(optionsBuilder).MinBatchSize(minBatchSize);
            return new BloggingContext(optionsBuilder.Options);
        }
    }
}
