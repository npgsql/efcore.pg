using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

// Note: timestamp range tests are in TimestampQueryTest
public class RangeQueryNpgsqlTest : IClassFixture<RangeQueryNpgsqlTest.RangeQueryNpgsqlFixture>
{
    private RangeQueryNpgsqlFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public RangeQueryNpgsqlTest(RangeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
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
        var result = context.RangeTestEntities.Single(x => x.IntRange.Contains(3));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" @> 3
LIMIT 2");
    }

    [ConditionalFact]
    public void Contains_range()
    {
        using var context = CreateContext();

        var range = new NpgsqlRange<int>(8, 13);
        var result = context.RangeTestEntities.Single(x => x.IntRange.Contains(range));
        Assert.Equal(2, result.Id);

        AssertSql(
            @"@__range_0='[8,13]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" @> @__range_0
LIMIT 2");
    }

    [ConditionalFact]
    public void ContainedBy()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(8, 13);
        var result = context.RangeTestEntities.Single(x => range.ContainedBy(x.IntRange));
        Assert.Equal(2, result.Id);

        AssertSql(
            @"@__range_0='[8,13]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE @__range_0 <@ r.""IntRange""
LIMIT 2");
    }

    [ConditionalFact]
    public void Equals_operator()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(1, 10);
        var result = context.RangeTestEntities.Single(x => x.IntRange == range);
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__range_0='[1,10]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" = @__range_0
LIMIT 2");
    }

    [ConditionalFact]
    public void Equals_method()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(1, 10);
        var result = context.RangeTestEntities.Single(x => x.IntRange.Equals(range));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__range_0='[1,10]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" = @__range_0
LIMIT 2");
    }

    [ConditionalFact]
    public void Overlaps_range()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(-5, 4);
        var result = context.RangeTestEntities.Single(x => x.IntRange.Overlaps(range));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__range_0='[-5,4]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" && @__range_0
LIMIT 2");
    }

    [ConditionalFact]
    public void IsStrictlyLeftOf_range()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(11, 15);
        var result = context.RangeTestEntities.Single(x => x.IntRange.IsStrictlyLeftOf(range));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__range_0='[11,15]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" << @__range_0
LIMIT 2");
    }

    [ConditionalFact]
    public void IsStrictlyRightOf_range()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(0, 4);
        var result = context.RangeTestEntities.Single(x => x.IntRange.IsStrictlyRightOf(range));
        Assert.Equal(2, result.Id);

        AssertSql(
            @"@__range_0='[0,4]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" >> @__range_0
LIMIT 2");
    }

    [ConditionalFact]
    public void DoesNotExtendLeftOf()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(2, 20);
        var result = context.RangeTestEntities.Single(x => range.DoesNotExtendLeftOf(x.IntRange));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__range_0='[2,20]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE @__range_0 &> r.""IntRange""
LIMIT 2");
    }

    [ConditionalFact]
    public void DoesNotExtendRightOf()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(1, 13);
        var result = context.RangeTestEntities.Single(x => range.DoesNotExtendRightOf(x.IntRange));
        Assert.Equal(2, result.Id);

        AssertSql(
            @"@__range_0='[1,13]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE @__range_0 &< r.""IntRange""
LIMIT 2");
    }

    [ConditionalFact]
    public void IsAdjacentTo()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(2, 4);
        var result = context.RangeTestEntities.Single(x => range.IsAdjacentTo(x.IntRange));
        Assert.Equal(2, result.Id);

        AssertSql(
            @"@__range_0='[2,4]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE @__range_0 -|- r.""IntRange""
LIMIT 2");
    }

    [ConditionalFact]
    public void Union()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(-2, 7);
        var result = context.RangeTestEntities.Single(x => x.IntRange.Union(range) == new NpgsqlRange<int>(-2, 10));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__range_0='[-2,7]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" + @__range_0 = '[-2,10]'::int4range
LIMIT 2");
    }

    [ConditionalFact]
    public void Intersect()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(-2, 3);
        var result = context.RangeTestEntities.Single(x => x.IntRange.Intersect(range) == new NpgsqlRange<int>(1, 3));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__range_0='[-2,3]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" * @__range_0 = '[1,3]'::int4range
LIMIT 2");
    }

    [ConditionalFact]
    public void Except()
    {
        using var context = CreateContext();
        var range = new NpgsqlRange<int>(1, 2);
        var result = context.RangeTestEntities.Single(x => x.IntRange.Except(range) == new NpgsqlRange<int>(3, 10));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__range_0='[1,2]' (DbType = Object)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" - @__range_0 = '[3,10]'::int4range
LIMIT 2");
    }

    #endregion Operators

    #region Functions

    [ConditionalFact]
    public void LowerBound()
    {
        using var context = CreateContext();
        var result = context.RangeTestEntities.Single(x => x.IntRange.LowerBound == 1);
        Assert.Equal(1, result.Id);

        AssertSql(
            @"SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE lower(r.""IntRange"") = 1
LIMIT 2");
    }

    [ConditionalFact]
    public void UpperBound()
    {
        using var context = CreateContext();
        var result = context.RangeTestEntities.Single(x => x.IntRange.UpperBound == 16); // PG normalizes to exclusive
        Assert.Equal(2, result.Id);

        AssertSql(
            @"SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE upper(r.""IntRange"") = 16
LIMIT 2");
    }

    [ConditionalFact]
    public void IsEmpty()
    {
        using var context = CreateContext();
        var result = context.RangeTestEntities.Single(x => x.IntRange.Intersect(new(1, 2)).IsEmpty);
        Assert.Equal(2, result.Id);

        AssertSql(
            @"SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE isempty(r.""IntRange"" * '[1,2]'::int4range)
LIMIT 2");
    }

    [ConditionalFact]
    public void LowerBoundIsInclusive()
    {
        using var context = CreateContext();
        var count = context.RangeTestEntities.Count(x => !x.IntRange.LowerBoundIsInclusive);
        Assert.Equal(0, count);

        AssertSql(
            @"SELECT COUNT(*)::INT
FROM ""RangeTestEntities"" AS r
WHERE NOT (lower_inc(r.""IntRange""))");
    }

    [ConditionalFact]
    public void UpperBoundIsInclusive()
    {
        using var context = CreateContext();
        var count = context.RangeTestEntities.Count(x => x.IntRange.UpperBoundIsInclusive);
        Assert.Equal(0, count);

        AssertSql(
            @"SELECT COUNT(*)::INT
FROM ""RangeTestEntities"" AS r
WHERE upper_inc(r.""IntRange"")");
    }

    [ConditionalFact]
    public void LowerBoundInfinite()
    {
        using var context = CreateContext();
        var count = context.RangeTestEntities.Count(x => x.IntRange.LowerBoundInfinite);
        Assert.Equal(0, count);

        AssertSql(
            @"SELECT COUNT(*)::INT
FROM ""RangeTestEntities"" AS r
WHERE lower_inf(r.""IntRange"")");
    }

    [ConditionalFact]
    public void UpperBoundInfinite()
    {
        using var context = CreateContext();
        var count = context.RangeTestEntities.Count(x => x.IntRange.UpperBoundInfinite);
        Assert.Equal(0, count);

        AssertSql(
            @"SELECT COUNT(*)::INT
FROM ""RangeTestEntities"" AS r
WHERE upper_inf(r.""IntRange"")");
    }

    [ConditionalFact]
    public void Merge()
    {
        using var context = CreateContext();
        var result = context.RangeTestEntities.Single(x => x.IntRange.Merge(new(12, 13)) == new NpgsqlRange<int>(1, 13));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE range_merge(r.""IntRange"", '[12,13]'::int4range) = '[1,13]'::int4range
LIMIT 2");
    }

    #endregion Functions

    #region Built-in ranges

    [ConditionalFact]
    public void IntRange()
    {
        using var context = CreateContext();
        var result = context.RangeTestEntities.Single(x => x.IntRange.Contains(3));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""IntRange"" @> 3
LIMIT 2");
    }

    [ConditionalFact]
    public void LongRange()
    {
        using var context = CreateContext();
        var value = 3;
        var result = context.RangeTestEntities.Single(x => x.LongRange.Contains(value));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__p_0='3'

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""LongRange"" @> @__p_0
LIMIT 2");
    }

    [ConditionalFact]
    public void DecimalRange()
    {
        using var context = CreateContext();
        var value = 3;
        var result = context.RangeTestEntities.Single(x => x.DecimalRange.Contains(value));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__p_0='3'

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""DecimalRange"" @> @__p_0
LIMIT 2");
    }

    [ConditionalFact]
    public void Daterange_DateOnly()
    {
        using var context = CreateContext();
        var value = new DateOnly(2020, 1, 3);
        var result = context.RangeTestEntities.Single(x => x.DateOnlyDateRange.Contains(value));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__value_0='01/03/2020' (DbType = Date)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""DateOnlyDateRange"" @> @__value_0
LIMIT 2");
    }

    [ConditionalFact]
    public void Daterange_DateTime()
    {
        using var context = CreateContext();
        var value = new DateTime(2020, 1, 3);
        var result = context.RangeTestEntities.Single(x => x.DateTimeDateRange.Contains(value));
        Assert.Equal(1, result.Id);

        AssertSql(
            @"@__value_0='2020-01-03T00:00:00.0000000' (DbType = DateTime)

SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE r.""DateTimeDateRange"" @> @__value_0
LIMIT 2");
    }

    #endregion Built-in ranges

    #region User-defined ranges

    [ConditionalFact]
    public void User_defined()
    {
        using var context = CreateContext();
        var result = context.RangeTestEntities.Single(x => x.UserDefinedRange.UpperBound > 12.0);
        Assert.Equal(2, result.Id);
        Assert.Equal(5.0, result.UserDefinedRange.LowerBound);
        Assert.Equal(15.0, result.UserDefinedRange.UpperBound);

        AssertSql(
            @"SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE upper(r.""UserDefinedRange"") > 12.0
LIMIT 2");
    }

    [ConditionalFact]
    public void User_defined_and_schema_qualified()
    {
        using var context = CreateContext();
        var result = context.RangeTestEntities.Single(x => x.UserDefinedRangeWithSchema.UpperBound > 12.0);
        Assert.Equal(2, result.Id);
        Assert.Equal(5.0, result.UserDefinedRangeWithSchema.LowerBound);
        Assert.Equal(15.0, result.UserDefinedRangeWithSchema.UpperBound);

        AssertSql(
            @"SELECT r.""Id"", r.""DateOnlyDateRange"", r.""DateTimeDateRange"", r.""DecimalRange"", r.""IntRange"", r.""LongRange"", r.""UserDefinedRange"", r.""UserDefinedRangeWithSchema""
FROM ""RangeTestEntities"" AS r
WHERE upper(r.""UserDefinedRangeWithSchema"")::double precision > 12.0
LIMIT 2");
    }

    #endregion

    #region Fixtures

    public class RangeQueryNpgsqlFixture : SharedStoreFixtureBase<RangeContext>
    {
        protected override string StoreName => "RangeQueryTest";
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
        protected override void Seed(RangeContext context) => RangeContext.Seed(context);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);
            var npgsqlOptionsBuilder = new NpgsqlDbContextOptionsBuilder(optionsBuilder);
            npgsqlOptionsBuilder.MapRange("doublerange", typeof(double));
            npgsqlOptionsBuilder.MapRange<float>("Schema_Range", "test");
            return optionsBuilder;
        }
    }

    public class RangeTestEntity
    {
        public int Id { get; set; }
        public NpgsqlRange<int> IntRange { get; set; }
        public NpgsqlRange<long> LongRange { get; set; }
        public NpgsqlRange<decimal> DecimalRange { get; set; }
        public NpgsqlRange<DateOnly> DateOnlyDateRange { get; set; }
        [Column(TypeName = "tsrange")]
        public NpgsqlRange<DateTime> DateTimeDateRange { get; set; }
        public NpgsqlRange<double> UserDefinedRange { get; set; }
        public NpgsqlRange<float> UserDefinedRangeWithSchema { get; set; }
    }

    public class RangeContext : PoolableDbContext
    {
        public DbSet<RangeTestEntity> RangeTestEntities { get; set; }

        public RangeContext(DbContextOptions options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder builder)
            => builder.HasPostgresRange("doublerange", "double precision")
                .HasPostgresRange("test", "Schema_Range", "real");

        public static void Seed(RangeContext context)
        {
            context.RangeTestEntities.AddRange(
                new RangeTestEntity
                {
                    Id = 1,
                    IntRange = new NpgsqlRange<int>(1, 10),
                    LongRange = new NpgsqlRange<long>(1, 10),
                    DecimalRange = new NpgsqlRange<decimal>(1, 10),
                    DateOnlyDateRange = new NpgsqlRange<DateOnly>(new(2020, 1, 1), new(2020, 1, 10)),
                    DateTimeDateRange = new NpgsqlRange<DateTime>(new(2020, 1, 1), new(2020, 1, 10)),
                    UserDefinedRange = new NpgsqlRange<double>(1, 10),
                    UserDefinedRangeWithSchema = new NpgsqlRange<float>(1, 10)
                },
                new RangeTestEntity
                {
                    Id = 2,
                    IntRange = new NpgsqlRange<int>(5, 15),
                    LongRange = new NpgsqlRange<long>(5, 15),
                    DecimalRange = new NpgsqlRange<decimal>(5, 15),
                    DateOnlyDateRange = new NpgsqlRange<DateOnly>(new(2020, 1, 5), new(2020, 1, 15)),
                    DateTimeDateRange = new NpgsqlRange<DateTime>(new(2020, 1, 5), new(2020, 1, 15)),
                    UserDefinedRange = new NpgsqlRange<double>(5, 15),
                    UserDefinedRangeWithSchema = new NpgsqlRange<float>(5, 15)
                });

            context.SaveChanges();
        }
    }

    #endregion

    #region Helpers

    protected RangeContext CreateContext() => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    #endregion
}