using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateTimeOffsetTranslationsNpgsqlTest : DateTimeOffsetTranslationsTestBase<BasicTypesQueryNpgsqlFixture>
{
    public DateTimeOffsetTranslationsNpgsqlTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Not supported by design (DateTimeOffset with non-zero offset)
    public override Task Now(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Now(async));

    public override async Task UtcNow(bool async)
    {
        await base.UtcNow(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTimeOffset" <> now()
""");
    }

    // The test compares with new DateTimeOffset().Date, which Npgsql sends as -infinity, causing a discrepancy with the client behavior
    // which uses 1/1/1:0:0:0
    public override Task Date(bool async)
        => Assert.ThrowsAsync<EqualException>(() => base.Date(async));

    public override async Task Year(bool async)
    {
        await base.Year(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('year', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 1998
""");
    }

    public override async Task Month(bool async)
    {
        await base.Month(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('month', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 5
""");
    }

    public override async Task DayOfYear(bool async)
    {
        await base.DayOfYear(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('doy', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 124
""");
    }

    public override async Task Day(bool async)
    {
        await base.Day(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('day', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 4
""");
    }

    public override async Task Hour(bool async)
    {
        await base.Hour(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('hour', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 15
""");
    }

    public override async Task Minute(bool async)
    {
        await base.Minute(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('minute', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 30
""");
    }

    public override async Task Second(bool async)
    {
        await base.Second(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('second', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 10
""");
    }

    // SQL translation not implemented, too annoying
    public override Task Millisecond(bool async)
        => AssertTranslationFailed(() => base.Millisecond(async));

    // TODO: #3406
    public override Task Microsecond(bool async)
        => AssertTranslationFailed(() => base.Microsecond(async));

    // TODO: #3406
    public override Task Nanosecond(bool async)
        => AssertTranslationFailed(() => base.Nanosecond(async));

    public override async Task TimeOfDay(bool async)
    {
        await base.TimeOfDay(async);

        AssertSql(
            """
SELECT CAST(b."DateTimeOffset" AT TIME ZONE 'UTC' AS time)
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddYears(bool async)
    {
        await base.AddYears(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 years'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddMonths(bool async)
    {
        await base.AddMonths(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 months'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddDays(bool async)
    {
        await base.AddDays(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 days'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddHours(bool async)
    {
        await base.AddHours(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 hours'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddMinutes(bool async)
    {
        await base.AddMinutes(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 mins'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddSeconds(bool async)
    {
        await base.AddSeconds(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 secs'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddMilliseconds(bool async)
    {
        await base.AddMilliseconds(async);

        AssertSql(
            """
SELECT b."DateTimeOffset"
FROM "BasicTypesEntities" AS b
""");
    }

    public override Task ToUnixTimeMilliseconds(bool async)
        => AssertTranslationFailed(() => base.ToUnixTimeMilliseconds(async));

    public override Task ToUnixTimeSecond(bool async)
        => AssertTranslationFailed(() => base.ToUnixTimeSecond(async));

    public override async Task Milliseconds_parameter_and_constant(bool async)
    {
        await base.Milliseconds_parameter_and_constant(async);

        AssertSql(
            """
SELECT count(*)::int
FROM "BasicTypesEntities" AS b
WHERE b."DateTimeOffset" = TIMESTAMPTZ '1902-01-02T10:00:00.123456+01:30'
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
