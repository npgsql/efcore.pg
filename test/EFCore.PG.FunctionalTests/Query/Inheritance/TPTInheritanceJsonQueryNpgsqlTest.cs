// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPTInheritanceJsonQueryNpgsqlTest(TPTInheritanceJsonQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : TPTInheritanceJsonQueryRelationalTestBase<TPTInheritanceJsonQueryNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT d."Id", d."SortIndex", c."CaffeineGrams", c."CokeCO2", c."Ints", c."SugarGrams", d."ComplexTypeCollection", d."ParentComplexType", c."ChildComplexType"
FROM "Drinks" AS d
INNER JOIN "Coke" AS c ON d."Id" = c."Id"
WHERE (CAST(c."ChildComplexType" ->> 'Int' AS integer)) = 10
""");
    }

    public override async Task Filter_on_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT d."Id", d."SortIndex", c."CaffeineGrams", c."CokeCO2", c."Ints", c."SugarGrams", l."LiltCO2", l."SugarGrams", t."CaffeineGrams", t."HasMilk", d."ComplexTypeCollection", d."ParentComplexType", c."ChildComplexType", t."ChildComplexType", CASE
    WHEN t."Id" IS NOT NULL THEN 'Tea'
    WHEN l."Id" IS NOT NULL THEN 'Lilt'
    WHEN c."Id" IS NOT NULL THEN 'Coke'
END AS "Discriminator"
FROM "Drinks" AS d
LEFT JOIN "Coke" AS c ON d."Id" = c."Id"
LEFT JOIN "Lilt" AS l ON d."Id" = l."Id"
LEFT JOIN "Tea" AS t ON d."Id" = t."Id"
WHERE (CAST(d."ParentComplexType" ->> 'Int' AS integer)) = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT d."Id", d."SortIndex", c."CaffeineGrams", c."CokeCO2", c."Ints", c."SugarGrams", d."ComplexTypeCollection", d."ParentComplexType", c."ChildComplexType"
FROM "Drinks" AS d
INNER JOIN "Coke" AS c ON d."Id" = c."Id"
WHERE (CAST(c."ChildComplexType" #>> '{Nested,NestedInt}' AS integer)) = 58
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT d."Id", d."SortIndex", c."CaffeineGrams", c."CokeCO2", c."Ints", c."SugarGrams", l."LiltCO2", l."SugarGrams", t."CaffeineGrams", t."HasMilk", d."ComplexTypeCollection", d."ParentComplexType", c."ChildComplexType", t."ChildComplexType", CASE
    WHEN t."Id" IS NOT NULL THEN 'Tea'
    WHEN l."Id" IS NOT NULL THEN 'Lilt'
    WHEN c."Id" IS NOT NULL THEN 'Coke'
END AS "Discriminator"
FROM "Drinks" AS d
LEFT JOIN "Coke" AS c ON d."Id" = c."Id"
LEFT JOIN "Lilt" AS l ON d."Id" = l."Id"
LEFT JOIN "Tea" AS t ON d."Id" = t."Id"
WHERE (CAST(d."ParentComplexType" #>> '{Nested,NestedInt}' AS integer)) = 50
""");
    }

    public override async Task Project_complex_type_on_derived_type(bool async)
    {
        await base.Project_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT c."ChildComplexType"
FROM "Drinks" AS d
INNER JOIN "Coke" AS c ON d."Id" = c."Id"
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
SELECT c."ChildComplexType" -> 'Nested'
FROM "Drinks" AS d
INNER JOIN "Coke" AS c ON d."Id" = c."Id"
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
SELECT d."Id", d."SortIndex", c."CaffeineGrams", c."CokeCO2", c."Ints", c."SugarGrams", l."LiltCO2", l."SugarGrams", t."CaffeineGrams", t."HasMilk", d."ComplexTypeCollection", d."ParentComplexType", c."ChildComplexType", t."ChildComplexType", CASE
    WHEN t."Id" IS NOT NULL THEN 'Tea'
    WHEN l."Id" IS NOT NULL THEN 'Lilt'
    WHEN c."Id" IS NOT NULL THEN 'Coke'
END AS "Discriminator"
FROM "Drinks" AS d
LEFT JOIN "Coke" AS c ON d."Id" = c."Id"
LEFT JOIN "Lilt" AS l ON d."Id" = l."Id"
LEFT JOIN "Tea" AS t ON d."Id" = t."Id"
WHERE (
    SELECT count(*)::int
    FROM ROWS FROM (jsonb_to_recordset(d."ComplexTypeCollection") AS ("Int" integer)) WITH ORDINALITY AS c0
    WHERE c0."Int" > 59) = 2
""");
    }

    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
