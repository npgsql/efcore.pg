namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPCInheritanceQueryNpgsqlTest(TPCInheritanceQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : TPCInheritanceQueryTestBase<TPCInheritanceQueryNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Byte_enum_value_constant_used_in_projection(bool async)
    {
        await base.Byte_enum_value_constant_used_in_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN k."IsFlightless" THEN 0
    ELSE 1
END
FROM "Kiwi" AS k
""");
    }

    public override async Task Can_filter_all_animals(bool async)
    {
        await base.Can_filter_all_animals(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE u."Name" = 'Great spotted kiwi'
ORDER BY u."Species" NULLS FIRST
""");
    }

    public override async Task Can_include_animals(bool async)
    {
        await base.Can_include_animals(async);

        AssertSql(
            """
SELECT c."Id", c."Name", u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM "Countries" AS c
LEFT JOIN (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u ON c."Id" = u."CountryId"
ORDER BY c."Name" NULLS FIRST, c."Id" NULLS FIRST
""");
    }

    public override async Task Can_include_prey(bool async)
    {
        await base.Can_include_prey(async);

        AssertSql(
            """
SELECT e1."Id", e1."CountryId", e1."Name", e1."Species", e1."EagleId", e1."IsFlightless", e1."Group", u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group"
    FROM "Eagle" AS e
    LIMIT 2
) AS e1
LEFT JOIN (
    SELECT e0."Id", e0."CountryId", e0."Name", e0."Species", e0."EagleId", e0."IsFlightless", e0."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e0
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u ON e1."Id" = u."EagleId"
ORDER BY e1."Id" NULLS FIRST
""");
    }

    // Seed data for the fixture manually inserts entities with IDs 1, 2; then this test attempts to insert another one with an auto-generated ID,
    // but the PG sequence wasn't updated so produces 1, resulting in a conflict. The test should be consistent in either using either
    // auto-generated IDs or not across the board.
    public override Task Can_insert_update_delete()
        => Assert.ThrowsAsync<DbUpdateException>(base.Can_insert_update_delete);

    public override async Task Can_query_all_animals(bool async)
    {
        await base.Can_query_all_animals(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
ORDER BY u."Species" NULLS FIRST
""");
    }

    public override async Task Can_query_all_birds(bool async)
    {
        await base.Can_query_all_birds(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
ORDER BY u."Species" NULLS FIRST
""");
    }

    public override async Task Can_query_all_plants(bool async)
    {
        await base.Can_query_all_plants(async);

        AssertSql(
            """
SELECT u."Species", u."CountryId", u."Genus", u."Name", u."HasThorns", u."Discriminator"
FROM (
    SELECT d."Species", d."CountryId", d."Genus", d."Name", NULL AS "HasThorns", 'Daisy' AS "Discriminator"
    FROM "Daisies" AS d
    UNION ALL
    SELECT r."Species", r."CountryId", r."Genus", r."Name", r."HasThorns", 'Rose' AS "Discriminator"
    FROM "Roses" AS r
) AS u
ORDER BY u."Species" NULLS FIRST
""");
    }

    public override async Task Can_query_all_types_when_shared_column(bool async)
    {
        await base.Can_query_all_types_when_shared_column(async);

        AssertSql(
            """
SELECT u."Id", u."SortIndex", u."CaffeineGrams", u."CokeCO2", u."Ints", u."SugarGrams", u."LiltCO2", u."SugarGrams1", u."CaffeineGrams1", u."HasMilk", u."ComplexTypeCollection", u."ParentComplexType_Int", u."ParentComplexType_UniqueInt", u."ParentComplexType_Nested_NestedInt", u."ParentComplexType_Nested_UniqueInt", u."ChildComplexType_Int", u."ChildComplexType_UniqueInt", u."ChildComplexType_Nested_NestedInt", u."ChildComplexType_Nested_UniqueInt", u."ChildComplexType_Int1", u."ChildComplexType_UniqueInt1", u."ChildComplexType_Nested_NestedInt1", u."ChildComplexType_Nested_UniqueInt1", u."Discriminator"
FROM (
    SELECT d."Id", d."SortIndex", d."ComplexTypeCollection", d."ParentComplexType_Int", d."ParentComplexType_UniqueInt", d."ParentComplexType_Nested_NestedInt", d."ParentComplexType_Nested_UniqueInt", NULL AS "CaffeineGrams", NULL AS "CokeCO2", NULL AS "Ints", NULL AS "SugarGrams", NULL AS "ChildComplexType_Int", NULL AS "ChildComplexType_UniqueInt", NULL AS "ChildComplexType_Nested_NestedInt", NULL AS "ChildComplexType_Nested_UniqueInt", NULL::int AS "LiltCO2", NULL::int AS "SugarGrams1", NULL::int AS "CaffeineGrams1", NULL::boolean AS "HasMilk", NULL::int AS "ChildComplexType_Int1", NULL::int AS "ChildComplexType_UniqueInt1", NULL::int AS "ChildComplexType_Nested_NestedInt1", NULL::int AS "ChildComplexType_Nested_UniqueInt1", 'Drink' AS "Discriminator"
    FROM "Drinks" AS d
    UNION ALL
    SELECT c."Id", c."SortIndex", c."ComplexTypeCollection", c."Int" AS "ParentComplexType_Int", c."UniqueInt" AS "ParentComplexType_UniqueInt", c."NestedInt" AS "ParentComplexType_Nested_NestedInt", c."NestedComplexType_UniqueInt" AS "ParentComplexType_Nested_UniqueInt", c."CaffeineGrams", c."CokeCO2", c."Ints", c."SugarGrams", c."ChildComplexType_Int", c."ChildComplexType_UniqueInt", c."ChildComplexType_Nested_NestedInt", c."ChildComplexType_Nested_UniqueInt", NULL AS "LiltCO2", NULL AS "SugarGrams1", NULL AS "CaffeineGrams1", NULL AS "HasMilk", NULL AS "ChildComplexType_Int1", NULL AS "ChildComplexType_UniqueInt1", NULL AS "ChildComplexType_Nested_NestedInt1", NULL AS "ChildComplexType_Nested_UniqueInt1", 'Coke' AS "Discriminator"
    FROM "Coke" AS c
    UNION ALL
    SELECT l."Id", l."SortIndex", l."ComplexTypeCollection", l."Int" AS "ParentComplexType_Int", l."UniqueInt" AS "ParentComplexType_UniqueInt", l."NestedInt" AS "ParentComplexType_Nested_NestedInt", l."NestedComplexType_UniqueInt" AS "ParentComplexType_Nested_UniqueInt", NULL AS "CaffeineGrams", NULL AS "CokeCO2", NULL AS "Ints", NULL AS "SugarGrams", NULL AS "ChildComplexType_Int", NULL AS "ChildComplexType_UniqueInt", NULL AS "ChildComplexType_Nested_NestedInt", NULL AS "ChildComplexType_Nested_UniqueInt", l."LiltCO2", l."SugarGrams" AS "SugarGrams1", NULL AS "CaffeineGrams1", NULL AS "HasMilk", NULL AS "ChildComplexType_Int1", NULL AS "ChildComplexType_UniqueInt1", NULL AS "ChildComplexType_Nested_NestedInt1", NULL AS "ChildComplexType_Nested_UniqueInt1", 'Lilt' AS "Discriminator"
    FROM "Lilt" AS l
    UNION ALL
    SELECT t."Id", t."SortIndex", t."ComplexTypeCollection", t."Int" AS "ParentComplexType_Int", t."UniqueInt" AS "ParentComplexType_UniqueInt", t."NestedInt" AS "ParentComplexType_Nested_NestedInt", t."NestedComplexType_UniqueInt" AS "ParentComplexType_Nested_UniqueInt", NULL AS "CaffeineGrams", NULL AS "CokeCO2", NULL AS "Ints", NULL AS "SugarGrams", NULL AS "ChildComplexType_Int", NULL AS "ChildComplexType_UniqueInt", NULL AS "ChildComplexType_Nested_NestedInt", NULL AS "ChildComplexType_Nested_UniqueInt", NULL AS "LiltCO2", NULL AS "SugarGrams1", t."CaffeineGrams" AS "CaffeineGrams1", t."HasMilk", t."ChildComplexType_Int" AS "ChildComplexType_Int1", t."ChildComplexType_UniqueInt" AS "ChildComplexType_UniqueInt1", t."ChildComplexType_Nested_NestedInt" AS "ChildComplexType_Nested_NestedInt1", t."ChildComplexType_Nested_UniqueInt" AS "ChildComplexType_Nested_UniqueInt1", 'Tea' AS "Discriminator"
    FROM "Tea" AS t
) AS u
""");
    }

    public override async Task Can_query_just_kiwis(bool async)
    {
        await base.Can_query_just_kiwis(async);

        AssertSql(
            """
SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", k."FoundOn"
FROM "Kiwi" AS k
LIMIT 2
""");
    }

    public override async Task Can_query_just_roses(bool async)
    {
        await base.Can_query_just_roses(async);

        AssertSql(
            """
SELECT r."Species", r."CountryId", r."Genus", r."Name", r."HasThorns"
FROM "Roses" AS r
LIMIT 2
""");
    }

    public override async Task Can_query_when_shared_column(bool async)
    {
        await base.Can_query_when_shared_column(async);

        AssertSql(
            """
SELECT c."Id", c."SortIndex", c."CaffeineGrams", c."CokeCO2", c."Ints", c."SugarGrams", c."ComplexTypeCollection", c."Int", c."UniqueInt", c."NestedInt", c."NestedComplexType_UniqueInt", c."ChildComplexType_Int", c."ChildComplexType_UniqueInt", c."ChildComplexType_Nested_NestedInt", c."ChildComplexType_Nested_UniqueInt"
FROM "Coke" AS c
LIMIT 2
""",
            //
            """
SELECT l."Id", l."SortIndex", l."LiltCO2", l."SugarGrams", l."ComplexTypeCollection", l."Int", l."UniqueInt", l."NestedInt", l."NestedComplexType_UniqueInt"
FROM "Lilt" AS l
LIMIT 2
""",
            //
            """
SELECT t."Id", t."SortIndex", t."CaffeineGrams", t."HasMilk", t."ComplexTypeCollection", t."Int", t."UniqueInt", t."NestedInt", t."NestedComplexType_UniqueInt", t."ChildComplexType_Int", t."ChildComplexType_UniqueInt", t."ChildComplexType_Nested_NestedInt", t."ChildComplexType_Nested_UniqueInt"
FROM "Tea" AS t
LIMIT 2
""");
    }

    public override async Task Can_use_backwards_is_animal(bool async)
    {
        await base.Can_use_backwards_is_animal(async);

        AssertSql(
            """
SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", k."FoundOn"
FROM "Kiwi" AS k
""");
    }

    public override async Task Can_use_backwards_of_type_animal(bool async)
    {
        await base.Can_use_backwards_of_type_animal(async);

        AssertSql(
            """
SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", k."FoundOn"
FROM "Kiwi" AS k
""");
    }

    public override async Task Can_use_is_kiwi(bool async)
    {
        await base.Can_use_is_kiwi(async);

        AssertSql(
            """
SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
FROM "Kiwi" AS k
""");
    }

    public override async Task Can_use_is_kiwi_with_cast(bool async)
    {
        await base.Can_use_is_kiwi_with_cast(async);

        AssertSql(
            """
SELECT CASE
    WHEN u."Discriminator" = 'Kiwi' THEN u."FoundOn"
    ELSE 0
END AS "Value"
FROM (
    SELECT NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
""");
    }

    public override async Task Can_use_is_kiwi_in_projection(bool async)
    {
        await base.Can_use_is_kiwi_in_projection(async);

        AssertSql(
            """
SELECT u."Discriminator" = 'Kiwi'
FROM (
    SELECT 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
""");
    }

    public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
    {
        await base.Can_use_is_kiwi_with_other_predicate(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE u."Discriminator" = 'Kiwi' AND u."CountryId" = 1
""");
    }

    public override async Task Can_use_of_type_animal(bool async)
    {
        await base.Can_use_of_type_animal(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
ORDER BY u."Species" NULLS FIRST
""");
    }

    public override async Task Can_use_of_type_bird(bool async)
    {
        await base.Can_use_of_type_bird(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
ORDER BY u."Species" NULLS FIRST
""");
    }

    public override async Task Can_use_of_type_bird_first(bool async)
    {
        await base.Can_use_of_type_bird_first(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
ORDER BY u."Species" NULLS FIRST
LIMIT 1
""");
    }

    public override async Task Can_use_of_type_bird_predicate(bool async)
    {
        await base.Can_use_of_type_bird_predicate(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE u."CountryId" = 1
ORDER BY u."Species" NULLS FIRST
""");
    }

    public override async Task Can_use_of_type_bird_with_projection(bool async)
    {
        await base.Can_use_of_type_bird_with_projection(async);

        AssertSql(
            """
SELECT e."EagleId"
FROM "Eagle" AS e
UNION ALL
SELECT k."EagleId"
FROM "Kiwi" AS k
""");
    }

    public override async Task Can_use_of_type_kiwi(bool async)
    {
        await base.Can_use_of_type_kiwi(async);

        AssertSql(
            """
SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", k."FoundOn", 'Kiwi' AS "Discriminator"
FROM "Kiwi" AS k
""");
    }

    public override async Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_north_on_derived_property(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."FoundOn", u."Discriminator"
FROM (
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE u."FoundOn" = 0
""");
    }

    public override async Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_south_on_derived_property(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."FoundOn", u."Discriminator"
FROM (
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE u."FoundOn" = 1
""");
    }

    public override async Task Can_use_of_type_rose(bool async)
    {
        await base.Can_use_of_type_rose(async);

        AssertSql(
            """
SELECT r."Species", r."CountryId", r."Genus", r."Name", r."HasThorns", 'Rose' AS "Discriminator"
FROM "Roses" AS r
""");
    }

    public override async Task Member_access_on_intermediate_type_works()
    {
        await base.Member_access_on_intermediate_type_works();

        AssertSql(
            """
SELECT k."Name"
FROM "Kiwi" AS k
ORDER BY k."Name" NULLS FIRST
""");
    }

    public override async Task OfType_Union_OfType(bool async)
    {
        await base.OfType_Union_OfType(async);

        AssertSql();
    }

    public override async Task OfType_Union_subquery(bool async)
    {
        await base.OfType_Union_subquery(async);

        AssertSql();
    }

    public override Task Setting_foreign_key_to_a_different_type_throws()
        => base.Setting_foreign_key_to_a_different_type_throws();

    public override async Task Subquery_OfType(bool async)
    {
        await base.Subquery_OfType(async);

        AssertSql(
            """
@p='5'

SELECT DISTINCT u0."Id", u0."CountryId", u0."Name", u0."Species", u0."EagleId", u0."IsFlightless", u0."FoundOn", u0."Discriminator"
FROM (
    SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."FoundOn", u."Discriminator"
    FROM (
        SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
        FROM "Eagle" AS e
        UNION ALL
        SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", k."FoundOn", 'Kiwi' AS "Discriminator"
        FROM "Kiwi" AS k
    ) AS u
    ORDER BY u."Species" NULLS FIRST
    LIMIT @p
) AS u0
WHERE u0."Discriminator" = 'Kiwi'
""");
    }

    public override async Task Union_entity_equality(bool async)
    {
        await base.Union_entity_equality(async);

        AssertSql();
    }

    public override async Task Union_siblings_with_duplicate_property_in_subquery(bool async)
    {
        await base.Union_siblings_with_duplicate_property_in_subquery(async);

        AssertSql();
    }

    public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
    {
        await base.Is_operator_on_result_of_FirstOrDefault(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT u0."Discriminator"
        FROM (
            SELECT e0."Name", 'Eagle' AS "Discriminator"
            FROM "Eagle" AS e0
            UNION ALL
            SELECT k0."Name", 'Kiwi' AS "Discriminator"
            FROM "Kiwi" AS k0
        ) AS u0
        WHERE u0."Name" = 'Great spotted kiwi'
        LIMIT 1
    ) AS u1
    WHERE u1."Discriminator" = 'Kiwi')
ORDER BY u."Species" NULLS FIRST
""");
    }

    public override async Task Selecting_only_base_properties_on_base_type(bool async)
    {
        await base.Selecting_only_base_properties_on_base_type(async);

        AssertSql(
            """
SELECT e."Name"
FROM "Eagle" AS e
UNION ALL
SELECT k."Name"
FROM "Kiwi" AS k
""");
    }

    public override async Task Selecting_only_base_properties_on_derived_type(bool async)
    {
        await base.Selecting_only_base_properties_on_derived_type(async);

        AssertSql(
            """
SELECT e."Name"
FROM "Eagle" AS e
UNION ALL
SELECT k."Name"
FROM "Kiwi" AS k
""");
    }

    public override async Task Can_query_all_animal_views(bool async)
    {
        await base.Can_query_all_animal_views(async);

        AssertSql();
    }

    public override async Task Discriminator_used_when_projection_over_derived_type(bool async)
    {
        await base.Discriminator_used_when_projection_over_derived_type(async);

        AssertSql();
    }

    public override async Task Discriminator_used_when_projection_over_derived_type2(bool async)
    {
        await base.Discriminator_used_when_projection_over_derived_type2(async);

        AssertSql();
    }

    public override async Task Discriminator_used_when_projection_over_of_type(bool async)
    {
        await base.Discriminator_used_when_projection_over_of_type(async);

        AssertSql();
    }

    public override async Task Discriminator_with_cast_in_shadow_property(bool async)
    {
        await base.Discriminator_with_cast_in_shadow_property(async);

        AssertSql();
    }

    public override void Using_from_sql_throws()
    {
        base.Using_from_sql_throws();

        AssertSql();
    }

    public override async Task Using_is_operator_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_is_operator_on_multiple_type_with_no_result(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE u."Discriminator" = 'Eagle'
""");
    }

    public override async Task Using_is_operator_with_of_type_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_is_operator_with_of_type_on_multiple_type_with_no_result(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."Discriminator"
FROM (
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE u."Discriminator" = 'Eagle'
""");
    }

    public override async Task Using_OfType_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_OfType_on_multiple_type_with_no_result(async);

        AssertSql();
    }

    public override async Task GetType_in_hierarchy_in_abstract_base_type(bool async)
    {
        await base.GetType_in_hierarchy_in_abstract_base_type(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE FALSE
""");
    }

    public override async Task GetType_in_hierarchy_in_intermediate_type(bool async)
    {
        await base.GetType_in_hierarchy_in_intermediate_type(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE FALSE
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling(async);

        AssertSql(
            """
SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
FROM "Eagle" AS e
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2(async);

        AssertSql(
            """
SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
FROM "Kiwi" AS k
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(async);

        AssertSql(
            """
SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
FROM "Kiwi" AS k
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(async);

        AssertSql(
            """
SELECT u."Id", u."CountryId", u."Name", u."Species", u."EagleId", u."IsFlightless", u."Group", u."FoundOn", u."Discriminator"
FROM (
    SELECT e."Id", e."CountryId", e."Name", e."Species", e."EagleId", e."IsFlightless", e."Group", NULL AS "FoundOn", 'Eagle' AS "Discriminator"
    FROM "Eagle" AS e
    UNION ALL
    SELECT k."Id", k."CountryId", k."Name", k."Species", k."EagleId", k."IsFlightless", NULL AS "Group", k."FoundOn", 'Kiwi' AS "Discriminator"
    FROM "Kiwi" AS k
) AS u
WHERE u."Discriminator" <> 'Kiwi'
""");
    }

    public override async Task Primitive_collection_on_subtype(bool async)
    {
        await base.Primitive_collection_on_subtype(async);

        AssertSql(
            """
SELECT u."Id", u."SortIndex", u."CaffeineGrams", u."CokeCO2", u."Ints", u."SugarGrams", u."LiltCO2", u."SugarGrams1", u."CaffeineGrams1", u."HasMilk", u."ComplexTypeCollection", u."ParentComplexType_Int", u."ParentComplexType_UniqueInt", u."ParentComplexType_Nested_NestedInt", u."ParentComplexType_Nested_UniqueInt", u."ChildComplexType_Int", u."ChildComplexType_UniqueInt", u."ChildComplexType_Nested_NestedInt", u."ChildComplexType_Nested_UniqueInt", u."ChildComplexType_Int1", u."ChildComplexType_UniqueInt1", u."ChildComplexType_Nested_NestedInt1", u."ChildComplexType_Nested_UniqueInt1", u."Discriminator"
FROM (
    SELECT d."Id", d."SortIndex", d."ComplexTypeCollection", d."ParentComplexType_Int", d."ParentComplexType_UniqueInt", d."ParentComplexType_Nested_NestedInt", d."ParentComplexType_Nested_UniqueInt", NULL AS "CaffeineGrams", NULL AS "CokeCO2", NULL AS "Ints", NULL AS "SugarGrams", NULL AS "ChildComplexType_Int", NULL AS "ChildComplexType_UniqueInt", NULL AS "ChildComplexType_Nested_NestedInt", NULL AS "ChildComplexType_Nested_UniqueInt", NULL::int AS "LiltCO2", NULL::int AS "SugarGrams1", NULL::int AS "CaffeineGrams1", NULL::boolean AS "HasMilk", NULL::int AS "ChildComplexType_Int1", NULL::int AS "ChildComplexType_UniqueInt1", NULL::int AS "ChildComplexType_Nested_NestedInt1", NULL::int AS "ChildComplexType_Nested_UniqueInt1", 'Drink' AS "Discriminator"
    FROM "Drinks" AS d
    UNION ALL
    SELECT c."Id", c."SortIndex", c."ComplexTypeCollection", c."Int" AS "ParentComplexType_Int", c."UniqueInt" AS "ParentComplexType_UniqueInt", c."NestedInt" AS "ParentComplexType_Nested_NestedInt", c."NestedComplexType_UniqueInt" AS "ParentComplexType_Nested_UniqueInt", c."CaffeineGrams", c."CokeCO2", c."Ints", c."SugarGrams", c."ChildComplexType_Int", c."ChildComplexType_UniqueInt", c."ChildComplexType_Nested_NestedInt", c."ChildComplexType_Nested_UniqueInt", NULL AS "LiltCO2", NULL AS "SugarGrams1", NULL AS "CaffeineGrams1", NULL AS "HasMilk", NULL AS "ChildComplexType_Int1", NULL AS "ChildComplexType_UniqueInt1", NULL AS "ChildComplexType_Nested_NestedInt1", NULL AS "ChildComplexType_Nested_UniqueInt1", 'Coke' AS "Discriminator"
    FROM "Coke" AS c
    UNION ALL
    SELECT l."Id", l."SortIndex", l."ComplexTypeCollection", l."Int" AS "ParentComplexType_Int", l."UniqueInt" AS "ParentComplexType_UniqueInt", l."NestedInt" AS "ParentComplexType_Nested_NestedInt", l."NestedComplexType_UniqueInt" AS "ParentComplexType_Nested_UniqueInt", NULL AS "CaffeineGrams", NULL AS "CokeCO2", NULL AS "Ints", NULL AS "SugarGrams", NULL AS "ChildComplexType_Int", NULL AS "ChildComplexType_UniqueInt", NULL AS "ChildComplexType_Nested_NestedInt", NULL AS "ChildComplexType_Nested_UniqueInt", l."LiltCO2", l."SugarGrams" AS "SugarGrams1", NULL AS "CaffeineGrams1", NULL AS "HasMilk", NULL AS "ChildComplexType_Int1", NULL AS "ChildComplexType_UniqueInt1", NULL AS "ChildComplexType_Nested_NestedInt1", NULL AS "ChildComplexType_Nested_UniqueInt1", 'Lilt' AS "Discriminator"
    FROM "Lilt" AS l
    UNION ALL
    SELECT t."Id", t."SortIndex", t."ComplexTypeCollection", t."Int" AS "ParentComplexType_Int", t."UniqueInt" AS "ParentComplexType_UniqueInt", t."NestedInt" AS "ParentComplexType_Nested_NestedInt", t."NestedComplexType_UniqueInt" AS "ParentComplexType_Nested_UniqueInt", NULL AS "CaffeineGrams", NULL AS "CokeCO2", NULL AS "Ints", NULL AS "SugarGrams", NULL AS "ChildComplexType_Int", NULL AS "ChildComplexType_UniqueInt", NULL AS "ChildComplexType_Nested_NestedInt", NULL AS "ChildComplexType_Nested_UniqueInt", NULL AS "LiltCO2", NULL AS "SugarGrams1", t."CaffeineGrams" AS "CaffeineGrams1", t."HasMilk", t."ChildComplexType_Int" AS "ChildComplexType_Int1", t."ChildComplexType_UniqueInt" AS "ChildComplexType_UniqueInt1", t."ChildComplexType_Nested_NestedInt" AS "ChildComplexType_Nested_NestedInt1", t."ChildComplexType_Nested_UniqueInt" AS "ChildComplexType_Nested_UniqueInt1", 'Tea' AS "Discriminator"
    FROM "Tea" AS t
) AS u
WHERE cardinality(u."Ints") > 0
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
