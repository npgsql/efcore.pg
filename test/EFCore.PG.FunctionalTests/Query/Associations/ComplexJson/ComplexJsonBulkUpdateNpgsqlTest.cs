namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonBulkUpdateNpgsqlTest(
    ComplexJsonNpgsqlFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexJsonBulkUpdateRelationalTestBase<ComplexJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    #region Delete

    public override async Task Delete_entity_with_associations()
    {
        await base.Delete_entity_with_associations();

        AssertSql(
            """
@deletableEntity_Name='Root3_With_different_values'

DELETE FROM "RootEntity" AS r
WHERE r."Name" = @deletableEntity_Name
""");
    }

    public override async Task Delete_required_associate()
    {
        await base.Delete_required_associate();

        AssertSql();
    }

    public override async Task Delete_optional_associate()
    {
        await base.Delete_optional_associate();

        AssertSql();
    }

    #endregion Delete

    #region Update properties

    public override async Task Update_property_inside_associate()
    {
        await base.Update_property_inside_associate();

        AssertExecuteUpdateSql(
            """
@p='foo_updated'

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{String}', to_jsonb(@p))
""");
    }

    public override async Task Update_property_inside_associate_with_special_chars()
    {
        await base.Update_property_inside_associate_with_special_chars();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{String}', to_jsonb('{ Some other/JSON:like text though it [isn''t]: ממש ממש לאéèéè }'::text))
WHERE (r."RequiredAssociate" ->> 'String') = '{ this may/look:like JSON but it [isn''t]: ממש ממש לאéèéè }'
""");
    }

    public override async Task Update_property_inside_nested_associate()
    {
        await base.Update_property_inside_nested_associate();

        AssertExecuteUpdateSql(
            """
@p='foo_updated'

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{RequiredNestedAssociate,String}', to_jsonb(@p))
""");
    }

    public override async Task Update_property_on_projected_associate()
    {
        await base.Update_property_on_projected_associate();

        AssertExecuteUpdateSql(
            """
@p='foo_updated'

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{String}', to_jsonb(@p))
""");
    }

    public override async Task Update_property_on_projected_associate_with_OrderBy_Skip()
    {
        await base.Update_property_on_projected_associate_with_OrderBy_Skip();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_associate_with_null_required_property()
    {
        await base.Update_associate_with_null_required_property();

        AssertExecuteUpdateSql();
    }

    #endregion Update properties

    #region Update association

    public override async Task Update_associate_to_parameter()
    {
        await base.Update_associate_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='{"Id":1000,"Int":80,"Ints":[1,2,3],"Name":"Updated associate name","String":"Updated nested string","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1000,"Int":80,"Ints":[1,2,3],"Name":"Updated nested name","String":"Updated nested string"}}' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = @complex_type_p
""");
    }

    public override async Task Update_nested_associate_to_parameter()
    {
        await base.Update_nested_associate_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{RequiredNestedAssociate}', @complex_type_p)
""");
    }

    public override async Task Update_associate_to_another_associate()
    {
        await base.Update_associate_to_another_associate();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalAssociate" = r."RequiredAssociate"
""");
    }

    public override async Task Update_nested_associate_to_another_nested_associate()
    {
        await base.Update_nested_associate_to_another_nested_associate();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{OptionalNestedAssociate}', r."RequiredAssociate" -> 'RequiredNestedAssociate')
""");
    }

    public override async Task Update_associate_to_inline()
    {
        await base.Update_associate_to_inline();

        AssertExecuteUpdateSql(
            """
@complex_type_p='{"Id":1000,"Int":70,"Ints":[1,2,4],"Name":"Updated associate name","String":"Updated associate string","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}}' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = @complex_type_p
""");
    }

    public override async Task Update_associate_to_inline_with_lambda()
    {
        await base.Update_associate_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate" = '{"Id":1000,"Int":70,"Ints":[1,2,4],"Name":"Updated associate name","String":"Updated associate string","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}}'
""");
    }

    public override async Task Update_nested_associate_to_inline_with_lambda()
    {
        await base.Update_nested_associate_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{RequiredNestedAssociate}', '{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}')
""");
    }

    public override async Task Update_associate_to_null()
    {
        await base.Update_associate_to_null();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalAssociate" = NULL
""");
    }

    public override async Task Update_associate_to_null_with_lambda()
    {
        await base.Update_associate_to_null_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalAssociate" = NULL
""");
    }

    public override async Task Update_associate_to_null_parameter()
    {
        await base.Update_associate_to_null_parameter();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalAssociate" = NULL
""");
    }

    public override async Task Update_required_nested_associate_to_null()
    {
        await base.Update_required_nested_associate_to_null();

        AssertExecuteUpdateSql();
    }

    #endregion Update association

    #region Update collection

    public override async Task Update_collection_to_parameter()
    {
        await base.Update_collection_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='[{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated associate name1","String":"Updated associate string1","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name1","String":"Updated nested string1"}},{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated associate name2","String":"Updated associate string2","NestedCollection":[],"OptionalNestedAssociate":null,"RequiredNestedAssociate":{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated nested name2","String":"Updated nested string2"}}]' (DbType = Object)

UPDATE "RootEntity" AS r
SET "AssociateCollection" = @complex_type_p
""");
    }

    public override async Task Update_nested_collection_to_parameter()
    {
        await base.Update_nested_collection_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='[{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name1","String":"Updated nested string1"},{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated nested name2","String":"Updated nested string2"}]' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{NestedCollection}', @complex_type_p)
""");
    }

    public override async Task Update_nested_collection_to_inline_with_lambda()
    {
        await base.Update_nested_collection_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{NestedCollection}', '[{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name1","String":"Updated nested string1"},{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated nested name2","String":"Updated nested string2"}]')
""");
    }

    public override async Task Update_nested_collection_to_another_nested_collection()
    {
        await base.Update_nested_collection_to_another_nested_collection();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{NestedCollection}', r."OptionalAssociate" -> 'NestedCollection')
WHERE (r."OptionalAssociate") IS NOT NULL
""");
    }

    public override async Task Update_collection_referencing_the_original_collection()
    {
        await base.Update_collection_referencing_the_original_collection();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_inside_structural_collection()
    {
        await base.Update_inside_structural_collection();

        AssertExecuteUpdateSql();
    }

    #endregion Update collection

    #region Update primitive collection

    public override async Task Update_primitive_collection_to_constant()
    {
        await base.Update_primitive_collection_to_constant();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{Ints}', '[1,2,4]')
""");
    }

    public override async Task Update_primitive_collection_to_parameter()
    {
        await base.Update_primitive_collection_to_parameter();

        AssertExecuteUpdateSql(
            """
@ints='[1,2,4]' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{Ints}', @ints)
""");
    }

    public override async Task Update_primitive_collection_to_another_collection()
    {
        await base.Update_primitive_collection_to_another_collection();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{OptionalNestedAssociate,Ints}', r."RequiredAssociate" #> '{RequiredNestedAssociate,Ints}')
""");
    }

    public override async Task Update_inside_primitive_collection()
    {
        await base.Update_inside_primitive_collection();

        AssertExecuteUpdateSql(
            """
@p='99'

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{Ints,1}', to_jsonb(@p))
WHERE jsonb_array_length(r."RequiredAssociate" -> 'Ints') >= 2
""");
    }

    #endregion Update primitive collection

    #region Multiple updates

    public override async Task Update_multiple_properties_inside_same_associate()
    {
        await base.Update_multiple_properties_inside_same_associate();

        AssertExecuteUpdateSql(
            """
@p='foo_updated'
@p1='20'

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(jsonb_set(r."RequiredAssociate", '{String}', to_jsonb(@p)), '{Int}', to_jsonb(@p1))
""");
    }

    public override async Task Update_multiple_properties_inside_associates_and_on_entity_type()
    {
        await base.Update_multiple_properties_inside_associates_and_on_entity_type();

        AssertExecuteUpdateSql(
            """
@p='foo_updated'

UPDATE "RootEntity" AS r
SET "Name" = r."Name" || 'Modified',
    "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{String}', r."OptionalAssociate" -> 'String'),
    "OptionalAssociate" = jsonb_set(r."OptionalAssociate", '{RequiredNestedAssociate,String}', to_jsonb(@p))
WHERE (r."OptionalAssociate") IS NOT NULL
""");
    }

    public override async Task Update_multiple_projected_associates_via_anonymous_type()
    {
        await base.Update_multiple_projected_associates_via_anonymous_type();

        AssertExecuteUpdateSql(
            """
@p='foo_updated'

UPDATE "RootEntity" AS r
SET "RequiredAssociate" = jsonb_set(r."RequiredAssociate", '{String}', r."OptionalAssociate" -> 'String'),
    "OptionalAssociate" = jsonb_set(r."OptionalAssociate", '{String}', to_jsonb(@p))
WHERE (r."OptionalAssociate") IS NOT NULL
""");
    }

    #endregion Multiple updates

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
