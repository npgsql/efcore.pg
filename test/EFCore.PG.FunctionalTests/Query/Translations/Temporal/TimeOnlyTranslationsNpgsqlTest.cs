using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeOnlyTranslationsNpgsqlTest : TimeOnlyTranslationsTestBase<BasicTypesQueryNpgsqlFixture>
{
    public TimeOnlyTranslationsNpgsqlTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Hour()
    {
        await base.Hour();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('hour', b."TimeOnly")::int = 15
""");
    }

    public override async Task Minute()
    {
        await base.Minute();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('minute', b."TimeOnly")::int = 30
""");
    }

    public override async Task Second()
    {
        await base.Second();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('second', b."TimeOnly")::int = 10
""");
    }

    // Translation not yet implemented
    public override Task Millisecond()
        => AssertTranslationFailed(() => base.Millisecond());

    // Translation not yet implemented
    public override Task Microsecond()
        => AssertTranslationFailed(() => base.Millisecond());

    // Probably not relevant for PostgreSQL, which supports microsecond precision only
    public override Task Nanosecond()
        => AssertTranslationFailed(() => base.Millisecond());

    public override async Task AddHours()
    {
        await base.AddHours();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" + INTERVAL '3 hours' = TIME '18:30:10'
""");
    }

    public override async Task AddMinutes()
    {
        await base.AddMinutes();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" + INTERVAL '3 mins' = TIME '15:33:10'
""");
    }

    public override async Task Add_TimeSpan()
    {
        await base.Add_TimeSpan();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" + INTERVAL '03:00:00' = TIME '18:30:10'
""");
    }

    public override async Task IsBetween()
    {
        await base.IsBetween();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" >= TIME '14:00:00' AND b."TimeOnly" < TIME '16:00:00'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task IsBetween_midnight(bool async)
    {
        await AssertQuery(async, ss => ss.Set<BasicTypesEntity>()
            .Where(b => b.TimeOnly.IsBetween(new TimeOnly(22, 0), new TimeOnly(2, 0))).Select(b => b.Id));

        AssertSql(
            """
SELECT b."Id"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" >= TIME '22:00:00' OR b."TimeOnly" < TIME '02:00:00'
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual async Task IsBetween_midnight_boundary_values(bool async, bool parameterized)
    {
        const string valuesSql = """
SELECT unnest(ARRAY[TIME '00:00:00', TIME '00:30:00', TIME '01:59:59.999999', TIME '02:00:00', TIME '02:00:00.000001', TIME '21:00:00', TIME '21:59:59.999999', TIME '22:00:00', TIME '23:00:00', TIME '23:59:59.999999']) AS "Value"
""";
        await using var context = Fixture.CreateContext();
        var times = context.Database.SqlQueryRaw<TimeOnly>(valuesSql);
        var start = new TimeOnly(22, 0);
        var end = new TimeOnly(2, 0);
        var query = (parameterized
            ? times.Where(time => time.IsBetween(start, end))
            : times.Where(time => time.IsBetween(new TimeOnly(22, 0), new TimeOnly(2, 0)))).OrderBy(time => time);

        TimeOnly[] expected =
        [
            new(0, 0), new(0, 30), new(1, 59, 59, 999, 999),
            new(22, 0), new(23, 0), new(23, 59, 59, 999, 999)
        ];
        Assert.Equal(expected, async ? await query.ToArrayAsync() : query.ToArray());

        var parameterDeclaration = parameterized
            ? string.Join(Environment.NewLine, "@start='22:00' (DbType = Time)", "@end='02:00' (DbType = Time)", "", "")
            : "";
        var predicate = parameterized
            ? """(@start <= @end AND s."Value"::time without time zone >= @start AND s."Value"::time without time zone < @end) OR (@start > @end AND (s."Value"::time without time zone >= @start OR s."Value"::time without time zone < @end))"""
            : """s."Value"::time without time zone >= TIME '22:00:00' OR s."Value"::time without time zone < TIME '02:00:00'""";
        AssertSql(
            $"""
{parameterDeclaration}SELECT s."Value"
FROM (
    {valuesSql}
) AS s
WHERE {predicate}
ORDER BY s."Value"::time without time zone NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public virtual async Task IsBetween_parameters(bool async, int range)
    {
        var start = range == 0 ? new TimeOnly(14, 0) : new TimeOnly(22, 0);
        var end = range switch { 0 => new TimeOnly(16, 0), 1 => new TimeOnly(2, 0), _ => start };

        await AssertQuery(async, ss => ss.Set<BasicTypesEntity>()
            .Where(b => b.TimeOnly.IsBetween(start, end)).Select(b => b.Id), assertEmpty: range == 2);

        AssertSql(
            $"""
@start='{start:HH:mm}' (DbType = Time)
@end='{end:HH:mm}' (DbType = Time)

SELECT b."Id"
FROM "BasicTypesEntities" AS b
WHERE (@start <= @end AND b."TimeOnly" >= @start AND b."TimeOnly" < @end) OR (@start > @end AND (b."TimeOnly" >= @start OR b."TimeOnly" < @end))
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task IsBetween_boundaries(bool async)
    {
        await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Select(b => new
        {
            b.Id,
            Start = b.TimeOnly.IsBetween(new TimeOnly(15, 30, 10), new TimeOnly(16, 0)),
            End = b.TimeOnly.IsBetween(new TimeOnly(14, 0), new TimeOnly(15, 30, 10)),
            WrappedStart = b.TimeOnly.IsBetween(new TimeOnly(15, 30, 10), new TimeOnly(14, 0)),
            WrappedEnd = b.TimeOnly.IsBetween(new TimeOnly(16, 0), new TimeOnly(15, 30, 10)),
            Empty = b.TimeOnly.IsBetween(new TimeOnly(15, 30, 10), new TimeOnly(15, 30, 10))
        }), elementSorter: x => x.Id);

        AssertSql(
            """
SELECT b."Id", b."TimeOnly" >= TIME '15:30:10' AND b."TimeOnly" < TIME '16:00:00' AS "Start", b."TimeOnly" >= TIME '14:00:00' AND b."TimeOnly" < TIME '15:30:10' AS "End", b."TimeOnly" >= TIME '15:30:10' OR b."TimeOnly" < TIME '14:00:00' AS "WrappedStart", b."TimeOnly" >= TIME '16:00:00' OR b."TimeOnly" < TIME '15:30:10' AS "WrappedEnd", b."TimeOnly" >= TIME '15:30:10' AND b."TimeOnly" < TIME '15:30:10' AS "Empty"
FROM "BasicTypesEntities" AS b
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task IsBetween_precision(bool async)
    {
        await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Select(b => new
        {
            b.Id,
            Start = b.TimeOnly.IsBetween(new TimeOnly(102000L), new TimeOnly(102010L)),
            End = b.TimeOnly.IsBetween(new TimeOnly(101990L), new TimeOnly(102000L))
        }), elementSorter: x => x.Id);

        AssertSql(
            """
SELECT b."Id", b."TimeOnly" >= TIME '00:00:00.0102' AND b."TimeOnly" < TIME '00:00:00.010201' AS "Start", b."TimeOnly" >= TIME '00:00:00.010199' AND b."TimeOnly" < TIME '00:00:00.0102' AS "End"
FROM "BasicTypesEntities" AS b
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task IsBetween_nullable(bool async)
    {
        await AssertQuery(async, ss => ss.Set<NullableBasicTypesEntity>().Select(b => new
        {
            b.Id,
            Inside = b.TimeOnly.HasValue && b.TimeOnly.Value.IsBetween(new TimeOnly(22, 0), new TimeOnly(2, 0)),
            Outside = !b.TimeOnly.HasValue || !b.TimeOnly.Value.IsBetween(new TimeOnly(22, 0), new TimeOnly(2, 0)),
            Start = b.TimeOnly.HasValue && new TimeOnly(0, 0).IsBetween(b.TimeOnly.Value, new TimeOnly(2, 0))
        }), elementSorter: x => x.Id);

        AssertSql(
            """
SELECT n."Id", n."TimeOnly" IS NOT NULL AND (n."TimeOnly" >= TIME '22:00:00' OR n."TimeOnly" < TIME '02:00:00') AS "Inside", n."TimeOnly" IS NULL OR (n."TimeOnly" < TIME '22:00:00' AND n."TimeOnly" >= TIME '02:00:00') AS "Outside", n."TimeOnly" IS NOT NULL AND ((n."TimeOnly" <= TIME '02:00:00' AND TIME '00:00:00' >= n."TimeOnly" AND TIME '00:00:00' < TIME '02:00:00') OR (n."TimeOnly" > TIME '02:00:00' AND (TIME '00:00:00' >= n."TimeOnly" OR TIME '00:00:00' < TIME '02:00:00'))) AS "Start"
FROM "NullableBasicTypesEntities" AS n
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task IsBetween_negated(bool async)
    {
        await AssertQuery(async, ss => ss.Set<BasicTypesEntity>()
            .Where(b => !b.TimeOnly.IsBetween(new TimeOnly(22, 0), new TimeOnly(2, 0))).Select(b => b.Id));

        AssertSql(
            """
SELECT b."Id"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" < TIME '22:00:00' AND b."TimeOnly" >= TIME '02:00:00'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task IsBetween_column_bounds(bool async)
    {
        var time = new TimeOnly(0, 0);

        await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Select(b => new
        {
            b.Id,
            Start = time.IsBetween(b.TimeOnly, new TimeOnly(16, 0)),
            End = time.IsBetween(new TimeOnly(22, 0), b.TimeOnly)
        }), elementSorter: x => x.Id);

        AssertSql(
            """
@time='00:00' (DbType = Time)

SELECT b."Id", (b."TimeOnly" <= TIME '16:00:00' AND @time >= b."TimeOnly" AND @time < TIME '16:00:00') OR (b."TimeOnly" > TIME '16:00:00' AND (@time >= b."TimeOnly" OR @time < TIME '16:00:00')) AS "Start", (TIME '22:00:00' <= b."TimeOnly" AND @time >= TIME '22:00:00' AND @time < b."TimeOnly") OR (TIME '22:00:00' > b."TimeOnly" AND (@time >= TIME '22:00:00' OR @time < b."TimeOnly")) AS "End"
FROM "BasicTypesEntities" AS b
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task IsBetween_calculated_bounds(bool async)
    {
        await AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Select(b => new
        {
            b.Id,
            Inside = b.TimeOnly.AddHours(1).IsBetween(b.TimeOnly, b.TimeOnly.AddHours(2))
        }), elementSorter: x => x.Id);

        AssertSql(
            """
SELECT b."Id", (b."TimeOnly" <= b."TimeOnly" + INTERVAL '2 hours' AND b."TimeOnly" + INTERVAL '1 hours' >= b."TimeOnly" AND b."TimeOnly" + INTERVAL '1 hours' < b."TimeOnly" + INTERVAL '2 hours') OR (b."TimeOnly" > b."TimeOnly" + INTERVAL '2 hours' AND (b."TimeOnly" + INTERVAL '1 hours' >= b."TimeOnly" OR b."TimeOnly" + INTERVAL '1 hours' < b."TimeOnly" + INTERVAL '2 hours')) AS "Inside"
FROM "BasicTypesEntities" AS b
""");
    }

    [Fact]
    public virtual async Task IsBetween_compiled_query_with_changing_parameters()
    {
        var query = EF.CompileAsyncQuery((BasicTypesContext context, TimeOnly start, TimeOnly end)
            => context.Set<BasicTypesEntity>().Where(b => b.TimeOnly.IsBetween(start, end)).OrderBy(b => b.Id).Select(b => b.Id));

        await using var context = Fixture.CreateContext();
        foreach (var (start, end) in new[]
                 {
                     (new TimeOnly(22, 0), new TimeOnly(2, 0)),
                     (new TimeOnly(14, 0), new TimeOnly(16, 0)),
                     (new TimeOnly(22, 0), new TimeOnly(22, 0))
                 })
        {
            var expected = Fixture.GetExpectedData().Set<BasicTypesEntity>()
                .Where(b => b.TimeOnly.IsBetween(start, end)).OrderBy(b => b.Id).Select(b => b.Id).ToArray();
            List<int> actual = [];
            await foreach (var id in query(context, start, end))
            {
                actual.Add(id);
            }

            Assert.Equal(expected, actual);
        }

        AssertSql(
            """
@start='22:00' (DbType = Time)
@end='02:00' (DbType = Time)

SELECT b."Id"
FROM "BasicTypesEntities" AS b
WHERE (@start <= @end AND b."TimeOnly" >= @start AND b."TimeOnly" < @end) OR (@start > @end AND (b."TimeOnly" >= @start OR b."TimeOnly" < @end))
ORDER BY b."Id" NULLS FIRST
""",
            //
            """
@start='14:00' (DbType = Time)
@end='16:00' (DbType = Time)

SELECT b."Id"
FROM "BasicTypesEntities" AS b
WHERE (@start <= @end AND b."TimeOnly" >= @start AND b."TimeOnly" < @end) OR (@start > @end AND (b."TimeOnly" >= @start OR b."TimeOnly" < @end))
ORDER BY b."Id" NULLS FIRST
""",
            //
            """
@start='22:00' (DbType = Time)
@end='22:00' (DbType = Time)

SELECT b."Id"
FROM "BasicTypesEntities" AS b
WHERE (@start <= @end AND b."TimeOnly" >= @start AND b."TimeOnly" < @end) OR (@start > @end AND (b."TimeOnly" >= @start OR b."TimeOnly" < @end))
ORDER BY b."Id" NULLS FIRST
""");
    }

    public override async Task Subtract()
    {
        await base.Subtract();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" - TIME '03:00:00' = INTERVAL '12:30:10'
""");
    }

    public override async Task FromDateTime_compared_to_property()
    {
        await base.FromDateTime_compared_to_property();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time without time zone) = b."TimeOnly"
""");
    }

    public override async Task FromDateTime_compared_to_parameter()
    {
        await base.FromDateTime_compared_to_parameter();

        AssertSql(
            """
@time='15:30' (DbType = Time)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time without time zone) = @time
""");
    }

    public override async Task FromDateTime_compared_to_constant()
    {
        await base.FromDateTime_compared_to_constant();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time without time zone) = TIME '15:30:10'
""");
    }

    public override async Task FromTimeSpan_compared_to_property()
    {
        await base.FromTimeSpan_compared_to_property();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan"::time without time zone < b."TimeOnly"
""");
    }

    public override async Task FromTimeSpan_compared_to_parameter()
    {
        await base.FromTimeSpan_compared_to_parameter();

        AssertSql(
            """
@time='01:02' (DbType = Time)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan"::time without time zone = @time
""");
    }

    public override async Task Order_by_FromTimeSpan()
    {
        // TODO: Base implementation is non-deterministic, remove this override once that's fixed on the EF side.
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().OrderBy(x => TimeOnly.FromTimeSpan(x.TimeSpan)).ThenBy(x => x.Id),
            assertOrder: true);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
ORDER BY b."TimeSpan"::time without time zone NULLS FIRST, b."Id" NULLS FIRST
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
