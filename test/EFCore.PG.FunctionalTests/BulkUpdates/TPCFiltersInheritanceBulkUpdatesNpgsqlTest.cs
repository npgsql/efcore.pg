using Microsoft.EntityFrameworkCore.BulkUpdates;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPCFiltersInheritanceBulkUpdatesNpgsqlTest
    : TPCFiltersInheritanceBulkUpdatesTestBase<TPCFiltersInheritanceBulkUpdatesNpgsqlFixture>
{
    public TPCFiltersInheritanceBulkUpdatesNpgsqlTest(TPCFiltersInheritanceBulkUpdatesNpgsqlFixture fixture)
        : base(fixture)
    {
        ClearLog();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_where_hierarchy(bool async)
    {
        await base.Delete_where_hierarchy(async);

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_derived(bool async)
    {
        await base.Delete_where_hierarchy_derived(async);

        AssertSql(
            @"DELETE FROM ""Kiwi"" AS k
WHERE k.""CountryId"" = 1 AND k.""Name"" = 'Great spotted kiwi'");
    }

    public override async Task Delete_where_using_hierarchy(bool async)
    {
        await base.Delete_where_using_hierarchy(async);

        AssertSql(
            @"DELETE FROM ""Countries"" AS c
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT e.""Id"", e.""CountryId"", e.""Name"", e.""Species"", e.""EagleId"", e.""IsFlightless"", e.""Group"", NULL AS ""FoundOn"", 'Eagle' AS ""Discriminator""
        FROM ""Eagle"" AS e
        UNION ALL
        SELECT k.""Id"", k.""CountryId"", k.""Name"", k.""Species"", k.""EagleId"", k.""IsFlightless"", NULL AS ""Group"", k.""FoundOn"", 'Kiwi' AS ""Discriminator""
        FROM ""Kiwi"" AS k
    ) AS t
    WHERE t.""CountryId"" = 1 AND c.""Id"" = t.""CountryId"" AND t.""CountryId"" > 0) > 0");
    }

    public override async Task Delete_where_using_hierarchy_derived(bool async)
    {
        await base.Delete_where_using_hierarchy_derived(async);

        AssertSql(
            @"DELETE FROM ""Countries"" AS c
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT k.""Id"", k.""CountryId"", k.""Name"", k.""Species"", k.""EagleId"", k.""IsFlightless"", NULL AS ""Group"", k.""FoundOn"", 'Kiwi' AS ""Discriminator""
        FROM ""Kiwi"" AS k
    ) AS t
    WHERE t.""CountryId"" = 1 AND c.""Id"" = t.""CountryId"" AND t.""CountryId"" > 0) > 0");
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

    protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
