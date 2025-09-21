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
@deletableEntity_Name='?'

DELETE FROM "RootEntity" AS r
WHERE r."Name" = @deletableEntity_Name
""");
    }

    public override async Task Delete_required_association()
    {
        await base.Delete_required_association();

        AssertSql();
    }

    public override async Task Delete_optional_association()
    {
        await base.Delete_optional_association();

        AssertSql();
    }

    #endregion Delete

    #region Update properties

    public override async Task Update_property_inside_association()
    {
        await base.Update_property_inside_association();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{String}', to_jsonb(@p))
""");
    }

    public override async Task Update_property_inside_association_with_special_chars()
    {
        await base.Update_property_inside_association_with_special_chars();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{String}', to_jsonb('{ Some other/JSON:like text though it [isn''t]: ממש ממש לאéèéè }'::text))
WHERE (r."RequiredRelated" ->> 'String') = '{ this may/look:like JSON but it [isn''t]: ממש ממש לאéèéè }'
""");
    }

    public override async Task Update_property_inside_nested()
    {
        await base.Update_property_inside_nested();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{RequiredNested,String}', to_jsonb(@p))
""");
    }

    public override async Task Update_property_on_projected_association()
    {
        await base.Update_property_on_projected_association();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{String}', to_jsonb(@p))
""");
    }

    public override async Task Update_property_on_projected_association_with_OrderBy_Skip()
    {
        await base.Update_property_on_projected_association_with_OrderBy_Skip();

        AssertExecuteUpdateSql();
    }

    #endregion Update properties

    #region Update association

    public override async Task Update_association_to_parameter()
    {
        await base.Update_association_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='?' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredRelated" = @complex_type_p
""");
    }

    public override async Task Update_nested_association_to_parameter()
    {
        await base.Update_nested_association_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='?' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{RequiredNested}', @complex_type_p)
""");
    }

    public override async Task Update_association_to_another_association()
    {
        await base.Update_association_to_another_association();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalRelated" = r."RequiredRelated"
""");
    }

    public override async Task Update_nested_association_to_another_nested_association()
    {
        await base.Update_nested_association_to_another_nested_association();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{OptionalNested}', r."RequiredRelated" -> 'RequiredNested')
""");
    }

    public override async Task Update_association_to_inline()
    {
        await base.Update_association_to_inline();

        AssertExecuteUpdateSql(
            """
@complex_type_p='?' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredRelated" = @complex_type_p
""");
    }

    public override async Task Update_association_to_inline_with_lambda()
    {
        await base.Update_association_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated" = '{"Id":1000,"Int":70,"Ints":[1,2,4],"Name":"Updated related name","String":"Updated related string","NestedCollection":[],"OptionalNested":null,"RequiredNested":{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}}'
""");
    }

    public override async Task Update_nested_association_to_inline_with_lambda()
    {
        await base.Update_nested_association_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{RequiredNested}', '{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name","String":"Updated nested string"}')
""");
    }

    public override async Task Update_association_to_null()
    {
        await base.Update_association_to_null();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalRelated" = NULL
""");
    }

    public override async Task Update_association_to_null_with_lambda()
    {
        await base.Update_association_to_null_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalRelated" = NULL
""");
    }

    public override async Task Update_association_to_null_parameter()
    {
        await base.Update_association_to_null_parameter();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalRelated" = NULL
""");
    }

    #endregion Update association

    #region Update collection

    public override async Task Update_collection_to_parameter()
    {
        await base.Update_collection_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='?' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RelatedCollection" = @complex_type_p
""");
    }

    public override async Task Update_nested_collection_to_parameter()
    {
        await base.Update_nested_collection_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p='?' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{NestedCollection}', @complex_type_p)
""");
    }

    public override async Task Update_nested_collection_to_inline_with_lambda()
    {
        await base.Update_nested_collection_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{NestedCollection}', '[{"Id":1000,"Int":80,"Ints":[1,2,4],"Name":"Updated nested name1","String":"Updated nested string1"},{"Id":1001,"Int":81,"Ints":[1,2,4],"Name":"Updated nested name2","String":"Updated nested string2"}]')
""");
    }

    public override async Task Update_nested_collection_to_another_nested_collection()
    {
        await base.Update_nested_collection_to_another_nested_collection();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{NestedCollection}', r."OptionalRelated" -> 'NestedCollection')
WHERE (r."OptionalRelated") IS NOT NULL
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
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{Ints}', to_jsonb(ARRAY[1,2,4]::integer[]::integer[]))
""");
    }

    public override async Task Update_primitive_collection_to_parameter()
    {
        await base.Update_primitive_collection_to_parameter();

        AssertExecuteUpdateSql(
            """
@ints='?' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{Ints}', to_jsonb(@ints))
""");
    }

    public override async Task Update_primitive_collection_to_another_collection()
    {
        await base.Update_primitive_collection_to_another_collection();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{OptionalNested,Ints}', to_jsonb((ARRAY(SELECT CAST(element AS integer) FROM jsonb_array_elements_text(r."RequiredRelated" #> '{RequiredNested,Ints}') WITH ORDINALITY AS t(element) ORDER BY ordinality))))
""");
    }

    public override async Task Update_inside_primitive_collection()
    {
        await base.Update_inside_primitive_collection();

        AssertExecuteUpdateSql();
    }

    #endregion Update primitive collection

    #region Multiple updates

    public override async Task Update_multiple_properties_inside_same_association()
    {
        await base.Update_multiple_properties_inside_same_association();

        AssertExecuteUpdateSql(
            """
@p='?'
@p0='?' (DbType = Int32)

UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(jsonb_set(r."RequiredRelated", '{String}', to_jsonb(@p)), '{Int}', to_jsonb(@p0))
""");
    }

    public override async Task Update_multiple_properties_inside_associations_and_on_entity_type()
    {
        await base.Update_multiple_properties_inside_associations_and_on_entity_type();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "Name" = r."Name" || 'Modified',
    "RequiredRelated" = jsonb_set(r."RequiredRelated", '{String}', to_jsonb(r."OptionalRelated" ->> 'String')),
    "OptionalRelated" = jsonb_set(r."OptionalRelated", '{RequiredNested,String}', to_jsonb(@p))
WHERE (r."OptionalRelated") IS NOT NULL
""");
    }

    public override async Task Update_multiple_projected_associations_via_anonymous_type()
    {
        await base.Update_multiple_projected_associations_via_anonymous_type();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredRelated" = jsonb_set(r."RequiredRelated", '{String}', to_jsonb(r."OptionalRelated" ->> 'String')),
    "OptionalRelated" = jsonb_set(r."OptionalRelated", '{String}', to_jsonb(@p))
WHERE (r."OptionalRelated") IS NOT NULL
""");
    }

    #endregion Multiple updates

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
