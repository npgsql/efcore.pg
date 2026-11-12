using System.ComponentModel.DataAnnotations.Schema;

// The EF Core specification test suite has:
//
// * Northwind Orders.OrderDate and others: DateTime mapped to 'timestamp without time zone' (so Unspecified/Local)
// * GearsOfWar Mission.Timeline: DateTimeOffset (mapped to 'timestamp with time zone')
//
// But there's no DateTime mapped to 'timestamp with time zone' (so Utc).
// This test suite checks this and various PG-specific things.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class TimestampTranslationsTest : QueryTestBase<TimestampTranslationsTest.TimestampQueryFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public TimestampTranslationsTest(TimestampQueryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Basic mapping

    [ConditionalFact]
    public void DateTime_maps_to_timestamptz_by_default()
    {
        using var ctx = CreateContext();

        Assert.Equal(
            "timestamp with time zone",
            ctx.Model.GetEntityTypes().Single().GetProperty(nameof(Entity.TimestamptzDateTime)).GetColumnType());
    }

    [ConditionalFact]
    public void DateTime_array_maps_to_timestamptz_by_default()
    {
        using var ctx = CreateContext();

        Assert.Equal(
            "timestamp with time zone[]",
            ctx.Model.GetEntityTypes().Single().GetProperty(nameof(Entity.TimestamptzDateTimeArray)).GetColumnType());
    }

    [ConditionalFact]
    public void DateTime_range_maps_to_timestamptz_by_default()
    {
        using var ctx = CreateContext();

        Assert.Equal(
            "tstzrange",
            ctx.Model.GetEntityTypes().Single().GetProperty(nameof(Entity.TimestamptzDateTimeRange)).GetColumnType());
    }

    [ConditionalFact]
    public async Task Cannot_insert_utc_datetime_into_timestamp()
    {
        await using var ctx = CreateContext();

        ctx.Entities.Add(new Entity { TimestampDateTime = DateTime.UtcNow });
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    [ConditionalFact]
    public async Task Cannot_insert_unspecified_datetime_into_timestamptz()
    {
        await using var ctx = CreateContext();

        ctx.Entities.Add(new Entity { TimestamptzDateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified) });
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    [ConditionalFact]
    public async Task Cannot_insert_local_datetime_into_timestamptz()
    {
        await using var ctx = CreateContext();

        ctx.Entities.Add(new Entity { TimestamptzDateTime = DateTime.Now });
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    #endregion Basic mapping

    #region Arithmetic

    // Arithmetic on timestamptz is performed in UTC, since timestamptz + interval follows the session time zone (see the
    // corresponding timestamptz tests); timestamp has no time zone, so its arithmetic must be left as-is.

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Add_TimeSpan_to_timestamp_column(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Select(e => e.TimestampDateTime + TimeSpan.FromHours(36)));

        AssertSql(
            """
SELECT e."TimestampDateTime" + INTERVAL '1 12:00:00'
FROM "Entities" AS e
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Subtract_TimeSpan_from_timestamp_column(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Select(e => e.TimestampDateTime - TimeSpan.FromHours(-26)));

        AssertSql(
            """
SELECT e."TimestampDateTime" - INTERVAL '-1 -02:00:00'
FROM "Entities" AS e
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task AddDays_on_timestamp_column(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Select(e => e.TimestampDateTime.AddDays(1)));

        AssertSql(
            """
SELECT e."TimestampDateTime" + INTERVAL '1 days'
FROM "Entities" AS e
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Add_TimeSpan_to_timestamptz_column(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Select(e => e.TimestamptzDateTime + TimeSpan.FromHours(36)));

        AssertSql(
            """
SELECT ((e."TimestamptzDateTime" AT TIME ZONE 'UTC') + INTERVAL '1 12:00:00') AT TIME ZONE 'UTC'
FROM "Entities" AS e
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_arithmetic_on_DateTime_parameter_infers_timestamp_from_comparison(bool async)
    {
        await using var ctx = CreateContext();

        // The parameter's mapping is inferred from the timestamp column on the other side of the comparison
        var date = new DateTime(2018, 3, 24, 12, 0, 0, DateTimeKind.Unspecified);

        // .NET has no TimeSpan + DateTime operator, so the TimeSpan is always on the right of the arithmetic;
        // the arithmetic is on either side of the comparison.
        var query = Arithmetic(ctx).Where(
            a => date + a.Interval == a.Later
                && date - a.Interval == a.Earlier
                && a.Later == date + a.Interval
                && a.Earlier == date - a.Interval
                && EF.Constant(date) + a.Interval == a.Later
                && EF.Constant(date) - a.Interval == a.Earlier
                && a.Later == EF.Constant(date) + a.Interval
                && a.Earlier == EF.Constant(date) - a.Interval);

        Assert.Equal(1, async ? await query.CountAsync() : query.Count());

        AssertSql(
            """
@date='2018-03-24T12:00:00.0000000'

SELECT count(*)::int
FROM (
    SELECT TIMESTAMP '2018-03-23T12:00:00' AS "Earlier", TIMESTAMP '2018-03-25T12:00:00' AS "Later", INTERVAL '1 day' AS "Interval", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDst", TIMESTAMPTZ '2018-03-25T12:00:00Z' AS "AfterDst", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstAlias", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstOffset"
) AS m
WHERE @date + m."Interval" = m."Later" AND @date - m."Interval" = m."Earlier" AND m."Later" = @date + m."Interval" AND m."Earlier" = @date - m."Interval" AND TIMESTAMP '2018-03-24T12:00:00' + m."Interval" = m."Later" AND TIMESTAMP '2018-03-24T12:00:00' - m."Interval" = m."Earlier" AND m."Later" = TIMESTAMP '2018-03-24T12:00:00' + m."Interval" AND m."Earlier" = TIMESTAMP '2018-03-24T12:00:00' - m."Interval"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_arithmetic_on_DateTime_parameter_infers_timestamptz_from_comparison(bool async)
    {
        await using var ctx = CreateContext();

        // The parameter's mapping is inferred from the timestamptz column on the other side of the comparison, after which
        // the arithmetic is performed in UTC (Europe/Berlin moves to daylight saving time on 2018-03-25).
        var date = new DateTime(2018, 3, 24, 12, 0, 0, DateTimeKind.Utc);

        var query = Arithmetic(ctx).Where(
            a => date + a.Interval == a.AfterDst
                && a.AfterDst == date + a.Interval
                && date + a.Interval - a.Interval == a.BeforeDst
                && a.BeforeDst == date + a.Interval - a.Interval);

        Assert.Equal(1, async ? await query.CountAsync() : query.Count());

        AssertSql(
            """
@date='2018-03-24T12:00:00.0000000Z' (DbType = DateTime)

SELECT count(*)::int
FROM (
    SELECT TIMESTAMP '2018-03-23T12:00:00' AS "Earlier", TIMESTAMP '2018-03-25T12:00:00' AS "Later", INTERVAL '1 day' AS "Interval", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDst", TIMESTAMPTZ '2018-03-25T12:00:00Z' AS "AfterDst", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstAlias", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstOffset"
) AS m
WHERE ((@date AT TIME ZONE 'UTC') + m."Interval") AT TIME ZONE 'UTC' = m."AfterDst" AND m."AfterDst" = ((@date AT TIME ZONE 'UTC') + m."Interval") AT TIME ZONE 'UTC' AND ((((@date AT TIME ZONE 'UTC') + m."Interval")) - m."Interval") AT TIME ZONE 'UTC' = m."BeforeDst" AND m."BeforeDst" = ((((@date AT TIME ZONE 'UTC') + m."Interval")) - m."Interval") AT TIME ZONE 'UTC'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task AddDays_with_constant_on_DateTime_parameter(bool async)
    {
        await using var ctx = CreateContext();

        // The parameter's mapping is only inferred after AddDays() etc. are translated: from the column on the other side of the
        // comparison, or from the default mapping when projected. The arithmetic must then be performed in UTC for timestamptz
        // (Europe/Berlin moves to daylight saving time on 2018-03-25), and left as-is for timestamp.
        var utc = new DateTime(2018, 3, 24, 12, 0, 0, DateTimeKind.Utc);
        var unspecified = new DateTime(2018, 3, 24, 12, 0, 0, DateTimeKind.Unspecified);
        var offset = new DateTimeOffset(utc);

        var query = Arithmetic(ctx)
            .Where(
                a => EF.Parameter(utc).AddDays(1) == a.AfterDst
                    && a.AfterDst == EF.Parameter(utc).AddDays(1)
                    && EF.Parameter(unspecified).AddDays(1) == a.Later
                    && a.Earlier == EF.Parameter(unspecified).AddDays(-1))
            .Select(
                a => new
                {
                    AddDays = EF.Parameter(utc).AddDays(1),
                    AddMonths = EF.Parameter(utc).AddMonths(1),
                    OffsetAddDays = EF.Parameter(offset).AddDays(1)
                });

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal(new DateTime(2018, 3, 25, 12, 0, 0, DateTimeKind.Utc), result.AddDays);
        Assert.Equal(new DateTime(2018, 4, 24, 12, 0, 0, DateTimeKind.Utc), result.AddMonths);
        Assert.Equal(new DateTimeOffset(2018, 3, 25, 12, 0, 0, TimeSpan.Zero), result.OffsetAddDays);

        AssertSql(
            """
@utc='2018-03-24T12:00:00.0000000Z' (DbType = DateTime)
@offset='2018-03-24T12:00:00.0000000+00:00' (DbType = DateTime)
@utc0='2018-03-24T12:00:00.0000000Z' (DbType = DateTime)
@unspecified='2018-03-24T12:00:00.0000000'

SELECT ((@utc AT TIME ZONE 'UTC') + INTERVAL '1 days') AT TIME ZONE 'UTC' AS "AddDays", ((@utc AT TIME ZONE 'UTC') + INTERVAL '1 months') AT TIME ZONE 'UTC' AS "AddMonths", ((@offset AT TIME ZONE 'UTC') + INTERVAL '1 days') AT TIME ZONE 'UTC' AS "OffsetAddDays"
FROM (
    SELECT TIMESTAMP '2018-03-23T12:00:00' AS "Earlier", TIMESTAMP '2018-03-25T12:00:00' AS "Later", INTERVAL '1 day' AS "Interval", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDst", TIMESTAMPTZ '2018-03-25T12:00:00Z' AS "AfterDst", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstAlias", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstOffset"
) AS m
WHERE ((@utc0 AT TIME ZONE 'UTC') + INTERVAL '1 days') AT TIME ZONE 'UTC' = m."AfterDst" AND m."AfterDst" = ((@utc0 AT TIME ZONE 'UTC') + INTERVAL '1 days') AT TIME ZONE 'UTC' AND @unspecified + INTERVAL '1 days' = m."Later" AND m."Earlier" = @unspecified + INTERVAL '-1 days'
LIMIT 2
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_arithmetic_on_timestamptz_with_precision_or_alias(bool async)
    {
        await using var ctx = CreateContext();

        // Europe/Berlin moves to daylight saving time on 2018-03-25, so arithmetic in the session time zone would be an hour off
        var interval = TimeSpan.FromDays(1);
        var query = Arithmetic(ctx).Select(
            a => new
            {
                AddDays = a.BeforeDst.AddDays(1),
                Add = a.BeforeDst + a.Interval,
                Subtract = a.AfterDst - a.Interval,
                AddParameter = a.BeforeDst + interval,
                SubtractParameter = a.AfterDst - interval,
                AliasAddDays = a.BeforeDstAlias.AddDays(1),
                AliasAdd = a.BeforeDstAlias + a.Interval,
                OffsetAddDays = a.BeforeDstOffset.AddDays(1),
                OffsetAdd = a.BeforeDstOffset + a.Interval
            });

        var result = async ? await query.SingleAsync() : query.Single();

        var beforeDst = new DateTime(2018, 3, 24, 12, 0, 0, DateTimeKind.Utc);
        var afterDst = new DateTime(2018, 3, 25, 12, 0, 0, DateTimeKind.Utc);
        Assert.Equal(afterDst, result.AddDays);
        Assert.Equal(afterDst, result.Add);
        Assert.Equal(beforeDst, result.Subtract);
        Assert.Equal(afterDst, result.AddParameter);
        Assert.Equal(beforeDst, result.SubtractParameter);
        Assert.Equal(afterDst, result.AliasAddDays);
        Assert.Equal(afterDst, result.AliasAdd);
        Assert.Equal(new DateTimeOffset(afterDst), result.OffsetAddDays);
        Assert.Equal(new DateTimeOffset(afterDst), result.OffsetAdd);

        AssertSql(
            """
@interval='1.00:00:00' (DbType = Object)

SELECT ((m."BeforeDst" AT TIME ZONE 'UTC') + INTERVAL '1 days') AT TIME ZONE 'UTC' AS "AddDays", ((m."BeforeDst" AT TIME ZONE 'UTC') + m."Interval") AT TIME ZONE 'UTC' AS "Add", ((m."AfterDst" AT TIME ZONE 'UTC') - m."Interval") AT TIME ZONE 'UTC' AS "Subtract", ((m."BeforeDst" AT TIME ZONE 'UTC') + @interval) AT TIME ZONE 'UTC' AS "AddParameter", ((m."AfterDst" AT TIME ZONE 'UTC') - @interval) AT TIME ZONE 'UTC' AS "SubtractParameter", ((m."BeforeDstAlias" AT TIME ZONE 'UTC') + INTERVAL '1 days') AT TIME ZONE 'UTC' AS "AliasAddDays", ((m."BeforeDstAlias" AT TIME ZONE 'UTC') + m."Interval") AT TIME ZONE 'UTC' AS "AliasAdd", ((m."BeforeDstOffset" AT TIME ZONE 'UTC') + INTERVAL '1 days') AT TIME ZONE 'UTC' AS "OffsetAddDays", ((m."BeforeDstOffset" AT TIME ZONE 'UTC') + m."Interval") AT TIME ZONE 'UTC' AS "OffsetAdd"
FROM (
    SELECT TIMESTAMP '2018-03-23T12:00:00' AS "Earlier", TIMESTAMP '2018-03-25T12:00:00' AS "Later", INTERVAL '1 day' AS "Interval", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDst", TIMESTAMPTZ '2018-03-25T12:00:00Z' AS "AfterDst", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstAlias", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstOffset"
) AS m
LIMIT 2
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Interval_arithmetic_on_timestamptz_with_precision_compared_to_parameter(bool async)
    {
        await using var ctx = CreateContext();

        // The parameters' mappings are inferred from the arithmetic, on either side of the comparison
        var add = new DateTime(2018, 3, 25, 12, 0, 0, DateTimeKind.Utc);
        var addDays = add;
        var subtract = new DateTime(2018, 3, 24, 12, 0, 0, DateTimeKind.Utc);

        var query = Arithmetic(ctx).Where(
            a => a.BeforeDst + a.Interval == add
                && a.BeforeDst.AddDays(1) == addDays
                && subtract == a.AfterDst - a.Interval);

        Assert.Equal(1, async ? await query.CountAsync() : query.Count());

        AssertSql(
            """
@add='2018-03-25T12:00:00.0000000Z' (DbType = DateTime)
@addDays='2018-03-25T12:00:00.0000000Z' (DbType = DateTime)
@subtract='2018-03-24T12:00:00.0000000Z' (DbType = DateTime)

SELECT count(*)::int
FROM (
    SELECT TIMESTAMP '2018-03-23T12:00:00' AS "Earlier", TIMESTAMP '2018-03-25T12:00:00' AS "Later", INTERVAL '1 day' AS "Interval", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDst", TIMESTAMPTZ '2018-03-25T12:00:00Z' AS "AfterDst", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstAlias", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstOffset"
) AS m
WHERE ((m."BeforeDst" AT TIME ZONE 'UTC') + m."Interval") AT TIME ZONE 'UTC' = @add AND ((m."BeforeDst" AT TIME ZONE 'UTC') + INTERVAL '1 days') AT TIME ZONE 'UTC' = @addDays AND @subtract = ((m."AfterDst" AT TIME ZONE 'UTC') - m."Interval") AT TIME ZONE 'UTC'
""");
    }

    [ConditionalTheory]
    [InlineData("timestamp(3) with time zone", "timestamp(3) with time zone", typeof(DateTime))]
    [InlineData("timestamptz(3)", "timestamp(3) with time zone", typeof(DateTime))]
    [InlineData("timestamptz", "timestamptz", typeof(DateTime))]
    [InlineData("timestamp(3) with time zone", "timestamp(3) with time zone", typeof(DateTimeOffset))]
    public void Interval_arithmetic_on_timestamptz_keeps_store_type(string storeType, string expectedStoreType, Type clrType)
    {
        using var ctx = CreateContext();

        var sqlExpressionFactory = ctx.GetService<ISqlExpressionFactory>();
        var mapping = ctx.GetService<IRelationalTypeMappingSource>().FindMapping(clrType, storeType)!;
        var instance = new SqlParameterExpression("p", "p", clrType, nullable: false, ParameterTranslationMode.Parameter, mapping);
        var interval = sqlExpressionFactory.Constant(TimeSpan.FromDays(1));

        // The arithmetic is performed on 'timestamp without time zone' (AT TIME ZONE 'UTC'), and the result converted back
        // to the instance's 'timestamp with time zone' mapping, including its precision.
        AssertResult(sqlExpressionFactory.Add(instance, interval));
        AssertResult(sqlExpressionFactory.Subtract(instance, interval));

        var addDays = ctx.GetService<IMethodCallTranslatorProvider>().Translate(
            ctx.Model,
            instance,
            clrType.GetMethod(nameof(DateTime.AddDays), [typeof(double)])!,
            [sqlExpressionFactory.Constant(1.0)],
            ctx.GetService<IDiagnosticsLogger<DbLoggerCategory.Query>>());
        AssertResult(addDays);

        void AssertResult(SqlExpression? result)
        {
            var outer = Assert.IsType<AtTimeZoneExpression>(result);
            Assert.Same(mapping, outer.TypeMapping);
            Assert.Equal(expectedStoreType, outer.TypeMapping!.StoreType);
            Assert.Equal(clrType, outer.Type);

            var arithmetic = Assert.IsType<SqlBinaryExpression>(outer.Operand);
            Assert.Equal("timestamp without time zone", arithmetic.TypeMapping!.StoreType);
            var utc = Assert.IsType<AtTimeZoneExpression>(arithmetic.Left);
            Assert.Equal("timestamp without time zone", utc.TypeMapping!.StoreType);
            Assert.Same(instance, utc.Operand);
        }
    }

    private static IQueryable<IntervalArithmetic> Arithmetic(TimestampQueryContext ctx)
        => ctx.Database.SqlQueryRaw<IntervalArithmetic>(
            """
SELECT TIMESTAMP '2018-03-23T12:00:00' AS "Earlier", TIMESTAMP '2018-03-25T12:00:00' AS "Later", INTERVAL '1 day' AS "Interval", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDst", TIMESTAMPTZ '2018-03-25T12:00:00Z' AS "AfterDst", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstAlias", TIMESTAMPTZ '2018-03-24T12:00:00Z' AS "BeforeDstOffset"
""");

    #endregion Arithmetic

    #region Comparisons

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Compare_timestamp_column_to_local_DateTime_literal(bool async)
    {
        // Note that we're in the Europe/Berlin timezone (see NpgsqlTestStore below)
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.TimestampDateTime == new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Local)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestampDateTime" = TIMESTAMP '1998-04-12T15:26:38'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Compare_timestamp_column_to_local_DateTime_parameter(bool async)
    {
        var dateTime = new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Local);

        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.TimestampDateTime == dateTime));

        // The string representation of our local DateTime is generated with the local time zone (by the EF Core test infra),
        // so we can't assert on it.
        Assert.Contains(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestampDateTime" = @dateTime
""",
            Fixture.TestSqlLoggerFactory.SqlStatements.Single());
    }

    // Compare_timestamp_column_to_unspecified_DateTime_literal: requires translating DateTime.SpecifyKind

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Compare_timestamp_column_to_unspecified_DateTime_parameter(bool async)
    {
        var dateTime = new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Unspecified);

        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.TimestampDateTime == dateTime));

        AssertSql(
            """
@dateTime='1998-04-12T15:26:38.0000000'

SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestampDateTime" = @dateTime
""");
    }

    [ConditionalFact]
    public async Task Cannot_compare_timestamp_column_to_utc_DateTime_literal()
    {
        await using var ctx = CreateContext();

        await Assert.ThrowsAsync<ArgumentException>(
            () => ctx.Entities.Where(e => e.TimestampDateTime == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc))
                .ToListAsync());
    }

    [ConditionalFact]
    public async Task Cannot_compare_timestamp_column_to_utc_DateTime_parameter()
    {
        await using var ctx = CreateContext();

        var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc);

        await Assert.ThrowsAsync<ArgumentException>(
            () => ctx.Entities.Where(e => e.TimestampDateTime == dateTime)
                .ToListAsync());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Compare_timestamptz_column_to_utc_DateTime_literal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.TimestamptzDateTime == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestamptzDateTime" = TIMESTAMPTZ '1998-04-12T13:26:38Z'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Compare_timestamptz_column_to_utc_DateTime_parameter(bool async)
    {
        var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc);

        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.TimestamptzDateTime == dateTime));

        AssertSql(
            """
@dateTime='1998-04-12T13:26:38.0000000Z' (DbType = DateTime)

SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestamptzDateTime" = @dateTime
""");
    }

    [ConditionalFact]
    public async Task Cannot_compare_timestamptz_column_to_local_DateTime_literal()
    {
        await using var ctx = CreateContext();

        await Assert.ThrowsAsync<ArgumentException>(
            () => ctx.Entities.Where(e => e.TimestamptzDateTime == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local))
                .ToListAsync());
    }

    [ConditionalFact]
    public async Task Cannot_compare_timestamptz_column_to_local_DateTime_parameter()
    {
        await using var ctx = CreateContext();

        var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local);

        await Assert.ThrowsAsync<ArgumentException>(
            () => ctx.Entities.Where(e => e.TimestamptzDateTime == dateTime)
                .ToListAsync());
    }

    [ConditionalFact]
    public async Task Cannot_compare_timestamptz_column_to_unspecified_DateTime_literal()
    {
        await using var ctx = CreateContext();

        await Assert.ThrowsAsync<ArgumentException>(
            () => ctx.Entities.Where(e => e.TimestamptzDateTime == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified))
                .ToListAsync());
    }

    [ConditionalFact]
    public async Task Cannot_compare_timestamptz_column_to_unspecified_DateTime_parameter()
    {
        await using var ctx = CreateContext();

        var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified);

        await Assert.ThrowsAsync<ArgumentException>(
            () => ctx.Entities.Where(e => e.TimestamptzDateTime == dateTime)
                .ToListAsync());
    }

    [ConditionalFact]
    public async Task Cannot_compare_timestamptz_column_to_timestamp_column()
    {
        await using var ctx = CreateContext();

        await Assert.ThrowsAsync<NotSupportedException>(
            () => ctx.Entities.Where(e => e.TimestamptzDateTime == e.TimestampDateTime)
                .ToListAsync());
    }

    [ConditionalFact]
    public async Task Compare_timestamptz_column_to_timestamp_column_with_ToUniversalTime()
    {
        await using var ctx = CreateContext();

        // We can't use AssertQuery since the local (expected) evaluation is dependent on the machine's timezone, which is out of
        // our control.
        var count = await ctx.Set<Entity>()
            .Where(e => e.TimestamptzDateTime == e.TimestampDateTime.ToUniversalTime())
            .CountAsync();

        Assert.Equal(1, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "Entities" AS e
WHERE e."TimestamptzDateTime" = e."TimestampDateTime"::timestamptz
""");
    }

    [ConditionalFact]
    public async Task Compare_timestamptz_column_to_timestamp_column_with_ToLocalTime()
    {
        await using var ctx = CreateContext();

        // We can't use AssertQuery since the local (expected) evaluation is dependent on the machine's timezone, which is out of
        // our control.
        var count = await ctx.Set<Entity>()
            .Where(e => e.TimestamptzDateTime.ToLocalTime() == e.TimestampDateTime)
            .CountAsync();

        Assert.Equal(1, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "Entities" AS e
WHERE e."TimestamptzDateTime"::timestamp = e."TimestampDateTime"
""");
    }

    #endregion Comparisons

    #region Now

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_datetime_now(bool async)
    {
        var myDatetime = new DateTime(2015, 4, 10);

        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(c => DateTime.Now != myDatetime));

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000'

SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE now()::timestamp <> @myDatetime
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_datetime_utcnow(bool async)
    {
        var myDatetime = new DateTime(2015, 4, 10, 0, 0, 0, DateTimeKind.Utc);

        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(c => DateTime.UtcNow != myDatetime));

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000Z' (DbType = DateTime)

SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE now() <> @myDatetime
""");
    }

    #endregion Now

    #region Date member

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    [MinimumPostgresVersion(12, 0)]
    public virtual async Task Where_datetime_date_on_timestamptz(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.TimestamptzDateTime.Date == new DateTime(1998, 4, 12, 0, 0, 0, DateTimeKind.Utc)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE date_trunc('day', e."TimestamptzDateTime", 'UTC') = TIMESTAMPTZ '1998-04-12T00:00:00Z'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_datetime_date_on_timestamp(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.TimestampDateTime.Date == new DateTime(1998, 4, 12, 0, 0, 0, DateTimeKind.Local)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE date_trunc('day', e."TimestampDateTime") = TIMESTAMP '1998-04-12T00:00:00'
""");
    }

    #endregion

    #region DateTimeOffset

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateTimeOffset_DateTime(bool async)
    {
        // We only support UTC DateTimeOffset, so DateTimeOffset.DateTime is just a matter of converting to timestamp without time zone
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => e.TimestampDateTimeOffset.DateTime == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestampDateTimeOffset" AT TIME ZONE 'UTC' = TIMESTAMP '1998-04-12T13:26:38'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateTimeOffset_UtcDateTime(bool async)
    {
        // We only support UTC DateTimeOffset, so DateTimeOffset.UtcDateTime does nothing (type change on CLR change, no change on the
        // PG side.
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => e.TimestampDateTimeOffset.UtcDateTime == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestampDateTimeOffset" = TIMESTAMPTZ '1998-04-12T13:26:38Z'
""");
    }

    [ConditionalFact]
    public async Task DateTimeOffset_LocalDateTime()
    {
        // Note that we're in the Europe/Berlin timezone (see NpgsqlTestStore below)

        // We can't use AssertQuery since the local (expected) evaluation is dependent on the machine's timezone, which is out of
        // our control.
        await using var ctx = CreateContext();

        var count = await ctx.Set<Entity>()
            .Where(e => e.TimestampDateTimeOffset.LocalDateTime == new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Local))
            .CountAsync();

        Assert.Equal(1, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "Entities" AS e
WHERE e."TimestampDateTimeOffset"::timestamp = TIMESTAMP '1998-04-12T15:26:38'
""");
    }

    [ConditionalTheory] // #2220
    [MemberData(nameof(IsAsyncData))]
    public async Task DateTimeOffset_Date_project(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Select(e => new { e.TimestampDateTimeOffset.Date }));

        AssertSql(
            """
SELECT date_trunc('day', e."TimestampDateTimeOffset" AT TIME ZONE 'UTC') AS "Date"
FROM "Entities" AS e
""");

        // Additional check to make sure Unspecified DateTimes are returned, as per the .NET DateTimeOffset.Date behavior
        // (AssertQuery above doesn't confirm this since Kind isn't taken into account for DateTime comparisons)
        await using var ctx = CreateContext();

        foreach (var date in ctx.Set<Entity>().Select(e => e.TimestampDateTimeOffset.Date))
        {
            Assert.Equal(DateTimeKind.Unspecified, date.Kind);
        }
    }

    [ConditionalFact] // #3831
    public async Task DateTime_DateTrunc()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Set<Entity>().Select(e => EF.Functions.DateTrunc("day", e.TimestampDateTime)).ToListAsync();

        AssertSql(
            """
SELECT date_trunc('day', e."TimestampDateTime")
FROM "Entities" AS e
""");
    }

    [ConditionalFact] // #3831
    [MinimumPostgresVersion(12, 0)]
    public async Task DateTimeOffset_DateTrunc_with_timezone()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Set<Entity>().Select(e => EF.Functions.DateTrunc("day", e.TimestampDateTimeOffset, "Europe/Berlin")).ToListAsync();

        AssertSql(
            """
SELECT date_trunc('day', e."TimestampDateTimeOffset", 'Europe/Berlin')
FROM "Entities" AS e
""");
    }

    [ConditionalFact] // #3831
    public async Task DateTime_DateTrunc_with_timezone_is_not_translated()
    {
        await using var ctx = CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.Set<Entity>()
                .Where(e => EF.Functions.DateTrunc("day", e.TimestampDateTime, "Europe/Berlin") == DateTime.Today)
                .ToListAsync());
    }

    [ConditionalFact] // #3831
    public async Task DateTimeOffset_DateTrunc_without_timezone()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Set<Entity>().Select(e => EF.Functions.DateTrunc("day", e.TimestampDateTimeOffset)).ToListAsync();

        AssertSql(
            """
SELECT date_trunc('day', e."TimestampDateTimeOffset")
FROM "Entities" AS e
""");
    }

    [ConditionalFact] // #3831
    public async Task TimeSpan_DateTrunc()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Set<Entity>().Select(e => EF.Functions.DateTrunc("hour", e.TimestamptzDateTime - e.TimestamptzDateTime)).ToListAsync();

        AssertSql(
            """
SELECT date_trunc('hour', e."TimestamptzDateTime" - e."TimestamptzDateTime")
FROM "Entities" AS e
""");
    }

    [ConditionalFact] // #3831
    public async Task TimeSpan_DateTrunc_with_timezone_is_not_translated()
    {
        await using var ctx = CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.Set<Entity>()
                .Where(e => EF.Functions.DateTrunc("hour", e.TimestamptzDateTime - e.TimestamptzDateTime, "Europe/Berlin") == TimeSpan.Zero)
                .ToListAsync());
    }

    #endregion DateTimeOffset

    #region DateTime constructors

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_datetime_ctor1(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e =>
                    new DateTime(e.TimestampDateTime.Year, e.TimestampDateTime.Month, 1) == new DateTime(1998, 4, 12)),
            assertEmpty: true);

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE make_date(date_part('year', e."TimestampDateTime")::int, date_part('month', e."TimestampDateTime")::int, 1) = TIMESTAMP '1998-04-12T00:00:00'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_datetime_ctor2(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e =>
                    new DateTime(e.TimestampDateTime.Year, e.TimestampDateTime.Month, 1, 0, 0, 0) == new DateTime(1998, 4, 12)),
            assertEmpty: true);

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE make_timestamp(date_part('year', e."TimestampDateTime")::int, date_part('month', e."TimestampDateTime")::int, 1, 0, 0, 0::double precision) = TIMESTAMP '1998-04-12T00:00:00'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_datetime_ctor3_local(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e =>
                    new DateTime(e.TimestampDateTime.Year, e.TimestampDateTime.Month, 1, 0, 0, 0, DateTimeKind.Local)
                    == new DateTime(1996, 9, 11)),
            assertEmpty: true);

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE make_timestamp(date_part('year', e."TimestampDateTime")::int, date_part('month', e."TimestampDateTime")::int, 1, 0, 0, 0::double precision) = TIMESTAMP '1996-09-11T00:00:00'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_datetime_ctor3_utc(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                o =>
                    new DateTime(o.TimestamptzDateTime.Year, o.TimestamptzDateTime.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                    == new DateTime(1998, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE make_timestamptz(date_part('year', e."TimestamptzDateTime" AT TIME ZONE 'UTC')::int, date_part('month', e."TimestamptzDateTime" AT TIME ZONE 'UTC')::int, 1, 0, 0, 0::double precision, 'UTC') = TIMESTAMPTZ '1998-04-01T00:00:00Z'
""");
    }

    #endregion DateTime constructors

    #region SpecifyKind

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateTime_SpecifyKind_on_timestamp_to_utc(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => DateTime.SpecifyKind(e.TimestampDateTime, DateTimeKind.Utc)
                    == new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestampDateTime" AT TIME ZONE 'UTC' = TIMESTAMPTZ '1998-04-12T15:26:38Z'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateTime_SpecifyKind_on_timestamptz_to_unspecified(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => DateTime.SpecifyKind(e.TimestamptzDateTime, DateTimeKind.Unspecified)
                    == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestamptzDateTime" AT TIME ZONE 'UTC' = TIMESTAMP '1998-04-12T13:26:38'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateTime_SpecifyKind_on_timestamptz_to_local(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => DateTime.SpecifyKind(e.TimestamptzDateTime, DateTimeKind.Local)
                    == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestamptzDateTime" AT TIME ZONE 'UTC' = TIMESTAMP '1998-04-12T13:26:38'
""");
    }

    [ConditionalFact]
    public async Task DateTime_SpecifyKind_with_parameter_kind_throws()
    {
        await using var ctx = CreateContext();

        var kind = DateTimeKind.Local;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.Set<Entity>().Where(e => DateTime.SpecifyKind(e.TimestamptzDateTime, kind) == default).ToListAsync());

        Assert.Equal("Translating SpecifyKind is only supported with a constant Kind argument", exception.Message);
    }

    #endregion SpecifyKind

    #region Time zone conversions

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_ConvertTimeBySystemTimeZoneId_on_DateTime_timestamptz_column(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                c => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(c.TimestamptzDateTime, "Europe/Berlin")
                    == new DateTime(1998, 4, 12, 15, 26, 38)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestamptzDateTime" AT TIME ZONE 'Europe/Berlin' = TIMESTAMP '1998-04-12T15:26:38'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_ConvertTimeBySystemTimeZoneId_on_DateTimeOffset_UtcDateTime(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(e.TimestampDateTimeOffset.UtcDateTime, "Europe/Berlin")
                    == new DateTime(1998, 4, 12, 15, 26, 38)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestampDateTimeOffset" AT TIME ZONE 'Europe/Berlin' = TIMESTAMP '1998-04-12T15:26:38'
""");
    }

    [ConditionalFact]
    public virtual async Task Where_ConvertTimeBySystemTimeZoneId_fails_on_DateTime_timestamp_column()
    {
        await using var ctx = CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.Set<Entity>().Where(
                c => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(c.TimestampDateTime, "Europe/Berlin")
                    == new DateTime(1998, 4, 12, 15, 26, 38)).ToListAsync());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_ConvertTimeBySystemTimeZoneId_with_source_time_zone_on_DateTime_timestamp_column(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(e.TimestampDateTime, "Europe/Berlin", "UTC")
                    == new DateTime(1998, 4, 12, 13, 26, 38)),
            expectedQuery: ss => ss.Set<Entity>().Where(
                e => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                        DateTime.SpecifyKind(e.TimestampDateTime, DateTimeKind.Unspecified), "Europe/Berlin", "UTC")
                    == new DateTime(1998, 4, 12, 13, 26, 38)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE (e."TimestampDateTime" AT TIME ZONE 'Europe/Berlin') AT TIME ZONE 'UTC' = TIMESTAMP '1998-04-12T13:26:38'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ConvertTimeBySystemTimeZoneId_with_source_time_zone_and_SpecifyKind_to_utc(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Entities.Where(e => e.Id == 1)
            .Select(e => DateTime.SpecifyKind(
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(e.TimestampDateTime, "Europe/Berlin", "UTC"), DateTimeKind.Utc));

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc), result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);

        AssertSql(
            """
SELECT (e."TimestampDateTime" AT TIME ZONE 'Europe/Berlin')
FROM "Entities" AS e
WHERE e."Id" = 1
LIMIT 2
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ConvertTimeBySystemTimeZoneId_with_source_and_destination_time_zone_parameters(bool async)
    {
        await using var ctx = CreateContext();

        var sourceTimeZoneId = "Europe/Berlin";
        var destinationTimeZoneId = "Asia/Tokyo";
        var query = ctx.Entities.Where(e => e.Id == 1)
            .Select(e => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(e.TimestampDateTime, sourceTimeZoneId, destinationTimeZoneId));

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal(new DateTime(1998, 4, 12, 22, 26, 38), result);
        Assert.Equal(DateTimeKind.Unspecified, result.Kind);

        AssertSql(
            """
@sourceTimeZoneId='Europe/Berlin'
@destinationTimeZoneId='Asia/Tokyo'

SELECT (e."TimestampDateTime" AT TIME ZONE @sourceTimeZoneId) AT TIME ZONE @destinationTimeZoneId
FROM "Entities" AS e
WHERE e."Id" = 1
LIMIT 2
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ConvertTimeBySystemTimeZoneId_with_source_and_destination_time_zone_columns(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Database.SqlQueryRaw<TimeZoneConversion>(
                """
SELECT * FROM (VALUES
    (1, TIMESTAMP '1998-04-12T15:26:38', 'Europe/Berlin', 'UTC'),
    (2, TIMESTAMP '1998-04-12T15:26:38', 'America/New_York', 'Asia/Tokyo')
) AS v("Id", "LocalTimestamp", "SourceTimeZoneId", "DestinationTimeZoneId")
""")
            .OrderBy(e => e.Id)
            .Select(e => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(e.LocalTimestamp, e.SourceTimeZoneId, e.DestinationTimeZoneId));

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal([new DateTime(1998, 4, 12, 13, 26, 38), new DateTime(1998, 4, 13, 4, 26, 38)], results);
        Assert.All(results, result => Assert.Equal(DateTimeKind.Unspecified, result.Kind));

        AssertSql(
            """
SELECT (m."LocalTimestamp" AT TIME ZONE m."SourceTimeZoneId") AT TIME ZONE m."DestinationTimeZoneId"
FROM (
    SELECT * FROM (VALUES
        (1, TIMESTAMP '1998-04-12T15:26:38', 'Europe/Berlin', 'UTC'),
        (2, TIMESTAMP '1998-04-12T15:26:38', 'America/New_York', 'Asia/Tokyo')
    ) AS v("Id", "LocalTimestamp", "SourceTimeZoneId", "DestinationTimeZoneId")
) AS m
ORDER BY m."Id" NULLS FIRST
""");
    }

    [ConditionalFact]
    public void Where_ConvertTimeBySystemTimeZoneId_with_source_time_zone_fails_on_DateTime_timestamptz_column()
    {
        using var ctx = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => ctx.Entities.Where(
                e => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(e.TimestamptzDateTime, "UTC", "Europe/Berlin")
                    == new DateTime(1998, 4, 12, 15, 26, 38)).ToQueryString());

        Assert.Equal(
            "TimeZoneInfo.ConvertTimeBySystemTimeZoneId is only supported on columns with type 'timestamp without time zone'",
            exception.Message);
    }

    [ConditionalFact]
    public void Where_ConvertTimeBySystemTimeZoneId_fails_on_DateTimeOffset_column()
    {
        using var ctx = CreateContext();

        Assert.Throws<InvalidOperationException>(
            () => ctx.Entities.Where(
                e => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(e.TimestampDateTimeOffset, "Europe/Berlin")
                    == new DateTimeOffset(1998, 4, 12, 15, 26, 38, TimeSpan.FromHours(2))).ToQueryString());
    }

    [ConditionalFact]
    public virtual async Task Where_ConvertTimeToUtc_on_DateTime_timestamp_column()
    {
        // We can't use AssertQuery since the local (expected) evaluation is dependent on the machine's timezone, which is out of
        // our control.
        await using var ctx = CreateContext();

        var count = await ctx.Set<Entity>()
            .Where(e => TimeZoneInfo.ConvertTimeToUtc(e.TimestampDateTime) == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc))
            .CountAsync();

        Assert.Equal(1, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "Entities" AS e
WHERE e."TimestampDateTime"::timestamptz = TIMESTAMPTZ '1998-04-12T13:26:38Z'
""");
    }

    [ConditionalFact]
    public virtual async Task Where_ConvertTimeToUtc_fails_on_DateTime_timestamptz_column()
    {
        await using var ctx = CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.Set<Entity>().Where(
                    c => TimeZoneInfo.ConvertTimeToUtc(c.TimestamptzDateTime)
                        == new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Utc))
                .ToListAsync());
    }

    #endregion Time zone conversions

    #region DateOnly

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateOnly_FromDateTime_with_timestamptz(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => DateOnly.FromDateTime(e.TimestamptzDateTime) == new DateOnly(1998, 4, 12)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS date) = DATE '1998-04-12'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateOnly_FromDateTime_with_timestamp(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => DateOnly.FromDateTime(e.TimestampDateTime) == new DateOnly(1998, 4, 12)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestampDateTime"::date = DATE '1998-04-12'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateOnly_ToDateTime_with_timestamptz(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => DateOnly.FromDateTime(e.TimestamptzDateTime).ToDateTime(new TimeOnly(15, 26, 38))
                    == new DateTime(1998, 4, 12, 15, 26, 38)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS date) + TIME '15:26:38' = TIMESTAMP '1998-04-12T15:26:38'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateOnly_DayNumber(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => DateOnly.FromDateTime(e.TimestamptzDateTime).DayNumber == 729490));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS date) - DATE '0001-01-01' = 729490
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateOnly_DayNumber_subtraction(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => DateOnly.FromDateTime(e.TimestamptzDateTime).DayNumber -
                    DateOnly.FromDateTime(e.TimestamptzDateTime - TimeSpan.FromDays(3)).DayNumber == 3));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS date) - CAST(((e."TimestamptzDateTime" AT TIME ZONE 'UTC') - INTERVAL '3 00:00:00') AS date) = 3
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateOnly_AddMonths_and_AddYears_on_parameter(bool async)
    {
        await using var ctx = CreateContext();

        // The parameter must be mapped to date, rather than inferred from the interval
        var monthEnd = new DateOnly(2018, 1, 31);
        var leapDay = new DateOnly(2016, 2, 29);

        var query = ctx.Entities
            .Where(e => e.Id == 1 && EF.Parameter(monthEnd).AddMonths(1) == new DateOnly(2018, 2, 28))
            .Select(e => new { AddMonths = EF.Parameter(monthEnd).AddMonths(1), AddYears = EF.Parameter(leapDay).AddYears(1) });

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal(new DateOnly(2018, 2, 28), result.AddMonths);
        Assert.Equal(new DateOnly(2017, 2, 28), result.AddYears);

        AssertSql(
            """
@monthEnd='01/31/2018' (DbType = Date)
@leapDay='02/29/2016' (DbType = Date)

SELECT CAST(@monthEnd + INTERVAL '1 months' AS date) AS "AddMonths", CAST(@leapDay + INTERVAL '1 years' AS date) AS "AddYears"
FROM "Entities" AS e
WHERE e."Id" = 1 AND CAST(@monthEnd + INTERVAL '1 months' AS date) = DATE '2018-02-28'
LIMIT 2
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task DateOnly_FromDayNumber(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => DateOnly.FromDayNumber(e.Id) == new DateOnly(0001, 01, 03)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE DATE '0001-01-01' + e."Id" = DATE '0001-01-03'
""");
    }

    #endregion DateOnly

    #region TimeOfDay

    // TimeOfDay is a TimeSpan mapped to PostgreSQL time, which justify_hours() (used for intervals) doesn't accept
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task TimeOfDay_components(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>()
                .Where(e => e.TimestamptzDateTime.TimeOfDay.Hours == 13 || e.TimestampDateTime.TimeOfDay.Milliseconds == 345)
                .Select(
                    e => new
                    {
                        e.Id,
                        e.TimestamptzDateTime.TimeOfDay.Days,
                        e.TimestamptzDateTime.TimeOfDay.Hours,
                        e.TimestamptzDateTime.TimeOfDay.Minutes,
                        e.TimestamptzDateTime.TimeOfDay.Seconds,
                        e.TimestamptzDateTime.TimeOfDay.Milliseconds,
                        TimestampHours = e.TimestampDateTime.TimeOfDay.Hours,
                        TimestampMilliseconds = e.TimestampDateTime.TimeOfDay.Milliseconds
                    }),
            elementSorter: e => e.Id);

        AssertSql(
            """
SELECT e."Id", date_part('hour', CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS time))::int / 24 AS "Days", date_part('hour', CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS time))::int % 24 AS "Hours", date_part('minute', CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS time))::int AS "Minutes", trunc(date_part('second', CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS time)))::int AS "Seconds", trunc(date_part('millisecond', CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS time)))::int % 1000 AS "Milliseconds", date_part('hour', e."TimestampDateTime"::time)::int % 24 AS "TimestampHours", trunc(date_part('millisecond', e."TimestampDateTime"::time))::int % 1000 AS "TimestampMilliseconds"
FROM "Entities" AS e
WHERE date_part('hour', CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS time))::int % 24 = 13 OR trunc(date_part('millisecond', e."TimestampDateTime"::time))::int % 1000 = 345
""");
    }

    #endregion TimeOfDay

    #region TimeOnly

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task TimeOnly_FromDateTime_with_timestamp(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => TimeOnly.FromDateTime(e.TimestampDateTime) == new TimeOnly(15, 26, 38)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE e."TimestampDateTime"::time without time zone = TIME '15:26:38'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task TimeOnly_FromDateTime_with_timestamptz(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => TimeOnly.FromDateTime(e.TimestamptzDateTime) == new TimeOnly(13, 26, 38)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS time without time zone) = TIME '13:26:38'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task TimeOnly_ToTimeSpan(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(
                e => TimeOnly.FromDateTime(e.TimestamptzDateTime) == new TimeOnly(13, 26, 38)));

        AssertSql(
            """
SELECT e."Id", e."TimestampDateTime", e."TimestampDateTimeArray", e."TimestampDateTimeOffset", e."TimestampDateTimeOffsetArray", e."TimestampDateTimeRange", e."TimestamptzDateTime", e."TimestamptzDateTimeArray", e."TimestamptzDateTimeRange"
FROM "Entities" AS e
WHERE CAST(e."TimestamptzDateTime" AT TIME ZONE 'UTC' AS time without time zone) = TIME '13:26:38'
""");
    }

    #endregion TimeOnly

    #region Misc

    [ConditionalFact]
    public void Range_parameter_contains_timestamp_with_no_time_zone_column()
    {
        // This scenario requires that the provider correctly infer the range's type mapping from the subtype's
        using var ctx = CreateContext();

        var range = new NpgsqlRange<DateTime>(new DateTime(1998, 4, 12), new DateTime(1998, 4, 13));

        var id = ctx.Entities.Single(e => range.Contains(e.TimestampDateTime)).Id;
        Assert.Equal(1, id);
    }

    #endregion

    #region Support

    private TimestampQueryContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class TimestampQueryContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }

        public static async Task SeedAsync (TimestampQueryContext context)
        {
            context.Entities.AddRange(TimestampData.CreateEntities());
            await context.SaveChangesAsync();
        }
    }

    public class Entity
    {
        public int Id { get; set; }

        public DateTime TimestamptzDateTime { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime TimestampDateTime { get; set; }

        public DateTimeOffset TimestampDateTimeOffset { get; set; }

        public DateTime[]? TimestamptzDateTimeArray { get; set; }

        [Column(TypeName = "timestamp without time zone[]")]
        public DateTime[]? TimestampDateTimeArray { get; set; }

        public DateTimeOffset[]? TimestampDateTimeOffsetArray { get; set; }

        public NpgsqlRange<DateTime> TimestamptzDateTimeRange { get; set; }

        [Column(TypeName = "tsrange")]
        public NpgsqlRange<DateTime> TimestampDateTimeRange { get; set; }
    }

    private class TimeZoneConversion
    {
        public int Id { get; set; }

        [Column(TypeName = "timestamp(6) without time zone")]
        public DateTime LocalTimestamp { get; set; }

        public string SourceTimeZoneId { get; set; } = null!;

        public string DestinationTimeZoneId { get; set; } = null!;
    }

    private class IntervalArithmetic
    {
        [Column(TypeName = "timestamp without time zone")]
        public DateTime Earlier { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime Later { get; set; }

        public TimeSpan Interval { get; set; }

        [Precision(3)]
        public DateTime BeforeDst { get; set; }

        [Precision(3)]
        public DateTime AfterDst { get; set; }

        [Column(TypeName = "timestamptz")]
        public DateTime BeforeDstAlias { get; set; }

        [Precision(3)]
        public DateTimeOffset BeforeDstOffset { get; set; }
    }

    public class TimestampQueryFixture : QueryFixtureBase<TimestampQueryContext>, ITestSqlLoggerFactory
    {
        protected override string StoreName
            => "TimestampQueryTest";

        // Set the PostgreSQL TimeZone parameter to something local, to ensure that operations which take TimeZone into account
        // don't depend on the database's time zone, and also that operations which shouldn't take TimeZone into account indeed
        // don't.
        protected override ITestStoreFactory TestStoreFactory
            => new NpgsqlTestStoreFactory(connectionStringOptions: "-c TimeZone=Europe/Berlin");

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        private TimestampData? _expectedData;

        protected override Task SeedAsync(TimestampQueryContext context)
            => TimestampQueryContext.SeedAsync(context);

        public override ISetSource GetExpectedData()
            => _expectedData ??= new TimestampData();

        public override IReadOnlyDictionary<Type, object> EntitySorters
            => new Dictionary<Type, Func<object, object?>> { { typeof(Entity), e => ((Entity)e)?.Id } }
                .ToDictionary(e => e.Key, e => (object)e.Value);

        public override IReadOnlyDictionary<Type, object> EntityAsserters
            => new Dictionary<Type, Action<object, object>>
            {
                {
                    typeof(Entity), (e, a) =>
                    {
                        Assert.Equal(e is null, a is null);
                        if (a is not null)
                        {
                            var ee = (Entity)e!;
                            var aa = (Entity)a;

                            Assert.Equal(ee.Id, aa.Id);

                            Assert.Equal(ee.TimestamptzDateTime, aa.TimestamptzDateTime);
                            Assert.Equal(ee.TimestampDateTime, aa.TimestampDateTime);
                            Assert.Equal(ee.TimestampDateTimeOffset, aa.TimestampDateTimeOffset);

                            Assert.Equal(ee.TimestamptzDateTimeArray, aa.TimestamptzDateTimeArray);
                            Assert.Equal(ee.TimestampDateTimeArray, aa.TimestampDateTimeArray);
                            Assert.Equal(ee.TimestampDateTimeOffsetArray, aa.TimestampDateTimeOffsetArray);

                            Assert.Equal(ee.TimestamptzDateTimeRange, aa.TimestamptzDateTimeRange);
                            Assert.Equal(ee.TimestampDateTimeRange, aa.TimestampDateTimeRange);
                        }
                    }
                }
            }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    protected class TimestampData : ISetSource
    {
        public IReadOnlyList<Entity> Entities { get; } = CreateEntities();

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
        {
            var utcDateTime1 = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc);
            var localDateTime1 = new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Local);
            var utcDateTime2 = new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Utc);
            var unspecifiedDateTime2 = new DateTime(2015, 1, 27, 10, 45, 12, 345, DateTimeKind.Unspecified);

            var utcDateTimeArray1 = new[]
            {
                new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc), new DateTime(1998, 4, 12, 17, 26, 38, DateTimeKind.Utc)
            };

            var localDateTimeArray1 = new[]
            {
                new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Local), new DateTime(1998, 4, 13, 15, 26, 38, DateTimeKind.Local)
            };

            var utcDateTimeArray2 = new[]
            {
                new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Utc), new DateTime(2015, 1, 28, 8, 45, 12, 345, DateTimeKind.Utc)
            };

            var localDateTimeArray2 = new[]
            {
                new DateTime(2015, 1, 27, 10, 45, 12, 345, DateTimeKind.Unspecified),
                new DateTime(2015, 1, 28, 10, 45, 12, 345, DateTimeKind.Unspecified)
            };

            var utcDateTimeRange1 = new NpgsqlRange<DateTime>(utcDateTimeArray1[0], utcDateTimeArray1[1]);
            var localDateTimeRange1 = new NpgsqlRange<DateTime>(localDateTimeArray1[0], localDateTimeArray1[1]);

            var utcDateTimeRange2 = new NpgsqlRange<DateTime>(utcDateTimeArray2[0], utcDateTimeArray2[1]);
            var localDateTimeRange2 = new NpgsqlRange<DateTime>(localDateTimeArray2[0], localDateTimeArray2[1]);

            return new List<Entity>
            {
                new()
                {
                    Id = 1,
                    TimestamptzDateTime = utcDateTime1,
                    TimestampDateTime = localDateTime1,
                    TimestampDateTimeOffset = new DateTimeOffset(utcDateTime1),
                    TimestamptzDateTimeArray = utcDateTimeArray1,
                    TimestampDateTimeArray = localDateTimeArray1,
                    TimestampDateTimeOffsetArray = [new(utcDateTimeArray1[0]), new(utcDateTimeArray1[1])],
                    TimestamptzDateTimeRange = utcDateTimeRange1,
                    TimestampDateTimeRange = localDateTimeRange1,
                },
                new()
                {
                    Id = 2,
                    TimestamptzDateTime = utcDateTime2,
                    TimestampDateTime = unspecifiedDateTime2,
                    TimestampDateTimeOffset = new DateTimeOffset(utcDateTime2),
                    TimestamptzDateTimeArray = utcDateTimeArray2,
                    TimestampDateTimeArray = localDateTimeArray2,
                    TimestampDateTimeOffsetArray = [new(utcDateTimeArray2[0]), new(utcDateTimeArray2[1])],
                    TimestamptzDateTimeRange = utcDateTimeRange2,
                    TimestampDateTimeRange = localDateTimeRange2,
                }
            };
        }
    }

    #endregion
}
