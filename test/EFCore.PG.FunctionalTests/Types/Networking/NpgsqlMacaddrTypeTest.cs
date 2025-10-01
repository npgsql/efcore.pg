using System.Net.NetworkInformation;

namespace Microsoft.EntityFrameworkCore.Types.Networking;

public class NpgsqlMacaddrTypeTest(NpgsqlMacaddrTypeTest.MacaddrTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<PhysicalAddress, NpgsqlMacaddrTypeTest.MacaddrTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query()
    {
        await base.Equality_in_query();

        AssertSql(
            """
@Fixture_Value='001422012345' (DbType = Object)

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
@Fixture_Value='001422012345' (DbType = Object)

SELECT j."Id", j."OtherValue", j."Value", j."JsonContainer"
FROM "JsonTypeEntity" AS j
WHERE (CAST(j."JsonContainer" ->> 'Value' AS macaddr)) = @Fixture_Value
LIMIT 2
""");
    }

    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"001422012346","Value":"001422012346"}' (Nullable = false) (DbType = Object)
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
@Fixture_OtherValue='001422012346' (DbType = Object)

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
SET "JsonContainer" = jsonb_set(j."JsonContainer", '{Value}', to_jsonb(MACADDR '001422012346'::macaddr))
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

    public class MacaddrTypeFixture : NpgsqlTypeFixture<PhysicalAddress>
    {
        public override PhysicalAddress Value { get; } = PhysicalAddress.Parse("00-14-22-01-23-45");
        public override PhysicalAddress OtherValue { get; } = PhysicalAddress.Parse("00-14-22-01-23-46");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
