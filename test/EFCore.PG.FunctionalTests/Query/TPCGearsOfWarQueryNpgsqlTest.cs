using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class TPCGearsOfWarQueryNpgsqlTest : TPCGearsOfWarQueryRelationalTestBase<TPCGearsOfWarQueryNpgsqlFixture>
{
    public TPCGearsOfWarQueryNpgsqlTest(TPCGearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

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
@start='1902-01-01T10:00:00.1234567+00:00' (DbType = DateTime)
@end='1902-01-03T10:00:00.1234567+00:00' (DbType = DateTime)
@dates={ '1902-01-02T10:00:00.1234567+00:00' } (DbType = Object)

SELECT m."Id", m."CodeName", m."Date", m."Difficulty", m."Duration", m."Rating", m."Time", m."Timeline"
FROM "Missions" AS m
WHERE @start <= date_trunc('day', m."Timeline" AT TIME ZONE 'UTC')::timestamptz AND m."Timeline" < @end AND m."Timeline" = ANY (@dates)
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
@dateTimeOffset_Date='0002-03-01T00:00:00.0000000'

SELECT m."Id", m."CodeName", m."Date", m."Difficulty", m."Duration", m."Rating", m."Time", m."Timeline"
FROM "Missions" AS m
WHERE date_trunc('day', m."Timeline" AT TIME ZONE 'UTC')::timestamp >= @dateTimeOffset_Date
""");
    }

    // Not supported by design: we support getting a local DateTime via DateTime.Now (based on PG TimeZone), but there's no way to get a
    // non-UTC DateTimeOffset.
    public override Task DateTimeOffsetNow_minus_timespan(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.DateTimeOffsetNow_minus_timespan(async));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
