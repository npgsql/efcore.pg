namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class TPTFiltersInheritanceBulkUpdatesSqlServerTest(
    TPTFiltersInheritanceBulkUpdatesNpgsqlFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPTFiltersInheritanceBulkUpdatesTestBase<TPTFiltersInheritanceBulkUpdatesNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Delete_where_hierarchy(bool async)
    {
        await base.Delete_where_hierarchy(async);

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_derived(bool async)
    {
        await base.Delete_where_hierarchy_derived(async);

        AssertSql();
    }

    public override async Task Delete_where_using_hierarchy(bool async)
    {
        await base.Delete_where_using_hierarchy(async);

        AssertSql(
            """
DELETE FROM "Countries" AS c
WHERE (
    SELECT count(*)::int
    FROM "Animals" AS a
    WHERE a."CountryId" = 1 AND c."Id" = a."CountryId" AND a."CountryId" > 0) > 0
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
    FROM "Animals" AS a
    LEFT JOIN "Kiwi" AS k ON a."Id" = k."Id"
    WHERE a."CountryId" = 1 AND c."Id" = a."CountryId" AND k."Id" IS NOT NULL AND a."CountryId" > 0) > 0
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

    public override async Task Delete_GroupBy_Where_Select_First_3(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First_3(async);

        AssertSql();
    }

    public override async Task Update_base_type(bool async)
    {
        await base.Update_base_type(async);

        // TODO: This over-complex SQL would get pruned after https://github.com/dotnet/efcore/issues/31083
        AssertExecuteUpdateSql(
            """
@p='Animal'

UPDATE "Animals" AS a0
SET "Name" = @p
FROM (
    SELECT a."Id"
    FROM "Animals" AS a
    WHERE a."CountryId" = 1 AND a."Name" = 'Great spotted kiwi'
) AS s
WHERE a0."Id" = s."Id"
""");
    }

    public override async Task Update_base_type_with_OfType(bool async)
    {
        await base.Update_base_type_with_OfType(async);

        // TODO: This over-complex SQL would get pruned after https://github.com/dotnet/efcore/issues/31083
        AssertExecuteUpdateSql(
            """
@p='NewBird'

UPDATE "Animals" AS a0
SET "Name" = @p
FROM "Birds" AS b,
    "Kiwi" AS k0,
    (
        SELECT a."Id"
        FROM "Animals" AS a
        LEFT JOIN "Kiwi" AS k ON a."Id" = k."Id"
        WHERE a."CountryId" = 1 AND k."Id" IS NOT NULL
    ) AS s
WHERE a0."Id" = s."Id" AND a0."Id" = k0."Id" AND a0."Id" = b."Id"
""");
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
@p='SomeOtherKiwi'

UPDATE "Animals" AS a
SET "Name" = @p
FROM "Birds" AS b,
    "Kiwi" AS k
WHERE a."Id" = k."Id" AND a."Id" = b."Id" AND a."CountryId" = 1
""");
    }

    public override async Task Update_derived_property_on_derived_type(bool async)
    {
        await base.Update_derived_property_on_derived_type(async);

        AssertExecuteUpdateSql(
            """
@p='0' (DbType = Int16)

UPDATE "Kiwi" AS k
SET "FoundOn" = @p
FROM "Animals" AS a
INNER JOIN "Birds" AS b ON a."Id" = b."Id"
WHERE a."Id" = k."Id" AND a."CountryId" = 1
""");
    }

    public override async Task Update_base_and_derived_types(bool async)
    {
        await base.Update_base_and_derived_types(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_where_using_hierarchy(bool async)
    {
        await base.Update_where_using_hierarchy(async);

        AssertExecuteUpdateSql(
            """
@p='Monovia'

UPDATE "Countries" AS c
SET "Name" = @p
WHERE (
    SELECT count(*)::int
    FROM "Animals" AS a
    WHERE a."CountryId" = 1 AND c."Id" = a."CountryId" AND a."CountryId" > 0) > 0
""");
    }

    public override async Task Update_where_using_hierarchy_derived(bool async)
    {
        await base.Update_where_using_hierarchy_derived(async);

        AssertExecuteUpdateSql(
            """
@p='Monovia'

UPDATE "Countries" AS c
SET "Name" = @p
WHERE (
    SELECT count(*)::int
    FROM "Animals" AS a
    LEFT JOIN "Kiwi" AS k ON a."Id" = k."Id"
    WHERE a."CountryId" = 1 AND c."Id" = a."CountryId" AND k."Id" IS NOT NULL AND a."CountryId" > 0) > 0
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
