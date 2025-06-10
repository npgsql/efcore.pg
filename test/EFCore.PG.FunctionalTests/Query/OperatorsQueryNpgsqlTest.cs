using Microsoft.EntityFrameworkCore.TestModels.Operators;

namespace Microsoft.EntityFrameworkCore.Query;

public class OperatorsQueryNpgsqlTest(NonSharedFixture fixture) : OperatorsQueryTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    public override async Task Bitwise_and_on_expression_with_like_and_null_check_being_compared_to_false()
    {
        await base.Bitwise_and_on_expression_with_like_and_null_check_being_compared_to_false();

        AssertSql(
            """
SELECT o."Value" AS "Value1", o0."Value" AS "Value2", o1."Value" AS "Value3"
FROM "OperatorEntityString" AS o
CROSS JOIN "OperatorEntityString" AS o0
CROSS JOIN "OperatorEntityBool" AS o1
WHERE ((o0."Value" LIKE 'B' AND o0."Value" IS NOT NULL) OR o1."Value") AND o."Value" IS NOT NULL
ORDER BY o."Id" NULLS FIRST, o0."Id" NULLS FIRST, o1."Id" NULLS FIRST
""");
    }

    public override async Task Complex_predicate_with_bitwise_and_modulo_and_negation()
    {
        await base.Complex_predicate_with_bitwise_and_modulo_and_negation();

        AssertSql(
            """
SELECT o."Value" AS "Value0", o0."Value" AS "Value1", o1."Value" AS "Value2", o2."Value" AS "Value3"
FROM "OperatorEntityLong" AS o
CROSS JOIN "OperatorEntityLong" AS o0
CROSS JOIN "OperatorEntityLong" AS o1
CROSS JOIN "OperatorEntityLong" AS o2
WHERE (o0."Value" % 2) / o."Value" & ((o2."Value" | o1."Value") - o."Value") - o1."Value" * o1."Value" >= ((o0."Value" / (~o2."Value")) % 2) % ((~o."Value") + 1)
ORDER BY o."Id" NULLS FIRST, o0."Id" NULLS FIRST, o1."Id" NULLS FIRST, o2."Id" NULLS FIRST
""");
    }

    public override async Task Complex_predicate_with_bitwise_and_arithmetic_operations()
    {
        await base.Complex_predicate_with_bitwise_and_arithmetic_operations();

        AssertSql(
            """
SELECT o."Value" AS "Value0", o0."Value" AS "Value1", o1."Value" AS "Value2"
FROM "OperatorEntityInt" AS o
CROSS JOIN "OperatorEntityInt" AS o0
CROSS JOIN "OperatorEntityBool" AS o1
WHERE (o0."Value" & o."Value" + o."Value" & o."Value") / 1 > o0."Value" & 10 AND o1."Value"
ORDER BY o."Id" NULLS FIRST, o0."Id" NULLS FIRST, o1."Id" NULLS FIRST
""");
    }

    public override async Task Or_on_two_nested_binaries_and_another_simple_comparison()
    {
        await base.Or_on_two_nested_binaries_and_another_simple_comparison();

        AssertSql(
            """
SELECT o."Id" AS "Id1", o0."Id" AS "Id2", o1."Id" AS "Id3", o2."Id" AS "Id4", o3."Id" AS "Id5"
FROM "OperatorEntityString" AS o
CROSS JOIN "OperatorEntityString" AS o0
CROSS JOIN "OperatorEntityString" AS o1
CROSS JOIN "OperatorEntityString" AS o2
CROSS JOIN "OperatorEntityInt" AS o3
WHERE ((o."Value" = 'A' AND o0."Value" = 'A') OR (o1."Value" = 'B' AND o2."Value" = 'B')) AND o3."Value" = 2
ORDER BY o."Id" NULLS FIRST, o0."Id" NULLS FIRST, o1."Id" NULLS FIRST, o2."Id" NULLS FIRST, o3."Id" NULLS FIRST
""");
    }

    public override async Task Projection_with_not_and_negation_on_integer()
    {
        await base.Projection_with_not_and_negation_on_integer();

        AssertSql(
            """
SELECT (~(-(-(o1."Value" + o."Value" + 2)))) % (-(o0."Value" + o0."Value") - o."Value")
FROM "OperatorEntityLong" AS o
CROSS JOIN "OperatorEntityLong" AS o0
CROSS JOIN "OperatorEntityLong" AS o1
ORDER BY o."Id" NULLS FIRST, o0."Id" NULLS FIRST, o1."Id" NULLS FIRST
""");
    }

    public override async Task Negate_on_column(bool async)
    {
        await base.Negate_on_column(async);

        AssertSql(
            """
SELECT o."Id"
FROM "OperatorEntityInt" AS o
WHERE o."Id" = -o."Value"
""");
    }

    public override async Task Double_negate_on_column()
    {
        await base.Double_negate_on_column();

        AssertSql(
            """
SELECT o."Id"
FROM "OperatorEntityInt" AS o
WHERE -(-o."Value") = o."Value"
""");
    }

    public override async Task Negate_on_binary_expression(bool async)
    {
        await base.Negate_on_binary_expression(async);

        AssertSql(
            """
SELECT o."Id" AS "Id1", o0."Id" AS "Id2"
FROM "OperatorEntityInt" AS o
CROSS JOIN "OperatorEntityInt" AS o0
WHERE -o."Value" = -(o."Id" + o0."Value")
""");
    }

    public override async Task Negate_on_like_expression(bool async)
    {
        await base.Negate_on_like_expression(async);

        AssertSql(
            """
SELECT o."Id"
FROM "OperatorEntityString" AS o
WHERE o."Value" NOT LIKE 'A%' OR o."Value" IS NULL
""");
    }

    public override async Task Concat_and_json_scalar(bool async)
    {
        await base.Concat_and_json_scalar(async);

        AssertSql(
            """
SELECT o."Id", o."Owned"
FROM "Owner" AS o
WHERE 'Foo' || (o."Owned" ->> 'SomeProperty') = 'FooBar'
LIMIT 2
""");
    }

    [ConditionalFact]
    public virtual async Task AtTimeZone_and_addition()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(
            seed: async context =>
            {
                context.Set<OperatorEntityDateTime>().AddRange(
                    new OperatorEntityDateTime { Id = 1, Value = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    new OperatorEntityDateTime { Id = 2, Value = new DateTime(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc) });
                await context.SaveChangesAsync();
            },
            onModelCreating: modelBuilder => modelBuilder.Entity<OperatorEntityDateTime>().Property(x => x.Id).ValueGeneratedNever());

        await using var context = contextFactory.CreateContext();

        var result = await context.Set<OperatorEntityDateTime>()
            .Where(b => new DateOnly(2020, 1, 15) > DateOnly.FromDateTime(b.Value.AddDays(1)))
            .SingleAsync();

        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT o."Id", o."Value"
FROM "OperatorEntityDateTime" AS o
WHERE DATE '2020-01-15' > CAST((o."Value" + INTERVAL '1 days') AT TIME ZONE 'UTC' AS date)
LIMIT 2
""");
    }

    public class OperatorEntityDateTime : OperatorEntityBase
    {
        public DateTime Value { get; set; }
    }

    protected override async Task Seed(OperatorsContext ctx)
    {
        ctx.Set<OperatorEntityString>().AddRange(ExpectedData.OperatorEntitiesString);
        ctx.Set<OperatorEntityInt>().AddRange(ExpectedData.OperatorEntitiesInt);
        ctx.Set<OperatorEntityNullableInt>().AddRange(ExpectedData.OperatorEntitiesNullableInt);
        ctx.Set<OperatorEntityLong>().AddRange(ExpectedData.OperatorEntitiesLong);
        ctx.Set<OperatorEntityBool>().AddRange(ExpectedData.OperatorEntitiesBool);
        ctx.Set<OperatorEntityNullableBool>().AddRange(ExpectedData.OperatorEntitiesNullableBool);
        // ctx.Set<OperatorEntityDateTimeOffset>().AddRange(ExpectedData.OperatorEntitiesDateTimeOffset);

        await ctx.SaveChangesAsync();
    }
}
