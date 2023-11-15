using System.Text;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NodaTime.Calendars;
using NodaTime.Text;
using NodaTime.TimeZones;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NpgsqlNodaTimeTypeMappingTest
{
    #region Timestamp without time zone

    [Fact]
    public void Timestamp_maps_to_LocalDateTime_by_default()
    {
        Assert.Equal("timestamp without time zone", GetMapping(typeof(LocalDateTime)).StoreType);
        Assert.Same(typeof(LocalDateTime), GetMapping("timestamp without time zone").ClrType);
    }

    // Mapping Instant to timestamp should only be possible in legacy mode.
    // However, when upgrading to 6.0 with existing migrations, model snapshots still contain old mappings (Instant mapped to timestamp),
    // and EF Core's model differ expects type mappings to be found for these. See https://github.com/dotnet/efcore/issues/26168.
    [Fact]
    public void Instant_maps_to_timestamp_legacy()
    {
        var mapping = GetMapping(typeof(Instant), "timestamp");
        Assert.Same(typeof(Instant), mapping.ClrType);
        Assert.Equal("timestamp", mapping.StoreType);
    }

    [Fact]
    public void Instant_with_precision()
        => Assert.Equal(
            "timestamp(3) with time zone",
            Mapper.FindMapping(typeof(Instant), "timestamp with time zone", precision: 3)!.StoreType);

    [Fact]
    public void GenerateSqlLiteral_returns_LocalDateTime_literal()
    {
        var mapping = GetMapping(typeof(LocalDateTime));
        Assert.Equal("timestamp without time zone", mapping.StoreType);

        var localDateTime = new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660);
        Assert.Equal("TIMESTAMP '2018-04-20T10:31:33.666666'", mapping.GenerateSqlLiteral(localDateTime));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_LocalDateTime_literal()
    {
        Assert.Equal("new NodaTime.LocalDateTime(2018, 4, 20, 10, 31)", CodeLiteral(new LocalDateTime(2018, 4, 20, 10, 31)));
        Assert.Equal("new NodaTime.LocalDateTime(2018, 4, 20, 10, 31, 33)", CodeLiteral(new LocalDateTime(2018, 4, 20, 10, 31, 33)));

        var localDateTime = new LocalDateTime(2018, 4, 20, 10, 31, 33) + Period.FromNanoseconds(1);
        Assert.Equal("new NodaTime.LocalDateTime(2018, 4, 20, 10, 31, 33).PlusNanoseconds(1L)", CodeLiteral(localDateTime));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_LocalDateTime_infinity_literal()
    {
        var mapping = GetMapping(typeof(LocalDateTime));
        Assert.Equal(typeof(LocalDateTime), mapping.ClrType);
        Assert.Equal("timestamp without time zone", mapping.StoreType);

        // TODO: Switch to use LocalDateTime.MinMaxValue when available (#4061)
        Assert.Equal("TIMESTAMP '-infinity'", mapping.GenerateSqlLiteral(LocalDate.MinIsoValue + LocalTime.MinValue));
        Assert.Equal("TIMESTAMP 'infinity'", mapping.GenerateSqlLiteral(LocalDate.MaxIsoValue + LocalTime.MaxValue));
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00")]
    [InlineData("9999-12-31T23:59:59.9999999")]
    [InlineData("2023-05-29T10:52:47.2064353")]
    public void LocalDateTime_json(string dateString)
    {
        var readerWriter = GetMapping(typeof(LocalDateTime)).JsonValueReaderWriter!;

        var date = LocalDateTimePattern.ExtendedIso.Parse(dateString).GetValueOrThrow();
        var actualJson = readerWriter.ToJsonString(date)[1..^1];
        Assert.Equal(dateString, actualJson);

        // TODO: The following should just do ToJsonString(), but see https://github.com/dotnet/efcore/issues/32269
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(Encoding.UTF8.GetBytes($"\"{dateString}\"")), null);
        readerManager.MoveNext();
        var actualDate = readerWriter.FromJson(ref readerManager, existingObject: null);
        Assert.Equal(date, actualDate);
    }

    [Fact]
    public void NpgsqlRange_of_LocalDateTime_is_properly_mapped()
    {
        Assert.Equal("tsrange", GetMapping(typeof(NpgsqlRange<LocalDateTime>)).StoreType);
        Assert.Same(typeof(NpgsqlRange<LocalDateTime>), GetMapping("tsrange").ClrType);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_tsrange_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping(typeof(NpgsqlRange<LocalDateTime>));
        Assert.Equal("tsrange", mapping.StoreType);
        Assert.Equal("timestamp without time zone", mapping.SubtypeMapping.StoreType);

        var value = new NpgsqlRange<LocalDateTime>(new LocalDateTime(2020, 1, 1, 12, 0, 0), new LocalDateTime(2020, 1, 2, 12, 0, 0));
        Assert.Equal(@"'[""2020-01-01T12:00:00"",""2020-01-02T12:00:00""]'::tsrange", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void Array_of_NpgsqlRange_of_LocalDateTime_is_properly_mapped()
    {
        Assert.Equal("tsmultirange", GetMapping(typeof(NpgsqlRange<LocalDateTime>[])).StoreType);
        Assert.Same(typeof(List<NpgsqlRange<LocalDateTime>>), GetMapping("tsmultirange").ClrType);
    }

    [Fact]
    public void List_of_NpgsqlRange_of_LocalDateTime_is_properly_mapped()
        => Assert.Equal("tsmultirange", GetMapping(typeof(List<NpgsqlRange<LocalDateTime>>)).StoreType);

    #endregion Timestamp without time zone

    #region Timestamp with time zone

    [Fact]
    public void Timestamptz_maps_to_Instant_by_default()
        => Assert.Same(typeof(Instant), GetMapping("timestamp with time zone").ClrType);

    [Fact]
    public void LocalDateTime_does_not_map_to_timestamptz()
        => Assert.Null(GetMapping(typeof(LocalDateTime), "timestamp with time zone"));

    [Fact]
    public void GenerateSqlLiteral_returns_timestamptz_Instant_literal()
    {
        var mapping = GetMapping(typeof(Instant));
        Assert.Equal(typeof(Instant), mapping.ClrType);
        Assert.Equal("timestamp with time zone", mapping.StoreType);

        var instant = (new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660)).InUtc().ToInstant();
        Assert.Equal("TIMESTAMPTZ '2018-04-20T10:31:33.666666Z'", mapping.GenerateSqlLiteral(instant));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_timestamptz_Instant_infinity_literal()
    {
        var mapping = GetMapping(typeof(Instant));
        Assert.Equal(typeof(Instant), mapping.ClrType);
        Assert.Equal("timestamp with time zone", mapping.StoreType);

        Assert.Equal("TIMESTAMPTZ '-infinity'", mapping.GenerateSqlLiteral(Instant.MinValue));
        Assert.Equal("TIMESTAMPTZ 'infinity'", mapping.GenerateSqlLiteral(Instant.MaxValue));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_ZonedDateTime_literal()
    {
        var mapping = GetMapping(typeof(ZonedDateTime));
        Assert.Equal("timestamp with time zone", mapping.StoreType);

        var zonedDateTime = (new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660))
            .InZone(DateTimeZone.ForOffset(Offset.FromHours(2)), Resolvers.LenientResolver);
        Assert.Equal("TIMESTAMPTZ '2018-04-20T10:31:33.666666+02'", mapping.GenerateSqlLiteral(zonedDateTime));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_ZonedDateTime_literal()
    {
        var zonedDateTime = (new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660))
            .InZone(DateTimeZone.ForOffset(Offset.FromHours(2)), Resolvers.LenientResolver);
        Assert.Equal(
            @"new NodaTime.ZonedDateTime(NodaTime.Instant.FromUnixTimeTicks(15242130936666660L), NodaTime.TimeZones.TzdbDateTimeZoneSource.Default.ForId(""UTC+02""))",
            CodeLiteral(zonedDateTime));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_OffsetDate_time_literal()
    {
        var mapping = GetMapping(typeof(OffsetDateTime));
        Assert.Equal("timestamp with time zone", mapping.StoreType);

        var offsetDateTime = new OffsetDateTime(
            new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660),
            Offset.FromHours(2));
        Assert.Equal("TIMESTAMPTZ '2018-04-20T10:31:33.666666+02'", mapping.GenerateSqlLiteral(offsetDateTime));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_Instant_literal()
        => Assert.Equal(
            "NodaTime.Instant.FromUnixTimeTicks(15832607590000000L)",
            CodeLiteral(Instant.FromUtc(2020, 3, 3, 18, 39, 19)));

    [Fact]
    public void GenerateCodeLiteral_returns_OffsetDate_time_literal()
    {
        Assert.Equal(
            "new NodaTime.OffsetDateTime(new NodaTime.LocalDateTime(2018, 4, 20, 10, 31), NodaTime.Offset.FromHours(-2))",
            CodeLiteral(new OffsetDateTime(new LocalDateTime(2018, 4, 20, 10, 31), Offset.FromHours(-2))));

        Assert.Equal(
            "new NodaTime.OffsetDateTime(new NodaTime.LocalDateTime(2018, 4, 20, 10, 31, 33), NodaTime.Offset.FromSeconds(9000))",
            CodeLiteral(new OffsetDateTime(new LocalDateTime(2018, 4, 20, 10, 31, 33), Offset.FromHoursAndMinutes(2, 30))));

        Assert.Equal(
            "new NodaTime.OffsetDateTime(new NodaTime.LocalDateTime(2018, 4, 20, 10, 31, 33), NodaTime.Offset.FromSeconds(-1))",
            CodeLiteral(new OffsetDateTime(new LocalDateTime(2018, 4, 20, 10, 31, 33), Offset.FromSeconds(-1))));
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00Z")]
    [InlineData("2023-05-29T10:52:47.2064353Z")]
    [InlineData("-0005-05-05T05:55:55.555Z")]
    public void Instant_json(string instantString)
    {
        var readerWriter = GetMapping(typeof(Instant)).JsonValueReaderWriter!;

        var date = InstantPattern.ExtendedIso.Parse(instantString).GetValueOrThrow();
        var actualJson = readerWriter.ToJsonString(date)[1..^1];
        Assert.Equal(instantString, actualJson);

        // TODO: The following should just do ToJsonString(), but see https://github.com/dotnet/efcore/issues/32269
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(Encoding.UTF8.GetBytes($"\"{instantString}\"")), null);
        readerManager.MoveNext();
        var actualInstant = readerWriter.FromJson(ref readerManager, existingObject: null);
        Assert.Equal(date, actualInstant);
    }

    [ConditionalFact]
    public void Instant_json_infinity()
    {
        var readerWriter = GetMapping(typeof(Instant)).JsonValueReaderWriter!;

        Assert.Equal("infinity", readerWriter.ToJsonString(Instant.MaxValue)[1..^1]);
        Assert.Equal("-infinity", readerWriter.ToJsonString(Instant.MinValue)[1..^1]);

        // TODO: The following should just do ToJsonString(), but see https://github.com/dotnet/efcore/issues/32269
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData("\"infinity\""u8.ToArray()), null);
        readerManager.MoveNext();
        Assert.Equal(Instant.MaxValue, readerWriter.FromJson(ref readerManager, existingObject: null));

        readerManager = new Utf8JsonReaderManager(new JsonReaderData("\"-infinity\""u8.ToArray()), null);
        readerManager.MoveNext();
        Assert.Equal(Instant.MinValue, readerWriter.FromJson(ref readerManager, existingObject: null));
    }

    [Fact]
    public void Interval_is_properly_mapped()
    {
        Assert.Equal("tstzrange", GetMapping(typeof(Interval)).StoreType);
        Assert.Same(typeof(Interval), GetMapping("tstzrange").ClrType);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_tstzrange_Interval_literal()
    {
        var mapping = (IntervalRangeMapping)GetMapping("tstzrange");

        var value = new Interval(
            new LocalDateTime(2020, 1, 1, 12, 0, 0).InUtc().ToInstant(),
            new LocalDateTime(2020, 1, 2, 12, 0, 0).InUtc().ToInstant());
        Assert.Equal(@"'[2020-01-01T12:00:00Z,2020-01-02T12:00:00Z)'::tstzrange", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_tstzrange_Interval_literal()
    {
        Assert.Equal(
            "new NodaTime.Interval(NodaTime.Instant.FromUnixTimeTicks(15778800000000000L), NodaTime.Instant.FromUnixTimeTicks(15782256000000000L))",
            CodeLiteral(
                new Interval(
                    new LocalDateTime(2020, 01, 01, 12, 0, 0).InUtc().ToInstant(),
                    new LocalDateTime(2020, 01, 05, 12, 0, 0).InUtc().ToInstant())));

        Assert.Equal(
            "new NodaTime.Interval((NodaTime.Instant?)NodaTime.Instant.FromUnixTimeTicks(15778800000000000L), null)",
            CodeLiteral(new Interval(new LocalDateTime(2020, 01, 01, 12, 0, 0).InUtc().ToInstant(), null)));
    }

    [Fact]
    public void Interval_array_is_properly_mapped()
    {
        Assert.Equal("tstzmultirange", GetMapping(typeof(Interval[])).StoreType);
        Assert.Same(typeof(Interval[]), GetMapping("tstzmultirange").ClrType);
    }

    [Fact]
    public void Interval_list_is_properly_mapped()
        => Assert.Equal("tstzmultirange", GetMapping(typeof(List<Interval>)).StoreType);

    [Fact]
    public void GenerateSqlLiteral_returns_Interval_array_literal()
    {
        var mapping = GetMapping(typeof(Interval[]));

        var interval = new Interval[]
        {
            new(
                new LocalDateTime(1998, 4, 12, 13, 26, 38).InUtc().ToInstant(),
                new LocalDateTime(1998, 4, 12, 15, 26, 38).InUtc().ToInstant()),
            new(
                new LocalDateTime(1998, 4, 13, 13, 26, 38).InUtc().ToInstant(),
                new LocalDateTime(1998, 4, 13, 15, 26, 38).InUtc().ToInstant()),
        };

        Assert.Equal(
            "'{[1998-04-12T13:26:38Z,1998-04-12T15:26:38Z), [1998-04-13T13:26:38Z,1998-04-13T15:26:38Z)}'::tstzmultirange",
            mapping.GenerateSqlLiteral(interval));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_Interval_list_literal()
    {
        var mapping = GetMapping(typeof(List<Interval>));

        var interval = new List<Interval>
        {
            new(
                new LocalDateTime(1998, 4, 12, 13, 26, 38).InUtc().ToInstant(),
                new LocalDateTime(1998, 4, 12, 15, 26, 38).InUtc().ToInstant()),
            new(
                new LocalDateTime(1998, 4, 13, 13, 26, 38).InUtc().ToInstant(),
                new LocalDateTime(1998, 4, 13, 15, 26, 38).InUtc().ToInstant()),
        };

        Assert.Equal(
            "'{[1998-04-12T13:26:38Z,1998-04-12T15:26:38Z), [1998-04-13T13:26:38Z,1998-04-13T15:26:38Z)}'::tstzmultirange",
            mapping.GenerateSqlLiteral(interval));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_Interval_array_literal()
        => Assert.Equal(
            "new[] { new NodaTime.Interval(NodaTime.Instant.FromUnixTimeTicks(8923875980000000L), NodaTime.Instant.FromUnixTimeTicks(8923947980000000L)), new NodaTime.Interval(NodaTime.Instant.FromUnixTimeTicks(8924739980000000L), NodaTime.Instant.FromUnixTimeTicks(8924811980000000L)) }",
            CodeLiteral(
                new Interval[]
                {
                    new(
                        new LocalDateTime(1998, 4, 12, 13, 26, 38).InUtc().ToInstant(),
                        new LocalDateTime(1998, 4, 12, 15, 26, 38).InUtc().ToInstant()),
                    new(
                        new LocalDateTime(1998, 4, 13, 13, 26, 38).InUtc().ToInstant(),
                        new LocalDateTime(1998, 4, 13, 15, 26, 38).InUtc().ToInstant()),
                }));

    [Fact]
    public void GenerateCodeLiteral_returns_Interval_list_literal()
        => Assert.Equal(
            "new List<Interval> { new NodaTime.Interval(NodaTime.Instant.FromUnixTimeTicks(8923875980000000L), NodaTime.Instant.FromUnixTimeTicks(8923947980000000L)), new NodaTime.Interval(NodaTime.Instant.FromUnixTimeTicks(8924739980000000L), NodaTime.Instant.FromUnixTimeTicks(8924811980000000L)) }",
            CodeLiteral(
                new List<Interval>
                {
                    new(
                        new LocalDateTime(1998, 4, 12, 13, 26, 38).InUtc().ToInstant(),
                        new LocalDateTime(1998, 4, 12, 15, 26, 38).InUtc().ToInstant()),
                    new(
                        new LocalDateTime(1998, 4, 13, 13, 26, 38).InUtc().ToInstant(),
                        new LocalDateTime(1998, 4, 13, 15, 26, 38).InUtc().ToInstant()),
                }));

    [Fact]
    public void GenerateSqlLiteral_returns_tstzrange_Instant_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping(typeof(NpgsqlRange<Instant>));
        Assert.Equal("tstzrange", mapping.StoreType);
        Assert.Equal("timestamp with time zone", mapping.SubtypeMapping.StoreType);

        var value = new NpgsqlRange<Instant>(
            new LocalDateTime(2020, 1, 1, 12, 0, 0).InUtc().ToInstant(),
            new LocalDateTime(2020, 1, 2, 12, 0, 0).InUtc().ToInstant());
        Assert.Equal(@"'[""2020-01-01T12:00:00Z"",""2020-01-02T12:00:00Z""]'::tstzrange", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_tstzrange_ZonedDateTime_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping(typeof(NpgsqlRange<ZonedDateTime>));
        Assert.Equal("tstzrange", mapping.StoreType);
        Assert.Equal("timestamp with time zone", mapping.SubtypeMapping.StoreType);

        var value = new NpgsqlRange<ZonedDateTime>(
            new LocalDateTime(2020, 1, 1, 12, 0, 0).InUtc(),
            new LocalDateTime(2020, 1, 2, 12, 0, 0).InUtc());
        Assert.Equal(@"'[""2020-01-01T12:00:00Z"",""2020-01-02T12:00:00Z""]'::tstzrange", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_tstzrange_OffsetDateTime_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping(typeof(NpgsqlRange<OffsetDateTime>));
        Assert.Equal("tstzrange", mapping.StoreType);
        Assert.Equal("timestamp with time zone", mapping.SubtypeMapping.StoreType);

        var value = new NpgsqlRange<OffsetDateTime>(
            new LocalDateTime(2020, 1, 1, 12, 0, 0).WithOffset(Offset.Zero),
            new LocalDateTime(2020, 1, 2, 12, 0, 0).WithOffset(Offset.Zero));
        Assert.Equal(@"'[""2020-01-01T12:00:00Z"",""2020-01-02T12:00:00Z""]'::tstzrange", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void Array_of_NpgsqlRange_of_Instant_is_properly_mapped()
        => Assert.Equal("tstzmultirange", GetMapping(typeof(NpgsqlRange<Instant>[])).StoreType);

    [Fact]
    public void List_of_NpgsqlRange_of_Instant_is_properly_mapped()
        => Assert.Equal("tstzmultirange", GetMapping(typeof(List<NpgsqlRange<Instant>>)).StoreType);

    #endregion Timestamp with time zone

    #region date/daterange/datemultirange

    [Fact]
    public void LocalDate_is_properly_mapped()
    {
        Assert.Equal("date", GetMapping(typeof(LocalDate)).StoreType);
        Assert.Same(typeof(LocalDate), GetMapping("date").ClrType);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_LocalDate_literal()
    {
        var mapping = GetMapping(typeof(LocalDate));

        Assert.Equal("DATE '2018-04-20'", mapping.GenerateSqlLiteral(new LocalDate(2018, 4, 20)));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_LocalDate_infinity_literal()
    {
        var mapping = GetMapping(typeof(LocalDate));

        Assert.Equal("DATE '-infinity'", mapping.GenerateSqlLiteral(LocalDate.MinIsoValue));
        Assert.Equal("DATE 'infinity'", mapping.GenerateSqlLiteral(LocalDate.MaxIsoValue));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_LocalDate_literal()
    {
        Assert.Equal("new NodaTime.LocalDate(2018, 4, 20)", CodeLiteral(new LocalDate(2018, 4, 20)));
        Assert.Equal("new NodaTime.LocalDate(-2017, 4, 20)", CodeLiteral(new LocalDate(Era.BeforeCommon, 2018, 4, 20)));
    }

    [ConditionalTheory]
    [InlineData("0001-01-01")]
    [InlineData("2023-05-29")]
    [InlineData("-0005-05-05")]
    public void LocalDate_json(string dateString)
    {
        var readerWriter = GetMapping(typeof(LocalDate)).JsonValueReaderWriter!;

        var date = LocalDatePattern.Iso.Parse(dateString).GetValueOrThrow();
        var actualJson = readerWriter.ToJsonString(date)[1..^1];
        Assert.Equal(dateString, actualJson);

        // TODO: The following should just do ToJsonString(), but see https://github.com/dotnet/efcore/issues/32269
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(Encoding.UTF8.GetBytes($"\"{dateString}\"")), null);
        readerManager.MoveNext();
        var actualDate = readerWriter.FromJson(ref readerManager, existingObject: null);
        Assert.Equal(date, actualDate);
    }

    [ConditionalFact]
    public void LocalDate_json_infinity()
    {
        var readerWriter = GetMapping(typeof(LocalDate)).JsonValueReaderWriter!;

        Assert.Equal("infinity", readerWriter.ToJsonString(LocalDate.MaxIsoValue)[1..^1]);
        Assert.Equal("-infinity", readerWriter.ToJsonString(LocalDate.MinIsoValue)[1..^1]);

        // TODO: The following should just do ToJsonString(), but see https://github.com/dotnet/efcore/issues/32269
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData("\"infinity\""u8.ToArray()), null);
        readerManager.MoveNext();
        Assert.Equal(LocalDate.MaxIsoValue, readerWriter.FromJson(ref readerManager, existingObject: null));

        readerManager = new Utf8JsonReaderManager(new JsonReaderData("\"-infinity\""u8.ToArray()), null);
        readerManager.MoveNext();
        Assert.Equal(LocalDate.MinIsoValue, readerWriter.FromJson(ref readerManager, existingObject: null));
    }

    [Fact]
    public void DateInterval_is_properly_mapped()
    {
        Assert.Equal("daterange", GetMapping(typeof(DateInterval)).StoreType);
        Assert.Same(typeof(DateInterval), GetMapping("daterange").ClrType);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_DateInterval_literal()
    {
        var mapping = GetMapping(typeof(DateInterval));
        Assert.Equal("daterange", mapping.StoreType);

        var interval = new DateInterval(new LocalDate(2020, 01, 01), new LocalDate(2020, 12, 25));
        Assert.Equal("'[2020-01-01,2020-12-25]'::daterange", mapping.GenerateSqlLiteral(interval));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_DateInterval_literal()
        => Assert.Equal(
            "new NodaTime.DateInterval(new NodaTime.LocalDate(2020, 1, 1), new NodaTime.LocalDate(2020, 12, 25))",
            CodeLiteral(new DateInterval(new LocalDate(2020, 01, 01), new LocalDate(2020, 12, 25))));

    [Fact]
    public void DateInterval_array_is_properly_mapped()
    {
        Assert.Equal("datemultirange", GetMapping(typeof(DateInterval[])).StoreType);
        Assert.Same(typeof(DateInterval[]), GetMapping("datemultirange").ClrType);
    }

    [Fact]
    public void DateInterval_list_is_properly_mapped()
        => Assert.Equal("datemultirange", GetMapping(typeof(List<DateInterval>)).StoreType);

    [Fact]
    public void GenerateSqlLiteral_returns_DateInterval_array_literal()
    {
        var mapping = GetMapping(typeof(DateInterval[]));

        var interval = new DateInterval[]
        {
            new(new LocalDate(2002, 3, 4), new LocalDate(2002, 3, 5)), new(new LocalDate(2002, 3, 8), new LocalDate(2002, 3, 10))
        };

        Assert.Equal("'{[2002-03-04,2002-03-05], [2002-03-08,2002-03-10]}'::datemultirange", mapping.GenerateSqlLiteral(interval));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_DateInterval_list_literal()
    {
        var mapping = GetMapping(typeof(List<DateInterval>));

        var interval = new List<DateInterval>
        {
            new(new LocalDate(2002, 3, 4), new LocalDate(2002, 3, 5)), new(new LocalDate(2002, 3, 8), new LocalDate(2002, 3, 10))
        };

        Assert.Equal("'{[2002-03-04,2002-03-05], [2002-03-08,2002-03-10]}'::datemultirange", mapping.GenerateSqlLiteral(interval));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_DateInterval_array_literal()
        => Assert.Equal(
            "new[] { new NodaTime.DateInterval(new NodaTime.LocalDate(2002, 3, 4), new NodaTime.LocalDate(2002, 3, 5)), new NodaTime.DateInterval(new NodaTime.LocalDate(2002, 3, 8), new NodaTime.LocalDate(2002, 3, 10)) }",
            CodeLiteral(
                new DateInterval[]
                {
                    new(new LocalDate(2002, 3, 4), new LocalDate(2002, 3, 5)),
                    new(new LocalDate(2002, 3, 8), new LocalDate(2002, 3, 10))
                }));

    [Fact]
    public void GenerateCodeLiteral_returns_DateInterval_list_literal()
        => Assert.Equal(
            "new List<DateInterval> { new NodaTime.DateInterval(new NodaTime.LocalDate(2002, 3, 4), new NodaTime.LocalDate(2002, 3, 5)), new NodaTime.DateInterval(new NodaTime.LocalDate(2002, 3, 8), new NodaTime.LocalDate(2002, 3, 10)) }",
            CodeLiteral(
                new List<DateInterval>
                {
                    new(new LocalDate(2002, 3, 4), new LocalDate(2002, 3, 5)),
                    new(new LocalDate(2002, 3, 8), new LocalDate(2002, 3, 10))
                }));

    [Fact]
    public void NpgsqlRange_of_LocalDate_is_properly_mapped()
        => Assert.Equal("daterange", GetMapping(typeof(NpgsqlRange<LocalDate>)).StoreType);

    [Fact]
    public void GenerateSqlLiteral_returns_daterange_LocalDate_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping(typeof(NpgsqlRange<LocalDate>));
        var value = new NpgsqlRange<LocalDate>(new LocalDate(2020, 1, 1), new LocalDate(2020, 1, 2));
        Assert.Equal(@"'[2020-01-01,2020-01-02]'::daterange", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void Array_of_NpgsqlRange_of_LocalDate_is_properly_mapped()
        => Assert.Equal("datemultirange", GetMapping(typeof(NpgsqlRange<LocalDate>[])).StoreType);

    [Fact]
    public void List_of_NpgsqlRange_of_LocalDate_is_properly_mapped()
        => Assert.Equal("datemultirange", GetMapping(typeof(List<NpgsqlRange<LocalDate>>)).StoreType);

    #endregion date/daterange/datemultirange

    #region time

    [Fact]
    public void GenerateSqlLiteral_returns_LocalTime_literal()
    {
        var mapping = GetMapping(typeof(LocalTime));

        Assert.Equal("TIME '10:31:33'", mapping.GenerateSqlLiteral(new LocalTime(10, 31, 33)));
        Assert.Equal("TIME '10:31:33.666'", mapping.GenerateSqlLiteral(new LocalTime(10, 31, 33, 666)));
        Assert.Equal("TIME '10:31:33.666666'", mapping.GenerateSqlLiteral(new LocalTime(10, 31, 33, 666) + Period.FromTicks(6660)));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_LocalTime_literal()
    {
        Assert.Equal("new NodaTime.LocalTime(9, 30)", CodeLiteral(new LocalTime(9, 30)));
        Assert.Equal("new NodaTime.LocalTime(9, 30, 15)", CodeLiteral(new LocalTime(9, 30, 15)));
        Assert.Equal(
            "NodaTime.LocalTime.FromHourMinuteSecondNanosecond(9, 30, 15, 500000000L)", CodeLiteral(new LocalTime(9, 30, 15, 500)));
        Assert.Equal(
            "NodaTime.LocalTime.FromHourMinuteSecondNanosecond(9, 30, 15, 1L)",
            CodeLiteral(LocalTime.FromHourMinuteSecondNanosecond(9, 30, 15, 1)));
    }

    [Fact]
    public void LocalTime_array_is_properly_mapped()
    {
        Assert.Equal("time[]", GetMapping(typeof(LocalTime[])).StoreType);
        Assert.Same(typeof(List<LocalTime>), GetMapping("time[]").ClrType);
    }

    [Fact]
    public void LocalTime_list_is_properly_mapped()
        => Assert.Equal("time[]", GetMapping(typeof(List<LocalTime>)).StoreType);

    [ConditionalTheory]
    [InlineData("00:00:00.0000000", "00:00:00")]
    [InlineData("23:59:59.9999999", "23:59:59.9999999")]
    [InlineData("11:05:12.3456789", "11:05:12.3456789")]
    public void LocalTime_json(string timeString, string json)
    {
        var readerWriter = GetMapping(typeof(LocalTime)).JsonValueReaderWriter!;

        var time = LocalTimePattern.ExtendedIso.Parse(timeString).GetValueOrThrow();
        var actualJson = readerWriter.ToJsonString(time)[1..^1];
        Assert.Equal(json, actualJson);

        // TODO: The following should just do ToJsonString(), but see https://github.com/dotnet/efcore/issues/32269
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(Encoding.UTF8.GetBytes($"\"{json}\"")), null);
        readerManager.MoveNext();
        var actualTime = readerWriter.FromJson(ref readerManager, existingObject: null);
        Assert.Equal(time, actualTime);
    }

    #endregion time

    #region timetz

    [Fact]
    public void GenerateSqlLiteral_returns_OffsetTime_literal()
    {
        var mapping = GetMapping(typeof(OffsetTime));

        Assert.Equal(
            "TIMETZ '10:31:33+02'", mapping.GenerateSqlLiteral(
                new OffsetTime(new LocalTime(10, 31, 33), Offset.FromHours(2))));
        Assert.Equal(
            "TIMETZ '10:31:33-02:30'", mapping.GenerateSqlLiteral(
                new OffsetTime(new LocalTime(10, 31, 33), Offset.FromHoursAndMinutes(-2, -30))));
        Assert.Equal(
            "TIMETZ '10:31:33.666666Z'", mapping.GenerateSqlLiteral(
                new OffsetTime(new LocalTime(10, 31, 33, 666) + Period.FromTicks(6660), Offset.Zero)));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_OffsetTime_literal()
        => Assert.Equal(
            "new NodaTime.OffsetTime(new NodaTime.LocalTime(10, 31, 33), NodaTime.Offset.FromHours(2))",
            CodeLiteral(new OffsetTime(new LocalTime(10, 31, 33), Offset.FromHours(2))));

    [ConditionalTheory]
    [InlineData("00:00:00.0000000Z", "00:00:00Z")]
    [InlineData("23:59:59.999999Z", "23:59:59.999999Z")]
    [InlineData("11:05:12-02", "11:05:12-02")]
    public void OffsetTime_json(string timeString, string json)
    {
        var readerWriter = GetMapping(typeof(OffsetTime)).JsonValueReaderWriter!;

        var timeOffset = OffsetTimePattern.ExtendedIso.Parse(timeString).GetValueOrThrow();
        var actualJson = readerWriter.ToJsonString(timeOffset)[1..^1];
        Assert.Equal(json, actualJson);

        // TODO: The following should just do ToJsonString(), but see https://github.com/dotnet/efcore/issues/32269
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(Encoding.UTF8.GetBytes($"\"{json}\"")), null);
        readerManager.MoveNext();
        var actualTimeOffset = readerWriter.FromJson(ref readerManager, existingObject: null);
        Assert.Equal(timeOffset, actualTimeOffset);
    }

    #endregion timetz

    #region interval

    [Fact]
    public void Duration_is_properly_mapped()
        => Assert.All(
            new[] { GetMapping(typeof(Duration)), GetMapping(typeof(Duration), "interval") },
            m =>
            {
                Assert.Equal("interval", m.StoreType);
                Assert.Same(typeof(Duration), m.ClrType);
            });

    [Fact]
    public void Period_is_properly_mapped()
        => Assert.All(
            new[] { GetMapping(typeof(Period)), GetMapping(typeof(Period), "interval") },
            m =>
            {
                Assert.Equal("interval", m.StoreType);
                Assert.Same(typeof(Period), m.ClrType);
            });

    [Fact]
    public void GenerateSqlLiteral_returns_Period_literal()
    {
        var mapping = GetMapping(typeof(Period));

        var hms = Period.FromHours(4) + Period.FromMinutes(3) + Period.FromSeconds(2);
        Assert.Equal("INTERVAL 'PT4H3M2S'", mapping.GenerateSqlLiteral(hms));

        var withMilliseconds = hms + Period.FromMilliseconds(1);
        Assert.Equal("INTERVAL 'PT4H3M2.001S'", mapping.GenerateSqlLiteral(withMilliseconds));

        var withMicroseconds = hms + Period.FromTicks(6660);
        Assert.Equal("INTERVAL 'PT4H3M2.000666S'", mapping.GenerateSqlLiteral(withMicroseconds));

        var withYearMonthDay = hms + Period.FromYears(2018) + Period.FromMonths(4) + Period.FromDays(20);
        Assert.Equal("INTERVAL 'P2018Y4M20DT4H3M2S'", mapping.GenerateSqlLiteral(withYearMonthDay));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_Period_literal()
    {
        Assert.Equal("NodaTime.Period.FromHours(5L)", CodeLiteral(Period.FromHours(5)));

        Assert.Equal(
            "NodaTime.Period.FromYears(1) + NodaTime.Period.FromMonths(2) + NodaTime.Period.FromWeeks(3) + "
            + "NodaTime.Period.FromDays(4) + NodaTime.Period.FromHours(5L) + NodaTime.Period.FromMinutes(6L) + "
            + "NodaTime.Period.FromSeconds(7L) + NodaTime.Period.FromMilliseconds(8L) + NodaTime.Period.FromNanoseconds(9L)",
            CodeLiteral(
                Period.FromYears(1)
                + Period.FromMonths(2)
                + Period.FromWeeks(3)
                + Period.FromDays(4)
                + Period.FromHours(5)
                + Period.FromMinutes(6)
                + Period.FromSeconds(7)
                + Period.FromMilliseconds(8)
                + Period.FromNanoseconds(9)));

        Assert.Equal("NodaTime.Period.Zero", CodeLiteral(Period.Zero));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_Duration_literal()
    {
        Assert.Equal("NodaTime.Duration.FromHours(5)", CodeLiteral(Duration.FromHours(5)));

        Assert.Equal(
            "NodaTime.Duration.FromDays(4) + NodaTime.Duration.FromHours(5) + NodaTime.Duration.FromMinutes(6L) + "
            + "NodaTime.Duration.FromSeconds(7L) + NodaTime.Duration.FromMilliseconds(8L)",
            CodeLiteral(
                Duration.FromDays(4)
                + Duration.FromHours(5)
                + Duration.FromMinutes(6)
                + Duration.FromSeconds(7)
                + Duration.FromMilliseconds(8)));

        Assert.Equal("NodaTime.Duration.Zero", CodeLiteral(Duration.Zero));
    }

    [ConditionalTheory]
    [InlineData("-10675199:02:48:05.477580", "-10675199 02:48:05.47758")]
    [InlineData("10675199:02:48:05.477580", "10675199 02:48:05.47758")]
    [InlineData("00:00:00", "00:00:00")]
    [InlineData("12:23:23.801885", "12:23:23.801885")]
    public void Duration_json(string durationString, string json)
    {
        var readerWriter = GetMapping(typeof(Duration)).JsonValueReaderWriter!;

        var duration = durationString.Count(c => c == ':') == 3 // there's a Days component
            ? DurationPattern.Roundtrip.Parse(durationString).GetValueOrThrow()
            : DurationPattern.JsonRoundtrip.Parse(durationString).GetValueOrThrow();
        var actualJson = readerWriter.ToJsonString(duration)[1..^1];
        Assert.Equal(json, actualJson);

        // TODO: The following should just do ToJsonString(), but see https://github.com/dotnet/efcore/issues/32269
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(Encoding.UTF8.GetBytes($"\"{json}\"")), null);
        readerManager.MoveNext();
        var actualDuration =  readerWriter.FromJson(ref readerManager, existingObject: null);
        Assert.Equal(duration, actualDuration);
    }

    [ConditionalTheory]
    [InlineData("P2018Y4M20DT4H3M2S")]
    public void Period_json(string intervalString)
    {
        var readerWriter = GetMapping(typeof(Period)).JsonValueReaderWriter!;

        var period = PeriodPattern.NormalizingIso.Parse(intervalString).GetValueOrThrow();
        var actualJson = readerWriter.ToJsonString(period)[1..^1];
        Assert.Equal(intervalString, actualJson);

        // TODO: The following should just do ToJsonString(), but see https://github.com/dotnet/efcore/issues/32269
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(Encoding.UTF8.GetBytes($"\"{intervalString}\"")), null);
        readerManager.MoveNext();
        var actualPeriod =  readerWriter.FromJson(ref readerManager, existingObject: null);
        Assert.Equal(period, actualPeriod);
    }


    #endregion interval

    #region DateTimeZone

    [Fact]
    public void DateTimeZone_is_properly_mapped()
    {
        var mapping = GetMapping(typeof(DateTimeZone));

        Assert.Same(typeof(DateTimeZone), mapping.ClrType);
        Assert.Equal("text", mapping.StoreType);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_DateTimeZone_literal()
    {
        var mapping = GetMapping(typeof(DateTimeZone));

        Assert.Equal("Europe/Berlin", mapping.GenerateSqlLiteral(DateTimeZoneProviders.Tzdb["Europe/Berlin"]));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_DateTimezone_literal()
        => Assert.Equal(
            """NodaTime.DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Berlin")""",
            CodeLiteral(DateTimeZoneProviders.Tzdb["Europe/Berlin"]));

    #endregion

    #region Support

    private static readonly NpgsqlTypeMappingSource Mapper = new(
        new TypeMappingSourceDependencies(
            new ValueConverterSelector(new ValueConverterSelectorDependencies()),
            new JsonValueReaderWriterSource(new JsonValueReaderWriterSourceDependencies()),
            Array.Empty<ITypeMappingSourcePlugin>()),
        new RelationalTypeMappingSourceDependencies(
            new IRelationalTypeMappingSourcePlugin[]
            {
                new NpgsqlNodaTimeTypeMappingSourcePlugin(
                    new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()))
            }),
        new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
        new NpgsqlSingletonOptions()
    );

    private static RelationalTypeMapping GetMapping(string storeType)
        => Mapper.FindMapping(storeType);

    private static RelationalTypeMapping GetMapping(Type clrType)
        => Mapper.FindMapping(clrType);

    private static RelationalTypeMapping GetMapping(Type clrType, string storeType)
        => Mapper.FindMapping(clrType, storeType);

    private static readonly CSharpHelper CsHelper = new(Mapper);

    private static string CodeLiteral(object value)
        => CsHelper.UnknownLiteral(value);

    #endregion Support
}
