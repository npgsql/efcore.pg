namespace Microsoft.EntityFrameworkCore.Types.Miscellaneous;

public class NpgsqlGuidTypeTest(NpgsqlGuidTypeTest.GuidTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<Guid, NpgsqlGuidTypeTest.GuidTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query()
    {
        await base.Equality_in_query();

        AssertSql(
            """
@Fixture_Value='8f7331d6-cde9-44fb-8611-81fff686f280'

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
@Fixture_Value='8f7331d6-cde9-44fb-8611-81fff686f280'

SELECT j."Id", j."OtherValue", j."Value", j."JsonContainer"
FROM "JsonTypeEntity" AS j
WHERE (CAST(j."JsonContainer" ->> 'Value' AS uuid)) = @Fixture_Value
LIMIT 2
""");
    }

    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"ae192c36-9004-49b2-b785-8be10d169627","Value":"ae192c36-9004-49b2-b785-8be10d169627"}' (Nullable = false) (DbType = Object)
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
@Fixture_OtherValue='ae192c36-9004-49b2-b785-8be10d169627'

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
SET "JsonContainer" = jsonb_set(j."JsonContainer", '{Value}', to_jsonb('ae192c36-9004-49b2-b785-8be10d169627'::uuid))
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

    public class GuidTypeFixture : NpgsqlTypeFixture<Guid>
    {
        public override Guid Value { get; } = new("8f7331d6-cde9-44fb-8611-81fff686f280");
        public override Guid OtherValue { get; } = new("ae192c36-9004-49b2-b785-8be10d169627");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
