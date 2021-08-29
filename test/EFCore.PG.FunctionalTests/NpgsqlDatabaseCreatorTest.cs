using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

// ReSharper disable HeapView.CanAvoidClosure
// ReSharper disable MethodHasAsyncOverload

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlDatabaseCreatorExistsTest : NpgsqlDatabaseCreatorTest
    {
        [ConditionalTheory]
        [InlineData(true, true, false)]
        [InlineData(false, false, false)]
        [InlineData(true, true, true)]
        [InlineData(false, false, true)]
        public async Task Returns_false_when_database_does_not_exist(bool async, bool ambientTransaction, bool useCanConnect)
        {
            using var testDatabase = NpgsqlTestStore.Create("NonExisting");
            using var context = new BloggingContext(testDatabase);
            var creator = GetDatabaseCreator(context);

            await context.Database.CreateExecutionStrategy().ExecuteAsync(
                async () =>
                {
                    using (CreateTransactionScope(ambientTransaction))
                    {
                        if (useCanConnect)
                        {
                            Assert.False(async ? await creator.CanConnectAsync() : creator.CanConnect());
                        }
                        else
                        {
                            Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
                        }
                    }
                });

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }

        [ConditionalTheory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        public async Task Returns_true_when_database_exists(bool async, bool ambientTransaction, bool useCanConnect)
        {
            using var testDatabase = NpgsqlTestStore.GetOrCreateInitialized("ExistingBlogging");
            using var context = new BloggingContext(testDatabase);
            var creator = GetDatabaseCreator(context);

            await context.Database.CreateExecutionStrategy().ExecuteAsync(
                async () =>
                {
                    using (CreateTransactionScope(ambientTransaction))
                    {
                        if (useCanConnect)
                        {
                            Assert.True(async ? await creator.CanConnectAsync() : creator.CanConnect());
                        }
                        else
                        {
                            Assert.True(async ? await creator.ExistsAsync() : creator.Exists());
                        }
                    }
                });

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }
    }

    public class NpgsqlDatabaseCreatorEnsureDeletedTest : NpgsqlDatabaseCreatorTest
    {
        [ConditionalTheory]
        [InlineData(true, true, true)]
        [InlineData(false, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        public async Task Deletes_database(bool async, bool open, bool ambientTransaction)
        {
            using var testDatabase = NpgsqlTestStore.CreateInitialized("EnsureDeleteBlogging");
            if (!open)
            {
                testDatabase.CloseConnection();
            }

            using var context = new BloggingContext(testDatabase);
            var creator = GetDatabaseCreator(context);

            Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

            await GetExecutionStrategy(testDatabase).ExecuteAsync(
                async () =>
                {
                    using (CreateTransactionScope(ambientTransaction))
                    {
                        if (async)
                        {
                            Assert.True(await context.Database.EnsureDeletedAsync());
                        }
                        else
                        {
                            Assert.True(context.Database.EnsureDeleted());
                        }
                    }
                });

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

            Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        private static async Task Noop_when_database_does_not_exist_test(bool async)
        {
            using var testDatabase = NpgsqlTestStore.Create("NonExisting");
            using var context = new BloggingContext(testDatabase);
            var creator = GetDatabaseCreator(context);

            Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

            if (async)
            {
                Assert.False(await creator.EnsureDeletedAsync());
            }
            else
            {
                Assert.False(creator.EnsureDeleted());
            }

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

            Assert.False(async ? await creator.ExistsAsync() : creator.Exists());

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }
    }

    public class NpgsqlDatabaseCreatorEnsureCreatedTest : NpgsqlDatabaseCreatorTest
    {
        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public Task Creates_schema_in_existing_database(bool async, bool ambientTransaction)
            => Creates_physical_database_and_schema_test((true, async, ambientTransaction));

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public Task Creates_physical_database_and_schema(bool async, bool ambientTransaction)
            => Creates_new_physical_database_and_schema_test(async, ambientTransaction);

        private static Task Creates_new_physical_database_and_schema_test(bool async, bool ambientTransaction)
            => Creates_physical_database_and_schema_test((false, async, ambientTransaction));

        private static async Task Creates_physical_database_and_schema_test(
            (bool CreateDatabase, bool Async, bool ambientTransaction) options)
        {
            var (createDatabase, async, ambientTransaction) = options;
            using var testDatabase = NpgsqlTestStore.Create("EnsureCreatedTest");
            using var context = new BloggingContext(testDatabase);
            if (createDatabase)
            {
                testDatabase.Initialize(null, (Func<DbContext>)null);
            }
            else
            {
                testDatabase.DeleteDatabase();
            }

            var creator = GetDatabaseCreator(context);

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

            using (CreateTransactionScope(ambientTransaction))
            {
                if (async)
                {
                    Assert.True(await creator.EnsureCreatedAsync());
                }
                else
                {
                    Assert.True(creator.EnsureCreated());
                }
            }

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

            if (testDatabase.ConnectionState != ConnectionState.Open)
            {
                await testDatabase.OpenConnectionAsync();
            }

            var tables = testDatabase.Query<string>(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND NOT TABLE_NAME LIKE ANY ('{pg_%,sql_%}')").ToList();
            Assert.Single(tables);
            Assert.Equal("Blogs", tables.Single());

            var columns = testDatabase.Query<string>(
                    "SELECT TABLE_NAME || '.' || COLUMN_NAME || ' (' || DATA_TYPE || ')' FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Blogs' ORDER BY TABLE_NAME, COLUMN_NAME")
                .ToArray();
            Assert.Equal(14, columns.Length);

            Assert.Equal(
                new[]
                {
                    "Blogs.AndChew (bytea)",
                    "Blogs.AndRow (bytea)",
                    "Blogs.Cheese (text)",
                    "Blogs.ErMilan (integer)",
                    "Blogs.Fuse (smallint)",
                    "Blogs.George (boolean)",
                    "Blogs.Key1 (text)",
                    "Blogs.Key2 (bytea)",
                    "Blogs.NotFigTime (timestamp with time zone)",
                    "Blogs.On (real)",
                    "Blogs.OrNothing (double precision)",
                    "Blogs.TheGu (uuid)",
                    "Blogs.ToEat (smallint)",
                    "Blogs.WayRound (bigint)"
                },
                columns);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Noop_when_database_exists_and_has_schema(bool async)
        {
            using var testDatabase = NpgsqlTestStore.CreateInitialized("InitializedBlogging");
            using var context = new BloggingContext(testDatabase);
            context.Database.EnsureCreatedResiliently();

            if (async)
            {
                Assert.False(await context.Database.EnsureCreatedResilientlyAsync());
            }
            else
            {
                Assert.False(context.Database.EnsureCreatedResiliently());
            }

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }
    }

    public class NpgsqlDatabaseCreatorHasTablesTest : NpgsqlDatabaseCreatorTest
    {
        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Throws_when_database_does_not_exist(bool async)
        {
            using var testDatabase = NpgsqlTestStore.GetOrCreate("NonExisting");
            var databaseCreator = GetDatabaseCreator(testDatabase);
            await databaseCreator.ExecutionStrategyFactory.Create().ExecuteAsync(
                databaseCreator,
                async creator =>
                {
                    var errorNumber = async
                        ? (await Assert.ThrowsAsync<PostgresException>(() => creator.HasTablesAsyncBase())).SqlState
                        : Assert.Throws<PostgresException>(() => creator.HasTablesBase()).SqlState;

                    Assert.Equal(PostgresErrorCodes.InvalidCatalogName, errorNumber);
                });
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task Returns_false_when_database_exists_but_has_no_tables(bool async, bool ambientTransaction)
        {
            using var testDatabase = NpgsqlTestStore.GetOrCreateInitialized("Empty");
            var creator = GetDatabaseCreator(testDatabase);

            await GetExecutionStrategy(testDatabase).ExecuteAsync(
                async () =>
                {
                    using (CreateTransactionScope(ambientTransaction))
                    {
                        Assert.False(async ? await creator.HasTablesAsyncBase() : creator.HasTablesBase());
                    }
                });
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task Returns_true_when_database_exists_and_has_any_tables(bool async, bool ambientTransaction)
        {
            using var testDatabase = NpgsqlTestStore.GetOrCreate("ExistingTables")
                .InitializeNpgsql(null, t => new BloggingContext(t), null);
            var creator = GetDatabaseCreator(testDatabase);

            await GetExecutionStrategy(testDatabase).ExecuteAsync(
                async () =>
                {
                    using (CreateTransactionScope(ambientTransaction))
                    {
                        Assert.True(async ? await creator.HasTablesAsyncBase() : creator.HasTablesBase());
                    }
                });
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [RequiresPostgis]
        public async Task Returns_false_when_database_exists_and_has_only_postgis_tables(bool async, bool ambientTransaction)
        {
            using var testDatabase = NpgsqlTestStore.GetOrCreateInitialized("Empty");
            testDatabase.ExecuteNonQuery("CREATE EXTENSION IF NOT EXISTS postgis");

            var creator = GetDatabaseCreator(testDatabase);

            await GetExecutionStrategy(testDatabase).ExecuteAsync(
                async () =>
                {
                    using (CreateTransactionScope(ambientTransaction))
                    {
                        Assert.False(async ? await creator.HasTablesAsyncBase() : creator.HasTablesBase());
                    }
                });
        }
    }

    public class NpgsqlDatabaseCreatorDeleteTest : NpgsqlDatabaseCreatorTest
    {
        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public static async Task Deletes_database(bool async, bool ambientTransaction)
        {
            using var testDatabase = NpgsqlTestStore.CreateInitialized("DeleteBlogging");
            testDatabase.CloseConnection();

            var creator = GetDatabaseCreator(testDatabase);

            Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

            using (CreateTransactionScope(ambientTransaction))
            {
                if (async)
                {
                    await creator.DeleteAsync();
                }
                else
                {
                    creator.Delete();
                }
            }

            Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Throws_when_database_does_not_exist(bool async)
        {
            using var testDatabase = NpgsqlTestStore.GetOrCreate("NonExistingBlogging");
            var creator = GetDatabaseCreator(testDatabase);

            if (async)
            {
                await Assert.ThrowsAsync<PostgresException>(() => creator.DeleteAsync());
            }
            else
            {
                Assert.Throws<PostgresException>(() => creator.Delete());
            }
        }
    }

    public class NpgsqlDatabaseCreatorCreateTablesTest : NpgsqlDatabaseCreatorTest
    {
        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task Creates_schema_in_existing_database_test(bool async, bool ambientTransaction)
        {
            using var testDatabase = NpgsqlTestStore.GetOrCreateInitialized("ExistingBlogging" + (async ? "Async" : ""));
            using var context = new BloggingContext(testDatabase);
            var creator = GetDatabaseCreator(context);

            using (CreateTransactionScope(ambientTransaction))
            {
                if (async)
                {
                    await creator.CreateTablesAsync();
                }
                else
                {
                    creator.CreateTables();
                }
            }

            if (testDatabase.ConnectionState != ConnectionState.Open)
            {
                await testDatabase.OpenConnectionAsync();
            }

            var tables = (await testDatabase.QueryAsync<string>(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND NOT TABLE_NAME LIKE ANY ('{pg_%,sql_%}')")).ToList();
            Assert.Single(tables);
            Assert.Equal("Blogs", tables.Single());

            var columns = (await testDatabase.QueryAsync<string>(
                "SELECT TABLE_NAME || '.' || COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Blogs'")).ToList();
            Assert.Equal(14, columns.Count);
            Assert.Contains(columns, c => c == "Blogs.Key1");
            Assert.Contains(columns, c => c == "Blogs.Key2");
            Assert.Contains(columns, c => c == "Blogs.Cheese");
            Assert.Contains(columns, c => c == "Blogs.ErMilan");
            Assert.Contains(columns, c => c == "Blogs.George");
            Assert.Contains(columns, c => c == "Blogs.TheGu");
            Assert.Contains(columns, c => c == "Blogs.NotFigTime");
            Assert.Contains(columns, c => c == "Blogs.ToEat");
            Assert.Contains(columns, c => c == "Blogs.OrNothing");
            Assert.Contains(columns, c => c == "Blogs.Fuse");
            Assert.Contains(columns, c => c == "Blogs.WayRound");
            Assert.Contains(columns, c => c == "Blogs.On");
            Assert.Contains(columns, c => c == "Blogs.AndChew");
            Assert.Contains(columns, c => c == "Blogs.AndRow");
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Throws_if_database_does_not_exist(bool async)
        {
            using var testDatabase = NpgsqlTestStore.GetOrCreate("NonExisting");
            var creator = GetDatabaseCreator(testDatabase);

            var errorNumber
                = async
                    ? (await Assert.ThrowsAsync<PostgresException>(() => creator.CreateTablesAsync())).SqlState
                    : Assert.Throws<PostgresException>(() => creator.CreateTables()).SqlState;

            Assert.Equal(PostgresErrorCodes.InvalidCatalogName, errorNumber);
        }

        [ConditionalFact]
        public void GenerateCreateScript_works()
        {
            using var context = new BloggingContext("Data Source=foo");
            var script = context.Database.GenerateCreateScript();
            Assert.Equal(
                @"CREATE TABLE ""Blogs"" ("
                + _eol
                + @"    ""Key1"" text NOT NULL,"
                + _eol
                + @"    ""Key2"" bytea NOT NULL,"
                + _eol
                + @"    ""Cheese"" text NULL,"
                + _eol
                + @"    ""ErMilan"" integer NOT NULL,"
                + _eol
                + @"    ""George"" boolean NOT NULL,"
                + _eol
                + @"    ""TheGu"" uuid NOT NULL,"
                + _eol
                + @"    ""NotFigTime"" timestamp with time zone NOT NULL,"
                + _eol
                + @"    ""ToEat"" smallint NOT NULL,"
                + _eol
                + @"    ""OrNothing"" double precision NOT NULL,"
                + _eol
                + @"    ""Fuse"" smallint NOT NULL,"
                + _eol
                + @"    ""WayRound"" bigint NOT NULL,"
                + _eol
                + @"    ""On"" real NOT NULL,"
                + _eol
                + @"    ""AndChew"" bytea NULL,"
                + _eol
                + @"    ""AndRow"" bytea NULL,"
                + _eol
                + @"    CONSTRAINT ""PK_Blogs"" PRIMARY KEY (""Key1"", ""Key2"")"
                + _eol
                + ");"
                + _eol
                + _eol
                + _eol,
                script);
        }

        private static readonly string _eol = Environment.NewLine;
    }

    public class NpgsqlDatabaseCreatorCreateTest : NpgsqlDatabaseCreatorTest
    {
        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task Creates_physical_database_but_not_tables(bool async, bool ambientTransaction)
        {
            using var testDatabase = NpgsqlTestStore.GetOrCreate("CreateTest");
            var creator = GetDatabaseCreator(testDatabase);

            creator.EnsureDeleted();

            await GetExecutionStrategy(testDatabase).ExecuteAsync(
                async () =>
                {
                    using (CreateTransactionScope(ambientTransaction))
                    {
                        if (async)
                        {
                            await creator.CreateAsync();
                        }
                        else
                        {
                            creator.Create();
                        }
                    }
                });

            Assert.True(creator.Exists());

            if (testDatabase.ConnectionState != ConnectionState.Open)
            {
                await testDatabase.OpenConnectionAsync();
            }

            Assert.Empty(
                await testDatabase.QueryAsync<string>(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND NOT TABLE_NAME LIKE ANY ('{pg_%,sql_%}')"));
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Throws_if_database_already_exists(bool async)
        {
            using var testDatabase = NpgsqlTestStore.GetOrCreateInitialized("ExistingBlogging");
            var creator = GetDatabaseCreator(testDatabase);

            var ex = async
                ? await Assert.ThrowsAsync<PostgresException>(() => creator.CreateAsync())
                : Assert.Throws<PostgresException>(() => creator.Create());
            Assert.Equal(PostgresErrorCodes.DuplicateDatabase, ex.SqlState);
        }
    }

    // When database creation/drop happens in parallel, there seem to be some deadlocks on the PostgreSQL side
    // which make the tests run significantly slower. This makes all the test suites run in serial.
    [Collection("NpgsqlDatabaseCreatorTest")]
    public class NpgsqlDatabaseCreatorTest
    {
        protected static IDisposable CreateTransactionScope(bool useTransaction)
            => TestStore.CreateTransactionScope(useTransaction);

        protected static TestDatabaseCreator GetDatabaseCreator(NpgsqlTestStore testStore)
            => GetDatabaseCreator(testStore.ConnectionString);

        protected static TestDatabaseCreator GetDatabaseCreator(string connectionString)
            => GetDatabaseCreator(new BloggingContext(connectionString));

        protected static TestDatabaseCreator GetDatabaseCreator(BloggingContext context)
            => (TestDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();

        protected static IExecutionStrategy GetExecutionStrategy(NpgsqlTestStore testStore)
            => new BloggingContext(testStore).GetService<IExecutionStrategyFactory>().Create();

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestNpgsqlExecutionStrategyFactory : NpgsqlExecutionStrategyFactory
        {
            public TestNpgsqlExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
                : base(dependencies)
            {
            }

            protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyDependencies dependencies)
                => new NonRetryingExecutionStrategy(dependencies);
        }

        private static IServiceProvider CreateServiceProvider()
            => new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddScoped<IExecutionStrategyFactory, TestNpgsqlExecutionStrategyFactory>()
                .AddScoped<IRelationalDatabaseCreator, TestDatabaseCreator>()
                .BuildServiceProvider();

        protected class BloggingContext : DbContext
        {
            private readonly string _connectionString;

            public BloggingContext(NpgsqlTestStore testStore)
                : this(testStore.ConnectionString)
            {
            }

            public BloggingContext(string connectionString)
                => _connectionString = connectionString;

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseNpgsql(_connectionString, b => b
                        .ApplyConfiguration()
                        .SetPostgresVersion(TestEnvironment.PostgresVersion))
                    .UseInternalServiceProvider(CreateServiceProvider());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(
                            e => new { e.Key1, e.Key2 });
                        b.Property(e => e.AndRow).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                    });

            public DbSet<Blog> Blogs { get; set; }
        }

        public class Blog
        {
            public string Key1 { get; set; }
            public byte[] Key2 { get; set; }
            public string Cheese { get; set; }
            public int ErMilan { get; set; }
            public bool George { get; set; }
            public Guid TheGu { get; set; }
            public DateTime NotFigTime { get; set; }
            public byte ToEat { get; set; }
            public double OrNothing { get; set; }
            public short Fuse { get; set; }
            public long WayRound { get; set; }
            public float On { get; set; }
            public byte[] AndChew { get; set; }
            public byte[] AndRow { get; set; }
        }

        public class TestDatabaseCreator : NpgsqlDatabaseCreator
        {
            public TestDatabaseCreator(
                RelationalDatabaseCreatorDependencies dependencies,
                INpgsqlRelationalConnection connection,
                IRawSqlCommandBuilder rawSqlCommandBuilder)
                : base(dependencies, connection, rawSqlCommandBuilder)
            {
            }

            public bool HasTablesBase()
                => HasTables();

            public Task<bool> HasTablesAsyncBase(CancellationToken cancellationToken = default)
                => HasTablesAsync(cancellationToken);

            public IExecutionStrategyFactory ExecutionStrategyFactory
                => Dependencies.ExecutionStrategyFactory;
        }
    }
}
