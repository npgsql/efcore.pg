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

    public override async Task Hour(bool async)
    {
        await base.Hour(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('hour', b."TimeOnly")::int = 15
""");
    }

    public override async Task Minute(bool async)
    {
        await base.Minute(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('minute', b."TimeOnly")::int = 30
""");
    }

    public override async Task Second(bool async)
    {
        await base.Second(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('second', b."TimeOnly")::int = 10
""");
    }

    // Translation not yet implemented
    public override Task Millisecond(bool async)
        => AssertTranslationFailed(() => base.Millisecond(async));

    // Translation not yet implemented
    public override Task Microsecond(bool async)
        => AssertTranslationFailed(() => base.Millisecond(async));

    // Probably not relevant for PostgreSQL, which supports microsecond precision only
    public override Task Nanosecond(bool async)
        => AssertTranslationFailed(() => base.Millisecond(async));

    public override async Task AddHours(bool async)
    {
        await base.AddHours(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" + INTERVAL '3 hours' = TIME '18:30:10'
""");
    }

    public override async Task AddMinutes(bool async)
    {
        await base.AddMinutes(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" + INTERVAL '3 mins' = TIME '15:33:10'
""");
    }

    public override async Task Add_TimeSpan(bool async)
    {
        await base.Add_TimeSpan(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" + INTERVAL '03:00:00' = TIME '18:30:10'
""");
    }

    public override async Task IsBetween(bool async)
    {
        await base.IsBetween(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" >= TIME '14:00:00' AND b."TimeOnly" < TIME '16:00:00'
""");
    }

    public override async Task Subtract(bool async)
    {
        await base.Subtract(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" - TIME '03:00:00' = INTERVAL '12:30:10'
""");
    }

    public override async Task FromDateTime_compared_to_property(bool async)
    {
        await base.FromDateTime_compared_to_property(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time without time zone) = b."TimeOnly"
""");
    }

    public override async Task FromDateTime_compared_to_parameter(bool async)
    {
        await base.FromDateTime_compared_to_parameter(async);

        AssertSql(
            """
@time='15:30' (DbType = Time)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time without time zone) = @time
""");
    }

    public override async Task FromDateTime_compared_to_constant(bool async)
    {
        await base.FromDateTime_compared_to_constant(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time without time zone) = TIME '15:30:10'
""");
    }

    public override async Task FromTimeSpan_compared_to_property(bool async)
    {
        await base.FromTimeSpan_compared_to_property(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan"::time without time zone < b."TimeOnly"
""");
    }

    public override async Task FromTimeSpan_compared_to_parameter(bool async)
    {
        await base.FromTimeSpan_compared_to_parameter(async);

        AssertSql(
            """
@time='01:02' (DbType = Time)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan"::time without time zone = @time
""");
    }

    public override async Task Order_by_FromTimeSpan(bool async)
    {
        // TODO: Base implementation is non-deterministic, remove this override once that's fixed on the EF side.
        await AssertQuery(
            async,
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
