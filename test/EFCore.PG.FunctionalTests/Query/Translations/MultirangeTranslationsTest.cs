﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

[MinimumPostgresVersion(14, 0)] // Multiranges were introduced in PostgreSQL 14
public class MultirangeTranslationsTest : IClassFixture<MultirangeTranslationsTest.MultirangeQueryNpgsqlFixture>
{
    private MultirangeQueryNpgsqlFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public MultirangeTranslationsTest(MultirangeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Operators

    [ConditionalFact]
    public void Contains_value()
    {
        using var context = CreateContext();

        var value = 3;
        var id = context.TestEntities
            .Single(x => x.IntMultirange.Contains(value))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@value='3'

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" @> @value
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Contains_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(1, 2) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Contains(multirange))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[1,2]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" @> @multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Contains_range()
    {
        using var context = CreateContext();

        var range = new NpgsqlRange<int>(1, 2);

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Contains(range))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@range='[1,2]' (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" @> @range
LIMIT 2
""");
    }

    [ConditionalFact]
    public void ContainedBy_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(1, 2) };

        var id = context.TestEntities
            .Single(x => multirange.ContainedBy(x.IntMultirange))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[1,2]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE @multirange <@ t."IntMultirange"
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Equals_operator()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(0, 5), new(7, 10) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange == multirange)
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[0,5]'
'[7,10]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" = @multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Equals_method()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(0, 5), new(7, 10) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Equals(multirange))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[0,5]'
'[7,10]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" = @multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Overlaps_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(-3, 0), new(100, 101) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Overlaps(multirange))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[-3,0]'
'[100,101]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" && @multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Overlaps_range()
    {
        using var context = CreateContext();

        var range = new NpgsqlRange<int>(-3, 0);

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Overlaps(range))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@range='[-3,0]' (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" && @range
LIMIT 2
""");
    }

    [ConditionalFact]
    public void IsStrictlyLeftOf_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(11, 13), new(15, 16) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.IsStrictlyLeftOf(multirange))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[11,13]'
'[15,16]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" << @multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void IsStrictlyLeftOf_range()
    {
        using var context = CreateContext();

        var range = new NpgsqlRange<int>(11, 13);

        var id = context.TestEntities
            .Single(x => x.IntMultirange.IsStrictlyLeftOf(range))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@range='[11,13]' (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" << @range
LIMIT 2
""");
    }

    [ConditionalFact]
    public void IsStrictlyRightOf_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(-10, -7), new(-5, 3) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.IsStrictlyRightOf(multirange))
            .Id;
        Assert.Equal(2, id);

        AssertSql(
            """
@multirange={ '[-10,-7]'
'[-5,3]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" >> @multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void IsStrictlyRightOf_range()
    {
        using var context = CreateContext();

        var range = new NpgsqlRange<int>(-5, 3);

        var id = context.TestEntities
            .Single(x => x.IntMultirange.IsStrictlyRightOf(range))
            .Id;
        Assert.Equal(2, id);

        AssertSql(
            """
@range='[-5,3]' (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" >> @range
LIMIT 2
""");
    }

    [ConditionalFact]
    public void DoesNotExtendLeftOf_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(2, 7), new(13, 18) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.DoesNotExtendLeftOf(multirange))
            .Id;
        Assert.Equal(2, id);

        AssertSql(
            """
@multirange={ '[2,7]'
'[13,18]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" &> @multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void DoesNotExtendLeftOf_range()
    {
        using var context = CreateContext();

        var range = new NpgsqlRange<int>(2, 7);

        var id = context.TestEntities
            .Single(x => x.IntMultirange.DoesNotExtendLeftOf(range))
            .Id;
        Assert.Equal(2, id);

        AssertSql(
            """
@range='[2,7]' (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" &> @range
LIMIT 2
""");
    }

    [ConditionalFact]
    public void DoesNotExtendRightOf_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(-5, -3), new(13, 18) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.DoesNotExtendRightOf(multirange))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[-5,-3]'
'[13,18]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" &< @multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void DoesNotExtendRightOf_range()
    {
        using var context = CreateContext();

        var range = new NpgsqlRange<int>(13, 18);

        var id = context.TestEntities
            .Single(x => x.IntMultirange.DoesNotExtendRightOf(range))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@range='[13,18]' (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" &< @range
LIMIT 2
""");
    }

    [ConditionalFact]
    public void IsAdjacentTo_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(-5, -4), new(-2, -1) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.IsAdjacentTo(multirange))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[-5,-4]'
'[-2,-1]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" -|- @multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void IsAdjacentTo_range()
    {
        using var context = CreateContext();

        var range = new NpgsqlRange<int>(-2, -1);

        var id = context.TestEntities
            .Single(x => x.IntMultirange.IsAdjacentTo(range))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@range='[-2,-1]' (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" -|- @range
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Union_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(-5, -1) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Union(multirange) == new NpgsqlRange<int>[] { new(-5, 5), new(7, 10) })
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[-5,-1]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" + @multirange = '{[-5,5], [7,10]}'::int4multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Intersect_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(-5, 1), new(9, 13) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Intersect(multirange) == new NpgsqlRange<int>[] { new(0, 1), new(9, 10) })
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[-5,1]'
'[9,13]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" * @multirange = '{[0,1], [9,10]}'::int4multirange
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Intersect_aggregate()
    {
        using var context = CreateContext();

        var intersection = context.TestEntities
            .Where(x => x.Id == 1 || x.Id == 2)
            .GroupBy(x => true)
            .Select(g => g.Select(x => x.IntMultirange).RangeIntersectAgg())
            .Single();

        Assert.Equal([new(4, true, 6, false), new(7, true, 9, false)], intersection);

        AssertSql(
            """
SELECT range_intersect_agg(t0."IntMultirange")
FROM (
    SELECT t."IntMultirange", TRUE AS "Key"
    FROM "TestEntities" AS t
    WHERE t."Id" IN (1, 2)
) AS t0
GROUP BY t0."Key"
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Except_multirange()
    {
        using var context = CreateContext();

        var multirange = new NpgsqlRange<int>[] { new(2, 3) };

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Except(multirange) == new NpgsqlRange<int>[] { new(0, 1), new(4, 5), new(7, 10) })
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
@multirange={ '[2,3]' } (DbType = Object)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" - @multirange = '{[0,1], [4,5], [7,10]}'::int4multirange
LIMIT 2
""");
    }

    #endregion

    #region Functions

    // TODO: lower, upper, lower_inc, upper_inc, lower_inf, upper_inf

    [ConditionalFact]
    public void Is_empty()
    {
        using var context = CreateContext();

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Intersect(new NpgsqlRange<int>[] { new(18, 19) }).Any())
            .Id;
        Assert.Equal(2, id);

        AssertSql(
            """
SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE NOT (isempty(t."IntMultirange" * '{[18,19]}'::int4multirange))
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Merge()
    {
        using var context = CreateContext();

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Merge() == new NpgsqlRange<int>(0, 10))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE range_merge(t."IntMultirange") = '[0,10]'::int4range
LIMIT 2
""");
    }

    #endregion

    #region Built-in multiranges

    [ConditionalFact]
    public void IntMultirange()
    {
        using var context = CreateContext();
        var value = 3;
        var result = context.TestEntities.Single(x => x.IntMultirange.Contains(value));
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@value='3'

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" @> @value
LIMIT 2
""");
    }

    [ConditionalFact]
    public void LongMultirange()
    {
        using var context = CreateContext();
        var value = 3;
        var result = context.TestEntities.Single(x => x.LongMultirange.Contains(value));
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@p='3'

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."LongMultirange" @> @p
LIMIT 2
""");
    }

    [ConditionalFact]
    public void DecimalMultirange()
    {
        using var context = CreateContext();
        var value = 3;
        var result = context.TestEntities.Single(x => x.DecimalMultirange.Contains(value));
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@p='3'

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."DecimalMultirange" @> @p
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Datemultirange_DateOnly()
    {
        using var context = CreateContext();
        var value = new DateOnly(2020, 1, 3);
        var result = context.TestEntities.Single(x => x.DateOnlyDateMultirange.Contains(value));
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@value='01/03/2020' (DbType = Date)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."DateOnlyDateMultirange" @> @value
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Datemultirange_DateTime()
    {
        using var context = CreateContext();
        var value = new DateTime(2020, 1, 3);
        var result = context.TestEntities.Single(x => x.DateTimeDateMultirange.Contains(value));
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@value='2020-01-03T00:00:00.0000000' (DbType = Date)

SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."DateTimeDateMultirange" @> @value
LIMIT 2
""");
    }

    #endregion Built-in multiranges

    #region Query as primitive collection

    [ConditionalFact]
    public void Contains()
    {
        using var context = CreateContext();

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Contains(new NpgsqlRange<int>(0, 5)))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE t."IntMultirange" @> '[0,5]'::int4range
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Skip_Contains()
    {
        using var context = CreateContext();

        var id = context.TestEntities
            .Single(x => x.IntMultirange.Skip(1).Contains(new NpgsqlRange<int>(7, 10)))
            .Id;
        Assert.Equal(1, id);

        AssertSql(
            """
SELECT t."Id", t."DateOnlyDateMultirange", t."DateTimeDateMultirange", t."DecimalMultirange", t."IntMultirange", t."LongMultirange"
FROM "TestEntities" AS t
WHERE '[7,10]'::int4range IN (
    SELECT i.value
    FROM unnest(t."IntMultirange") AS i(value)
    OFFSET 1
)
LIMIT 2
""");
    }

    #endregion

    #region Fixtures

    public class MultirangeQueryNpgsqlFixture : SharedStoreFixtureBase<MultirangeContext>
    {
        protected override string StoreName
            => "MultirangeQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(MultirangeContext context)
            => MultirangeContext.SeedAsync(context);
    }

    public class MultirangeTestEntity
    {
        public int Id { get; set; }
        public NpgsqlRange<int>[] IntMultirange { get; set; } = null!;
        public NpgsqlRange<long>[] LongMultirange { get; set; } = null!;
        public NpgsqlRange<decimal>[] DecimalMultirange { get; set; } = null!;
        public NpgsqlRange<DateOnly>[] DateOnlyDateMultirange { get; set; } = null!;

        [Column(TypeName = "datemultirange")]
        public NpgsqlRange<DateTime>[] DateTimeDateMultirange { get; set; } = null!;
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class MultirangeContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<MultirangeTestEntity> TestEntities { get; set; }

        public static async Task SeedAsync(MultirangeContext context)
        {
            context.TestEntities.AddRange(
                new MultirangeTestEntity
                {
                    Id = 1,
                    IntMultirange = [new(0, 5), new(7, 10)],
                    LongMultirange = [new(0, 5), new(7, 10)],
                    DecimalMultirange = [new(0, 5), new(7, 10)],
                    DateOnlyDateMultirange =
                    [
                        new(new DateOnly(2020, 1, 1), new DateOnly(2020, 1, 5)),
                            new(new DateOnly(2020, 1, 7), new DateOnly(2020, 1, 10))
                    ],
                    DateTimeDateMultirange =
                    [
                        new(new DateTime(2020, 1, 1), new DateTime(2020, 1, 5)), new(new DateTime(2020, 1, 7), new DateTime(2020, 1, 10))
                    ]
                },
                new MultirangeTestEntity
                {
                    Id = 2,
                    IntMultirange = [new(4, 8), new(13, 20)],
                    LongMultirange = [new(4, 8), new(13, 20)],
                    DecimalMultirange = [new(4, 8), new(13, 20)],
                    DateOnlyDateMultirange =
                    [
                        new(new DateOnly(2020, 1, 4), new DateOnly(2020, 1, 8)),
                            new(new DateOnly(2020, 1, 13), new DateOnly(2020, 1, 20))
                    ],
                    DateTimeDateMultirange =
                    [
                        new(new DateTime(2020, 1, 4), new DateTime(2020, 1, 8)), new(new DateTime(2020, 1, 13), new DateTime(2020, 1, 20))
                    ]
                });

            await context.SaveChangesAsync();
        }
    }

    #endregion

    #region Helpers

    protected MultirangeContext CreateContext()
        => Fixture.CreateContext();

    #endregion
}
