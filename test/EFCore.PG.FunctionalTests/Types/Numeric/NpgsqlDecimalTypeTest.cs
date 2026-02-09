namespace Microsoft.EntityFrameworkCore.Types.Numeric;

public class NpgsqlDecimalTypeTest(NpgsqlDecimalTypeTest.DecimalTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<decimal, NpgsqlDecimalTypeTest.DecimalTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_constant()
    {
        await base.Equality_in_query_with_constant();

        AssertSql(
            """
SELECT t."Id", t."OtherValue", t."Value"
FROM "TypeEntity" AS t
WHERE t."Value" = 30.5
LIMIT 2
""");
    }


    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='30.5'

SELECT t."Id", t."OtherValue", t."Value"
FROM "TypeEntity" AS t
WHERE t."Value" = @Fixture_Value
LIMIT 2
""");
    }

    public override async Task SaveChanges()
        => await base.SaveChanges();

    #region JSON

    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        AssertSql(
            """
@Fixture_Value='30.5'

SELECT j."Id", j."OtherValue", j."Value", j."JsonContainer"
FROM "JsonTypeEntity" AS j
WHERE (CAST(j."JsonContainer" ->> 'Value' AS numeric)) = @Fixture_Value
LIMIT 2
""");
    }

    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":30,"Value":30}' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonTypeEntity" SET "JsonContainer" = @p0
WHERE "Id" = @p1;
""");
    }

    public override async Task ExecuteUpdate_within_json_to_parameter()
    {
        await base.ExecuteUpdate_within_json_to_parameter();

        AssertSql(
            """
@Fixture_OtherValue='30'

UPDATE "JsonTypeEntity" AS j
SET "JsonContainer" = jsonb_set(j."JsonContainer", '{Value}', to_jsonb(@Fixture_OtherValue))
""");
    }

    public override async Task ExecuteUpdate_within_json_to_constant()
    {
        await base.ExecuteUpdate_within_json_to_constant();

        AssertSql(
            """
UPDATE "JsonTypeEntity" AS j
SET "JsonContainer" = jsonb_set(j."JsonContainer", '{Value}', to_jsonb(30.0::numeric))
""");
    }

    public override async Task ExecuteUpdate_within_json_to_another_json_property()
    {
        await base.ExecuteUpdate_within_json_to_another_json_property();

        AssertSql(
            """
UPDATE "JsonTypeEntity" AS j
SET "JsonContainer" = jsonb_set(j."JsonContainer", '{Value}', j."JsonContainer" -> 'OtherValue')
""");
    }

    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        await base.ExecuteUpdate_within_json_to_nonjson_column();

        AssertSql(
            """
UPDATE "JsonTypeEntity" AS j
SET "JsonContainer" = jsonb_set(j."JsonContainer", '{Value}', to_jsonb(j."OtherValue"))
""");
    }

    #endregion JSON

    public class DecimalTypeFixture : NpgsqlTypeFixture<decimal>
    {
        public override decimal Value { get; } = 30.5m;
        public override decimal OtherValue { get; } = 30m;
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
