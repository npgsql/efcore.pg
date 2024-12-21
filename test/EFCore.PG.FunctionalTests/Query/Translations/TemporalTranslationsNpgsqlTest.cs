using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

/// <remarks>
///     Note that <see cref="BasicTypesEntity.DateTime" /> is mapped to PG <c>timestamp with time zone</c>, as is the provider default;
///     this causes issues with various tests. See also <see cref="TemporalTranslationsNpgsqlTimestampWithoutTimeZoneTest" />, which
///     explicitly maps <see cref="BasicTypesEntity.DateTime" /> to <c>timestamp without time zone</c>.
/// </remarks>
public class TemporalTranslationsNpgsqlTest : TemporalTranslationsTestBase<BasicTypesQueryNpgsqlFixture>
{
    public TemporalTranslationsNpgsqlTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region DateTime

    public override async Task DateTime_Now(bool async)
    {
        await base.DateTime_Now(async);

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE now()::timestamp <> @myDatetime
""");
    }

    public override async Task DateTime_UtcNow(bool async)
    {
        // Overriding to set Kind=Utc for timestamptz
        var myDatetime = DateTime.SpecifyKind(new DateTime(2015, 4, 10), DateTimeKind.Utc);

        await AssertQuery(
            async,
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
    public override Task DateTime_Today(bool async)
        => Assert.ThrowsAsync<NotSupportedException>(() => base.DateTime_Today(async));

    public override async Task DateTime_Date(bool async)
    {
        // Overriding to set Kind=Utc for timestamptz
        var myDatetime = DateTime.SpecifyKind(new DateTime(1998, 5, 4), DateTimeKind.Utc);

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Date == myDatetime));

        AssertSql(
            """
@myDatetime='1998-05-04T00:00:00.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_trunc('day', b."DateTime", 'UTC') = @myDatetime
""");
    }

    public override async Task DateTime_AddYear(bool async)
    {
        await base.DateTime_AddYear(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('year', (b."DateTime" + INTERVAL '1 years') AT TIME ZONE 'UTC')::int = 1999
""");
    }

    public override async Task DateTime_Year(bool async)
    {
        await base.DateTime_Year(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('year', b."DateTime" AT TIME ZONE 'UTC')::int = 1998
""");
    }

    public override async Task DateTime_Month(bool async)
    {
        await base.DateTime_Month(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('month', b."DateTime" AT TIME ZONE 'UTC')::int = 5
""");
    }

    public override async Task DateTime_DayOfYear(bool async)
    {
        await base.DateTime_DayOfYear(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('doy', b."DateTime" AT TIME ZONE 'UTC')::int = 124
""");
    }

    public override async Task DateTime_Day(bool async)
    {
        await base.DateTime_Day(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('day', b."DateTime" AT TIME ZONE 'UTC')::int = 4
""");
    }

    public override async Task DateTime_Hour(bool async)
    {
        await base.DateTime_Hour(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('hour', b."DateTime" AT TIME ZONE 'UTC')::int = 15
""");
    }

    public override async Task DateTime_Minute(bool async)
    {
        await base.DateTime_Minute(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('minute', b."DateTime" AT TIME ZONE 'UTC')::int = 30
""");
    }

    public override async Task DateTime_Second(bool async)
    {
        await base.DateTime_Second(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('second', b."DateTime" AT TIME ZONE 'UTC')::int = 10
""");
    }

    // SQL translation not implemented, too annoying
    public override Task DateTime_Millisecond(bool async)
        => AssertTranslationFailed(() => base.DateTime_Millisecond(async));

    public override async Task DateTime_TimeOfDay(bool async)
    {
        await base.DateTime_TimeOfDay(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time) = TIME '00:00:00'
""");
    }

    public override async Task DateTime_subtract_and_TotalDays(bool async)
    {
        // Overriding to set Kind=Utc for timestamptz
        var date = DateTime.SpecifyKind(new DateTime(1997, 1, 1), DateTimeKind.Utc);

        await AssertQuery(
            async,
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
    public override Task DateTime_Parse_with_constant(bool async)
        => Assert.ThrowsAsync<ArgumentException>(() => base.DateTime_Parse_with_constant(async));

    // DateTime.Parse() returns either a Local or Unspecified DateTime, which can't be compared with timestamptz
    // (see TemporalTranslationsNpgsqlTimestampWithoutTimeZoneTest for a working version of this test)
    public override Task DateTime_Parse_with_parameter(bool async)
        => Assert.ThrowsAsync<ArgumentException>(() => base.DateTime_Parse_with_parameter(async));

    public override async Task DateTime_new_with_constant(bool async)
    {
        // Overriding to set Kind=Utc for timestamptz
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == new DateTime(1998, 5, 4, 15, 30, 10, DateTimeKind.Utc)));

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" = TIMESTAMPTZ '1998-05-04T15:30:10Z'
""");
    }

    public override async Task DateTime_new_with_parameters(bool async)
    {
        // Overriding to set Kind=Utc for timestamptz
        var year = 1998;
        var month = 5;
        var date = 4;
        var hour = 15;

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == new DateTime(year, month, date, hour, 30, 10, DateTimeKind.Utc)));

        AssertSql(
            """
@p='1998-05-04T15:30:10.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" = @p
""");
    }

    #endregion DateTime

    #region DateOnly

    public override async Task DateOnly_Year(bool async)
    {
        await base.DateOnly_Year(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('year', b."DateOnly")::int = 1990
""");
    }

    public override async Task DateOnly_Month(bool async)
    {
        await base.DateOnly_Month(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('month', b."DateOnly")::int = 11
""");
    }

    public override async Task DateOnly_Day(bool async)
    {
        await base.DateOnly_Day(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('day', b."DateOnly")::int = 10
""");
    }

    public override async Task DateOnly_DayOfYear(bool async)
    {
        await base.DateOnly_DayOfYear(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('doy', b."DateOnly")::int = 314
""");
    }

    public override async Task DateOnly_DayOfWeek(bool async)
    {
        await base.DateOnly_DayOfWeek(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(date_part('dow', b."DateOnly"))::int = 6
""");
    }

    public override async Task DateOnly_AddYears(bool async)
    {
        await base.DateOnly_AddYears(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateOnly" + INTERVAL '3 years' AS date) = DATE '1993-11-10'
""");
    }

    public override async Task DateOnly_AddMonths(bool async)
    {
        await base.DateOnly_AddMonths(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateOnly" + INTERVAL '3 months' AS date) = DATE '1991-02-10'
""");
    }

    public override async Task DateOnly_AddDays(bool async)
    {
        await base.DateOnly_AddDays(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateOnly" + 3 = DATE '1990-11-13'
""");
    }

    public override async Task DateOnly_FromDateTime(bool async)
    {
        await base.DateOnly_FromDateTime(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS date) = DATE '1998-05-04'
""");
    }

    public override async Task DateOnly_FromDateTime_compared_to_property(bool async)
    {
        await base.DateOnly_FromDateTime_compared_to_property(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS date) = b."DateOnly"
""");
    }

    public override async Task DateOnly_FromDateTime_compared_to_constant_and_parameter(bool async)
    {
        await base.DateOnly_FromDateTime_compared_to_constant_and_parameter(async);

        AssertSql(
            """
@dateOnly='10/11/0002' (DbType = Date)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS date) IN (@dateOnly, DATE '1998-05-04')
""");
    }

    public override async Task DateOnly_ToDateTime_property_DateOnly_with_constant_TimeOnly(bool async)
    {
        await base.DateOnly_ToDateTime_property_DateOnly_with_constant_TimeOnly(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateOnly" + TIME '21:05:19.9405' = TIMESTAMP '2020-01-01T21:05:19.9405'
""");
    }

    public override async Task DateOnly_ToDateTime_property_DateOnly_with_property_TimeOnly(bool async)
    {
        await base.DateOnly_ToDateTime_property_DateOnly_with_property_TimeOnly(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateOnly" + b."TimeOnly" = TIMESTAMP '2020-01-01T15:30:10'
""");
    }

    public override async Task DateOnly_ToDateTime_constant_DateTime_with_property_TimeOnly(bool async)
    {
        await base.DateOnly_ToDateTime_constant_DateTime_with_property_TimeOnly(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE DATE '1990-11-10' + b."TimeOnly" = TIMESTAMP '1990-11-10T15:30:10'
""");
    }

    public override async Task DateOnly_ToDateTime_with_complex_DateTime(bool async)
    {
        await base.DateOnly_ToDateTime_with_complex_DateTime(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateOnly" + INTERVAL '1 years' AS date) + b."TimeOnly" = TIMESTAMP '2021-01-01T15:30:10'
""");
    }

    public override async Task DateOnly_ToDateTime_with_complex_TimeOnly(bool async)
    {
        await base.DateOnly_ToDateTime_with_complex_TimeOnly(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateOnly" + b."TimeOnly" + INTERVAL '1 hours' = TIMESTAMP '2020-01-01T16:30:10'
""");
    }

    #endregion DateOnly

    #region TimeOnly

    public override async Task TimeOnly_Hour(bool async)
    {
        await base.TimeOnly_Hour(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('hour', b."TimeOnly")::int = 15
""");
    }

    public override async Task TimeOnly_Minute(bool async)
    {
        await base.TimeOnly_Minute(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('minute', b."TimeOnly")::int = 30
""");
    }

    public override async Task TimeOnly_Second(bool async)
    {
        await base.TimeOnly_Second(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('second', b."TimeOnly")::int = 10
""");
    }

    // Translation not yet implemented
    public override Task TimeOnly_Millisecond(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_Millisecond(async));

    // Translation not yet implemented
    public override Task TimeOnly_Microsecond(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_Millisecond(async));

    // Probably not relevant for PostgreSQL, which supports microsecond precision only
    public override Task TimeOnly_Nanosecond(bool async)
        => AssertTranslationFailed(() => base.TimeOnly_Millisecond(async));

    public override async Task TimeOnly_AddHours(bool async)
    {
        await base.TimeOnly_AddHours(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" + INTERVAL '3 hours' = TIME '18:30:10'
""");
    }

    public override async Task TimeOnly_AddMinutes(bool async)
    {
        await base.TimeOnly_AddMinutes(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" + INTERVAL '3 mins' = TIME '15:33:10'
""");
    }

    public override async Task TimeOnly_Add_TimeSpan(bool async)
    {
        await base.TimeOnly_Add_TimeSpan(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" + INTERVAL '03:00:00' = TIME '18:30:10'
""");
    }

    public override async Task TimeOnly_IsBetween(bool async)
    {
        await base.TimeOnly_IsBetween(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" >= TIME '14:00:00' AND b."TimeOnly" < TIME '16:00:00'
""");
    }

    public override async Task TimeOnly_subtract_TimeOnly(bool async)
    {
        await base.TimeOnly_subtract_TimeOnly(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeOnly" - TIME '03:00:00' = INTERVAL '12:30:10'
""");
    }

    public override async Task TimeOnly_FromDateTime_compared_to_property(bool async)
    {
        await base.TimeOnly_FromDateTime_compared_to_property(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time without time zone) = b."TimeOnly"
""");
    }

    public override async Task TimeOnly_FromDateTime_compared_to_parameter(bool async)
    {
        await base.TimeOnly_FromDateTime_compared_to_parameter(async);

        AssertSql(
            """
@time='15:30' (DbType = Time)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time without time zone) = @time
""");
    }

    public override async Task TimeOnly_FromDateTime_compared_to_constant(bool async)
    {
        await base.TimeOnly_FromDateTime_compared_to_constant(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."DateTime" AT TIME ZONE 'UTC' AS time without time zone) = TIME '15:30:10'
""");
    }

    public override async Task TimeOnly_FromTimeSpan_compared_to_property(bool async)
    {
        await base.TimeOnly_FromTimeSpan_compared_to_property(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan"::time without time zone < b."TimeOnly"
""");
    }

    public override async Task TimeOnly_FromTimeSpan_compared_to_parameter(bool async)
    {
        await base.TimeOnly_FromTimeSpan_compared_to_parameter(async);

        AssertSql(
            """
@time='01:02' (DbType = Time)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan"::time without time zone = @time
""");
    }

    public override async Task Order_by_TimeOnly_FromTimeSpan(bool async)
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

    #endregion TimeOnly

    #region DateTimeOffset

    // Not supported by design (DateTimeOffset with non-zero offset)
    public override Task DateTimeOffset_Now(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.DateTimeOffset_Now(async));

    public override async Task DateTimeOffset_UtcNow(bool async)
    {
        await base.DateTimeOffset_UtcNow(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTimeOffset" <> now()
""");
    }

    // The test compares with new DateTimeOffset().Date, which Npgsql sends as -infinity, causing a discrepancy with the client behavior
    // which uses 1/1/1:0:0:0
    public override Task DateTimeOffset_Date(bool async)
        => Assert.ThrowsAsync<EqualException>(() => base.DateTimeOffset_Date(async));

    public override async Task DateTimeOffset_Year(bool async)
    {
        await base.DateTimeOffset_Year(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('year', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 1998
""");
    }

    public override async Task DateTimeOffset_Month(bool async)
    {
        await base.DateTimeOffset_Month(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('month', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 5
""");
    }

    public override async Task DateTimeOffset_DayOfYear(bool async)
    {
        await base.DateTimeOffset_DayOfYear(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('doy', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 124
""");
    }

    public override async Task DateTimeOffset_Day(bool async)
    {
        await base.DateTimeOffset_Day(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('day', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 4
""");
    }

    public override async Task DateTimeOffset_Hour(bool async)
    {
        await base.DateTimeOffset_Hour(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('hour', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 15
""");
    }

    public override async Task DateTimeOffset_Minute(bool async)
    {
        await base.DateTimeOffset_Minute(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('minute', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 30
""");
    }

    public override async Task DateTimeOffset_Second(bool async)
    {
        await base.DateTimeOffset_Second(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('second', b."DateTimeOffset" AT TIME ZONE 'UTC')::int = 10
""");
    }

    // SQL translation not implemented, too annoying
    public override Task DateTimeOffset_Millisecond(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_Millisecond(async));

    // TODO: #3406
    public override Task DateTimeOffset_Microsecond(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_Microsecond(async));

    // TODO: #3406
    public override Task DateTimeOffset_Nanosecond(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_Nanosecond(async));

    public override async Task DateTimeOffset_TimeOfDay(bool async)
    {
        await base.DateTimeOffset_TimeOfDay(async);

        AssertSql(
            """
SELECT CAST(b."DateTimeOffset" AT TIME ZONE 'UTC' AS time)
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task DateTimeOffset_AddYears(bool async)
    {
        await base.DateTimeOffset_AddYears(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 years'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task DateTimeOffset_AddMonths(bool async)
    {
        await base.DateTimeOffset_AddMonths(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 months'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task DateTimeOffset_AddDays(bool async)
    {
        await base.DateTimeOffset_AddDays(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 days'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task DateTimeOffset_AddHours(bool async)
    {
        await base.DateTimeOffset_AddHours(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 hours'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task DateTimeOffset_AddMinutes(bool async)
    {
        await base.DateTimeOffset_AddMinutes(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 mins'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task DateTimeOffset_AddSeconds(bool async)
    {
        await base.DateTimeOffset_AddSeconds(async);

        AssertSql(
            """
SELECT b."DateTimeOffset" + INTERVAL '1 secs'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task DateTimeOffset_AddMilliseconds(bool async)
    {
        await base.DateTimeOffset_AddMilliseconds(async);

        AssertSql(
            """
SELECT b."DateTimeOffset"
FROM "BasicTypesEntities" AS b
""");
    }

    public override Task DateTimeOffset_ToUnixTimeMilliseconds(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_ToUnixTimeMilliseconds(async));

    public override Task DateTimeOffset_ToUnixTimeSecond(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_ToUnixTimeSecond(async));

    public override async Task DateTimeOffset_milliseconds_parameter_and_constant(bool async)
    {
        await base.DateTimeOffset_milliseconds_parameter_and_constant(async);

        AssertSql(
            """
SELECT count(*)::int
FROM "BasicTypesEntities" AS b
WHERE b."DateTimeOffset" = TIMESTAMPTZ '1902-01-02T10:00:00.123456+01:30'
""");
    }

    #endregion DateTimeOffset

    #region TimeSpan

    public override async Task TimeSpan_Hours(bool async)
    {
        await base.TimeSpan_Hours(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(date_part('hour', b."TimeSpan"))::int = 3
""");
    }

    public override async Task TimeSpan_Minutes(bool async)
    {
        await base.TimeSpan_Minutes(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(date_part('minute', b."TimeSpan"))::int = 4
""");
    }

    public override async Task TimeSpan_Seconds(bool async)
    {
        await base.TimeSpan_Seconds(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(date_part('second', b."TimeSpan"))::int = 5
""");
    }

    public override async Task TimeSpan_Milliseconds(bool async)
    {
        await base.TimeSpan_Milliseconds(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(date_part('millisecond', b."TimeSpan"))::int % 1000 = 678
""");
    }

    public override Task TimeSpan_Microseconds(bool async)
        => AssertTranslationFailed(() => base.TimeSpan_Microseconds(async));

    public override Task TimeSpan_Nanoseconds(bool async)
        => AssertTranslationFailed(() => base.TimeSpan_Nanoseconds(async));

    #endregion TimeSpan

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
