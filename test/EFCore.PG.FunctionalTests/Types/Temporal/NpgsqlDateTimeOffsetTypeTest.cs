namespace Microsoft.EntityFrameworkCore.Types.Temporal;

public class DateTimeOffsetTypeTest(DateTimeOffsetTypeTest.DateTimeOffsetTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<DateTimeOffset, DateTimeOffsetTypeTest.DateTimeOffsetTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_constant()
    {
        await base.Equality_in_query_with_constant();

        AssertSql(
            """
SELECT t."Id", t."OtherValue", t."Value"
FROM "TypeEntity" AS t
WHERE t."Value" = TIMESTAMPTZ '2020-01-05T12:30:45+00:00'
LIMIT 2
""");
    }


    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='2020-01-05T12:30:45.0000000+00:00' (DbType = DateTime)

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
@Fixture_Value='2020-01-05T12:30:45.0000000+00:00' (DbType = DateTime)

SELECT j."Id", j."OtherValue", j."Value", j."JsonContainer"
FROM "JsonTypeEntity" AS j
WHERE (CAST(j."JsonContainer" ->> 'Value' AS timestamp with time zone)) = @Fixture_Value
LIMIT 2
""");
    }

    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"2020-01-05T13:30:45\u002B00:00","Value":"2020-01-05T13:30:45\u002B00:00"}' (Nullable = false) (DbType = Object)
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
@Fixture_OtherValue='2020-01-05T13:30:45.0000000+00:00' (DbType = DateTime)

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
SET "JsonContainer" = jsonb_set(j."JsonContainer", '{Value}', to_jsonb(TIMESTAMPTZ '2020-01-05T13:30:45+00:00'::timestamptz))
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

    public class DateTimeOffsetTypeFixture : NpgsqlTypeFixture<DateTimeOffset>
    {
        // Note that we don't support DateTimeOffset with Offset != 0, since the offset doesn't get stored in the database.
        public override DateTimeOffset Value { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.Zero);
        public override DateTimeOffset OtherValue { get; } = new DateTimeOffset(2020, 1, 5, 13, 30, 45, TimeSpan.Zero);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
