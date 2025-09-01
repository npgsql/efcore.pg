
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
SET "RequiredRelated_String" = @p
""");
    }

    public override async Task Update_property_inside_association_with_special_chars()
    {
        await base.Update_property_inside_association_with_special_chars();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated_String" = '{ Some other/JSON:like text though it [isn''t]: ממש ממש לאéèéè }'
WHERE r."RequiredRelated_String" = '{ this may/look:like JSON but it [isn''t]: ממש ממש לאéèéè }'
""");
    }

    public override async Task Update_property_inside_nested()
    {
        await base.Update_property_inside_nested();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredRelated_RequiredNested_String" = @p
""");
    }

    public override async Task Update_property_on_projected_association()
    {
        await base.Update_property_on_projected_association();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredRelated_String" = @p
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
@complex_type_p_Id='?' (DbType = Int32)
@complex_type_p_Int='?' (DbType = Int32)
@complex_type_p_Name='?'
@complex_type_p_String='?'

UPDATE "RootEntity" AS r
SET "RequiredRelated_Id" = @complex_type_p_Id,
    "RequiredRelated_Int" = @complex_type_p_Int,
    "RequiredRelated_Name" = @complex_type_p_Name,
    "RequiredRelated_String" = @complex_type_p_String,
    "RequiredRelated_OptionalNested_Id" = @complex_type_p_Id,
    "RequiredRelated_OptionalNested_Int" = @complex_type_p_Int,
    "RequiredRelated_OptionalNested_Name" = @complex_type_p_Name,
    "RequiredRelated_OptionalNested_String" = @complex_type_p_String,
    "RequiredRelated_RequiredNested_Id" = @complex_type_p_Id,
    "RequiredRelated_RequiredNested_Int" = @complex_type_p_Int,
    "RequiredRelated_RequiredNested_Name" = @complex_type_p_Name,
    "RequiredRelated_RequiredNested_String" = @complex_type_p_String
""");
    }

    public override async Task Update_nested_association_to_parameter()
    {
        await base.Update_nested_association_to_parameter();

        AssertExecuteUpdateSql(
            """
@complex_type_p_Id='?' (DbType = Int32)
@complex_type_p_Int='?' (DbType = Int32)
@complex_type_p_Name='?'
@complex_type_p_String='?'

UPDATE "RootEntity" AS r
SET "RequiredRelated_RequiredNested_Id" = @complex_type_p_Id,
    "RequiredRelated_RequiredNested_Int" = @complex_type_p_Int,
    "RequiredRelated_RequiredNested_Name" = @complex_type_p_Name,
    "RequiredRelated_RequiredNested_String" = @complex_type_p_String
""");
    }

    public override async Task Update_association_to_another_association()
    {
        await base.Update_association_to_another_association();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalRelated_Id" = r."RequiredRelated_Id",
    "OptionalRelated_Int" = r."RequiredRelated_Int",
    "OptionalRelated_Name" = r."RequiredRelated_Name",
    "OptionalRelated_String" = r."RequiredRelated_String",
    "OptionalRelated_OptionalNested_Id" = r."OptionalRelated_OptionalNested_Id",
    "OptionalRelated_OptionalNested_Int" = r."OptionalRelated_OptionalNested_Int",
    "OptionalRelated_OptionalNested_Name" = r."OptionalRelated_OptionalNested_Name",
    "OptionalRelated_OptionalNested_String" = r."OptionalRelated_OptionalNested_String",
    "OptionalRelated_RequiredNested_Id" = r."OptionalRelated_RequiredNested_Id",
    "OptionalRelated_RequiredNested_Int" = r."OptionalRelated_RequiredNested_Int",
    "OptionalRelated_RequiredNested_Name" = r."OptionalRelated_RequiredNested_Name",
    "OptionalRelated_RequiredNested_String" = r."OptionalRelated_RequiredNested_String"
""");
    }

    public override async Task Update_nested_association_to_another_nested_association()
    {
        await base.Update_nested_association_to_another_nested_association();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated_OptionalNested_Id" = r."RequiredRelated_RequiredNested_Id",
    "RequiredRelated_OptionalNested_Int" = r."RequiredRelated_RequiredNested_Int",
    "RequiredRelated_OptionalNested_Name" = r."RequiredRelated_RequiredNested_Name",
    "RequiredRelated_OptionalNested_String" = r."RequiredRelated_RequiredNested_String"
""");
    }

    public override async Task Update_association_to_inline()
    {
        await base.Update_association_to_inline();

        AssertExecuteUpdateSql(
            """
@complex_type_p_Id='?' (DbType = Int32)
@complex_type_p_Int='?' (DbType = Int32)
@complex_type_p_Name='?'
@complex_type_p_String='?'

UPDATE "RootEntity" AS r
SET "RequiredRelated_Id" = @complex_type_p_Id,
    "RequiredRelated_Int" = @complex_type_p_Int,
    "RequiredRelated_Name" = @complex_type_p_Name,
    "RequiredRelated_String" = @complex_type_p_String,
    "RequiredRelated_OptionalNested_Id" = @complex_type_p_Id,
    "RequiredRelated_OptionalNested_Int" = @complex_type_p_Int,
    "RequiredRelated_OptionalNested_Name" = @complex_type_p_Name,
    "RequiredRelated_OptionalNested_String" = @complex_type_p_String,
    "RequiredRelated_RequiredNested_Id" = @complex_type_p_Id,
    "RequiredRelated_RequiredNested_Int" = @complex_type_p_Int,
    "RequiredRelated_RequiredNested_Name" = @complex_type_p_Name,
    "RequiredRelated_RequiredNested_String" = @complex_type_p_String
""");
    }

    public override async Task Update_association_to_inline_with_lambda()
    {
        await base.Update_association_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated_Id" = 0,
    "RequiredRelated_Int" = 70,
    "RequiredRelated_Name" = 'Updated related name',
    "RequiredRelated_String" = 'Updated related string',
    "RequiredRelated_OptionalNested_Id" = NULL,
    "RequiredRelated_OptionalNested_Int" = NULL,
    "RequiredRelated_OptionalNested_Name" = NULL,
    "RequiredRelated_OptionalNested_String" = NULL,
    "RequiredRelated_RequiredNested_Id" = 0,
    "RequiredRelated_RequiredNested_Int" = 80,
    "RequiredRelated_RequiredNested_Name" = 'Updated nested name',
    "RequiredRelated_RequiredNested_String" = 'Updated nested string'
""");
    }

    public override async Task Update_nested_association_to_inline_with_lambda()
    {
        await base.Update_nested_association_to_inline_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "RequiredRelated_RequiredNested_Id" = 0,
    "RequiredRelated_RequiredNested_Int" = 80,
    "RequiredRelated_RequiredNested_Name" = 'Updated nested name',
    "RequiredRelated_RequiredNested_String" = 'Updated nested string'
""");
    }

    public override async Task Update_association_to_null()
    {
        await base.Update_association_to_null();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalRelated_Id" = NULL,
    "OptionalRelated_Int" = NULL,
    "OptionalRelated_Name" = NULL,
    "OptionalRelated_String" = NULL,
    "OptionalRelated_OptionalNested_Id" = NULL,
    "OptionalRelated_OptionalNested_Int" = NULL,
    "OptionalRelated_OptionalNested_Name" = NULL,
    "OptionalRelated_OptionalNested_String" = NULL,
    "OptionalRelated_RequiredNested_Id" = NULL,
    "OptionalRelated_RequiredNested_Int" = NULL,
    "OptionalRelated_RequiredNested_Name" = NULL,
    "OptionalRelated_RequiredNested_String" = NULL
""");
    }

    public override async Task Update_association_to_null_with_lambda()
    {
        await base.Update_association_to_null_with_lambda();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalRelated_Id" = NULL,
    "OptionalRelated_Int" = NULL,
    "OptionalRelated_Name" = NULL,
    "OptionalRelated_String" = NULL,
    "OptionalRelated_OptionalNested_Id" = NULL,
    "OptionalRelated_OptionalNested_Int" = NULL,
    "OptionalRelated_OptionalNested_Name" = NULL,
    "OptionalRelated_OptionalNested_String" = NULL,
    "OptionalRelated_RequiredNested_Id" = NULL,
    "OptionalRelated_RequiredNested_Int" = NULL,
    "OptionalRelated_RequiredNested_Name" = NULL,
    "OptionalRelated_RequiredNested_String" = NULL
""");
    }

    public override async Task Update_association_to_null_parameter()
    {
        await base.Update_association_to_null_parameter();

        AssertExecuteUpdateSql(
            """
UPDATE "RootEntity" AS r
SET "OptionalRelated_Id" = NULL,
    "OptionalRelated_Int" = NULL,
    "OptionalRelated_Name" = NULL,
    "OptionalRelated_String" = NULL,
    "OptionalRelated_OptionalNested_Id" = NULL,
    "OptionalRelated_OptionalNested_Int" = NULL,
    "OptionalRelated_OptionalNested_Name" = NULL,
    "OptionalRelated_OptionalNested_String" = NULL,
    "OptionalRelated_RequiredNested_Id" = NULL,
    "OptionalRelated_RequiredNested_Int" = NULL,
    "OptionalRelated_RequiredNested_Name" = NULL,
    "OptionalRelated_RequiredNested_String" = NULL
""");
    }

    #endregion Update association

    #region Update collection

    public override async Task Update_collection_to_parameter()
    {
        await base.Update_collection_to_parameter();

        AssertExecuteUpdateSql(
);
    }

    public override async Task Update_nested_collection_to_parameter()
    {
        await base.Update_nested_collection_to_parameter();

        AssertExecuteUpdateSql(
);
    }

    public override async Task Update_nested_collection_to_inline_with_lambda()
    {
        await base.Update_nested_collection_to_inline_with_lambda();

        AssertExecuteUpdateSql(
);
    }

    public override async Task Update_nested_collection_to_another_nested_collection()
    {
        await base.Update_nested_collection_to_another_nested_collection();

        AssertExecuteUpdateSql(
);
    }

    public override async Task Update_collection_referencing_the_original_collection()
    {
        await base.Update_collection_referencing_the_original_collection();

        AssertExecuteUpdateSql();
    }

    #endregion Update collection

    #region Multiple updates

    public override async Task Update_multiple_properties_inside_same_association()
    {
        await base.Update_multiple_properties_inside_same_association();

        // Note that since two properties within the same JSON column are updated, SQL Server 2025 modify
        // is not used (it only supports modifying a single property)
        AssertExecuteUpdateSql(
            """
@p='?'
@p0='?' (DbType = Int32)

UPDATE "RootEntity" AS r
SET "RequiredRelated_String" = @p,
    "RequiredRelated_Int" = @p0
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
    "RequiredRelated_String" = r."OptionalRelated_String",
    "OptionalRelated_RequiredNested_String" = @p
WHERE r."OptionalRelated_Id" IS NOT NULL
""");
    }

    public override async Task Update_multiple_projected_associations_via_anonymous_type()
    {
        await base.Update_multiple_projected_associations_via_anonymous_type();

        AssertExecuteUpdateSql(
            """
@p='?'

UPDATE "RootEntity" AS r
SET "RequiredRelated_String" = r."OptionalRelated_String",
    "OptionalRelated_String" = @p
WHERE r."OptionalRelated_Id" IS NOT NULL
""");
    }

    #endregion Multiple updates

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
