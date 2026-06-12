namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPHInheritanceQueryNpgsqlTest(TPHInheritanceQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : TPHInheritanceQueryTestBase<TPHInheritanceQueryNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Byte_enum_value_constant_used_in_projection(bool async)
    {
        await base.Byte_enum_value_constant_used_in_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN a."IsFlightless" THEN 0
    ELSE 1
END
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
""");
    }

    public override async Task Can_filter_all_animals(bool async)
    {
        await base.Can_filter_all_animals(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE a."Name" = 'Great spotted kiwi'
ORDER BY a."Species" NULLS FIRST
""");
    }

    public override async Task Can_include_animals(bool async)
    {
        await base.Can_include_animals(async);

        AssertSql(
            """
SELECT c."Id", c."Name", a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Countries" AS c
LEFT JOIN "Animals" AS a ON c."Id" = a."CountryId"
ORDER BY c."Name" NULLS FIRST, c."Id" NULLS FIRST
""");
    }

    public override async Task Can_include_prey(bool async)
    {
        await base.Can_include_prey(async);

        AssertSql(
            """
SELECT a1."Id", a1."CountryId", a1."Discriminator", a1."Name", a1."Species", a1."EagleId", a1."IsFlightless", a1."Group", a0."Id", a0."CountryId", a0."Discriminator", a0."Name", a0."Species", a0."EagleId", a0."IsFlightless", a0."Group", a0."FoundOn"
FROM (
    SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group"
    FROM "Animals" AS a
    WHERE a."Discriminator" = 'Eagle'
    LIMIT 2
) AS a1
LEFT JOIN "Animals" AS a0 ON a1."Id" = a0."EagleId"
ORDER BY a1."Id" NULLS FIRST
""");
    }

    public override Task Can_insert_update_delete()
        => base.Can_insert_update_delete();

    public override async Task Can_query_all_animals(bool async)
    {
        await base.Can_query_all_animals(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
ORDER BY a."Species" NULLS FIRST
""");
    }

    public override async Task Can_query_all_birds(bool async)
    {
        await base.Can_query_all_birds(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
ORDER BY a."Species" NULLS FIRST
""");
    }

    public override async Task Can_query_all_plants(bool async)
    {
        await base.Can_query_all_plants(async);

        AssertSql(
            """
SELECT p."Species", p."CountryId", p."Genus", p."Name", p."HasThorns"
FROM "Plants" AS p
ORDER BY p."Species" NULLS FIRST
""");
    }

    public override void FromSql_on_root()
    {
        base.FromSql_on_root();

        AssertSql(
            """
select * from "Animals"
""");
    }

    public override void FromSql_on_derived()
    {
        base.FromSql_on_derived();

        AssertSql(
            """
SELECT m."Id", m."CountryId", m."Discriminator", m."Name", m."Species", m."EagleId", m."IsFlightless", m."Group"
FROM (
    select * from "Animals"
) AS m
WHERE m."Discriminator" = 'Eagle'
""");
    }

    public override async Task Can_query_all_types_when_shared_column(bool async)
    {
        await base.Can_query_all_types_when_shared_column(async);

        AssertSql(
            """
SELECT d."Id", d."Discriminator", d."SortIndex", d."CaffeineGrams", d."CokeCO2", d."Ints", d."SugarGrams", d."LiltCO2", d."HasMilk", d."ComplexTypeCollection", d."ParentComplexType_Int", d."ParentComplexType_UniqueInt", d."ParentComplexType_Nested_NestedInt", d."ParentComplexType_Nested_UniqueInt", d."ChildComplexType_Int", d."ChildComplexType_UniqueInt", d."ChildComplexType_Nested_NestedInt", d."ChildComplexType_Nested_UniqueInt", d."Tea_ChildComplexType_Int", d."Tea_ChildComplexType_UniqueInt", d."Tea_ChildComplexType_Nested_NestedInt", d."Tea_ChildComplexType_Nested_UniqueInt"
FROM "Drinks" AS d
""");
    }

    public override async Task Can_query_just_kiwis(bool async)
    {
        await base.Can_query_just_kiwis(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
LIMIT 2
""");
    }

    public override async Task Can_query_just_roses(bool async)
    {
        await base.Can_query_just_roses(async);

        AssertSql(
            """
SELECT p."Species", p."CountryId", p."Genus", p."Name", p."HasThorns"
FROM "Plants" AS p
WHERE p."Genus" = 0
LIMIT 2
""");
    }

    public override async Task Can_query_when_shared_column(bool async)
    {
        await base.Can_query_when_shared_column(async);

        AssertSql(
            """
SELECT d."Id", d."Discriminator", d."SortIndex", d."CaffeineGrams", d."CokeCO2", d."Ints", d."SugarGrams", d."ComplexTypeCollection", d."ParentComplexType_Int", d."ParentComplexType_UniqueInt", d."ParentComplexType_Nested_NestedInt", d."ParentComplexType_Nested_UniqueInt", d."ChildComplexType_Int", d."ChildComplexType_UniqueInt", d."ChildComplexType_Nested_NestedInt", d."ChildComplexType_Nested_UniqueInt"
FROM "Drinks" AS d
WHERE d."Discriminator" = 1
LIMIT 2
""",
            //
            """
SELECT d."Id", d."Discriminator", d."SortIndex", d."LiltCO2", d."SugarGrams", d."ComplexTypeCollection", d."ParentComplexType_Int", d."ParentComplexType_UniqueInt", d."ParentComplexType_Nested_NestedInt", d."ParentComplexType_Nested_UniqueInt"
FROM "Drinks" AS d
WHERE d."Discriminator" = 2
LIMIT 2
""",
            //
            """
SELECT d."Id", d."Discriminator", d."SortIndex", d."CaffeineGrams", d."HasMilk", d."ComplexTypeCollection", d."ParentComplexType_Int", d."ParentComplexType_UniqueInt", d."ParentComplexType_Nested_NestedInt", d."ParentComplexType_Nested_UniqueInt", d."Tea_ChildComplexType_Int", d."Tea_ChildComplexType_UniqueInt", d."Tea_ChildComplexType_Nested_NestedInt", d."Tea_ChildComplexType_Nested_UniqueInt"
FROM "Drinks" AS d
WHERE d."Discriminator" = 3
LIMIT 2
""");
    }

    public override async Task Can_use_backwards_is_animal(bool async)
    {
        await base.Can_use_backwards_is_animal(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
""");
    }

    public override async Task Can_use_backwards_of_type_animal(bool async)
    {
        await base.Can_use_backwards_of_type_animal(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
""");
    }

    public override async Task Can_use_is_kiwi(bool async)
    {
        await base.Can_use_is_kiwi(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
""");
    }

    public override async Task Can_use_is_kiwi_with_cast(bool async)
    {
        await base.Can_use_is_kiwi_with_cast(async);

        AssertSql(
            """
SELECT CASE
    WHEN a."Discriminator" = 'Kiwi' THEN a."FoundOn"
    ELSE 0
END AS "Value"
FROM "Animals" AS a
""");
    }

    public override async Task Can_use_is_kiwi_in_projection(bool async)
    {
        await base.Can_use_is_kiwi_in_projection(async);

        AssertSql(
            """
SELECT a."Discriminator" = 'Kiwi'
FROM "Animals" AS a
""");
    }

    public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
    {
        await base.Can_use_is_kiwi_with_other_predicate(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi' AND a."CountryId" = 1
""");
    }

    public override async Task Can_use_of_type_animal(bool async)
    {
        await base.Can_use_of_type_animal(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
ORDER BY a."Species" NULLS FIRST
""");
    }

    public override async Task Can_use_of_type_bird(bool async)
    {
        await base.Can_use_of_type_bird(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
ORDER BY a."Species" NULLS FIRST
""");
    }

    public override async Task Can_use_of_type_bird_first(bool async)
    {
        await base.Can_use_of_type_bird_first(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
ORDER BY a."Species" NULLS FIRST
LIMIT 1
""");
    }

    public override async Task Can_use_of_type_bird_predicate(bool async)
    {
        await base.Can_use_of_type_bird_predicate(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE a."CountryId" = 1
ORDER BY a."Species" NULLS FIRST
""");
    }

    public override async Task Can_use_of_type_bird_with_projection(bool async)
    {
        await base.Can_use_of_type_bird_with_projection(async);

        AssertSql(
            """
SELECT a."EagleId"
FROM "Animals" AS a
""");
    }

    public override async Task Can_use_of_type_kiwi(bool async)
    {
        await base.Can_use_of_type_kiwi(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
""");
    }

    public override async Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_north_on_derived_property(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi' AND a."FoundOn" = 0
""");
    }

    public override async Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_south_on_derived_property(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi' AND a."FoundOn" = 1
""");
    }

    public override async Task Can_use_of_type_rose(bool async)
    {
        await base.Can_use_of_type_rose(async);

        AssertSql(
            """
SELECT p."Species", p."CountryId", p."Genus", p."Name", p."HasThorns"
FROM "Plants" AS p
WHERE p."Genus" = 0
""");
    }

    public override async Task Member_access_on_intermediate_type_works()
    {
        await base.Member_access_on_intermediate_type_works();

        AssertSql(
            """
SELECT a."Name"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
ORDER BY a."Name" NULLS FIRST
""");
    }

    public override void Casting_to_base_type_joining_with_query_type_works()
    {
        base.Casting_to_base_type_joining_with_query_type_works();

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", m."CountryId", m."Discriminator", m."Name", m."EagleId", m."IsFlightless", m."Group", m."FoundOn"
FROM "Animals" AS a
INNER JOIN (
    Select * from "Animals"
) AS m ON a."Name" = m."Name"
WHERE a."Discriminator" = 'Eagle'
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

SELECT DISTINCT a0."Id", a0."CountryId", a0."Discriminator", a0."Name", a0."Species", a0."EagleId", a0."IsFlightless", a0."FoundOn"
FROM (
    SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."FoundOn"
    FROM "Animals" AS a
    ORDER BY a."Species" NULLS FIRST
    LIMIT @p
) AS a0
WHERE a0."Discriminator" = 'Kiwi'
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
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE (
    SELECT a0."Discriminator"
    FROM "Animals" AS a0
    WHERE a0."Name" = 'Great spotted kiwi'
    LIMIT 1) = 'Kiwi'
ORDER BY a."Species" NULLS FIRST
""");
    }

    public override async Task Selecting_only_base_properties_on_base_type(bool async)
    {
        await base.Selecting_only_base_properties_on_base_type(async);

        AssertSql(
            """
SELECT a."Name"
FROM "Animals" AS a
""");
    }

    public override async Task Selecting_only_base_properties_on_derived_type(bool async)
    {
        await base.Selecting_only_base_properties_on_derived_type(async);

        AssertSql(
            """
SELECT a."Name"
FROM "Animals" AS a
""");
    }

    public override async Task Can_query_all_animal_views(bool async)
    {
        await base.Can_query_all_animal_views(async);

        AssertSql(
            """
SELECT m."CountryId", m."Discriminator", m."Name", m."EagleId", m."IsFlightless", m."Group", m."FoundOn"
FROM (
    SELECT * FROM "Animals"
) AS m
ORDER BY m."CountryId" NULLS FIRST
""");
    }

    public override async Task Discriminator_used_when_projection_over_derived_type(bool async)
    {
        await base.Discriminator_used_when_projection_over_derived_type(async);

        AssertSql(
            """
SELECT a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
""");
    }

    public override async Task Discriminator_used_when_projection_over_derived_type2(bool async)
    {
        await base.Discriminator_used_when_projection_over_derived_type2(async);

        AssertSql(
            """
SELECT a."IsFlightless", a."Discriminator"
FROM "Animals" AS a
""");
    }

    public override async Task Discriminator_used_when_projection_over_of_type(bool async)
    {
        await base.Discriminator_used_when_projection_over_of_type(async);

        AssertSql(
            """
SELECT a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
""");
    }

    public override async Task Discriminator_with_cast_in_shadow_property(bool async)
    {
        await base.Discriminator_with_cast_in_shadow_property(async);

        AssertSql(
            """
SELECT a."Name" AS "Predator"
FROM "Animals" AS a
WHERE 'Kiwi' = a."Discriminator"
""");
    }

    public override async Task Using_is_operator_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_is_operator_on_multiple_type_with_no_result(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE FALSE
""");
    }

    public override async Task Using_is_operator_with_of_type_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_is_operator_with_of_type_on_multiple_type_with_no_result(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group"
FROM "Animals" AS a
WHERE FALSE
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
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE FALSE
""");
    }

    public override async Task GetType_in_hierarchy_in_intermediate_type(bool async)
    {
        await base.GetType_in_hierarchy_in_intermediate_type(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE FALSE
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Eagle'
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" = 'Kiwi'
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(async);

        AssertSql(
            """
SELECT a."Id", a."CountryId", a."Discriminator", a."Name", a."Species", a."EagleId", a."IsFlightless", a."Group", a."FoundOn"
FROM "Animals" AS a
WHERE a."Discriminator" <> 'Kiwi'
""");
    }

    public override async Task Primitive_collection_on_subtype(bool async)
    {
        await base.Primitive_collection_on_subtype(async);

        AssertSql(
            """
SELECT d."Id", d."Discriminator", d."SortIndex", d."CaffeineGrams", d."CokeCO2", d."Ints", d."SugarGrams", d."LiltCO2", d."HasMilk", d."ComplexTypeCollection", d."ParentComplexType_Int", d."ParentComplexType_UniqueInt", d."ParentComplexType_Nested_NestedInt", d."ParentComplexType_Nested_UniqueInt", d."ChildComplexType_Int", d."ChildComplexType_UniqueInt", d."ChildComplexType_Nested_NestedInt", d."ChildComplexType_Nested_UniqueInt", d."Tea_ChildComplexType_Int", d."Tea_ChildComplexType_UniqueInt", d."Tea_ChildComplexType_Nested_NestedInt", d."Tea_ChildComplexType_Nested_UniqueInt"
FROM "Drinks" AS d
WHERE cardinality(d."Ints") > 0
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
