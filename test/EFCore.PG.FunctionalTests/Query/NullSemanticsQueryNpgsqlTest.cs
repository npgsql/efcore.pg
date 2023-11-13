using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

// ReSharper disable once UnusedMember.Global
public class NullSemanticsQueryNpgsqlTest : NullSemanticsQueryTestBase<NullSemanticsQueryNpgsqlFixture>
{
    public NullSemanticsQueryNpgsqlTest(NullSemanticsQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Compare_row_values_equal_without_expansion(bool async)
    {
        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(e => ValueTuple.Create(e.IntA, e.StringA).Equals(ValueTuple.Create(e.IntB, e.StringB)))
                .Select(e => e.Id));

        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(e => ValueTuple.Create(e.IntA, e.StringA).Equals(ValueTuple.Create(e.IntB, e.NullableStringB)))
                .Select(e => e.Id));

        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(e => ValueTuple.Create(e.IntA, e.NullableStringA).Equals(ValueTuple.Create(e.IntB, e.StringB)))
                .Select(e => e.Id));

        AssertSql(
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE (e."IntA", e."StringA") = (e."IntB", e."StringB")
""",
            //
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE (e."IntA", e."StringA") = (e."IntB", e."NullableStringB")
""",
            //
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE (e."IntA", e."NullableStringA") = (e."IntB", e."StringB")
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Compare_row_values_equal_with_expansion(bool async)
    {
        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(
                    e => ValueTuple.Create(e.NullableStringA, e.IntA, e.BoolA)
                        .Equals(ValueTuple.Create(e.NullableStringB, e.IntB, e.BoolB)))
                .Select(e => e.Id));

        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(
                    e => ValueTuple.Create(e.IntA, e.NullableStringA, e.BoolA)
                        .Equals(ValueTuple.Create(e.IntB, e.NullableStringB, e.BoolB)))
                .Select(e => e.Id));

        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(
                    e => ValueTuple.Create(e.IntA, e.StringA, e.NullableBoolA)
                        .Equals(ValueTuple.Create(e.IntB, e.StringB, e.NullableBoolB)))
                .Select(e => e.Id));

        AssertSql(
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE (e."IntA", e."BoolA") = (e."IntB", e."BoolB") AND (e."NullableStringA" = e."NullableStringB" OR (e."NullableStringA" IS NULL AND e."NullableStringB" IS NULL))
""",
            //
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE (e."IntA", e."BoolA") = (e."IntB", e."BoolB") AND (e."NullableStringA" = e."NullableStringB" OR (e."NullableStringA" IS NULL AND e."NullableStringB" IS NULL))
""",
            //
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE (e."IntA", e."StringA") = (e."IntB", e."StringB") AND (e."NullableBoolA" = e."NullableBoolB" OR (e."NullableBoolA" IS NULL AND e."NullableBoolB" IS NULL))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Compare_row_values_not_equal(bool async)
    {
        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(e => !ValueTuple.Create(e.IntA, e.StringA).Equals(ValueTuple.Create(e.IntB, e.StringB)))
                .Select(e => e.Id));

        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(e => !ValueTuple.Create(e.IntA, e.StringA).Equals(ValueTuple.Create(e.IntB, e.NullableStringB)))
                .Select(e => e.Id));

        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(e => !ValueTuple.Create(e.IntA, e.NullableStringA).Equals(ValueTuple.Create(e.IntB, e.StringB)))
                .Select(e => e.Id));

        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(e => !ValueTuple.Create(e.IntA, e.NullableStringA).Equals(ValueTuple.Create(e.IntB, e.NullableStringB)))
                .Select(e => e.Id));

        await AssertQueryScalar(
            async, ss => ss.Set<NullSemanticsEntity1>()
                .Where(
                    e => !ValueTuple.Create(e.IntA, e.StringA, e.NullableBoolA)
                        .Equals(ValueTuple.Create(e.IntB, e.StringB, e.NullableBoolB)))
                .Select(e => e.Id));

        AssertSql(
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE (e."IntA", e."StringA") <> (e."IntB", e."StringB")
""",
            //
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE e."IntA" <> e."IntB" OR e."StringA" <> e."NullableStringB" OR e."NullableStringB" IS NULL
""",
            //
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE e."IntA" <> e."IntB" OR e."NullableStringA" <> e."StringB" OR e."NullableStringA" IS NULL
""",
            //
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE e."IntA" <> e."IntB" OR ((e."NullableStringA" <> e."NullableStringB" OR e."NullableStringA" IS NULL OR e."NullableStringB" IS NULL) AND (e."NullableStringA" IS NOT NULL OR e."NullableStringB" IS NOT NULL))
""",
            //
            """
SELECT e."Id"
FROM "Entities1" AS e
WHERE (e."IntA", e."StringA") <> (e."IntB", e."StringB") OR ((e."NullableBoolA" <> e."NullableBoolB" OR e."NullableBoolA" IS NULL OR e."NullableBoolB" IS NULL) AND (e."NullableBoolA" IS NOT NULL OR e."NullableBoolB" IS NOT NULL))
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected override NullSemanticsContext CreateContext(bool useRelationalNulls = false)
    {
        var options = new DbContextOptionsBuilder(Fixture.CreateOptions());
        if (useRelationalNulls)
        {
            new NpgsqlDbContextOptionsBuilder(options).UseRelationalNulls();
        }

        var context = new NullSemanticsContext(options.Options);

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return context;
    }
}
