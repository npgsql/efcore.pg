namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class ComplexTypeBulkUpdatesNpgsqlTest(
    ComplexTypeBulkUpdatesNpgsqlTest.ComplexTypeBulkUpdatesNpgsqlFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTypeBulkUpdatesRelationalTestBase<ComplexTypeBulkUpdatesNpgsqlTest.ComplexTypeBulkUpdatesNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Delete_entity_type_with_complex_type(bool async)
    {
        await base.Delete_entity_type_with_complex_type(async);

        AssertSql(
            """
DELETE FROM "Customer" AS c
WHERE c."Name" = 'Monty Elias'
""");
    }

    public override async Task Delete_complex_type(bool async)
    {
        await base.Delete_complex_type(async);

        AssertSql();
    }

    public override async Task Update_property_inside_complex_type(bool async)
    {
        await base.Update_property_inside_complex_type(async);

        AssertExecuteUpdateSql(
            """
@p='12345'

UPDATE "Customer" AS c
SET "ShippingAddress_ZipCode" = @p
WHERE c."ShippingAddress_ZipCode" = 7728
""");
    }

    public override async Task Update_property_inside_nested_complex_type(bool async)
    {
        await base.Update_property_inside_nested_complex_type(async);

        AssertExecuteUpdateSql(
            """
@p='United States Modified'

UPDATE "Customer" AS c
SET "ShippingAddress_Country_FullName" = @p
WHERE c."ShippingAddress_Country_Code" = 'US'
""");
    }

    public override async Task Update_multiple_properties_inside_multiple_complex_types_and_on_entity_type(bool async)
    {
        await base.Update_multiple_properties_inside_multiple_complex_types_and_on_entity_type(async);

        AssertExecuteUpdateSql(
            """
@p='54321'

UPDATE "Customer" AS c
SET "Name" = c."Name" || 'Modified',
    "ShippingAddress_ZipCode" = c."BillingAddress_ZipCode",
    "BillingAddress_ZipCode" = @p
WHERE c."ShippingAddress_ZipCode" = 7728
""");
    }

    public override async Task Update_projected_complex_type(bool async)
    {
        await base.Update_projected_complex_type(async);

        AssertExecuteUpdateSql(
            """
@p='12345'

UPDATE "Customer" AS c
SET "ShippingAddress_ZipCode" = @p
""");
    }

    public override async Task Update_multiple_projected_complex_types_via_anonymous_type(bool async)
    {
        await base.Update_multiple_projected_complex_types_via_anonymous_type(async);

        AssertExecuteUpdateSql(
            """
@p='54321'

UPDATE "Customer" AS c
SET "ShippingAddress_ZipCode" = c."BillingAddress_ZipCode",
    "BillingAddress_ZipCode" = @p
""");
    }

    public override async Task Update_projected_complex_type_via_OrderBy_Skip(bool async)
    {
        await base.Update_projected_complex_type_via_OrderBy_Skip(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_complex_type_to_parameter(bool async)
    {
        await base.Update_complex_type_to_parameter(async);

        AssertExecuteUpdateSql(
            """
@complex_type_p_AddressLine1='New AddressLine1'
@complex_type_p_AddressLine2='New AddressLine2'
@complex_type_p_Tags={ 'new_tag1', 'new_tag2' } (DbType = Object)
@complex_type_p_ZipCode='99999' (Nullable = true)
@complex_type_p_Code='FR'
@complex_type_p_FullName='France'

UPDATE "Customer" AS c
SET "ShippingAddress_AddressLine1" = @complex_type_p_AddressLine1,
    "ShippingAddress_AddressLine2" = @complex_type_p_AddressLine2,
    "ShippingAddress_Tags" = @complex_type_p_Tags,
    "ShippingAddress_ZipCode" = @complex_type_p_ZipCode,
    "ShippingAddress_Country_Code" = @complex_type_p_Code,
    "ShippingAddress_Country_FullName" = @complex_type_p_FullName
""");
    }

    public override async Task Update_nested_complex_type_to_parameter(bool async)
    {
        await base.Update_nested_complex_type_to_parameter(async);

        AssertExecuteUpdateSql(
            """
@complex_type_p_Code='FR'
@complex_type_p_FullName='France'

UPDATE "Customer" AS c
SET "ShippingAddress_Country_Code" = @complex_type_p_Code,
    "ShippingAddress_Country_FullName" = @complex_type_p_FullName
""");
    }

    public override async Task Update_complex_type_to_another_database_complex_type(bool async)
    {
        await base.Update_complex_type_to_another_database_complex_type(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Customer" AS c
SET "ShippingAddress_AddressLine1" = c."BillingAddress_AddressLine1",
    "ShippingAddress_AddressLine2" = c."BillingAddress_AddressLine2",
    "ShippingAddress_Tags" = c."BillingAddress_Tags",
    "ShippingAddress_ZipCode" = c."BillingAddress_ZipCode",
    "ShippingAddress_Country_Code" = c."ShippingAddress_Country_Code",
    "ShippingAddress_Country_FullName" = c."ShippingAddress_Country_FullName"
""");
    }

    public override async Task Update_complex_type_to_inline_without_lambda(bool async)
    {
        await base.Update_complex_type_to_inline_without_lambda(async);

        AssertExecuteUpdateSql(
            """
@complex_type_p_AddressLine1='New AddressLine1'
@complex_type_p_AddressLine2='New AddressLine2'
@complex_type_p_Tags={ 'new_tag1', 'new_tag2' } (DbType = Object)
@complex_type_p_ZipCode='99999' (Nullable = true)
@complex_type_p_Code='FR'
@complex_type_p_FullName='France'

UPDATE "Customer" AS c
SET "ShippingAddress_AddressLine1" = @complex_type_p_AddressLine1,
    "ShippingAddress_AddressLine2" = @complex_type_p_AddressLine2,
    "ShippingAddress_Tags" = @complex_type_p_Tags,
    "ShippingAddress_ZipCode" = @complex_type_p_ZipCode,
    "ShippingAddress_Country_Code" = @complex_type_p_Code,
    "ShippingAddress_Country_FullName" = @complex_type_p_FullName
""");
    }

    public override async Task Update_complex_type_to_inline_with_lambda(bool async)
    {
        await base.Update_complex_type_to_inline_with_lambda(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Customer" AS c
SET "ShippingAddress_AddressLine1" = 'New AddressLine1',
    "ShippingAddress_AddressLine2" = 'New AddressLine2',
    "ShippingAddress_Tags" = ARRAY['new_tag1','new_tag2']::text[],
    "ShippingAddress_ZipCode" = 99999,
    "ShippingAddress_Country_Code" = 'FR',
    "ShippingAddress_Country_FullName" = 'France'
""");
    }

    public override async Task Update_complex_type_to_another_database_complex_type_with_subquery(bool async)
    {
        await base.Update_complex_type_to_another_database_complex_type_with_subquery(async);

        AssertExecuteUpdateSql(
            """
@p='1'

UPDATE "Customer" AS c0
SET "ShippingAddress_AddressLine1" = c1."BillingAddress_AddressLine1",
    "ShippingAddress_AddressLine2" = c1."BillingAddress_AddressLine2",
    "ShippingAddress_Tags" = c1."BillingAddress_Tags",
    "ShippingAddress_ZipCode" = c1."BillingAddress_ZipCode",
    "ShippingAddress_Country_Code" = c1."ShippingAddress_Country_Code",
    "ShippingAddress_Country_FullName" = c1."ShippingAddress_Country_FullName"
FROM (
    SELECT c."Id", c."BillingAddress_AddressLine1", c."BillingAddress_AddressLine2", c."BillingAddress_Tags", c."BillingAddress_ZipCode", c."ShippingAddress_Country_Code", c."ShippingAddress_Country_FullName"
    FROM "Customer" AS c
    ORDER BY c."Id" NULLS FIRST
    OFFSET @p
) AS c1
WHERE c0."Id" = c1."Id"
""");
    }

    public override async Task Update_collection_inside_complex_type(bool async)
    {
        await base.Update_collection_inside_complex_type(async);

        AssertExecuteUpdateSql(
            """
@p={ 'new_tag1', 'new_tag2' } (DbType = Object)

UPDATE "Customer" AS c
SET "ShippingAddress_Tags" = @p
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    public class ComplexTypeBulkUpdatesNpgsqlFixture : ComplexTypeBulkUpdatesRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
