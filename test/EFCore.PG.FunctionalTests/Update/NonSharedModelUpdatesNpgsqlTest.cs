using System.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Update;

public class NonSharedModelUpdatesNpgsqlTest(NonSharedFixture fixture) : NonSharedModelUpdatesTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    [ConditionalTheory]
    [InlineData(false, IsolationLevel.RepeatableRead)]
    [InlineData(true, IsolationLevel.RepeatableRead)]
    [InlineData(false, IsolationLevel.Serializable)]
    [InlineData(true, IsolationLevel.Serializable)]
    public async Task Serialization_failure_throws_concurrency_exception(bool async, IsolationLevel isolationLevel)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<Blog>(),
            onConfiguring: b => b.UseNpgsql(
                ((NpgsqlTestStore)NonSharedTestStore).ConnectionString,
                o => o.ExecutionStrategy(d => new NonRetryingExecutionStrategy(d))),
            seed: async context =>
            {
                context.Add(new Blog { Name = "Original" });
                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(isolationLevel);
        var blog = await context.Set<Blog>().SingleAsync();

        await using var otherContext = contextFactory.CreateDbContext();
        var otherBlog = await otherContext.Set<Blog>().SingleAsync();
        otherBlog.Name = "Other";
        await otherContext.SaveChangesAsync();

        blog.Name = "First";
        var exception = async
            ? await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync())
            : Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());

        Assert.Equal(PostgresErrorCodes.SerializationFailure, Assert.IsType<PostgresException>(exception.InnerException).SqlState);
        Assert.Same(blog, Assert.Single(exception.Entries).Entity);
    }

    [ConditionalTheory]
    [InlineData(false, PostgresErrorCodes.SerializationFailure)]
    [InlineData(true, PostgresErrorCodes.SerializationFailure)]
    [InlineData(false, PostgresErrorCodes.DeadlockDetected)]
    [InlineData(true, PostgresErrorCodes.DeadlockDetected)]
    [InlineData(false, PostgresErrorCodes.UniqueViolation)]
    [InlineData(true, PostgresErrorCodes.UniqueViolation)]
    public async Task Store_exception_in_later_batch_command_is_classified_before_retrying(bool async, string sqlState)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<Blog>(),
            onConfiguring: b => new NpgsqlDbContextOptionsBuilder(b).EnableRetryOnFailure(0),
            shouldLogCategory: category => category == DbLoggerCategory.Update.Name);

        await using var context = contextFactory.CreateDbContext();
        await context.Database.ExecuteSqlRawAsync(StoreExceptionTrigger);

        try
        {
            var blog = new Blog { Name = sqlState };
            context.AddRange(new Blog { Name = "Before" }, blog);

            var exception = async
                ? await Assert.ThrowsAnyAsync<Exception>(() => context.SaveChangesAsync())
                : Assert.ThrowsAny<Exception>(() => context.SaveChanges());

            DbUpdateException updateException;
            if (sqlState is PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected)
            {
                updateException = Assert.IsType<DbUpdateConcurrencyException>(
                    Assert.IsType<RetryLimitExceededException>(exception).InnerException);
                Assert.Contains(
                    ((TestSqlLoggerFactory)ListLoggerFactory).Log,
                    entry => entry.Id == CoreEventId.OptimisticConcurrencyException);
            }
            else
            {
                updateException = Assert.IsType<DbUpdateException>(exception);
                Assert.DoesNotContain(
                    ((TestSqlLoggerFactory)ListLoggerFactory).Log,
                    entry => entry.Id == CoreEventId.OptimisticConcurrencyException);
            }

            Assert.Equal(sqlState, Assert.IsType<PostgresException>(updateException.InnerException).SqlState);
            Assert.Same(blog, Assert.Single(updateException.Entries).Entity);
        }
        finally
        {
            await context.Database.ExecuteSqlRawAsync("DROP FUNCTION raise_store_exception() CASCADE");
        }
    }

    [ConditionalTheory]
    [InlineData(false, PostgresErrorCodes.SerializationFailure, 1)]
    [InlineData(true, PostgresErrorCodes.SerializationFailure, 1)]
    [InlineData(false, PostgresErrorCodes.DeadlockDetected, 1)]
    [InlineData(true, PostgresErrorCodes.DeadlockDetected, 1)]
    [InlineData(false, PostgresErrorCodes.SerializationFailure, 1000)]
    [InlineData(true, PostgresErrorCodes.SerializationFailure, 1000)]
    [InlineData(false, PostgresErrorCodes.DeadlockDetected, 1000)]
    [InlineData(true, PostgresErrorCodes.DeadlockDetected, 1000)]
    public async Task Suppressing_store_concurrency_exception_does_not_bypass_rollback(bool async, string sqlState, int maxBatchSize)
    {
        var interceptor = new SuppressingConcurrencyInterceptor();
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<Blog>(),
            onConfiguring: b =>
            {
                new NpgsqlDbContextOptionsBuilder(b)
                    .ExecutionStrategy(d => new NonRetryingExecutionStrategy(d))
                    .MaxBatchSize(maxBatchSize);
                b.AddInterceptors(interceptor);
            });

        await using var context = contextFactory.CreateDbContext();
        await context.Database.ExecuteSqlRawAsync(StoreExceptionTrigger);

        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            var previouslySaved = new Blog { Name = "Before" };
            context.Add(previouslySaved);
            await context.SaveChangesAsync();

            var unrelated = new Blog { Name = "Before" };
            var conflicting = new Blog { Name = sqlState };
            context.AddRange(unrelated, conflicting);

            var exception = async
                ? await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync())
                : Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());

            Assert.Equal(sqlState, Assert.IsType<PostgresException>(exception.InnerException).SqlState);
            Assert.Same(exception, interceptor.Exception);
            Assert.Equal(EntityState.Added, context.Entry(unrelated).State);
            Assert.Equal(EntityState.Added, context.Entry(conflicting).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(previouslySaved).State);

            await transaction.CommitAsync();

            var persisted = await context.Set<Blog>().AsNoTracking().SingleAsync();
            Assert.Equal(previouslySaved.Id, persisted.Id);
        }
        finally
        {
            await context.Database.ExecuteSqlRawAsync("DROP FUNCTION raise_store_exception() CASCADE");
        }
    }

    private class SuppressingConcurrencyInterceptor : SaveChangesInterceptor
    {
        public DbUpdateConcurrencyException? Exception { get; private set; }

        public override InterceptionResult ThrowingConcurrencyException(ConcurrencyExceptionEventData eventData, InterceptionResult result)
        {
            Exception = eventData.Exception;
            return InterceptionResult.Suppress();
        }

        public override ValueTask<InterceptionResult> ThrowingConcurrencyExceptionAsync(
            ConcurrencyExceptionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
            => new(ThrowingConcurrencyException(eventData, result));
    }

    private const string StoreExceptionTrigger = """
CREATE FUNCTION raise_store_exception() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF NEW."Name" <> 'Before' THEN
        RAISE EXCEPTION 'Store failure' USING ERRCODE = NEW."Name";
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER raise_store_exception AFTER INSERT ON "Blog"
FOR EACH ROW EXECUTE FUNCTION raise_store_exception();
""";
}
