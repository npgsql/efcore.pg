using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.TestModels.NodaTime;
using NodaTime;

namespace Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;

/// <summary>
///     Checks that NodaTime expressions evaluated by PostgreSQL produce exactly the same results as NodaTime itself.
/// </summary>
/// <remarks>
///     <para>
///         Each case is a lambda, its arguments and its expected (correct) result. The lambda is evaluated in .NET
///         (via <see cref="LambdaExpression.Compile()" />), which must return the expected result (so that each case's expectation
///         is checked against NodaTime/.NET itself), and twice in the database: once with each lambda parameter replaced by
///         <see cref="EF.Parameter{T}" />, and once by <see cref="EF.Constant{T}" /> (so that literal generation is covered too), which
///         must also return the expected result. This prevents EF from evaluating the expression on the client, and
///         <see cref="AssertEvaluatedInDatabase" /> verifies that it didn't. The result is projected, so it must also be read back as the
///         right type; in addition, <see cref="NodaTimeParityFixture" /> verifies that every SQL expression in the query has a type mapping
///         matching its NodaTime type.
///     </para>
///     <para>
///         To reproduce a discrepancy, add a single <c>yield return Case(...)</c> line to the relevant group below.
///         Known discrepancies are marked with <see cref="NodaTimeParityTestExtensions.Skip" />.
///     </para>
/// </remarks>
[SuppressMessage("Usage", "xUnit1046:Avoid using TheoryDataRow arguments that are not serializable")]
public class NodaTimeParityTest : IClassFixture<NodaTimeParityFixture>
{
    public NodaTimeParityTest(NodaTimeParityFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    private NodaTimeParityFixture Fixture { get; }

    // Europe/Berlin (the fixture's session time zone) switched to daylight saving time at 2018-03-25T01:00Z
    // Durations of whole days must not be affected by this.
    private static readonly Instant Instant = Instant.FromUtc(2018, 4, 20, 10, 31, 33) + Duration.FromMilliseconds(666);
    private static readonly Instant BeforeDst = Instant.FromUtc(2018, 3, 24, 12, 0);
    private static readonly ZonedDateTime Zoned = Instant.InUtc();
    private static readonly ZonedDateTime ZonedBeforeDst = BeforeDst.InUtc();

    #region Instant

    public static IEnumerable<TheoryDataRow<LambdaExpression, object?[], object?>> InstantCases()
    {
        // Instant +/- Duration, across a daylight saving time transition in the session time zone
        yield return Case((i, d) => i + d, BeforeDst, Duration.FromDays(1), Instant.FromUtc(2018, 3, 25, 12, 0));
        yield return Case((i, d) => i + d, BeforeDst, Duration.FromHours(36), Instant.FromUtc(2018, 3, 26, 0, 0));
        yield return Case((i, d) => i + d, BeforeDst, Duration.FromHours(5), Instant.FromUtc(2018, 3, 24, 17, 0));
        yield return Case(
            (i, d) => i - d, Instant, Duration.FromDays(30),
            Instant.FromUtc(2018, 3, 21, 10, 31, 33) + Duration.FromMilliseconds(666));

        // Negative durations (with a days component)
        yield return Case((i, d) => i + d, BeforeDst, Duration.FromHours(-26), Instant.FromUtc(2018, 3, 23, 10, 0));
        yield return Case((i, d) => i - d, BeforeDst, Duration.FromHours(-36), Instant.FromUtc(2018, 3, 26, 0, 0));
    }

    [ConditionalTheory]
    [MemberData(nameof(InstantCases))]
    public Task Instant_matches_NodaTime(LambdaExpression expression, object?[] arguments, object? expected)
        => AssertCorrect(expression, arguments, expected);

    #endregion Instant

    #region ZonedDateTime

    public static IEnumerable<TheoryDataRow<LambdaExpression, object?[], object?>> ZonedDateTimeCases()
    {
        // ZonedDateTime +/- Duration, across a daylight saving time transition in the session time zone
        yield return Case(
            (z, d) => z + d, ZonedBeforeDst, Duration.FromDays(1), Instant.FromUtc(2018, 3, 25, 12, 0).InUtc());
        yield return Case(
            (z, d) => z + d, ZonedBeforeDst, Duration.FromHours(36), Instant.FromUtc(2018, 3, 26, 0, 0).InUtc());
        yield return Case(
            (z, d) => z - d, Zoned, Duration.FromDays(30),
            (Instant.FromUtc(2018, 3, 21, 10, 31, 33) + Duration.FromMilliseconds(666)).InUtc());
    }

    [ConditionalTheory]
    [MemberData(nameof(ZonedDateTimeCases))]
    public Task ZonedDateTime_matches_NodaTime(LambdaExpression expression, object?[] arguments, object? expected)
        => AssertCorrect(expression, arguments, expected);

    #endregion ZonedDateTime

    #region TimeSpan and Duration

    // TimeSpan and NodaTime's Duration are both an exact amount of time mapped to PostgreSQL interval, with the same arithmetic and the
    // same component/total members (with the same semantics), so the exact same cases are generated for both.
    // Each value with its expected total and components, which are truncated towards zero and all have the value's sign
    private static readonly (TimeSpan Value, IntervalParts Expected)[] IntervalValues =
    [
        (TimeSpan.Zero, new(0, 0, 0, 0, 0, 0)),
        (new(1, 2, 3, 4, 500), new(93_784_500_000, 1, 2, 3, 4, 500)),
        (-new TimeSpan(1, 2, 3, 4, 500), new(-93_784_500_000, -1, -2, -3, -4, -500)),
        (TimeSpan.FromHours(36), new(129_600_000_000, 1, 12, 0, 0, 0)),
        (TimeSpan.FromHours(-26), new(-93_600_000_000, -1, -2, 0, 0, 0)),
        (TimeSpan.FromSeconds(-90), new(-90_000_000, 0, 0, -1, -30, 0)),
        (TimeSpan.FromMilliseconds(-1500), new(-1_500_000, 0, 0, 0, -1, -500)),
        (TimeSpan.FromMilliseconds(-1500.5), new(-1_500_500, 0, 0, 0, -1, -500)),
        (TimeSpan.FromMicroseconds(500), new(500, 0, 0, 0, 0, 0)),
        (TimeSpan.FromMicroseconds(-999_999), new(-999_999, 0, 0, 0, 0, -999)),
        (new(1000, 23, 59, 59, 999, 999), new(86_486_399_999_999, 1000, 23, 59, 59, 999))
    ];

    // Pairs of values which are added and subtracted in SQL, with the expected results.
    // PostgreSQL adds and subtracts an interval's day and time fields separately and Npgsql writes a
    // TimeSpan/Duration with the same sign in both fields and less than a day in the time field.
    // So the result's fields can have opposite signs, or a time field of a day or more,
    // in which case the components must be read from justify_hours() (JustifyRequired).
    private static readonly IntervalPair[] IntervalPairs =
    [
        // Fields with the same sign, and a time field of less than a day
        new(TimeSpan.FromHours(1), TimeSpan.FromMinutes(30),
            Sum: new(5_400_000_000, 0, 1, 30, 0, 0), SumJustifyRequired: false,
            Difference: new(1_800_000_000, 0, 0, 30, 0, 0), DifferenceJustifyRequired: false),
        new(TimeSpan.FromDays(-1), TimeSpan.FromHours(-1),
            Sum: new(-90_000_000_000, -1, -1, 0, 0, 0), SumJustifyRequired: false,
            // -1 day +01:00:00
            Difference: new(-82_800_000_000, 0, -23, 0, 0, 0), DifferenceJustifyRequired: true),

        // Fields with opposite signs
        new(TimeSpan.FromDays(1), TimeSpan.FromHours(-1),
            // 1 day -01:00:00
            Sum: new(82_800_000_000, 0, 23, 0, 0, 0), SumJustifyRequired: true,
            Difference: new(90_000_000_000, 1, 1, 0, 0, 0), DifferenceJustifyRequired: false),
        new(TimeSpan.FromDays(-1), TimeSpan.FromMilliseconds(2500.5),
            // -1 day +00:00:02.5005
            Sum: new(-86_397_499_500, 0, -23, -59, -57, -499), SumJustifyRequired: true,
            Difference: new(-86_402_500_500, -1, 0, 0, -2, -500), DifferenceJustifyRequired: false),
        new(TimeSpan.FromDays(1), TimeSpan.FromMilliseconds(1500),
            Sum: new(86_401_500_000, 1, 0, 0, 1, 500), SumJustifyRequired: false,
            // 1 day -00:00:01.5
            Difference: new(86_398_500_000, 0, 23, 59, 58, 500), DifferenceJustifyRequired: true),
        new(TimeSpan.FromDays(1), TimeSpan.FromHours(25),
            // 2 days 01:00:00
            Sum: new(176_400_000_000, 2, 1, 0, 0, 0), SumJustifyRequired: false,
            // -01:00:00 (0 days)
            Difference: new(-3_600_000_000, 0, -1, 0, 0, 0), DifferenceJustifyRequired: false),

        // A time field of a day or more
        new(TimeSpan.FromHours(20), TimeSpan.FromHours(20),
            // 40:00:00
            Sum: new(144_000_000_000, 1, 16, 0, 0, 0), SumJustifyRequired: true,
            Difference: new(0, 0, 0, 0, 0, 0), DifferenceJustifyRequired: false),
        new(TimeSpan.FromHours(-20), TimeSpan.FromHours(20),
            Sum: new(0, 0, 0, 0, 0, 0), SumJustifyRequired: false,
            // -40:00:00
            Difference: new(-144_000_000_000, -1, -16, 0, 0, 0), DifferenceJustifyRequired: true)
    ];

    private static readonly string[] IntervalMembers =
    [
        nameof(TimeSpan.Days), nameof(TimeSpan.Hours), nameof(TimeSpan.Minutes), nameof(TimeSpan.Seconds),
        nameof(TimeSpan.Milliseconds), nameof(TimeSpan.TotalDays), nameof(TimeSpan.TotalHours), nameof(TimeSpan.TotalMinutes),
        nameof(TimeSpan.TotalSeconds), nameof(TimeSpan.TotalMilliseconds)
    ];

    // TimeSpan and NodaTime's Duration are both an exact amount of time mapped to PostgreSQL interval, with the same arithmetic
    // and the same component/total members (with the same semantics), so the exact same cases are generated for both.
    public static IEnumerable<TheoryDataRow<LambdaExpression, object?[], object?>> IntervalCases(Type type)
    {
        var a = Expression.Parameter(type, "a");
        var b = Expression.Parameter(type, "b");

        foreach (var (value, expected) in IntervalValues)
        {
            yield return Row(Expression.Lambda(Expression.Negate(a), a), [Convert(value)], Convert(expected.Negate().ToTimeSpan()));

            foreach (var member in IntervalMembers)
            {
                yield return Row(Expression.Lambda(Expression.Property(a, member), a), [Convert(value)], expected.Member(member));
            }
        }

        foreach (var pair in IntervalPairs)
        {
            object?[] arguments = [Convert(pair.Left), Convert(pair.Right)];

            yield return Row(Expression.Lambda(Expression.Add(a, b), a, b), arguments, Convert(pair.Sum.ToTimeSpan()));
            yield return Row(Expression.Lambda(Expression.Subtract(a, b), a, b), arguments, Convert(pair.Difference.ToTimeSpan()));
            yield return Row(Expression.Lambda(Expression.Equal(a, b), a, b), [Convert(pair.Left), Convert(pair.Left)], true);
            yield return Row(Expression.Lambda(Expression.Equal(a, b), a, b), arguments, pair.Left == pair.Right);

            foreach (var member in IntervalMembers)
            {
                yield return Row(
                    Expression.Lambda(Expression.Property(Expression.Add(a, b), member), a, b), arguments, pair.Sum.Member(member));
                yield return Row(
                    Expression.Lambda(Expression.Property(Expression.Subtract(a, b), member), a, b), arguments,
                    pair.Difference.Member(member));
            }
        }

        yield break;

        object Convert(TimeSpan value)
            => type == typeof(Duration) ? Duration.FromTimeSpan(value) : value;
    }

    [ConditionalTheory]
    [MemberData(nameof(IntervalCases), typeof(TimeSpan))]
    public Task TimeSpan_matches_NET(LambdaExpression expression, object?[] arguments, object? expected)
        => AssertCorrect(expression, arguments, expected);

    [ConditionalTheory]
    [MemberData(nameof(IntervalCases), typeof(Duration))]
    public Task Duration_matches_NodaTime(LambdaExpression expression, object?[] arguments, object? expected)
        => AssertCorrect(expression, arguments, expected);

    public static IEnumerable<TheoryDataRow<TimeSpan, TimeSpan, bool, IntervalParts, bool>> IntervalPairCases()
        => IntervalPairs.SelectMany(p => new TheoryDataRow<TimeSpan, TimeSpan, bool, IntervalParts, bool>[]
        {
            new(p.Left, p.Right, false, p.Sum, p.SumJustifyRequired) { TestDisplayName = $"{p.Left} + {p.Right}" },
            new(p.Left, p.Right, true, p.Difference, p.DifferenceJustifyRequired) { TestDisplayName = $"{p.Left} - {p.Right}" }
        });

    /// <summary>
    ///     Checks that the components of an interval computed in SQL can only be read from justify_hours() when its fields have opposite
    ///     signs or its time field is a day or more, i.e. that the justify_hours() in the translation is required,
    ///     and that the pairs above cover both cases.
    /// </summary>
    [ConditionalTheory]
    [MemberData(nameof(IntervalPairCases))]
    public async Task Interval_components_require_justify_hours(
        TimeSpan left,
        TimeSpan right,
        bool subtract,
        IntervalParts expected,
        bool justifyRequired)
    {
        await using var context = Fixture.CreateContext();

        var expectedFields = new IntervalFields
        {
            Days = expected.Days,
            Hours = expected.Hours,
            Minutes = expected.Minutes,
            Seconds = expected.Seconds,
            Milliseconds = expected.Milliseconds
        };

        Assert.Equal(expectedFields, await ReadComponents("justify_hours(v)"));

        var withoutJustifyHours = await ReadComponents("v");
        Assert.True(
            justifyRequired != (withoutJustifyHours == expectedFields),
            justifyRequired
                ? $"Expected the components without justify_hours() to be wrong, but they were {withoutJustifyHours}"
                : $"Expected {expectedFields} without justify_hours(), but got {withoutJustifyHours}");

        return;

        Task<IntervalFields> ReadComponents(string interval)
        {
            // Only fixed SQL fragments are interpolated; the values are parameters
            var sql = $"""
                       SELECT
                           date_part('day', {interval})::int AS "Days",
                           date_part('hour', {interval})::int AS "Hours",
                           date_part('minute', {interval})::int AS "Minutes",
                           trunc(date_part('second', {interval}))::int AS "Seconds",
                           trunc(date_part('millisecond', {interval}))::int % 1000 AS "Milliseconds"
                       FROM (SELECT @left {(subtract ? "-" : "+")} @right AS v) AS i
                       """;

            return context.Database
                .SqlQueryRaw<IntervalFields>(sql, new NpgsqlParameter("left", left), new NpgsqlParameter("right", right))
                .SingleAsync();
        }
    }

    public sealed record IntervalParts(long Microseconds, int Days, int Hours, int Minutes, int Seconds, int Milliseconds)
    {
        public TimeSpan ToTimeSpan()
            => TimeSpan.FromTicks(Microseconds * TimeSpan.TicksPerMicrosecond);

        public IntervalParts Negate()
            => new(-Microseconds, -Days, -Hours, -Minutes, -Seconds, -Milliseconds);

        // The components are int and the totals double (without the object cast, a switch expression would make them all double)
        public object Member(string name)
            => name switch
            {
                nameof(TimeSpan.Days) => (object)Days,
                nameof(TimeSpan.Hours) => Hours,
                nameof(TimeSpan.Minutes) => Minutes,
                nameof(TimeSpan.Seconds) => Seconds,
                nameof(TimeSpan.Milliseconds) => Milliseconds,
                nameof(TimeSpan.TotalDays) => Microseconds / 86_400_000_000.0,
                nameof(TimeSpan.TotalHours) => Microseconds / 3_600_000_000.0,
                nameof(TimeSpan.TotalMinutes) => Microseconds / 60_000_000.0,
                nameof(TimeSpan.TotalSeconds) => Microseconds / 1_000_000.0,
                nameof(TimeSpan.TotalMilliseconds) => Microseconds / 1_000.0,
                _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
            };
    }

    private sealed record IntervalPair(
        TimeSpan Left,
        TimeSpan Right,
        IntervalParts Sum,
        bool SumJustifyRequired,
        IntervalParts Difference,
        bool DifferenceJustifyRequired);

    private sealed record IntervalFields
    {
        public int Days { get; init; }
        public int Hours { get; init; }
        public int Minutes { get; init; }
        public int Seconds { get; init; }
        public int Milliseconds { get; init; }
    }

    #endregion TimeSpan and Duration

    #region BCL date/time types

    // The same discrepancies apply to the BCL date/time types; they're covered here since this fixture's session time zone has daylight
    // saving time. DateTime/DateTimeOffset values are mapped to timestamptz, so they're UTC.
    public static IEnumerable<TheoryDataRow<LambdaExpression, object?[], object?>> BclCases()
    {
        var utcBeforeDst = new DateTime(2018, 3, 24, 12, 0, 0, DateTimeKind.Utc);
        var offsetBeforeDst = new DateTimeOffset(2018, 3, 24, 12, 0, 0, TimeSpan.Zero);
        var monthEndBeforeDst = new DateTimeOffset(2018, 2, 28, 23, 30, 0, TimeSpan.Zero);

        // Arithmetic on timestamptz
        yield return Case((DateTime d, double n) => d.AddDays(n), utcBeforeDst, 1, Utc(2018, 3, 25, 12));
        yield return Case(
            (d, n) => d.AddMonths(n), new DateTime(2018, 2, 24, 12, 0, 0, DateTimeKind.Utc), 1, Utc(2018, 3, 24, 12));
        yield return Case((DateTime d, double n) => d.AddHours(n), utcBeforeDst, 24, Utc(2018, 3, 25, 12));
        yield return Case((d, t) => d + t, utcBeforeDst, TimeSpan.FromDays(1), Utc(2018, 3, 25, 12));
        yield return Case(
            (d, t) => d - t, new DateTime(2018, 3, 26, 12, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(36),
            Utc(2018, 3, 25, 0));
        yield return Case((d, t) => d + t, utcBeforeDst, TimeSpan.FromHours(-26), Utc(2018, 3, 23, 10));
        yield return Case(
            (DateTimeOffset d, double n) => d.AddDays(n), offsetBeforeDst, 1, new DateTimeOffset(2018, 3, 25, 12, 0, 0, TimeSpan.Zero));
        yield return Case(
            (d, n) => d.AddMonths(n), monthEndBeforeDst, 1, new DateTimeOffset(2018, 3, 28, 23, 30, 0, TimeSpan.Zero));
        yield return Case(
            (d, t) => d + t, offsetBeforeDst, TimeSpan.FromDays(1),
            new DateTimeOffset(2018, 3, 25, 12, 0, 0, TimeSpan.Zero));

        // TimeSpan arithmetic and members are covered by IntervalCases, along with Duration's
        yield break;

        static DateTime Utc(int year, int month, int day, int hour)
            => new(year, month, day, hour, 0, 0, DateTimeKind.Utc);
    }

    [ConditionalTheory]
    [MemberData(nameof(BclCases))]
    public Task Bcl_matches_NET(LambdaExpression expression, object?[] arguments, object? expected)
        => AssertCorrect(expression, arguments, expected);

    #endregion BCL date/time types

    #region Support

    private static TheoryDataRow<LambdaExpression, object?[], object?> Case<T, TResult>(
        Expression<Func<T, TResult>> expression,
        T argument,
        TResult expected)
        => Row(expression, [argument], expected);

    private static TheoryDataRow<LambdaExpression, object?[], object?> Case<T1, T2, TResult>(
        Expression<Func<T1, T2, TResult>> expression,
        T1 argument1,
        T2 argument2,
        TResult expected)
        => Row(expression, [argument1, argument2], expected);

    private static TheoryDataRow<LambdaExpression, object?[], object?> Row(
        LambdaExpression expression,
        object?[] arguments,
        object? expected)
        => new(expression, arguments, expected) { TestDisplayName = $"{Describe(expression, arguments)} = {expected}" };

    private static string Describe(LambdaExpression expression, object?[] arguments)
        => $"{expression.Body} with " + string.Join(", ", expression.Parameters.Select((p, i) => $"{p.Name}={arguments[i]}"));

    private Task AssertCorrect(LambdaExpression expression, object?[] arguments, object? expected)
        => (Task)AssertCorrectMethod.MakeGenericMethod(expression.ReturnType).Invoke(this, [expression, arguments, expected])!;

    private static readonly MethodInfo AssertCorrectMethod
        = typeof(NodaTimeParityTest).GetMethod(nameof(AssertCorrectCore), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private async Task AssertCorrectCore<TResult>(LambdaExpression expression, object?[] arguments, object? expectedObject)
    {
        if (expectedObject is not null)
        {
            Assert.IsType<TResult>(expectedObject);
        }

        var expected = (TResult)expectedObject!;

        // The case's expected result must be NodaTime's/.NET's own result
        var client = (TResult)expression.Compile().DynamicInvoke(arguments)!;
        Assert.True(
            IsEqual(expression, expected, client),
            $"{Describe(expression, arguments)}: expected {expected}, .NET returned {client}");

        await using var context = Fixture.CreateContext();

        foreach (var argumentMethod in new[] { EfParameterMethod, EfConstantMethod })
        {
            Fixture.TestSqlLoggerFactory.Clear();
            var actual = await QueryDatabase<TResult>(context, ToServerSelector<TResult>(expression, arguments, argumentMethod));

            AssertEvaluatedInDatabase(argumentMethod == EfParameterMethod ? arguments.Length : 0);
            Assert.True(
                IsEqual(expression, expected, actual),
                $"{Describe(expression, arguments)} (as {argumentMethod.Name}): expected {expected}, PostgreSQL returned {actual}");
        }
    }

    private static Task<TResult> QueryDatabase<TResult>(NodaTimeContext context, Expression<Func<NodaTimeTypes, TResult>> selector)
        => context.Set<NodaTimeTypes>().Select(selector).SingleAsync();

    /// <summary>
    ///     EF evaluates an untranslated final projection on the client (e.g. <c>SELECT @p</c>, with the method applied to the parameter's
    ///     value when reading it), which would make a case pass without testing the database.
    ///     <see cref="NodaTimeParityFixture" /> rejects such queries.
    /// </summary>
    [ConditionalFact]
    public async Task Client_evaluation_is_rejected()
    {
        await using var context = Fixture.CreateContext();

        foreach (var argumentMethod in new[] { EfParameterMethod, EfConstantMethod })
        {
            var selector = ToServerSelector<long>((Duration d) => ClientOnly(d), [Duration.FromDays(1)], argumentMethod);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => QueryDatabase(context, selector));
            Assert.Contains("evaluated on the client", exception.Message);
        }
    }

    private static long ClientOnly(Duration duration)
        => duration.BclCompatibleTicks;

    private static bool IsEqual<TResult>(LambdaExpression expression, TResult expected, TResult actual)
        => EqualityComparer<TResult>.Default.Equals(expected, actual) || IsWithinTotalMemberTolerance(expression, expected, actual);

    /// <summary>
    ///     Known discrepancies are skipped, but their expected result must still be NodaTime's/.NET's own result.
    /// </summary>
    [ConditionalFact]
    public void Known_discrepancies_expect_the_NodaTime_result()
    {
        var rows = typeof(NodaTimeParityTest).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.ReturnType == typeof(IEnumerable<TheoryDataRow<LambdaExpression, object?[], object?>>))
            .SelectMany(m => m.GetParameters() is [{ ParameterType: var parameterType }] && parameterType == typeof(Type)
                ? new object?[][] { [typeof(TimeSpan)], [typeof(Duration)] }.Select(a => m.Invoke(null, a))
                : [m.Invoke(null, [])])
            .SelectMany(r => (IEnumerable<TheoryDataRow<LambdaExpression, object?[], object?>>)r!)
            .Where(r => r.Skip is not null);

        foreach (var row in rows)
        {
            var (expression, arguments, expected) = row.Data;
            var client = expression.Compile().DynamicInvoke(arguments);

            Assert.True(
                Equals(expected, client) || IsWithinTotalMemberTolerance(expression, expected, client),
                $"{Describe(expression, arguments)}: expected {expected}, .NET returned {client}");
        }
    }

    /// <summary>
    ///     TimeSpan/Duration's TotalDays...TotalMilliseconds are floating-point, and can differ from the database's result in the last digits.
    ///     The database computes date_part('epoch', x) / n, rounding twice; TimeSpan computes ticks / n and
    ///     NodaTime's Duration computes days * unitsPerDay + nanoOfDay / n, whose error is relative to days * unitsPerDay.
    ///     So these are compared within 1e-14 * (|expected| + unitsPerDay), which still catches any actual mistranslation (e.g. wrong units or sign).
    /// </summary>
    private static bool IsWithinTotalMemberTolerance(LambdaExpression expression, object? expected, object? actual)
    {
        if (expected is not double expectedDouble
            || actual is not double actualDouble
            || expression.Body is not MemberExpression { Member.Name: var memberName })
        {
            return false;
        }

        double? unitsPerDay = memberName switch
        {
            nameof(TimeSpan.TotalDays) => 1,
            nameof(TimeSpan.TotalHours) => 24,
            nameof(TimeSpan.TotalMinutes) => 24 * 60,
            nameof(TimeSpan.TotalSeconds) => 24 * 60 * 60,
            nameof(TimeSpan.TotalMilliseconds) => 24 * 60 * 60 * 1000,
            _ => null
        };

        return unitsPerDay is not null
            && Math.Abs(expectedDouble - actualDouble) <= 1e-14 * (Math.Abs(expectedDouble) + unitsPerDay.Value);
    }

    /// <summary>
    ///     Rewrites <c>(a, b) => a.Plus(b)</c> to <c>t => EF.Parameter(a).Plus(EF.Parameter(b))</c> (or <see cref="EF.Constant{T}" />),
    ///     so that the database evaluates the whole expression.
    /// </summary>
    private static Expression<Func<NodaTimeTypes, TResult>> ToServerSelector<TResult>(
        LambdaExpression expression,
        object?[] arguments,
        MethodInfo argumentMethod)
    {
        var body = expression.Body;
        for (var i = 0; i < expression.Parameters.Count; i++)
        {
            var parameter = expression.Parameters[i];
            var holder = Activator.CreateInstance(typeof(ValueHolder<>).MakeGenericType(parameter.Type), arguments[i])!;
            var value = Expression.Call(
                argumentMethod.MakeGenericMethod(parameter.Type),
                Expression.Field(Expression.Constant(holder), nameof(ValueHolder<>.Value)));

            body = ReplacingExpressionVisitor.Replace(parameter, value, body);
        }

        return Expression.Lambda<Func<NodaTimeTypes, TResult>>(body, Expression.Parameter(typeof(NodaTimeTypes), "t"));
    }

    /// <summary>
    ///     Guards against accidental client-side evaluation: each argument must have been sent as its own parameter, and the projection
    ///     must reference all of them (rather than e.g. a single pre-computed parameter). Note that EF can't evaluate a subtree containing
    ///     <see cref="EF.Parameter{T}" /> on the client, so it would fail rather than silently pre-compute it.
    /// </summary>
    private void AssertEvaluatedInDatabase(int argumentCount)
    {
        var sql = Assert.Single(Fixture.TestSqlLoggerFactory.SqlStatements);

        var parameterNames = Regex.Matches(sql, @"^(@\w+)=", RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToList();
        Assert.Equal(argumentCount, parameterNames.Count);

        var projection = Regex.Match(sql, @"^SELECT (.*?)\s+FROM ", RegexOptions.Multiline | RegexOptions.Singleline).Groups[1].Value;
        Assert.All(parameterNames, p => Assert.Matches(Regex.Escape(p) + @"\b", projection));
    }

    private static readonly MethodInfo EfParameterMethod = typeof(EF).GetMethod(nameof(EF.Parameter))!;
    private static readonly MethodInfo EfConstantMethod = typeof(EF).GetMethod(nameof(EF.Constant))!;

    private sealed class ValueHolder<T>(T value)
    {
        public readonly T Value = value;
    }

    #endregion Support
}

internal static class NodaTimeParityTestExtensions
{
    /// <summary>
    ///     Marks a case as a known discrepancy between PostgreSQL and NodaTime.
    /// </summary>
    public static TheoryDataRow<LambdaExpression, object?[], object?> Skip(
        this TheoryDataRow<LambdaExpression, object?[], object?> row,
        string reason)
    {
        row.Skip = reason;
        return row;
    }
}
