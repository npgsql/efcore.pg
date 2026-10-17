namespace Microsoft.EntityFrameworkCore.Migrations;

public class NpgsqlHistoryRepositoryTest
{
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Missing_database_does_not_log_an_error(bool async)
    {
        await using var store = NpgsqlTestStore.Create("HistoryRepositoryMissing");
        using var logger = new ListLoggerFactory();
        await using var context = CreateContext(store, logger: logger);
        var history = context.GetService<IHistoryRepository>();

        Assert.False(async ? await history.ExistsAsync() : history.Exists());
        Assert.Empty(async ? await history.GetAppliedMigrationsAsync() : history.GetAppliedMigrations());
        Assert.DoesNotContain(logger.Log, e => e.Level == LogLevel.Error);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Creation_in_user_transaction_can_be_rolled_back(bool async, bool script)
    {
        await using var store = NpgsqlTestStore.Create("HistoryRepositoryTransaction");
        using var logger = new ListLoggerFactory();
        await using var context = CreateContext(store, "history.schema'\"", "History.Table'\"", logger);
        var history = context.GetService<IHistoryRepository>();
        await context.Database.EnsureDeletedAsync();
        await context.GetService<IRelationalDatabaseCreator>().CreateAsync();

        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            Assert.False(async ? await history.ExistsAsync() : history.Exists());
            Assert.Empty(async ? await history.GetAppliedMigrationsAsync() : history.GetAppliedMigrations());

            if (script)
            {
                if (async)
                {
                    await context.Database.ExecuteSqlRawAsync(history.GetCreateIfNotExistsScript());
                }
                else
                {
                    context.Database.ExecuteSqlRaw(history.GetCreateIfNotExistsScript());
                }
            }
            else if (async)
            {
                await history.CreateIfNotExistsAsync();
            }
            else
            {
                history.CreateIfNotExists();
            }

            Assert.True(async ? await history.ExistsAsync() : history.Exists());
            await context.Database.ExecuteSqlRawAsync(history.GetInsertScript(new HistoryRow("Migration1", "11.0.0")));
            Assert.Equal(
                "Migration1",
                Assert.Single(async ? await history.GetAppliedMigrationsAsync() : history.GetAppliedMigrations()).MigrationId);

            await transaction.RollbackAsync();
        }

        Assert.False(async ? await history.ExistsAsync() : history.Exists());
        Assert.Empty(async ? await history.GetAppliedMigrationsAsync() : history.GetAppliedMigrations());
        Assert.DoesNotContain(logger.Log, e => e.Level == LogLevel.Error);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Creation_respects_search_path(bool async)
    {
        await using var store = NpgsqlTestStore.Create("HistoryRepositorySearchPath");
        await using var existingContext = CreateContext(store, "second");
        await existingContext.Database.EnsureDeletedAsync();
        await existingContext.GetService<IRelationalDatabaseCreator>().CreateAsync();
        await store.ExecuteNonQueryAsync("CREATE SCHEMA first; CREATE SCHEMA second;");
        var existingHistory = existingContext.GetService<IHistoryRepository>();
        await existingHistory.CreateIfNotExistsAsync();
        await existingContext.Database.ExecuteSqlRawAsync(existingHistory.GetInsertScript(new HistoryRow("Migration1", "11.0.0")));

        using var logger = new ListLoggerFactory();
        await using var context = CreateContext(store, logger: logger);
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync("SET search_path = first, second");
        var history = context.GetService<IHistoryRepository>();

        Assert.True(async ? await history.ExistsAsync() : history.Exists());

        if (async)
        {
            await history.CreateIfNotExistsAsync();
        }
        else
        {
            history.CreateIfNotExists();
        }

        Assert.Equal(
            "Migration1",
            Assert.Single(async ? await history.GetAppliedMigrationsAsync() : history.GetAppliedMigrations()).MigrationId);
        Assert.True(await store.ExecuteScalarAsync<bool>("SELECT pg_catalog.to_regclass('first.\"__EFMigrationsHistory\"') IS NULL"));

        await context.Database.ExecuteSqlRawAsync("SET search_path = first");
        Assert.False(async ? await history.ExistsAsync() : history.Exists());
        Assert.Empty(async ? await history.GetAppliedMigrationsAsync() : history.GetAppliedMigrations());

        if (async)
        {
            await history.CreateIfNotExistsAsync();
        }
        else
        {
            history.CreateIfNotExists();
        }

        Assert.True(async ? await history.ExistsAsync() : history.Exists());
        Assert.Empty(async ? await history.GetAppliedMigrationsAsync() : history.GetAppliedMigrations());
        Assert.DoesNotContain(logger.Log, e => e.Level == LogLevel.Error);
    }

    [ConditionalTheory]
    [InlineData(false, false, false, null)]
    [InlineData(true, false, false, null)]
    [InlineData(false, false, false, "new_schema")]
    [InlineData(true, false, false, "new_schema")]
    [InlineData(false, true, false, null)]
    [InlineData(true, true, false, "new_schema")]
    [InlineData(false, false, true, "new_schema")]
    [InlineData(true, true, true, "new_schema")]
    [InlineData(false, false, true, null)]
    [InlineData(true, true, true, null)]
    public async Task Concurrent_creation(bool async, bool script, bool differentTables, string? schema)
    {
        await using var store = NpgsqlTestStore.Create("HistoryRepositoryConcurrent");
        using var logger = new ListLoggerFactory();
        await using var context1 = CreateContext(store, schema, logger: logger);
        await using var context2 = CreateContext(store, schema, differentTables ? "OtherHistory" : HistoryRepository.DefaultTableName, logger);
        await context1.Database.EnsureDeletedAsync();
        await context1.GetService<IRelationalDatabaseCreator>().CreateAsync();
        await context2.Database.OpenConnectionAsync();

        Assert.False(await context1.GetService<IHistoryRepository>().ExistsAsync());
        Assert.False(await context2.GetService<IHistoryRepository>().ExistsAsync());

        await using var userTransaction = await context2.Database.BeginTransactionAsync();
        await context2.Database.ExecuteSqlRawAsync(
            """CREATE TABLE "UserWork" ("Value" integer); INSERT INTO "UserWork" VALUES (1);""");

        var creation = Task.CompletedTask;
        try
        {
            await using var transaction = await context1.Database.BeginTransactionAsync();
            await Create(context1);
            creation = Create(context2);

            if (differentTables && schema is null)
            {
                await creation;
            }
            else
            {
                var processId1 = ((NpgsqlConnection)context1.Database.GetDbConnection()).ProcessID;
                var processId2 = ((NpgsqlConnection)context2.Database.GetDbConnection()).ProcessID;
                while (!creation.IsCompleted
                       && !await store.ExecuteScalarAsync<bool>(
                           $"SELECT {processId1} = ANY(pg_catalog.pg_blocking_pids({processId2}))"))
                {
                    await Task.Delay(10);
                }

                Assert.False(creation.IsCompleted);
            }

            await transaction.CommitAsync();
        }
        finally
        {
            await creation;
        }

        Assert.True(await context1.GetService<IHistoryRepository>().ExistsAsync());
        Assert.True(await context2.GetService<IHistoryRepository>().ExistsAsync());
        Assert.Equal(1, await context2.Database.SqlQueryRaw<int>("SELECT \"Value\" FROM \"UserWork\"").SingleAsync());
        var history2 = context2.GetService<IHistoryRepository>();
        await context2.Database.ExecuteSqlRawAsync(history2.GetInsertScript(new HistoryRow("Migration1", "11.0.0")));
        Assert.Single(await history2.GetAppliedMigrationsAsync());
        Assert.DoesNotContain(logger.Log, e => e.Level == LogLevel.Error);

        await userTransaction.RollbackAsync();
        Assert.Empty(await context1.GetService<IHistoryRepository>().GetAppliedMigrationsAsync());
        Assert.Equal(!differentTables, await history2.ExistsAsync());
        Assert.True(await store.ExecuteScalarAsync<bool>("SELECT pg_catalog.to_regclass('\"UserWork\"') IS NULL"));

        Task Create(DbContext context)
            => Task.Run(
                async () =>
                {
                    var history = context.GetService<IHistoryRepository>();
                    if (script)
                    {
                        if (async)
                        {
                            await context.Database.ExecuteSqlRawAsync(history.GetCreateIfNotExistsScript());
                        }
                        else
                        {
                            context.Database.ExecuteSqlRaw(history.GetCreateIfNotExistsScript());
                        }
                    }
                    else if (async)
                    {
                        await history.CreateIfNotExistsAsync();
                    }
                    else
                    {
                        history.CreateIfNotExists();
                    }
                });
    }

    [ConditionalFact]
    public async Task Concurrent_creation_after_schema_recreation()
    {
        await using var store = NpgsqlTestStore.Create("HistoryRepositoryConcurrentSchema");
        using var logger = new ListLoggerFactory();
        var contexts = Enumerable.Range(0, 16)
            .Select(i => CreateContext(store, "new_schema", $"__EFMigrationsHistory{i % 8}", logger))
            .ToArray();

        try
        {
            await contexts[0].Database.EnsureDeletedAsync();
            await contexts[0].GetService<IRelationalDatabaseCreator>().CreateAsync();
            await store.ExecuteNonQueryAsync("""CREATE TABLE "UserWork" ("Value" integer)""");
            await Task.WhenAll(contexts.Select(c => c.Database.OpenConnectionAsync()));
            var histories = contexts.Select(c => c.GetService<IHistoryRepository>()).ToArray();
            var scripts = histories.Select(h => h.GetCreateIfNotExistsScript()).ToArray();

            for (var round = 0; round < 2; round++)
            {
                await store.ExecuteNonQueryAsync("DROP SCHEMA IF EXISTS new_schema CASCADE");
                var transactions = await Task.WhenAll(contexts.Select(c => c.Database.BeginTransactionAsync()));
                await Task.WhenAll(contexts.Select(c => c.Database.ExecuteSqlRawAsync("""INSERT INTO "UserWork" VALUES (1)""")));
                await Task.WhenAll(contexts.Select((context, i) => Create(context, i)));

                async Task Create(DbContext context, int i)
                {
                    await using var transaction = transactions[i];
                    await context.Database.ExecuteSqlRawAsync(scripts[i]);
                    Assert.True(await histories[i].ExistsAsync());
                    Assert.Empty(await histories[i].GetAppliedMigrationsAsync());
                    await transaction.CommitAsync();
                }
            }

            Assert.Equal(32L, await store.ExecuteScalarAsync<long>("""SELECT count(*) FROM "UserWork" """));
            Assert.DoesNotContain(logger.Log, e => e.Level == LogLevel.Error);
        }
        finally
        {
            foreach (var context in contexts)
            {
                await context.DisposeAsync();
            }
        }
    }

    private static DbContext CreateContext(
        NpgsqlTestStore store,
        string? schema = null,
        string tableName = HistoryRepository.DefaultTableName,
        ListLoggerFactory? logger = null)
        => new(
            new DbContextOptionsBuilder()
                .UseNpgsql(store.ConnectionString, b => b.MigrationsHistoryTable(tableName, schema))
                .UseLoggerFactory(logger)
                .Options);
}
