using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

/// <remarks>
///     Note that <see cref="BasicTypesEntity.DateTime" /> is mapped to PG <c>timestamp with time zone</c>, as is the provider default;
///     this causes issues with various tests. See also <see cref="DateTimeTranslationsWithoutTimeZoneTest" />, which
///     explicitly maps <see cref="BasicTypesEntity.DateTime" /> to <c>timestamp without time zone</c>.
/// </remarks>
public class DateTimeTranslationsNpgsqlTest : DateTimeTranslationsTestBase<BasicTypesQueryNpgsqlFixture>
{
    public DateTimeTranslationsNpgsqlTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Now()
    {
        await base.Now();

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE now()::timestamp <> @myDatetime
""");
    }

    public override async Task UtcNow()
    {
        // Overriding to set Kind=Utc for timestamptz
        var myDatetime = DateTime.SpecifyKind(new DateTime(2015, 4, 10), DateTimeKind.Utc);

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.UtcNow != myDatetime));

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE now() <> @myDatetime
""");
    }

    // DateTime.Today returns a Local DateTime, which can't be compared with timestamptz
    // (see TemporalTranslationsNpgsqlTimestampWithoutTimeZoneTest for a working version of this test)
    public override Task Today()
        => Assert.ThrowsAsync<NotSupportedException>(() => base.Today());

    public override async Task Date()
    {
        // Overriding to set Kind=Utc for timestamptz
        var myDatetime = DateTime.SpecifyKind(new DateTime(1998, 5, 4), DateTimeKind.Utc);

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Date == myDatetime));

        AssertSql(
            """
@myDatetime='1998-05-04T00:00:00.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_trunc('day', b."DateTime", 'UTC') = @myDatetime
""");
    }

    public override async Task AddYear()
    {
        await base.AddYear();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('year', (b."DateTime" + INTERVAL '1 years') AT TIME ZONE 'UTC')::int = 1999
""");
    }

    [ConditionalFact]
    public virtual async Task Add_subsecond_constants()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().OrderBy(b => b.Id).Select(b => new
            {
                Milliseconds = b.DateTime.AddMilliseconds(1.5),
                NegativeMilliseconds = b.DateTime.AddMilliseconds(-0.125),
                Microseconds = b.DateTime.AddMicroseconds(1500),
                NegativeMicroseconds = b.DateTime.AddMicroseconds(-1)
            }),
            assertOrder: true);

        AssertSql(
            """
SELECT b."DateTime" + 1.5 * INTERVAL '1 milliseconds' AS "Milliseconds", b."DateTime" + -0.125 * INTERVAL '1 milliseconds' AS "NegativeMilliseconds", b."DateTime" + 1500.0 * INTERVAL '1 microseconds' AS "Microseconds", b."DateTime" + -1.0 * INTERVAL '1 microseconds' AS "NegativeMicroseconds"
FROM "BasicTypesEntities" AS b
ORDER BY b."Id" NULLS FIRST
""");
    }

    [ConditionalFact]
    public virtual async Task Add_subsecond_constants_beyond_int32_bounds()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b =>
                b.Id == 1
                && b.DateTime.AddMilliseconds(2_592_000_000d) == b.DateTime.AddHours(720)
                && b.DateTime.AddMilliseconds(-2_592_000_000d) == b.DateTime.AddHours(-720)
                && b.DateTime.AddMicroseconds(3_600_000_000d) == b.DateTime.AddHours(1)
                && b.DateTime.AddMicroseconds(-3_600_000_000d) == b.DateTime.AddHours(-1)));

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Id" = 1 AND b."DateTime" + 2592000000.0 * INTERVAL '1 milliseconds' = b."DateTime" + INTERVAL '720 hours' AND b."DateTime" + -2592000000.0 * INTERVAL '1 milliseconds' = b."DateTime" + INTERVAL '-720 hours' AND b."DateTime" + 3600000000.0 * INTERVAL '1 microseconds' = b."DateTime" + INTERVAL '1 hours' AND b."DateTime" + -3600000000.0 * INTERVAL '1 microseconds' = b."DateTime" + INTERVAL '-1 hours'
""");
    }

    [ConditionalTheory]
    [InlineData(-1.5)]
    [InlineData(0)]
    [InlineData(1.5)]
    public virtual async Task Add_subsecond_parameters(double value)
    {
        var microseconds = value * 1000;

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTime.AddMilliseconds(value) == b.DateTime.AddMicroseconds(microseconds)));

        AssertSql(
            $"""
@value='{value}'
@microseconds='{microseconds}'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" + @value * INTERVAL '1 milliseconds' = b."DateTime" + @microseconds * INTERVAL '1 microseconds'
""");
    }

    [ConditionalFact]
    public virtual async Task Add_subsecond_constants_in_scientific_notation()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Id == 1).Select(b => new
            {
                Milliseconds = b.DateTime.AddMilliseconds(0.00001),
                Microseconds = b.DateTime.AddMicroseconds(0.00001),
                LargeMicroseconds = b.DateTime.AddMicroseconds(1e17)
            }));

        AssertSql(
            """
SELECT b."DateTime" + 1.0000000000000001E-05 * INTERVAL '1 milliseconds' AS "Milliseconds", b."DateTime" + 1.0000000000000001E-05 * INTERVAL '1 microseconds' AS "Microseconds", b."DateTime" + 1E+17 * INTERVAL '1 microseconds' AS "LargeMicroseconds"
FROM "BasicTypesEntities" AS b
WHERE b."Id" = 1
""");
    }

    [ConditionalTheory]
    [InlineData(0.00001)]
    [InlineData(1e17)]
    public virtual async Task Add_subsecond_parameters_in_scientific_notation(double microseconds)
    {
        var milliseconds = microseconds / 1000;

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Id == 1).Select(b => new
            {
                Milliseconds = b.DateTime.AddMilliseconds(milliseconds),
                Microseconds = b.DateTime.AddMicroseconds(microseconds)
            }));

        AssertSql(
            $"""
@milliseconds='{milliseconds}'
@microseconds='{microseconds}'

SELECT b."DateTime" + @milliseconds * INTERVAL '1 milliseconds' AS "Milliseconds", b."DateTime" + @microseconds * INTERVAL '1 microseconds' AS "Microseconds"
FROM "BasicTypesEntities" AS b
WHERE b."Id" = 1
""");
    }

    [ConditionalTheory]
    [InlineData(-1, null)]
    [InlineData(0, 0)]
    [InlineData(1, 8)]
    [InlineData(4, -9)]
    public virtual async Task Add_subsecond_with_nullable_argument(int id, int? value)
    {
        await using var context = Fixture.CreateContext();
        var date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = await context.Set<NullableBasicTypesEntity>()
            .Where(b => b.Id == id)
            .Select(b => new
            {
                Milliseconds = (DateTime?)date.AddMilliseconds(b.Int!.Value),
                Microseconds = (DateTime?)date.AddMicroseconds(b.Int!.Value)
            })
            .SingleAsync();

        Assert.Equal(value.HasValue ? date.AddMilliseconds(value.Value) : (DateTime?)null, result.Milliseconds);
        Assert.Equal(value.HasValue ? date.AddMicroseconds(value.Value) : (DateTime?)null, result.Microseconds);

        AssertSql(
            $"""
@date='2000-01-01T00:00:00.0000000Z' (DbType = DateTime)
@id='{id}'

SELECT @date + n."Int"::double precision * INTERVAL '1 milliseconds' AS "Milliseconds", @date + n."Int"::double precision * INTERVAL '1 microseconds' AS "Microseconds"
FROM "NullableBasicTypesEntities" AS n
WHERE n."Id" = @id
LIMIT 2
""");
    }

    [ConditionalFact]
    public virtual async Task Add_subsecond_rounds_to_microsecond_precision()
    {
        // PostgreSQL rounds intervals to microseconds rather than preserving .NET's 100ns ticks.
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().OrderBy(b => b.Id).Select(b => new
            {
                Milliseconds = b.DateTime.AddMilliseconds(0.00075),
                NegativeMilliseconds = b.DateTime.AddMilliseconds(-0.00025),
                Microseconds = b.DateTime.AddMicroseconds(0.25),
                NegativeMicroseconds = b.DateTime.AddMicroseconds(-0.75)
            }),
            expectedQuery: ss => ss.Set<BasicTypesEntity>().OrderBy(b => b.Id).Select(b => new
            {
                Milliseconds = b.DateTime.AddMicroseconds(1),
                NegativeMilliseconds = b.DateTime,
                Microseconds = b.DateTime,
                NegativeMicroseconds = b.DateTime.AddMicroseconds(-1)
            }),
            assertOrder: true);

        AssertSql(
            """
SELECT b."DateTime" + 0.00075000000000000002 * INTERVAL '1 milliseconds' AS "Milliseconds", b."DateTime" + -0.00025000000000000001 * INTERVAL '1 milliseconds' AS "NegativeMilliseconds", b."DateTime" + 0.25 * INTERVAL '1 microseconds' AS "Microseconds", b."DateTime" + -0.75 * INTERVAL '1 microseconds' AS "NegativeMicroseconds"
FROM "BasicTypesEntities" AS b
ORDER BY b."Id" NULLS FIRST
""");
    }

    [ConditionalTheory]
    [InlineData(-1, null)]
    [InlineData(0, 0)]
    [InlineData(1, 8)]
    [InlineData(4, -9)]
    public virtual async Task AddDays_with_nullable_argument(int id, int? days)
    {
        await using var context = Fixture.CreateContext();
        var date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = await context.Set<NullableBasicTypesEntity>()
            .Where(b => b.Id == id)
            .Select(b => (DateTime?)date.AddDays(b.Int!.Value))
            .SingleAsync();

        Assert.Equal(days.HasValue ? date.AddDays(days.Value) : (DateTime?)null, result);

        AssertSql(
            $"""
@date='2000-01-01T00:00:00.0000000Z' (DbType = DateTime)
@id='{id}'

SELECT @date + CAST((n."Int"::double precision::text || ' days') AS interval)
FROM "NullableBasicTypesEntities" AS n
WHERE n."Id" = @id
LIMIT 2
""");
    }

    public override async Task Year()
    {
        await base.Year();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('year', b."DateTime" AT TIME ZONE 'UTC')::int = 1998
""");
    }

    public override async Task Month()
    {
        await base.Month();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('month', b."DateTime" AT TIME ZONE 'UTC')::int = 5
""");
    }

    public override async Task DayOfYear()
    {
        await base.DayOfYear();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('doy', b."DateTime" AT TIME ZONE 'UTC')::int = 124
""");
    }

    public override async Task Day()
    {
        await base.Day();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('day', b."DateTime" AT TIME ZONE 'UTC')::int = 4
""");
    }

    public override async Task Hour()
    {
        await base.Hour();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('hour', b."DateTime" AT TIME ZONE 'UTC')::int = 15
""");
    }

    public override async Task Minute()
    {
        await base.Minute();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('minute', b."DateTime" AT TIME ZONE 'UTC')::int = 30
""");
    }

    public override async Task Second()
    {
        await base.Second();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('second', b."DateTime" AT TIME ZONE 'UTC')::int = 10
""");
    }

    // SQL translation not implemented, too annoying
    public override Task Millisecond()
        => AssertTranslationFailed(() => base.Millisecond());

    public override async Task TimeOfDay()
    {
        await base.TimeOfDay();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time) = TIME '00:00:00'
""");
    }

    public override async Task subtract_and_TotalDays()
    {
        // Overriding to set Kind=Utc for timestamptz
        var date = DateTime.SpecifyKind(new DateTime(1997, 1, 1), DateTimeKind.Utc);

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(o => (o.DateTime - date).TotalDays > 365));

        AssertSql(
            """
@date='1997-01-01T00:00:00.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('epoch', b."DateTime" - @date) / 86400.0 > 365.0
""");
    }

    // DateTime.Parse() returns either a Local or Unspecified DateTime, which can't be compared with timestamptz
    // (see TemporalTranslationsNpgsqlTimestampWithoutTimeZoneTest for a working version of this test)
    public override Task Parse_with_constant()
        => Assert.ThrowsAsync<ArgumentException>(() => base.Parse_with_constant());

    // DateTime.Parse() returns either a Local or Unspecified DateTime, which can't be compared with timestamptz
    // (see TemporalTranslationsNpgsqlTimestampWithoutTimeZoneTest for a working version of this test)
    public override Task Parse_with_parameter()
        => Assert.ThrowsAsync<ArgumentException>(() => base.Parse_with_parameter());

    public override async Task New_with_constant()
    {
        // Overriding to set Kind=Utc for timestamptz
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == new DateTime(1998, 5, 4, 15, 30, 10, DateTimeKind.Utc)));

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" = TIMESTAMPTZ '1998-05-04T15:30:10Z'
""");
    }

    public override async Task New_with_parameters()
    {
        // Overriding to set Kind=Utc for timestamptz
        var year = 1998;
        var month = 5;
        var date = 4;
        var hour = 15;

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == new DateTime(year, month, date, hour, 30, 10, DateTimeKind.Utc)));

        AssertSql(
            """
@p='1998-05-04T15:30:10.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" = @p
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
