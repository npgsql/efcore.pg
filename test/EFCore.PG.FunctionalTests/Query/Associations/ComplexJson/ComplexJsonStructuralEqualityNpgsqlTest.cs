// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonStructuralEqualityNpgsqlTest(ComplexJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonStructuralEqualityRelationalTestBase<ComplexJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    // The SQL Server json type cannot be compared ("The JSON data type cannot be compared or sorted, except when using the
    // IS NULL operator").
    // So we find comparisons that involve the json type, and apply a conversion to string (nvarchar(max)) to both sides.

    public override async Task Two_related()
    {
        await base.Two_related();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."RequiredRelated") = (r."OptionalRelated")
""");
    }

    public override async Task Two_nested()
    {
        await base.Two_nested();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."RequiredRelated" -> 'RequiredNested') = (r."OptionalRelated" -> 'RequiredNested')
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."RequiredRelated") <> (r."OptionalRelated") OR (r."OptionalRelated") IS NULL
""");
    }

    public override async Task Related_with_inline_null()
    {
        await base.Related_with_inline_null();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."OptionalRelated") IS NULL
""");
    }

    public override async Task Related_with_parameter_null()
    {
        await base.Related_with_parameter_null();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."OptionalRelated") IS NULL
""");
    }

    public override async Task Nested_with_inline_null()
    {
        await base.Nested_with_inline_null();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."RequiredRelated" ->> 'OptionalNested') IS NULL
""");
    }

    public override async Task Nested_with_inline()
    {
        await base.Nested_with_inline();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."RequiredRelated" -> 'RequiredNested') = '{"Id":1000,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredRelated_RequiredNested","String":"foo"}'
""");
    }

    public override async Task Nested_with_parameter()
    {
        await base.Nested_with_parameter();

        AssertSql(
            """
@entity_equality_nested='?' (DbType = Object)

SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."RequiredRelated" -> 'RequiredNested') = @entity_equality_nested
""");
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."RequiredRelated" -> 'NestedCollection') = (r."OptionalRelated" -> 'NestedCollection')
""");
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."RequiredRelated" -> 'NestedCollection') = '[{"Id":1002,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredRelated_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredRelated_NestedCollection_2","String":"foo"}]'
""");
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql(
            """
@entity_equality_nestedCollection='?' (DbType = Object)

SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (r."RequiredRelated" -> 'NestedCollection') = @entity_equality_nestedCollection
""");
    }

    #region Contains

    public override async Task Contains_with_inline()
    {
        // https://github.com/dotnet/efcore/issues/36837
        await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await base.Contains_with_inline();

            // TODO: The following translation is sub-optimal: we should be using OPENSJON to extract elements of the collection as JSON elements (OPENJSON WITH JSON),
            // and comparison those elements to a single entire JSON fragment on the other side (just like non-collection JSON comparison), rather than breaking the
            // elements down to their columns and doing column-by-column comparison. See #32576.
            AssertSql(
                """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE EXISTS (
    SELECT 1
    FROM ROWS FROM (jsonb_to_recordset(r."RequiredRelated" -> 'NestedCollection') AS (
        "Id" integer,
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text
    )) WITH ORDINALITY AS n
    WHERE n."Id" = 1002 AND n."Int" = 8 AND n."Ints" = '[1,2,3]' AND n."Name" = 'Root1_RequiredRelated_NestedCollection_1' AND n."String" = 'foo')
""");
        });
    }

    public override async Task Contains_with_parameter()
    {
        // https://github.com/dotnet/efcore/issues/36837
        await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await base.Contains_with_parameter();

            // TODO: The following translation is sub-optimal: we should be using OPENSJON to extract elements of the collection as JSON elements (OPENJSON WITH JSON),
            // and comparison those elements to a single entire JSON fragment on the other side (just like non-collection JSON comparison), rather than breaking the
            // elements down to their columns and doing column-by-column comparison. See #32576.
            AssertSql(
                """
@entity_equality_nested_Id='?' (DbType = Int32)
@entity_equality_nested_Int='?' (DbType = Int32)
@entity_equality_nested_Ints='?' (DbType = Object)
@entity_equality_nested_Name='?'
@entity_equality_nested_String='?'

SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE EXISTS (
    SELECT 1
    FROM ROWS FROM (jsonb_to_recordset(r."RequiredRelated" -> 'NestedCollection') AS (
        "Id" integer,
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text
    )) WITH ORDINALITY AS n
    WHERE n."Id" = @entity_equality_nested_Id AND n."Int" = @entity_equality_nested_Int AND n."Ints" = @entity_equality_nested_Ints AND n."Name" = @entity_equality_nested_Name AND n."String" = @entity_equality_nested_String)
""");
        });
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        // https://github.com/dotnet/efcore/issues/36837
        await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await base.Contains_with_operators_composed_on_the_collection();

            AssertSql(
                """
@get_Item_Int='?' (DbType = Int32)
@entity_equality_get_Item_Id='?' (DbType = Int32)
@entity_equality_get_Item_Int='?' (DbType = Int32)
@entity_equality_get_Item_Ints='?' (DbType = Object)
@entity_equality_get_Item_Name='?'
@entity_equality_get_Item_String='?'

SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE EXISTS (
    SELECT 1
    FROM ROWS FROM (jsonb_to_recordset(r."RequiredRelated" -> 'NestedCollection') AS (
        "Id" integer,
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text
    )) WITH ORDINALITY AS n
    WHERE n."Int" > @get_Item_Int AND n."Id" = @entity_equality_get_Item_Id AND n."Int" = @entity_equality_get_Item_Int AND n."Ints" = @entity_equality_get_Item_Ints AND n."Name" = @entity_equality_get_Item_Name AND n."String" = @entity_equality_get_Item_String)
""");
        });
    }

    public override async Task Contains_with_nested_and_composed_operators()
    {
        // https://github.com/dotnet/efcore/issues/36837
        await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await base.Contains_with_nested_and_composed_operators();

            AssertSql(
                """
@get_Item_Id='?' (DbType = Int32)
@entity_equality_get_Item_Id='?' (DbType = Int32)
@entity_equality_get_Item_Int='?' (DbType = Int32)
@entity_equality_get_Item_Ints='?' (DbType = Object)
@entity_equality_get_Item_Name='?'
@entity_equality_get_Item_String='?'
@entity_equality_get_Item_NestedCollection='?' (DbType = Object)
@entity_equality_get_Item_OptionalNested='?' (DbType = Object)
@entity_equality_get_Item_RequiredNested='?' (DbType = Object)

SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE EXISTS (
    SELECT 1
    FROM ROWS FROM (jsonb_to_recordset(r."RelatedCollection") AS (
        "Id" integer,
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text,
        "NestedCollection" jsonb,
        "OptionalNested" jsonb,
        "RequiredNested" jsonb
    )) WITH ORDINALITY AS r0
    WHERE r0."Id" > @get_Item_Id AND r0."Id" = @entity_equality_get_Item_Id AND r0."Int" = @entity_equality_get_Item_Int AND r0."Ints" = @entity_equality_get_Item_Ints AND r0."Name" = @entity_equality_get_Item_Name AND r0."String" = @entity_equality_get_Item_String AND (r0."NestedCollection") = @entity_equality_get_Item_NestedCollection AND (r0."OptionalNested") = @entity_equality_get_Item_OptionalNested AND (r0."RequiredNested") = @entity_equality_get_Item_RequiredNested)
""");
        });
    }

    #endregion Contains

    #region Value types

    public override async Task Nullable_value_type_with_null()
    {
        await base.Nullable_value_type_with_null();

        AssertSql(
            """
SELECT v."Id", v."Name", v."OptionalRelated", v."RelatedCollection", v."RequiredRelated"
FROM "ValueRootEntity" AS v
WHERE (v."OptionalRelated") IS NULL
""");
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
