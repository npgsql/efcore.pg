// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonStructuralEqualityNpgsqlTest(ComplexJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonStructuralEqualityRelationalTestBase<ComplexJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    // The SQL Server json type cannot be compared ("The JSON data type cannot be compared or sorted, except when using the
    // IS NULL operator").
    // So we find comparisons that involve the json type, and apply a conversion to string (nvarchar(max)) to both sides.

    public override async Task Two_associates()
    {
        await base.Two_associates();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate") = (r."OptionalAssociate")
""");
    }

    public override async Task Two_nested_associates()
    {
        await base.Two_nested_associates();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate" -> 'RequiredNestedAssociate') = (r."OptionalAssociate" -> 'RequiredNestedAssociate')
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate") <> (r."OptionalAssociate") OR (r."OptionalAssociate") IS NULL
""");
    }

    public override async Task Associate_with_inline_null()
    {
        await base.Associate_with_inline_null();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."OptionalAssociate") IS NULL
""");
    }

    public override async Task Associate_with_parameter_null()
    {
        await base.Associate_with_parameter_null();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."OptionalAssociate") IS NULL
""");
    }

    public override async Task Nested_associate_with_inline_null()
    {
        await base.Nested_associate_with_inline_null();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate" ->> 'OptionalNestedAssociate') IS NULL
""");
    }

    public override async Task Nested_associate_with_inline()
    {
        await base.Nested_associate_with_inline();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate" -> 'RequiredNestedAssociate') = '{"Id":1000,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_RequiredNestedAssociate","String":"foo"}'
""");
    }

    public override async Task Nested_associate_with_parameter()
    {
        await base.Nested_associate_with_parameter();

        AssertSql(
            """
@entity_equality_nested='{"Id":1000,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_RequiredNestedAssociate","String":"foo"}' (DbType = Object)

SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate" -> 'RequiredNestedAssociate') = @entity_equality_nested
""");
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate" -> 'NestedCollection') = (r."OptionalAssociate" -> 'NestedCollection')
""");
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate" -> 'NestedCollection') = '[{"Id":1002,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_2","String":"foo"}]'
""");
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql(
            """
@entity_equality_nestedCollection='[{"Id":1002,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_2","String":"foo"}]' (DbType = Object)

SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate" -> 'NestedCollection') = @entity_equality_nestedCollection
""");
    }

    #region Contains

    public override async Task Contains_with_inline()
    {
        await base.Contains_with_inline();

        // TODO: The following translation is sub-optimal: we should be using OPENSJON to extract elements of the collection as JSON elements (OPENJSON WITH JSON),
        // and comparison those elements to a single entire JSON fragment on the other side (just like non-collection JSON comparison), rather than breaking the
        // elements down to their columns and doing column-by-column comparison. See #32576.
        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE EXISTS (
    SELECT 1
    FROM ROWS FROM (jsonb_to_recordset(r."RequiredAssociate" -> 'NestedCollection') AS (
        "Id" integer,
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text
    )) WITH ORDINALITY AS n
    WHERE n."Id" = 1002 AND n."Int" = 8 AND n."Ints" = '[1,2,3]' AND n."Name" = 'Root1_RequiredAssociate_NestedCollection_1' AND n."String" = 'foo')
""");
    }

    public override async Task Contains_with_parameter()
    {
        await base.Contains_with_parameter();

        // TODO: The following translation is sub-optimal: we should be using OPENSJON to extract elements of the collection as JSON elements (OPENJSON WITH JSON),
        // and comparison those elements to a single entire JSON fragment on the other side (just like non-collection JSON comparison), rather than breaking the
        // elements down to their columns and doing column-by-column comparison. See #32576.
        AssertSql(
            """
@entity_equality_nested_Id='1002' (Nullable = true)
@entity_equality_nested_Int='8' (Nullable = true)
@entity_equality_nested_Ints='[1,2,3]' (DbType = Object)
@entity_equality_nested_Name='Root1_RequiredAssociate_NestedCollection_1'
@entity_equality_nested_String='foo'

SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE EXISTS (
    SELECT 1
    FROM ROWS FROM (jsonb_to_recordset(r."RequiredAssociate" -> 'NestedCollection') AS (
        "Id" integer,
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text
    )) WITH ORDINALITY AS n
    WHERE n."Id" = @entity_equality_nested_Id AND n."Int" = @entity_equality_nested_Int AND n."Ints" = @entity_equality_nested_Ints AND n."Name" = @entity_equality_nested_Name AND n."String" = @entity_equality_nested_String)
""");
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        await base.Contains_with_operators_composed_on_the_collection();

        AssertSql(
            """
@get_Item_Int='106'
@entity_equality_get_Item_Id='3003' (Nullable = true)
@entity_equality_get_Item_Int='108' (Nullable = true)
@entity_equality_get_Item_Ints='[8,9,109]' (DbType = Object)
@entity_equality_get_Item_Name='Root3_RequiredAssociate_NestedCollection_2'
@entity_equality_get_Item_String='foo104'

SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE EXISTS (
    SELECT 1
    FROM ROWS FROM (jsonb_to_recordset(r."RequiredAssociate" -> 'NestedCollection') AS (
        "Id" integer,
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text
    )) WITH ORDINALITY AS n
    WHERE n."Int" > @get_Item_Int AND n."Id" = @entity_equality_get_Item_Id AND n."Int" = @entity_equality_get_Item_Int AND n."Ints" = @entity_equality_get_Item_Ints AND n."Name" = @entity_equality_get_Item_Name AND n."String" = @entity_equality_get_Item_String)
""");
    }

    public override async Task Contains_with_nested_and_composed_operators()
    {
        await base.Contains_with_nested_and_composed_operators();

        AssertSql(
            """
@get_Item_Id='302'
@entity_equality_get_Item_Id='303' (Nullable = true)
@entity_equality_get_Item_Int='130' (Nullable = true)
@entity_equality_get_Item_Ints='[8,9,131]' (DbType = Object)
@entity_equality_get_Item_Name='Root3_AssociateCollection_2'
@entity_equality_get_Item_String='foo115'
@entity_equality_get_Item_NestedCollection='[{"Id":3014,"Int":136,"Ints":[8,9,137],"Name":"Root3_AssociateCollection_2_NestedCollection_1","String":"foo118"},{"Id":3015,"Int":138,"Ints":[8,9,139],"Name":"Root3_Root1_AssociateCollection_2_NestedCollection_2","String":"foo119"}]' (DbType = Object)
@entity_equality_get_Item_OptionalNestedAssociate='{"Id":3013,"Int":134,"Ints":[8,9,135],"Name":"Root3_AssociateCollection_2_OptionalNestedAssociate","String":"foo117"}' (DbType = Object)
@entity_equality_get_Item_RequiredNestedAssociate='{"Id":3012,"Int":132,"Ints":[8,9,133],"Name":"Root3_AssociateCollection_2_RequiredNestedAssociate","String":"foo116"}' (DbType = Object)

SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE EXISTS (
    SELECT 1
    FROM ROWS FROM (jsonb_to_recordset(r."AssociateCollection") AS (
        "Id" integer,
        "Int" integer,
        "Ints" jsonb,
        "Name" text,
        "String" text,
        "NestedCollection" jsonb,
        "OptionalNestedAssociate" jsonb,
        "RequiredNestedAssociate" jsonb
    )) WITH ORDINALITY AS a
    WHERE a."Id" > @get_Item_Id AND a."Id" = @entity_equality_get_Item_Id AND a."Int" = @entity_equality_get_Item_Int AND a."Ints" = @entity_equality_get_Item_Ints AND a."Name" = @entity_equality_get_Item_Name AND a."String" = @entity_equality_get_Item_String AND (a."NestedCollection") = @entity_equality_get_Item_NestedCollection AND (a."OptionalNestedAssociate") = @entity_equality_get_Item_OptionalNestedAssociate AND (a."RequiredNestedAssociate") = @entity_equality_get_Item_RequiredNestedAssociate)
""");
    }

    #endregion Contains

    #region Value types

    public override async Task Nullable_value_type_with_null()
    {
        await base.Nullable_value_type_with_null();

        AssertSql(
            """
SELECT v."Id", v."Name", v."AssociateCollection", v."OptionalAssociate", v."RequiredAssociate"
FROM "ValueRootEntity" AS v
WHERE (v."OptionalAssociate") IS NULL
""");
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
