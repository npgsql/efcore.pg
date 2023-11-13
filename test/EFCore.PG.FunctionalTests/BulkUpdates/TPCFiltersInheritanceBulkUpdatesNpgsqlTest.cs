using Microsoft.EntityFrameworkCore.BulkUpdates;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPCFiltersInheritanceBulkUpdatesNpgsqlTest
    : TPCFiltersInheritanceBulkUpdatesTestBase<TPCFiltersInheritanceBulkUpdatesNpgsqlFixture>
{
    public TPCFiltersInheritanceBulkUpdatesNpgsqlTest(
        TPCFiltersInheritanceBulkUpdatesNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    public override async Task Delete_where_hierarchy(bool async)
    {
        await base.Delete_where_hierarchy(async);

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_derived(bool async)
    {
        await base.Delete_where_hierarchy_derived(async);

        AssertSql(
            """
DELETE FROM "Kiwi" AS k
WHERE k."CountryId" = 1 AND k."Name" = 'Great spotted kiwi'
""");
    }

    public override async Task Delete_where_using_hierarchy(bool async)
    {
        await base.Delete_where_using_hierarchy(async);

        AssertSql(
            """
DELETE FROM "Countries" AS c
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
        FROM "Eagle" AS e
        UNION ALL
        SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
        FROM "Kiwi" AS k
    ) AS t
    WHERE t."CountryId" = 1 AND c."Id" = t."CountryId" AND t."CountryId" > 0) > 0
""");
    }

    public override async Task Delete_where_using_hierarchy_derived(bool async)
    {
        await base.Delete_where_using_hierarchy_derived(async);

        AssertSql(
            """
DELETE FROM "Countries" AS c
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
        FROM "Kiwi" AS k
    ) AS t
    WHERE t."CountryId" = 1 AND c."Id" = t."CountryId" AND t."CountryId" > 0) > 0
""");
    }

    public override async Task Delete_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        await base.Delete_where_keyless_entity_mapped_to_sql_query(async);

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_subquery(bool async)
    {
        await base.Delete_where_hierarchy_subquery(async);

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_First(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First(async);

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_First_2(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First_2(async);

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_First_3(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First_3(async);

        AssertSql();
    }

    public override async Task Update_base_type(bool async)
    {
        await base.Update_base_type(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_base_type_with_OfType(bool async)
    {
        await base.Update_base_type_with_OfType(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_where_hierarchy_subquery(bool async)
    {
        await base.Update_where_hierarchy_subquery(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_base_property_on_derived_type(bool async)
    {
        await base.Update_base_property_on_derived_type(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Kiwi" AS k
SET "Name" = 'SomeOtherKiwi'
WHERE k."CountryId" = 1
""");
    }

    public override async Task Update_derived_property_on_derived_type(bool async)
    {
        await base.Update_derived_property_on_derived_type(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Kiwi" AS k
SET "FoundOn" = 0
WHERE k."CountryId" = 1
""");
    }

    public override async Task Update_where_using_hierarchy(bool async)
    {
        await base.Update_where_using_hierarchy(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Countries" AS c
SET "Name" = 'Monovia'
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
        FROM "Eagle" AS e
        UNION ALL
        SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
        FROM "Kiwi" AS k
    ) AS t
    WHERE t."CountryId" = 1 AND c."Id" = t."CountryId" AND t."CountryId" > 0) > 0
""");
    }

    public override async Task Update_base_and_derived_types(bool async)
    {
        await base.Update_base_and_derived_types(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Kiwi" AS k
SET "FoundOn" = 0,
    "Name" = 'Kiwi'
WHERE k."CountryId" = 1
""");
    }

    public override async Task Update_where_using_hierarchy_derived(bool async)
    {
        await base.Update_where_using_hierarchy_derived(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Countries" AS c
SET "Name" = 'Monovia'
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
        FROM "Kiwi" AS k
    ) AS t
    WHERE t."CountryId" = 1 AND c."Id" = t."CountryId" AND t."CountryId" > 0) > 0
""");
    }

    public override async Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        await base.Update_where_keyless_entity_mapped_to_sql_query(async);

        AssertExecuteUpdateSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
