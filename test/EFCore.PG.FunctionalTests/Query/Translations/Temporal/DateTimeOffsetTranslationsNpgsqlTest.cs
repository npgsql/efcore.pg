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
    public override Task Now()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Now());

    public override async Task UtcNow()
    {
        await base.UtcNow();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTimeOffset" <> now()
""");
    }

    // The test compares with new DateTimeOffset().Date, which Npgsql sends as -infinity, causing a discrepancy with the client behavior
    // which uses 1/1/1:0:0:0
    public override Task Date()
        => Assert.ThrowsAsync<EqualException>(() => base.Date());

    public override async Task Year()
    {
        await base.Year();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('year', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 1998
""");
    }

    public override async Task Month()
    {
        await base.Month();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('month', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 5
""");
    }

    public override async Task DayOfYear()
    {
        await base.DayOfYear();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('doy', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 124
""");
    }

    public override async Task Day()
    {
        await base.Day();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('day', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 4
""");
    }

    public override async Task Hour()
    {
        await base.Hour();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('hour', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 15
""");
    }

    public override async Task Minute()
    {
        await base.Minute();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('minute', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 30
""");
    }

    public override async Task Second()
    {
        await base.Second();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('second', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 10
""");
    }

    // SQL translation not implemented, too annoying
    public override Task Millisecond()
        => AssertTranslationFailed(() => base.Millisecond());

    // TODO: #3406
    public override Task Microsecond()
        => AssertTranslationFailed(() => base.Microsecond());

    // TODO: #3406
    public override Task Nanosecond()
        => AssertTranslationFailed(() => base.Nanosecond());

    public override async Task TimeOfDay()
    {
        await base.TimeOfDay();

        AssertSql(
            """
SELECT CAST(b."DateTimeOffset" AT TIME ZONE 'UTC' AS time)
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddYears()
    {
        await base.AddYears();

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 years'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddMonths()
    {
        await base.AddMonths();

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 months'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddDays()
    {
        await base.AddDays();

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 days'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddHours()
    {
        await base.AddHours();

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 hours'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddMinutes()
    {
        await base.AddMinutes();

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 mins'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddSeconds()
    {
        await base.AddSeconds();

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 secs'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task AddMilliseconds()
    {
        await base.AddMilliseconds();

        AssertSql(
            """
SELECT b."DateTimeOffset"
FROM "BasicTypesEntities" AS b
""");
    }

    public override Task ToUnixTimeMilliseconds()
        => AssertTranslationFailed(() => base.ToUnixTimeMilliseconds());

    public override Task ToUnixTimeSecond()
        => AssertTranslationFailed(() => base.ToUnixTimeSecond());

    public override async Task Milliseconds_parameter_and_constant()
    {
        await base.Milliseconds_parameter_and_constant();

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
