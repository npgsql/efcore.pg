// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPHInheritanceJsonQueryNpgsqlTest(TPHInheritanceJsonQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : TPHInheritanceJsonQueryRelationalTestBase<TPHInheritanceJsonQueryNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT d."Id", d."Discriminator", d."SortIndex", d."CaffeineGrams", d."CokeCO2", d."Ints", d."SugarGrams", d."ComplexTypeCollection", d."ParentComplexType", d."ChildComplexType"
FROM "Drinks" AS d
WHERE d."Discriminator" = 1 AND (CAST(d."ChildComplexType" ->> 'Int' AS integer)) = 10
""");
    }

    public override async Task Filter_on_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT d."Id", d."Discriminator", d."SortIndex", d."CaffeineGrams", d."CokeCO2", d."Ints", d."SugarGrams", d."LiltCO2", d."HasMilk", d."ComplexTypeCollection", d."ParentComplexType", d."ChildComplexType", d."ChildComplexType"
FROM "Drinks" AS d
WHERE (CAST(d."ParentComplexType" ->> 'Int' AS integer)) = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT d."Id", d."Discriminator", d."SortIndex", d."CaffeineGrams", d."CokeCO2", d."Ints", d."SugarGrams", d."ComplexTypeCollection", d."ParentComplexType", d."ChildComplexType"
FROM "Drinks" AS d
WHERE d."Discriminator" = 1 AND (CAST(d."ChildComplexType" #>> '{Nested,NestedInt}' AS integer)) = 58
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT d."Id", d."Discriminator", d."SortIndex", d."CaffeineGrams", d."CokeCO2", d."Ints", d."SugarGrams", d."LiltCO2", d."HasMilk", d."ComplexTypeCollection", d."ParentComplexType", d."ChildComplexType", d."ChildComplexType"
FROM "Drinks" AS d
WHERE (CAST(d."ParentComplexType" #>> '{Nested,NestedInt}' AS integer)) = 50
""");
    }

    public override async Task Project_complex_type_on_derived_type(bool async)
    {
        await base.Project_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT d."ChildComplexType"
FROM "Drinks" AS d
WHERE d."Discriminator" = 1
""");
    }

    public override async Task Project_complex_type_on_base_type(bool async)
    {
        await base.Project_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT d."ParentComplexType"
FROM "Drinks" AS d
""");
    }

    public override async Task Project_nested_complex_type_on_derived_type(bool async)
    {
        await base.Project_nested_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT d."ChildComplexType" -> 'Nested'
FROM "Drinks" AS d
WHERE d."Discriminator" = 1
""");
    }

    public override async Task Project_nested_complex_type_on_base_type(bool async)
    {
        await base.Project_nested_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT d."ParentComplexType" -> 'Nested'
FROM "Drinks" AS d
""");
    }

    public override async Task Subquery_over_complex_collection(bool async)
    {
        await base.Subquery_over_complex_collection(async);

        AssertSql(
            """
SELECT d."Id", d."Discriminator", d."SortIndex", d."CaffeineGrams", d."CokeCO2", d."Ints", d."SugarGrams", d."LiltCO2", d."HasMilk", d."ComplexTypeCollection", d."ParentComplexType", d."ChildComplexType", d."ChildComplexType"
FROM "Drinks" AS d
WHERE (
    SELECT count(*)::int
    FROM ROWS FROM (jsonb_to_recordset(d."ComplexTypeCollection") AS ("Int" integer)) WITH ORDINALITY AS c
    WHERE c."Int" > 59) = 2
""");
    }

    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
