using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class PostgresOptimizationsQueryTest : QueryTestBase<PostgresOptimizationsQueryTest.PostgresOptimizationsQueryFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public PostgresOptimizationsQueryTest(PostgresOptimizationsQueryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Row_value_comparison_two_items(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e => e.X > 5 || e.X == 5 && e.Y > 6),
                entryCount: 1);

            AssertSql(
                @"SELECT e.""Id"", e.""X"", e.""Y"", e.""Z""
FROM ""Entities"" AS e
WHERE (e.""X"", e.""Y"") > (5, 6)");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Row_value_comparison_not_rewritten_with_incompatible_operators(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e => e.X > 5 || e.X == 5 && e.Y < 6),
                entryCount: 0);

            AssertSql(
                @"SELECT e.""Id"", e.""X"", e.""Y"", e.""Z""
FROM ""Entities"" AS e
WHERE (e.""X"" > 5) OR ((e.""X"" = 5) AND (e.""Y"" < 6))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Row_value_comparison_not_rewritten_with_incompatible_operands(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e => e.Z > 5 || e.X == 5 && e.Y > 6),
                entryCount: 2);

            AssertSql(
                @"SELECT e.""Id"", e.""X"", e.""Y"", e.""Z""
FROM ""Entities"" AS e
WHERE (e.""Z"" > 5) OR ((e.""X"" = 5) AND (e.""Y"" > 6))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Row_value_comparison_three_items(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e => e.X > 5 || e.X == 5 && (e.Y > 6 || e.Y == 6 && e.Z > 7)),
                entryCount: 1);

            AssertSql(
                @"SELECT e.""Id"", e.""X"", e.""Y"", e.""Z""
FROM ""Entities"" AS e
WHERE (e.""X"", e.""Y"", e.""Z"") > (5, 6, 7)");
        }

        #region Support

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class PostgresOptimizationsQueryContext : PoolableDbContext
        {
            public DbSet<Entity> Entities { get; set; }

            public PostgresOptimizationsQueryContext(DbContextOptions options) : base(options) {}

            public static void Seed(PostgresOptimizationsQueryContext context)
            {
                context.Entities.AddRange(PostgresOptimizationsData.CreateEntities());
                context.SaveChanges();
            }
        }

        public class Entity
        {
            public int Id { get; set; }

            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
        }

        public class PostgresOptimizationsQueryFixture : SharedStoreFixtureBase<PostgresOptimizationsQueryContext>, IQueryFixtureBase
        {
            protected override string StoreName => "PostgresOptimizationsQueryTest";

            // Set the PostgreSQL TimeZone parameter to something local, to ensure that operations which take TimeZone into account
            // don't depend on the database's time zone, and also that operations which shouldn't take TimeZone into account indeed
            // don't.
            protected override ITestStoreFactory TestStoreFactory
                => NpgsqlTestStoreFactory.WithConnectionStringOptions("-c TimeZone=Europe/Berlin");

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            private PostgresOptimizationsData _expectedData;

            protected override void Seed(PostgresOptimizationsQueryContext context) => PostgresOptimizationsQueryContext.Seed(context);

            public Func<DbContext> GetContextCreator()
                => CreateContext;

            public ISetSource GetExpectedData()
                => _expectedData ??= new PostgresOptimizationsData();

            public IReadOnlyDictionary<Type, object> GetEntitySorters()
                => new Dictionary<Type, Func<object, object>> { { typeof(Entity), e => ((Entity)e)?.Id } }
                    .ToDictionary(e => e.Key, e => (object)e.Value);

            public IReadOnlyDictionary<Type, object> GetEntityAsserters()
                => new Dictionary<Type, Action<object, object>>
                {
                    {
                        typeof(Entity), (e, a) =>
                        {
                            Assert.Equal(e is null, a is null);
                            if (a is not null)
                            {
                                var ee = (Entity)e;
                                var aa = (Entity)a;

                                Assert.Equal(ee.Id, aa.Id);

                                Assert.Equal(ee.X, aa.X);
                                Assert.Equal(ee.Y, aa.Y);
                                Assert.Equal(ee.Z, aa.Z);
                            }
                        }
                    }
                }.ToDictionary(e => e.Key, e => (object)e.Value);
        }

        protected class PostgresOptimizationsData : ISetSource
        {
            public IReadOnlyList<Entity> Entities { get; }

            public PostgresOptimizationsData()
                => Entities = CreateEntities();

            public IQueryable<TEntity> Set<TEntity>()
                where TEntity : class
            {
                if (typeof(TEntity) == typeof(Entity))
                {
                    return (IQueryable<TEntity>)Entities.AsQueryable();
                }

                throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
            }

            public static IReadOnlyList<Entity> CreateEntities()
                => new List<Entity>
                {
                    new()
                    {
                        Id = 1,
                        X = 5,
                        Y = 7,
                        Z = 9
                    },
                    new()
                    {
                        Id = 2,
                        X = 4,
                        Y = 10,
                        Z = 10
                    }
                };
        }

        #endregion
    }
}
