using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeSpanTranslationsNpgsqlTest : TimeSpanTranslationsTestBase<BasicTypesQueryNpgsqlFixture>
{
    public TimeSpanTranslationsNpgsqlTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Hours()
    {
        await base.Hours();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(date_part('hour', b."TimeSpan"))::int = 3
""");
    }

    public override async Task Minutes()
    {
        await base.Minutes();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(date_part('minute', b."TimeSpan"))::int = 4
""");
    }

    public override async Task Seconds()
    {
        await base.Seconds();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(date_part('second', b."TimeSpan"))::int = 5
""");
    }

    public override async Task Milliseconds()
    {
        await base.Milliseconds();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(date_part('millisecond', b."TimeSpan"))::int % 1000 = 678
""");
    }

    public override Task Microseconds()
        => AssertTranslationFailed(() => base.Microseconds());

    public override Task Nanoseconds()
        => AssertTranslationFailed(() => base.Nanoseconds());

    [ConditionalFact]
    public virtual async Task GroupBy_Select_Sum_over_TimeSpan_with_null_check_in_filter()
    {
        await AssertQuery(
            ss => ss.Set<NullableBasicTypesEntity>()
                .GroupBy(b => b.Bool)
                .Select(g => new { g.Key, Sum = EF.Functions.Sum(g.Where(b => b.TimeSpan != null).Select(b => b.TimeSpan!.Value)) }),
            ss => ss.Set<NullableBasicTypesEntity>()
                .GroupBy(b => b.Bool)
                .Select(g => new
                {
                    g.Key,
                    Sum = g.Any(b => b.TimeSpan != null)
                        ? new TimeSpan(g.Where(b => b.TimeSpan != null).Sum(b => b.TimeSpan!.Value.Ticks))
                        : (TimeSpan?)null
                }),
            elementSorter: e => e.Key);

        AssertSql(
            """
SELECT n."Bool" AS "Key", sum(n."TimeSpan") FILTER (WHERE n."TimeSpan" IS NOT NULL) AS "Sum"
FROM "NullableBasicTypesEntities" AS n
GROUP BY n."Bool"
""");
    }

    [ConditionalFact]
    public virtual async Task GroupBy_Select_Sum_over_TimeSpan_compared_to_null()
    {
        await AssertQuery(
            ss => ss.Set<NullableBasicTypesEntity>()
                .GroupBy(b => b.Bool)
                .Select(g => new { g.Key, Sum = EF.Functions.Sum(g.Where(b => b.TimeSpan != null).Select(b => b.TimeSpan!.Value)) })
                .Where(x => x.Sum == null)
                .Select(x => x.Key),
            ss => ss.Set<NullableBasicTypesEntity>()
                .GroupBy(b => b.Bool)
                .Where(g => g.All(b => b.TimeSpan == null))
                .Select(g => g.Key));

        AssertSql(
            """
SELECT n."Bool"
FROM "NullableBasicTypesEntities" AS n
GROUP BY n."Bool"
HAVING sum(n."TimeSpan") FILTER (WHERE n."TimeSpan" IS NOT NULL) IS NULL
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
