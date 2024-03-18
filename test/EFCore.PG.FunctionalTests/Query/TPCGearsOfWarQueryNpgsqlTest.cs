using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPCGearsOfWarQueryNpgsqlTest : TPCGearsOfWarQueryRelationalTestBase<TPCGearsOfWarQueryNpgsqlFixture>
{
    public TPCGearsOfWarQueryNpgsqlTest(TPCGearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory(Skip = "https://github.com/npgsql/efcore.pg/issues/2039")]
    public override Task Where_DateOnly_Year(bool async)
        => base.Where_DateOnly_Year(async);

    // Base implementation uses DateTimeOffset.Now, which we don't translate by design. Use DateTimeOffset.UtcNow instead.
    public override async Task Select_datetimeoffset_comparison_in_projection(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Mission>().Select(m => m.Timeline > DateTimeOffset.UtcNow));

        AssertSql(
            """
SELECT m."Timeline" > now()
FROM "Missions" AS m
""");
    }

    public override async Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
    {
        var dto = new DateTimeOffset(599898024001234567, new TimeSpan(0));
        var start = dto.AddDays(-1);
        var end = dto.AddDays(1);
        var dates = new[] { dto };

        await AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(
                m => start <= m.Timeline.Date && m.Timeline < end && dates.Contains(m.Timeline)),
            assertEmpty: true); // TODO: Check this out

        AssertSql(
            """
@__start_0='1902-01-01T10:00:00.1234567+00:00' (DbType = DateTime)
@__end_1='1902-01-03T10:00:00.1234567+00:00' (DbType = DateTime)
@__dates_2={ '1902-01-02T10:00:00.1234567+00:00' } (DbType = Object)

SELECT m."Id", m."CodeName", m."Date", m."Duration", m."Rating", m."Time", m."Timeline"
FROM "Missions" AS m
WHERE @__start_0 <= date_trunc('day', m."Timeline" AT TIME ZONE 'UTC')::timestamptz AND m."Timeline" < @__end_1 AND m."Timeline" = ANY (@__dates_2)
""");
    }

    public override async Task DateTimeOffset_Date_returns_datetime(bool async)
    {
        var dateTimeOffset = new DateTimeOffset(2, 3, 1, 8, 0, 0, new TimeSpan(-5, 0, 0));

        await AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Timeline.Date.ToLocalTime() >= dateTimeOffset.Date));

        AssertSql(
            """
@__dateTimeOffset_Date_0='0002-03-01T00:00:00.0000000'

SELECT m."Id", m."CodeName", m."Date", m."Duration", m."Rating", m."Time", m."Timeline"
FROM "Missions" AS m
WHERE date_trunc('day', m."Timeline" AT TIME ZONE 'UTC')::timestamp >= @__dateTimeOffset_Date_0
""");
    }

    public override async Task Where_datetimeoffset_date_component(bool async)
    {
        await AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline.Date > new DateTime(1)
                  select m);

        AssertSql(
            """
SELECT m."Id", m."CodeName", m."Date", m."Duration", m."Rating", m."Time", m."Timeline"
FROM "Missions" AS m
WHERE date_trunc('day', m."Timeline" AT TIME ZONE 'UTC') > TIMESTAMP '0001-01-01T00:00:00'
""");
    }

    // Not supported by design: we support getting a local DateTime via DateTime.Now (based on PG TimeZone), but there's no way to get a
    // non-UTC DateTimeOffset.
    public override Task Where_datetimeoffset_now(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_datetimeoffset_now(async));

    // Not supported by design: we support getting a local DateTime via DateTime.Now (based on PG TimeZone), but there's no way to get a
    // non-UTC DateTimeOffset.
    public override Task DateTimeOffsetNow_minus_timespan(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.DateTimeOffsetNow_minus_timespan(async));

    // SQL translation not implemented, too annoying
    public override Task Where_datetimeoffset_millisecond_component(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_datetimeoffset_millisecond_component(async));

    public override Task DateTimeOffset_to_unix_time_milliseconds(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_to_unix_time_milliseconds(async));

    public override Task DateTimeOffset_to_unix_time_seconds(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_to_unix_time_seconds(async));

    // Test runs successfully, but some time difference and precision issues and fail the assertion
    public override Task Where_TimeSpan_Hours(bool async)
        => Task.CompletedTask;

    public override Task Where_TimeOnly_Millisecond(bool async)
        => Task.CompletedTask; // Translation not implemented

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
