using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlDatabaseCreatorTest
    {
        [Fact]
        public async Task Exists_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task ExistsAsync_returns_false_when_database_doesnt_exist()
        {
            await Exists_returns_false_when_database_doesnt_exist_test(async: true);
        }

        private static async Task Exists_returns_false_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: false))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        [Fact]
        public async Task Exists_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: false);
        }

        [Fact]
        public async Task ExistsAsync_returns_true_when_database_exists()
        {
            await Exists_returns_true_when_database_exists_test(async: true);
        }

        private static async Task Exists_returns_true_when_database_exists_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: true))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.True(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        [Fact]
        public async Task HasTables_throws_when_database_doesnt_exist()
        {
            await HasTables_throws_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task HasTablesAsync_throws_when_database_doesnt_exist()
        {
            await HasTables_throws_when_database_doesnt_exist_test(async: true);
        }

        private static async Task HasTables_throws_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: false))
            {
                await ((NpgsqlDatabaseCreatorTest.TestDatabaseCreator)NpgsqlDatabaseCreatorTest.GetDatabaseCreator(testDatabase)).ExecutionStrategyFactory.Create()
                    .ExecuteAsync(
                        (NpgsqlDatabaseCreatorTest.TestDatabaseCreator)NpgsqlDatabaseCreatorTest.GetDatabaseCreator(testDatabase),
                        async creator =>
                        {
                            var sqlState = async
                                ? (await Assert.ThrowsAsync<PostgresException>(() => creator.HasTablesAsyncBase())).SqlState
                                : Assert.Throws<PostgresException>(() => creator.HasTablesBase()).SqlState;

                            Assert.Equal(
                                "3D000", // Login failed error number
                                sqlState);
                        });
            }
        }

        [Fact]
        public async Task HasTables_returns_false_when_database_exists_but_has_no_tables()
        {
            await HasTables_returns_false_when_database_exists_but_has_no_tables_test(async: false);
        }

        [Fact]
        public async Task HasTablesAsync_returns_false_when_database_exists_but_has_no_tables()
        {
            await HasTables_returns_false_when_database_exists_but_has_no_tables_test(async: true);
        }

        private static async Task HasTables_returns_false_when_database_exists_but_has_no_tables_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: true))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.False(async
                    ? await ((TestDatabaseCreator)creator).HasTablesAsyncBase()
                    : ((TestDatabaseCreator)creator).HasTablesBase());
            }
        }

        [Fact]
        public async Task HasTables_returns_true_when_database_exists_and_has_any_tables()
        {
            await HasTables_returns_true_when_database_exists_and_has_any_tables_test(async: false);
        }

        [Fact]
        public async Task HasTablesAsync_returns_true_when_database_exists_and_has_any_tables()
        {
            await HasTables_returns_true_when_database_exists_and_has_any_tables_test(async: true);
        }

        private static async Task HasTables_returns_true_when_database_exists_and_has_any_tables_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: true))
            {
                await testDatabase.ExecuteNonQueryAsync("CREATE TABLE some_table (id SERIAL PRIMARY KEY)");

                var creator = GetDatabaseCreator(testDatabase);

                Assert.True(async ? await ((TestDatabaseCreator)creator).HasTablesAsyncBase() : ((TestDatabaseCreator)creator).HasTablesBase());
            }
        }

        [Fact]
        public async Task Delete_will_delete_database()
        {
            await Delete_will_delete_database_test(async: false);
        }

        [Fact]
        public async Task DeleteAsync_will_delete_database()
        {
            await Delete_will_delete_database_test(async: true);
        }

        private static async Task Delete_will_delete_database_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: true))
            {
                testDatabase.CloseConnection();

                var creator = GetDatabaseCreator(testDatabase);

                Assert.True(async ? await creator.ExistsAsync() : creator.Exists());

                if (async)
                {
                    await creator.DeleteAsync();
                }
                else
                {
                    creator.Delete();
                }

                Assert.False(async ? await creator.ExistsAsync() : creator.Exists());
            }
        }

        [Fact]
        public async Task Delete_throws_when_database_doesnt_exist()
        {
            await Delete_throws_when_database_doesnt_exist_test(async: false);
        }

        [Fact]
        public async Task DeleteAsync_throws_when_database_doesnt_exist()
        {
            await Delete_throws_when_database_doesnt_exist_test(async: true);
        }

        private static async Task Delete_throws_when_database_doesnt_exist_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: false))
            {
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

        [Fact]
        public async Task CreateTables_creates_schema_in_existing_database()
        {
            await CreateTables_creates_schema_in_existing_database_test(async: false);
        }

        [Fact]
        public async Task CreateTablesAsync_creates_schema_in_existing_database()
        {
            await CreateTables_creates_schema_in_existing_database_test(async: true);
        }

        private static async Task CreateTables_creates_schema_in_existing_database_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: true))
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkNpgsql()
                    .BuildServiceProvider();

                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(serviceProvider)
                    .UseNpgsql(testDatabase.ConnectionString, b => b.ApplyConfiguration());

                using (var context = new BloggingContext(optionsBuilder.Options))
                {
                    var creator = (RelationalDatabaseCreator)context.GetService<IDatabaseCreator>();

                    if (async)
                    {
                        await creator.CreateTablesAsync();
                    }
                    else
                    {
                        creator.CreateTables();
                    }

                    if (testDatabase.ConnectionState != ConnectionState.Open)
                    {
                        await testDatabase.OpenConnectionAsync();
                    }

                    var tables = await testDatabase.QueryAsync<string>("SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('pg_catalog', 'information_schema')");
                    Assert.Equal(1, tables.Count());
                    Assert.Equal("Blogs", tables.Single());

                    var columns = await testDatabase.QueryAsync<string>("SELECT table_name || '.' || column_name FROM information_schema.columns WHERE table_name='Blogs'");
                    Assert.Equal(2, columns.Count());
                    Assert.True(columns.Any(c => c == "Blogs.Id"));
                    Assert.True(columns.Any(c => c == "Blogs.Name"));
                }
            }
        }

        [Fact]
        public async Task CreateTables_throws_if_database_does_not_exist()
        {
            await CreateTables_throws_if_database_does_not_exist_test(async: false);
        }

        [Fact]
        public async Task CreateTablesAsync_throws_if_database_does_not_exist()
        {
            await CreateTables_throws_if_database_does_not_exist_test(async: true);
        }

        private static async Task CreateTables_throws_if_database_does_not_exist_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: false))
            {
                var creator = GetDatabaseCreator(testDatabase);

                var ex = async
                    ? await Assert.ThrowsAsync<PostgresException>(() => creator.CreateTablesAsync())
                    : Assert.Throws<PostgresException>(() => creator.CreateTables());
                Assert.Equal(
                    "3D000", // Login failed error number
                    ex.SqlState);
            }
        }

        [Fact]
        public async Task Create_creates_physical_database_but_not_tables()
        {
            await Create_creates_physical_database_but_not_tables_test(async: false);
        }

        [Fact]
        public async Task CreateAsync_creates_physical_database_but_not_tables()
        {
            await Create_creates_physical_database_but_not_tables_test(async: true);
        }

        private static async Task Create_creates_physical_database_but_not_tables_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: false))
            {
                var creator = GetDatabaseCreator(testDatabase);

                Assert.False(creator.Exists());

                if (async)
                {
                    await creator.CreateAsync();
                }
                else
                {
                    creator.Create();
                }

                Assert.True(creator.Exists());

                if (testDatabase.ConnectionState != ConnectionState.Open)
                {
                    await testDatabase.OpenConnectionAsync();
                }

                Assert.Equal(0, (await testDatabase.QueryAsync<string>("SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('pg_catalog', 'information_schema')")).Count());
            }
        }

        [Fact]
        public async Task Create_throws_if_database_already_exists()
        {
            await Create_throws_if_database_already_exists_test(async: false);
        }

        [Fact]
        public async Task CreateAsync_throws_if_database_already_exists()
        {
            await Create_throws_if_database_already_exists_test(async: true);
        }

        private static async Task Create_throws_if_database_already_exists_test(bool async)
        {
            using (var testDatabase = NpgsqlTestStore.CreateScratch(createDatabase: true))
            {
                var creator = GetDatabaseCreator(testDatabase);

                var ex = async
                    ? await Assert.ThrowsAsync<PostgresException>(() => creator.CreateAsync())
                    : Assert.Throws<PostgresException>(() => creator.Create());
                Assert.Equal(
                    "42P04", // Database with given name already exists
                    ex.SqlState);
            }
        }

        private static IServiceProvider CreateContextServices(NpgsqlTestStore testStore)
            => ((IInfrastructure<IServiceProvider>)new BloggingContext(
                    new DbContextOptionsBuilder()
                        .UseNpgsql(testStore.ConnectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(new ServiceCollection()
                            .AddEntityFrameworkNpgsql()
                            //.AddScoped<NpgsqlExecutionStrategyFactory, TestNpgsqlExecutionStrategyFactory>()
                            .AddScoped<IRelationalDatabaseCreator, TestDatabaseCreator>().BuildServiceProvider()).Options))
                .Instance;

        private static IRelationalDatabaseCreator GetDatabaseCreator(NpgsqlTestStore testStore)
            => CreateContextServices(testStore).GetRequiredService<IRelationalDatabaseCreator>();

        /*
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestNpgsqlExecutionStrategyFactory : NpgsqlExecutionStrategyFactory
        {
            public TestNpgsqlExecutionStrategyFactory(IDbContextOptions options, ICurrentDbContext currentDbContext, ILogger<IExecutionStrategy> logger)
                : base(options, currentDbContext, logger)
            {
            }

            protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyContext context) => NoopExecutionStrategy.Instance;
        }*/

        private class BloggingContext : DbContext
        {
            public BloggingContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
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

            public bool HasTablesBase() => HasTables();

            public Task<bool> HasTablesAsyncBase(CancellationToken cancellationToken = default(CancellationToken))
                => HasTablesAsync(cancellationToken);

            public IExecutionStrategyFactory ExecutionStrategyFactory => Dependencies.ExecutionStrategyFactory;
        }
    }
}
