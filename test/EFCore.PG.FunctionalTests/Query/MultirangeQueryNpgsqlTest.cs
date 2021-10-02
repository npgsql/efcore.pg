using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    [MinimumPostgresVersion(14, 0)] // Multiranges were introduced in PostgreSQL 14
    public class MultirangeQueryNpgsqlTest : IClassFixture<MultirangeQueryNpgsqlTest.MultirangeQueryNpgsqlFixture>
    {
        private MultirangeQueryNpgsqlFixture Fixture { get; }

        // ReSharper disable once UnusedParameter.Local
        public MultirangeQueryNpgsqlTest(MultirangeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Operators

        [ConditionalFact]
        public void Multirange_Contains_value()
        {
            using var context = CreateContext();

            var value = 3;
            var id = context.TestEntities
                .Single(x => x.Multirange.Contains(value))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__value_0='3'

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" @> @__value_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_Contains_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(1, 2) };

            var id = context.TestEntities
                .Single(x => x.Multirange.Contains(multirange))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[1,2]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" @> @__multirange_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_Contains_range()
        {
            using var context = CreateContext();

            var range = new NpgsqlRange<int>(1, 2);

            var id = context.TestEntities
                .Single(x => x.Multirange.Contains(range))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__range_0='[1,2]' (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" @> @__range_0
LIMIT 2");
        }


        [ConditionalFact]
        public void Multirange_ContainedBy_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(1, 2) };

            var id = context.TestEntities
                .Single(x => multirange.ContainedBy(x.Multirange))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[1,2]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE @__multirange_0 <@ t.""Multirange""
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_equals_operator()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(0, 5), new(7, 10) };

            var id = context.TestEntities
                .Single(x => x.Multirange == multirange)
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[0,5]', '[7,10]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" = @__multirange_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_equals_method()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(0, 5), new(7, 10) };

            var id = context.TestEntities
                .Single(x => x.Multirange.Equals(multirange))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[0,5]', '[7,10]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" = @__multirange_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_not_equals_operator()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(0, 5), new(7, 10) };

            var id = context.TestEntities
                .Single(x => x.Multirange != multirange)
                .Id;
            Assert.Equal(2, id);

            AssertSql(
                @"@__multirange_0={ '[0,5]', '[7,10]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE (t.""Multirange"" <> @__multirange_0) OR (t.""Multirange"" IS NULL)
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_not_equals_method()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(0, 5), new(7, 10) };

            var id = context.TestEntities
                .Single(x => !x.Multirange.Equals(multirange))
                .Id;
            Assert.Equal(2, id);

            AssertSql(
                @"@__multirange_0={ '[0,5]', '[7,10]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE (t.""Multirange"" <> @__multirange_0) OR (t.""Multirange"" IS NULL)
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_Overlaps_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(-3, 0), new(100, 101) };

            var id = context.TestEntities
                .Single(x => x.Multirange.Overlaps(multirange))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[-3,0]', '[100,101]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" && @__multirange_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_Overlaps_range()
        {
            using var context = CreateContext();

            var range = new NpgsqlRange<int>(-3, 0);

            var id = context.TestEntities
                .Single(x => x.Multirange.Overlaps(range))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__range_0='[-3,0]' (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" && @__range_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_IsStrictlyLeftOf_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(11, 13), new(15, 16) };

            var id = context.TestEntities
                .Single(x => x.Multirange.IsStrictlyLeftOf(multirange))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[11,13]', '[15,16]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" << @__multirange_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_IsStrictlyLeftOf_range()
        {
            using var context = CreateContext();

            var range = new NpgsqlRange<int>(11, 13);

            var id = context.TestEntities
                .Single(x => x.Multirange.IsStrictlyLeftOf(range))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__range_0='[11,13]' (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" << @__range_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_IsStrictlyRightOf_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(-10, -7), new(-5, 3) };

            var id = context.TestEntities
                .Single(x => x.Multirange.IsStrictlyRightOf(multirange))
                .Id;
            Assert.Equal(2, id);

            AssertSql(
                @"@__multirange_0={ '[-10,-7]', '[-5,3]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" >> @__multirange_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_IsStrictlyRightOf_range()
        {
            using var context = CreateContext();

            var range = new NpgsqlRange<int>(-5, 3);

            var id = context.TestEntities
                .Single(x => x.Multirange.IsStrictlyRightOf(range))
                .Id;
            Assert.Equal(2, id);

            AssertSql(
                @"@__range_0='[-5,3]' (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" >> @__range_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_DoesNotExtendLeftOf_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(2, 7), new(13, 18) };

            var id = context.TestEntities
                .Single(x => x.Multirange.DoesNotExtendLeftOf(multirange))
                .Id;
            Assert.Equal(2, id);

            AssertSql(
                @"@__multirange_0={ '[2,7]', '[13,18]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" &> @__multirange_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_DoesNotExtendLeftOf_range()
        {
            using var context = CreateContext();

            var range = new NpgsqlRange<int>(2, 7);

            var id = context.TestEntities
                .Single(x => x.Multirange.DoesNotExtendLeftOf(range))
                .Id;
            Assert.Equal(2, id);

            AssertSql(
                @"@__range_0='[2,7]' (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" &> @__range_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_DoesNotExtendRightOf_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(-5, -3), new(13, 18) };

            var id = context.TestEntities
                .Single(x => x.Multirange.DoesNotExtendRightOf(multirange))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[-5,-3]', '[13,18]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" &< @__multirange_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_DoesNotExtendRightOf_range()
        {
            using var context = CreateContext();

            var range = new NpgsqlRange<int>(13, 18);

            var id = context.TestEntities
                .Single(x => x.Multirange.DoesNotExtendRightOf(range))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__range_0='[13,18]' (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" &< @__range_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_IsAdjacentTo_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(-5, -4), new(-2, -1) };

            var id = context.TestEntities
                .Single(x => x.Multirange.IsAdjacentTo(multirange))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[-5,-4]', '[-2,-1]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" -|- @__multirange_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_IsAdjacentTo_range()
        {
            using var context = CreateContext();

            var range = new NpgsqlRange<int>(-2, -1);

            var id = context.TestEntities
                .Single(x => x.Multirange.IsAdjacentTo(range))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__range_0='[-2,-1]' (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" -|- @__range_0
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_Union_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(-5, -1) };

            var id = context.TestEntities
                .Single(x => x.Multirange.Union(multirange) == new NpgsqlRange<int>[] { new(-5, 5), new(7, 10) })
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[-5,-1]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" + @__multirange_0 = '{[-5,5], [7,10]}'::int4multirange
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_Intersect_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(-5, 1), new(9, 13) };

            var id = context.TestEntities
                .Single(x => x.Multirange.Intersect(multirange) == new NpgsqlRange<int>[] { new(0, 1), new(9, 10) })
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[-5,1]', '[9,13]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" * @__multirange_0 = '{[0,1], [9,10]}'::int4multirange
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_Except_multirange()
        {
            using var context = CreateContext();

            var multirange = new NpgsqlRange<int>[] { new(2, 3) };

            var id = context.TestEntities
                .Single(x => x.Multirange.Except(multirange) == new NpgsqlRange<int>[] { new(0, 1), new(4, 5), new(7, 10) })
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"@__multirange_0={ '[2,3]' } (DbType = Object)

SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE t.""Multirange"" - @__multirange_0 = '{[0,1], [4,5], [7,10]}'::int4multirange
LIMIT 2");
        }

        #endregion

        #region Functions

        // TODO: lower, upper, lower_inc, upper_inc, lower_inf, upper_inf

        [ConditionalFact]
        public void Multirange_is_empty()
        {
            using var context = CreateContext();

            var id = context.TestEntities
                .Single(x => x.Multirange.Intersect(new NpgsqlRange<int>[] { new(18, 19) }).Any())
                .Id;
            Assert.Equal(2, id);

            AssertSql(
                @"SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE NOT (isempty(t.""Multirange"" * '{[18,19]}'::int4multirange))
LIMIT 2");
        }

        [ConditionalFact]
        public void Multirange_merge()
        {
            using var context = CreateContext();

            var id = context.TestEntities
                .Single(x => x.Multirange.Merge() == new NpgsqlRange<int>(0, 10))
                .Id;
            Assert.Equal(1, id);

            AssertSql(
                @"SELECT t.""Id"", t.""Multirange""
FROM ""TestEntities"" AS t
WHERE range_merge(t.""Multirange"") = '[0,10]'::int4range
LIMIT 2");
        }

        #endregion

        #region Fixtures

        public class MultirangeQueryNpgsqlFixture : SharedStoreFixtureBase<MultirangeContext>
        {
            protected override string StoreName => "MultirangeQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override void Seed(MultirangeContext context) => MultirangeContext.Seed(context);
        }

        public class MultirangeTestEntity
        {
            public int Id { get; set; }
            public NpgsqlRange<int>[] Multirange { get; set; }
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class MultirangeContext : PoolableDbContext
        {
            public DbSet<MultirangeTestEntity> TestEntities { get; set; }

            public MultirangeContext(DbContextOptions options) : base(options) {}

            public static void Seed(MultirangeContext context)
            {
                context.TestEntities.AddRange(
                    new MultirangeTestEntity { Id = 1, Multirange = new NpgsqlRange<int>[] { new(0, 5), new(7, 10) } },
                    new MultirangeTestEntity { Id = 2, Multirange = new NpgsqlRange<int>[] { new(4, 8), new(13, 20) } });

                context.SaveChanges();
            }
        }

        #endregion

        #region Helpers

        protected MultirangeContext CreateContext() => Fixture.CreateContext();

        #endregion
    }
}
