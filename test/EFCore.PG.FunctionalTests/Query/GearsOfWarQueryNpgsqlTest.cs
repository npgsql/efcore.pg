using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once UnusedMember.Global
public class GearsOfWarQueryNpgsqlTest : GearsOfWarQueryRelationalTestBase<GearsOfWarQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public GearsOfWarQueryNpgsqlTest(GearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Byte array

    public override async Task Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(bool async)
    {
        await base.Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(async);

        AssertSql(
            """
SELECT s."Id", s."Banner", s."Banner5", s."InternalNumber", s."Name"
FROM "Squads" AS s
WHERE length(s."Banner5") = 5
""");
    }

    #endregion Byte array

    #region DateTimeOffset

    // Not supported by design: we support getting a local DateTime via DateTime.Now (based on PG TimeZone), but there's no way to get a
    // non-UTC DateTimeOffset.
    public override Task DateTimeOffsetNow_minus_timespan(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.DateTimeOffsetNow_minus_timespan(async));

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
            assertEmpty: true); // TODO: Look into this

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

    #endregion DateTimeOffset

    #region DateTime

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_subtraction(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(
                m =>
                    new DateTimeOffset(2, 3, 2, 8, 0, 0, new TimeSpan(-5, 0, 0)) - m.Timeline > TimeSpan.FromDays(3)),
            assertEmpty: true); // TODO: Look into this

    #endregion DateTime

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_TimeSpan_TotalDays(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Duration.TotalDays < 0.042));

        AssertSql(
            """
SELECT m."Id", m."CodeName", m."Date", m."Difficulty", m."Duration", m."Rating", m."Time", m."Timeline"
FROM "Missions" AS m
WHERE date_part('epoch', m."Duration") / 86400.0 < 0.042000000000000003
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_TimeSpan_TotalHours(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Duration.TotalHours < 1.02));

        AssertSql(
            """
SELECT m."Id", m."CodeName", m."Date", m."Difficulty", m."Duration", m."Rating", m."Time", m."Timeline"
FROM "Missions" AS m
WHERE date_part('epoch', m."Duration") / 3600.0 < 1.02
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_TimeSpan_TotalMinutes(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Duration.TotalMinutes < 61));

        AssertSql(
            """
SELECT m."Id", m."CodeName", m."Date", m."Difficulty", m."Duration", m."Rating", m."Time", m."Timeline"
FROM "Missions" AS m
WHERE date_part('epoch', m."Duration") / 60.0 < 61.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_TimeSpan_TotalSeconds(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Duration.TotalSeconds < 3700));

        AssertSql(
            """
SELECT m."Id", m."CodeName", m."Date", m."Difficulty", m."Duration", m."Rating", m."Time", m."Timeline"
FROM "Missions" AS m
WHERE date_part('epoch', m."Duration") < 3700.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_TimeSpan_TotalMilliseconds(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Duration.TotalMilliseconds < 3700000));

        AssertSql(
            """
SELECT m."Id", m."CodeName", m."Date", m."Difficulty", m."Duration", m."Rating", m."Time", m."Timeline"
FROM "Missions" AS m
WHERE date_part('epoch', m."Duration") / 0.001 < 3700000.0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_Property_Select_Sum_over_TimeSpan(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Mission>()
                .GroupBy(o => o.Id)
                .Select(g => EF.Functions.Sum(g.Select(o => o.Duration))),
            ss => ss.Set<Mission>()
                .GroupBy(o => o.Id)
                .Select(g => (TimeSpan?)new TimeSpan(g.Sum(o => o.Duration.Ticks))));

        AssertSql(
            """
SELECT sum(m."Duration")
FROM "Missions" AS m
GROUP BY m."Id"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_Property_Select_Average_over_TimeSpan(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Mission>()
                .GroupBy(o => o.Id)
                .Select(g => EF.Functions.Average(g.Select(o => o.Duration))),
            ss => ss.Set<Mission>()
                .GroupBy(o => o.Id)
                .Select(g => (TimeSpan?)new TimeSpan((long)g.Average(o => o.Duration.Ticks))));

        AssertSql(
            """
SELECT avg(m."Duration")
FROM "Missions" AS m
GROUP BY m."Id"
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
