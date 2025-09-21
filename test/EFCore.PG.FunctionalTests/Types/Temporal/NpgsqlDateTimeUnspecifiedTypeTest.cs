namespace Microsoft.EntityFrameworkCore.Types.Temporal;

public class DateTimeUnspecifiedTypeTest(DateTimeUnspecifiedTypeTest.DateTimeTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<DateTime, DateTimeUnspecifiedTypeTest.DateTimeTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query()
    {
        await base.Equality_in_query();

        AssertSql(
            """
@Fixture_Value='2020-01-05T12:30:45.0000000'

SELECT t."Id", t."OtherValue", t."Value"
FROM "TypeEntity" AS t
WHERE t."Value" = @Fixture_Value
LIMIT 2
""");
    }

    #region JSON

    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        AssertSql(
            """
@Fixture_Value='2020-01-05T12:30:45.0000000'

SELECT j."Id", j."OtherValue", j."Value", j."JsonContainer"
FROM "JsonTypeEntity" AS j
WHERE (CAST(j."JsonContainer" ->> 'Value' AS timestamp without time zone)) = @Fixture_Value
LIMIT 2
""");
    }

    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"2022-05-03T00:00:00","Value":"2022-05-03T00:00:00"}' (Nullable = false) (DbType = Object)
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
@Fixture_OtherValue='2022-05-03T00:00:00.0000000'

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
SET "JsonContainer" = jsonb_set(j."JsonContainer", '{Value}', to_jsonb(TIMESTAMP '2022-05-03T00:00:00'::timestamp))
""");
    }

    public override async Task ExecuteUpdate_within_json_to_another_json_property()
    {
        await base.ExecuteUpdate_within_json_to_another_json_property();

        AssertSql(
            """
UPDATE "JsonTypeEntity" AS j
SET "JsonContainer" = jsonb_set(j."JsonContainer", '{Value}', to_jsonb(CAST(j."JsonContainer" ->> 'OtherValue' AS timestamp without time zone)))
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

    public class DateTimeTypeFixture : NpgsqlTypeFixture<DateTime>
    {
        public override string? StoreType => "timestamp without time zone";

        public override DateTime Value { get; } = new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Unspecified);
        public override DateTime OtherValue { get; } = new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Unspecified);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
