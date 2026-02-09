namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocComplexTypeQueryNpgsqlTest(NonSharedFixture fixture) : AdHocComplexTypeQueryTestBase(fixture)
{
    public override async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        await base.Complex_type_equals_parameter_with_nested_types_with_property_of_same_name();

        AssertSql(
            """
@entity_equality_container_Id='1' (Nullable = true)
@entity_equality_container_Containee1_Id='2' (Nullable = true)
@entity_equality_container_Containee2_Id='3' (Nullable = true)

SELECT e."Id", e."ComplexContainer_Id", e."ComplexContainer_Containee1_Id", e."ComplexContainer_Containee2_Id"
FROM "EntityType" AS e
WHERE e."ComplexContainer_Id" = @entity_equality_container_Id AND e."ComplexContainer_Containee1_Id" = @entity_equality_container_Containee1_Id AND e."ComplexContainer_Containee2_Id" = @entity_equality_container_Containee2_Id
LIMIT 2
""");
    }

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
