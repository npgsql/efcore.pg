namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingBulkUpdateNpgsqlTest(
    ComplexTableSplittingNpgsqlFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingBulkUpdateRelationalTestBase<ComplexTableSplittingNpgsqlFixture>(fixture, testOutputHelper)
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
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredAssociate_String" = @p
""");
    }

    public override async Task Update_property_inside_associate_with_special_chars()
    {
        await base.Update_property_inside_associate_with_special_chars();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate_String" = '{ Some other/JSON:like text though it [isn''t]: ממש ממש לאéèéè }'
WHERE r."RequiredAssociate_String" = '{ this may/look:like JSON but it [isn''t]: ממש ממש לאéèéè }'
""");
    }

    public override async Task Update_property_inside_nested_associate()
    {
        await base.Update_property_inside_nested_associate();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredAssociate_RequiredNestedAssociate_String" = @p
""");
    }

    public override async Task Update_property_on_projected_associate()
    {
        await base.Update_property_on_projected_associate();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredAssociate_String" = @p
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
@complex_type_p_Id='?' (DbType = Int32)
@complex_type_p_Int='?' (DbType = Int32)
@complex_type_p_Ints='?' (DbType = Object)
@complex_type_p_Name='?'
@complex_type_p_String='?'
@complex_type_p_RequiredNestedAssociate_Id='?' (DbType = Int32)
@complex_type_p_RequiredNestedAssociate_Int='?' (DbType = Int32)
@complex_type_p_RequiredNestedAssociate_Ints='?' (DbType = Object)
@complex_type_p_RequiredNestedAssociate_Name='?'
@complex_type_p_RequiredNestedAssociate_String='?'

UPDATE "RootEntity" AS r
SET "RequiredAssociate_Id" = @complex_type_p_Id,
    "RequiredAssociate_Int" = @complex_type_p_Int,
    "RequiredAssociate_Ints" = @complex_type_p_Ints,
    "RequiredAssociate_Name" = @complex_type_p_Name,
    "RequiredAssociate_String" = @complex_type_p_String,
    "RequiredAssociate_OptionalNestedAssociate_Id" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_Int" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_Ints" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_Name" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_String" = NULL,
    "RequiredAssociate_RequiredNestedAssociate_Id" = @complex_type_p_RequiredNestedAssociate_Id,
    "RequiredAssociate_RequiredNestedAssociate_Int" = @complex_type_p_RequiredNestedAssociate_Int,
    "RequiredAssociate_RequiredNestedAssociate_Ints" = @complex_type_p_RequiredNestedAssociate_Ints,
    "RequiredAssociate_RequiredNestedAssociate_Name" = @complex_type_p_RequiredNestedAssociate_Name,
    "RequiredAssociate_RequiredNestedAssociate_String" = @complex_type_p_RequiredNestedAssociate_String
""");
    }

    public override async Task Update_nested_associate_to_parameter()
    {
        await base.Update_nested_associate_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p_Id='?' (DbType = Int32)
@complex_type_p_Int='?' (DbType = Int32)
@complex_type_p_Ints='?' (DbType = Object)
@complex_type_p_Name='?'
@complex_type_p_String='?'

UPDATE "RootEntity" AS r
SET "RequiredAssociate_RequiredNestedAssociate_Id" = @complex_type_p_Id,
    "RequiredAssociate_RequiredNestedAssociate_Int" = @complex_type_p_Int,
    "RequiredAssociate_RequiredNestedAssociate_Ints" = @complex_type_p_Ints,
    "RequiredAssociate_RequiredNestedAssociate_Name" = @complex_type_p_Name,
    "RequiredAssociate_RequiredNestedAssociate_String" = @complex_type_p_String
""");
    }

    public override async Task Update_associate_to_another_associate()
    {
        await base.Update_associate_to_another_associate();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalAssociate_Id" = r."RequiredAssociate_Id",
    "OptionalAssociate_Int" = r."RequiredAssociate_Int",
    "OptionalAssociate_Ints" = r."RequiredAssociate_Ints",
    "OptionalAssociate_Name" = r."RequiredAssociate_Name",
    "OptionalAssociate_String" = r."RequiredAssociate_String",
    "OptionalAssociate_OptionalNestedAssociate_Id" = r."OptionalAssociate_OptionalNestedAssociate_Id",
    "OptionalAssociate_OptionalNestedAssociate_Int" = r."OptionalAssociate_OptionalNestedAssociate_Int",
    "OptionalAssociate_OptionalNestedAssociate_Ints" = r."OptionalAssociate_OptionalNestedAssociate_Ints",
    "OptionalAssociate_OptionalNestedAssociate_Name" = r."OptionalAssociate_OptionalNestedAssociate_Name",
    "OptionalAssociate_OptionalNestedAssociate_String" = r."OptionalAssociate_OptionalNestedAssociate_String",
    "OptionalAssociate_RequiredNestedAssociate_Id" = r."OptionalAssociate_RequiredNestedAssociate_Id",
    "OptionalAssociate_RequiredNestedAssociate_Int" = r."OptionalAssociate_RequiredNestedAssociate_Int",
    "OptionalAssociate_RequiredNestedAssociate_Ints" = r."OptionalAssociate_RequiredNestedAssociate_Ints",
    "OptionalAssociate_RequiredNestedAssociate_Name" = r."OptionalAssociate_RequiredNestedAssociate_Name",
    "OptionalAssociate_RequiredNestedAssociate_String" = r."OptionalAssociate_RequiredNestedAssociate_String"
""");
    }

    public override async Task Update_nested_associate_to_another_nested_associate()
    {
        await base.Update_nested_associate_to_another_nested_associate();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate_OptionalNestedAssociate_Id" = r."RequiredAssociate_RequiredNestedAssociate_Id",
    "RequiredAssociate_OptionalNestedAssociate_Int" = r."RequiredAssociate_RequiredNestedAssociate_Int",
    "RequiredAssociate_OptionalNestedAssociate_Ints" = r."RequiredAssociate_RequiredNestedAssociate_Ints",
    "RequiredAssociate_OptionalNestedAssociate_Name" = r."RequiredAssociate_RequiredNestedAssociate_Name",
    "RequiredAssociate_OptionalNestedAssociate_String" = r."RequiredAssociate_RequiredNestedAssociate_String"
""");
    }

    public override async Task Update_associate_to_inline()
    {
        await base.Update_associate_to_inline();

        AssertExecuteUpdateSql(
            """
@complex_type_p_Id='?' (DbType = Int32)
@complex_type_p_Int='?' (DbType = Int32)
@complex_type_p_Ints='?' (DbType = Object)
@complex_type_p_Name='?'
@complex_type_p_String='?'
@complex_type_p_RequiredNestedAssociate_Id='?' (DbType = Int32)
@complex_type_p_RequiredNestedAssociate_Int='?' (DbType = Int32)
@complex_type_p_RequiredNestedAssociate_Ints='?' (DbType = Object)
@complex_type_p_RequiredNestedAssociate_Name='?'
@complex_type_p_RequiredNestedAssociate_String='?'

UPDATE "RootEntity" AS r
SET "RequiredAssociate_Id" = @complex_type_p_Id,
    "RequiredAssociate_Int" = @complex_type_p_Int,
    "RequiredAssociate_Ints" = @complex_type_p_Ints,
    "RequiredAssociate_Name" = @complex_type_p_Name,
    "RequiredAssociate_String" = @complex_type_p_String,
    "RequiredAssociate_OptionalNestedAssociate_Id" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_Int" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_Ints" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_Name" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_String" = NULL,
    "RequiredAssociate_RequiredNestedAssociate_Id" = @complex_type_p_RequiredNestedAssociate_Id,
    "RequiredAssociate_RequiredNestedAssociate_Int" = @complex_type_p_RequiredNestedAssociate_Int,
    "RequiredAssociate_RequiredNestedAssociate_Ints" = @complex_type_p_RequiredNestedAssociate_Ints,
    "RequiredAssociate_RequiredNestedAssociate_Name" = @complex_type_p_RequiredNestedAssociate_Name,
    "RequiredAssociate_RequiredNestedAssociate_String" = @complex_type_p_RequiredNestedAssociate_String
""");
    }

    public override async Task Update_associate_to_inline_with_lambda()
    {
        await base.Update_associate_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate_Id" = 1000,
    "RequiredAssociate_Int" = 70,
    "RequiredAssociate_Ints" = ARRAY[1,2,4]::integer[],
    "RequiredAssociate_Name" = 'Updated associate name',
    "RequiredAssociate_String" = 'Updated associate string',
    "RequiredAssociate_OptionalNestedAssociate_Id" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_Int" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_Ints" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_Name" = NULL,
    "RequiredAssociate_OptionalNestedAssociate_String" = NULL,
    "RequiredAssociate_RequiredNestedAssociate_Id" = 1000,
    "RequiredAssociate_RequiredNestedAssociate_Int" = 80,
    "RequiredAssociate_RequiredNestedAssociate_Ints" = ARRAY[1,2,4]::integer[],
    "RequiredAssociate_RequiredNestedAssociate_Name" = 'Updated nested name',
    "RequiredAssociate_RequiredNestedAssociate_String" = 'Updated nested string'
""");
    }

    public override async Task Update_nested_associate_to_inline_with_lambda()
    {
        await base.Update_nested_associate_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate_RequiredNestedAssociate_Id" = 1000,
    "RequiredAssociate_RequiredNestedAssociate_Int" = 80,
    "RequiredAssociate_RequiredNestedAssociate_Ints" = ARRAY[1,2,4]::integer[],
    "RequiredAssociate_RequiredNestedAssociate_Name" = 'Updated nested name',
    "RequiredAssociate_RequiredNestedAssociate_String" = 'Updated nested string'
""");
    }

    public override async Task Update_associate_to_null()
    {
        await base.Update_associate_to_null();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalAssociate_Id" = NULL,
    "OptionalAssociate_Int" = NULL,
    "OptionalAssociate_Ints" = NULL,
    "OptionalAssociate_Name" = NULL,
    "OptionalAssociate_String" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Id" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Int" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Ints" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Name" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_String" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Id" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Int" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Ints" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Name" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_String" = NULL
""");
    }

    public override async Task Update_associate_to_null_with_lambda()
    {
        await base.Update_associate_to_null_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalAssociate_Id" = NULL,
    "OptionalAssociate_Int" = NULL,
    "OptionalAssociate_Ints" = NULL,
    "OptionalAssociate_Name" = NULL,
    "OptionalAssociate_String" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Id" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Int" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Ints" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Name" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_String" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Id" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Int" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Ints" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Name" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_String" = NULL
""");
    }

    public override async Task Update_associate_to_null_parameter()
    {
        await base.Update_associate_to_null_parameter();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalAssociate_Id" = NULL,
    "OptionalAssociate_Int" = NULL,
    "OptionalAssociate_Ints" = NULL,
    "OptionalAssociate_Name" = NULL,
    "OptionalAssociate_String" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Id" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Int" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Ints" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_Name" = NULL,
    "OptionalAssociate_OptionalNestedAssociate_String" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Id" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Int" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Ints" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_Name" = NULL,
    "OptionalAssociate_RequiredNestedAssociate_String" = NULL
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

        AssertExecuteUpdateSql();
    }

    public override async Task Update_nested_collection_to_parameter()
    {
        await base.Update_nested_collection_to_parameter();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_nested_collection_to_inline_with_lambda()
    {
        await base.Update_nested_collection_to_inline_with_lambda();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_nested_collection_to_another_nested_collection()
    {
        await base.Update_nested_collection_to_another_nested_collection();

        AssertExecuteUpdateSql();
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
SET "RequiredAssociate_Ints" = ARRAY[1,2,4]::integer[]
""");
    }

    public override async Task Update_primitive_collection_to_parameter()
    {
        await base.Update_primitive_collection_to_parameter();

        AssertExecuteUpdateSql(
            """
@ints='?' (DbType = Object)

UPDATE "RootEntity" AS r
SET "RequiredAssociate_Ints" = @ints
""");
    }

    public override async Task Update_primitive_collection_to_another_collection()
    {
        await base.Update_primitive_collection_to_another_collection();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredAssociate_OptionalNestedAssociate_Ints" = r."RequiredAssociate_RequiredNestedAssociate_Ints"
""");
    }

    public override async Task Update_inside_primitive_collection()
    {
        // #3622, Support updating an element in an array with ExecuteUpdate
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Update_inside_primitive_collection());

        AssertExecuteUpdateSql();
    }

    #endregion Update primitive collection

    #region Multiple updates

    public override async Task Update_multiple_properties_inside_same_associate()
    {
        await base.Update_multiple_properties_inside_same_associate();

        AssertExecuteUpdateSql(
            """
@p='?'
@p0='?' (DbType = Int32)

UPDATE "RootEntity" AS r
SET "RequiredAssociate_String" = @p,
    "RequiredAssociate_Int" = @p0
""");
    }

    public override async Task Update_multiple_properties_inside_associates_and_on_entity_type()
    {
        await base.Update_multiple_properties_inside_associates_and_on_entity_type();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "Name" = r."Name" || 'Modified',
    "RequiredAssociate_String" = r."OptionalAssociate_String",
    "OptionalAssociate_RequiredNestedAssociate_String" = @p
WHERE r."OptionalAssociate_Id" IS NOT NULL
""");
    }

    public override async Task Update_multiple_projected_associates_via_anonymous_type()
    {
        await base.Update_multiple_projected_associates_via_anonymous_type();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredAssociate_String" = r."OptionalAssociate_String",
    "OptionalAssociate_String" = @p
WHERE r."OptionalAssociate_Id" IS NOT NULL
""");
    }

    #endregion Multiple updates

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
