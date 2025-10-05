namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingPrimitiveCollectionNpgsqlTest(ComplexTableSplittingNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingPrimitiveCollectionRelationalTestBase<ComplexTableSplittingNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalAssociate_Id", r."OptionalAssociate_Int", r."OptionalAssociate_Ints", r."OptionalAssociate_Name", r."OptionalAssociate_String", r."OptionalAssociate_OptionalNestedAssociate_Id", r."OptionalAssociate_OptionalNestedAssociate_Int", r."OptionalAssociate_OptionalNestedAssociate_Ints", r."OptionalAssociate_OptionalNestedAssociate_Name", r."OptionalAssociate_OptionalNestedAssociate_String", r."OptionalAssociate_RequiredNestedAssociate_Id", r."OptionalAssociate_RequiredNestedAssociate_Int", r."OptionalAssociate_RequiredNestedAssociate_Ints", r."OptionalAssociate_RequiredNestedAssociate_Name", r."OptionalAssociate_RequiredNestedAssociate_String", r."RequiredAssociate_Id", r."RequiredAssociate_Int", r."RequiredAssociate_Ints", r."RequiredAssociate_Name", r."RequiredAssociate_String", r."RequiredAssociate_OptionalNestedAssociate_Id", r."RequiredAssociate_OptionalNestedAssociate_Int", r."RequiredAssociate_OptionalNestedAssociate_Ints", r."RequiredAssociate_OptionalNestedAssociate_Name", r."RequiredAssociate_OptionalNestedAssociate_String", r."RequiredAssociate_RequiredNestedAssociate_Id", r."RequiredAssociate_RequiredNestedAssociate_Int", r."RequiredAssociate_RequiredNestedAssociate_Ints", r."RequiredAssociate_RequiredNestedAssociate_Name", r."RequiredAssociate_RequiredNestedAssociate_String"
FROM "RootEntity" AS r
WHERE cardinality(r."RequiredAssociate_Ints") = 3
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalAssociate_Id", r."OptionalAssociate_Int", r."OptionalAssociate_Ints", r."OptionalAssociate_Name", r."OptionalAssociate_String", r."OptionalAssociate_OptionalNestedAssociate_Id", r."OptionalAssociate_OptionalNestedAssociate_Int", r."OptionalAssociate_OptionalNestedAssociate_Ints", r."OptionalAssociate_OptionalNestedAssociate_Name", r."OptionalAssociate_OptionalNestedAssociate_String", r."OptionalAssociate_RequiredNestedAssociate_Id", r."OptionalAssociate_RequiredNestedAssociate_Int", r."OptionalAssociate_RequiredNestedAssociate_Ints", r."OptionalAssociate_RequiredNestedAssociate_Name", r."OptionalAssociate_RequiredNestedAssociate_String", r."RequiredAssociate_Id", r."RequiredAssociate_Int", r."RequiredAssociate_Ints", r."RequiredAssociate_Name", r."RequiredAssociate_String", r."RequiredAssociate_OptionalNestedAssociate_Id", r."RequiredAssociate_OptionalNestedAssociate_Int", r."RequiredAssociate_OptionalNestedAssociate_Ints", r."RequiredAssociate_OptionalNestedAssociate_Name", r."RequiredAssociate_OptionalNestedAssociate_String", r."RequiredAssociate_RequiredNestedAssociate_Id", r."RequiredAssociate_RequiredNestedAssociate_Int", r."RequiredAssociate_RequiredNestedAssociate_Ints", r."RequiredAssociate_RequiredNestedAssociate_Name", r."RequiredAssociate_RequiredNestedAssociate_String"
FROM "RootEntity" AS r
WHERE r."RequiredAssociate_Ints"[1] = 1
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalAssociate_Id", r."OptionalAssociate_Int", r."OptionalAssociate_Ints", r."OptionalAssociate_Name", r."OptionalAssociate_String", r."OptionalAssociate_OptionalNestedAssociate_Id", r."OptionalAssociate_OptionalNestedAssociate_Int", r."OptionalAssociate_OptionalNestedAssociate_Ints", r."OptionalAssociate_OptionalNestedAssociate_Name", r."OptionalAssociate_OptionalNestedAssociate_String", r."OptionalAssociate_RequiredNestedAssociate_Id", r."OptionalAssociate_RequiredNestedAssociate_Int", r."OptionalAssociate_RequiredNestedAssociate_Ints", r."OptionalAssociate_RequiredNestedAssociate_Name", r."OptionalAssociate_RequiredNestedAssociate_String", r."RequiredAssociate_Id", r."RequiredAssociate_Int", r."RequiredAssociate_Ints", r."RequiredAssociate_Name", r."RequiredAssociate_String", r."RequiredAssociate_OptionalNestedAssociate_Id", r."RequiredAssociate_OptionalNestedAssociate_Int", r."RequiredAssociate_OptionalNestedAssociate_Ints", r."RequiredAssociate_OptionalNestedAssociate_Name", r."RequiredAssociate_OptionalNestedAssociate_String", r."RequiredAssociate_RequiredNestedAssociate_Id", r."RequiredAssociate_RequiredNestedAssociate_Int", r."RequiredAssociate_RequiredNestedAssociate_Ints", r."RequiredAssociate_RequiredNestedAssociate_Name", r."RequiredAssociate_RequiredNestedAssociate_String"
FROM "RootEntity" AS r
WHERE 3 = ANY (r."RequiredAssociate_Ints")
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalAssociate_Id", r."OptionalAssociate_Int", r."OptionalAssociate_Ints", r."OptionalAssociate_Name", r."OptionalAssociate_String", r."OptionalAssociate_OptionalNestedAssociate_Id", r."OptionalAssociate_OptionalNestedAssociate_Int", r."OptionalAssociate_OptionalNestedAssociate_Ints", r."OptionalAssociate_OptionalNestedAssociate_Name", r."OptionalAssociate_OptionalNestedAssociate_String", r."OptionalAssociate_RequiredNestedAssociate_Id", r."OptionalAssociate_RequiredNestedAssociate_Int", r."OptionalAssociate_RequiredNestedAssociate_Ints", r."OptionalAssociate_RequiredNestedAssociate_Name", r."OptionalAssociate_RequiredNestedAssociate_String", r."RequiredAssociate_Id", r."RequiredAssociate_Int", r."RequiredAssociate_Ints", r."RequiredAssociate_Name", r."RequiredAssociate_String", r."RequiredAssociate_OptionalNestedAssociate_Id", r."RequiredAssociate_OptionalNestedAssociate_Int", r."RequiredAssociate_OptionalNestedAssociate_Ints", r."RequiredAssociate_OptionalNestedAssociate_Name", r."RequiredAssociate_OptionalNestedAssociate_String", r."RequiredAssociate_RequiredNestedAssociate_Id", r."RequiredAssociate_RequiredNestedAssociate_Int", r."RequiredAssociate_RequiredNestedAssociate_Ints", r."RequiredAssociate_RequiredNestedAssociate_Name", r."RequiredAssociate_RequiredNestedAssociate_String"
FROM "RootEntity" AS r
WHERE 2 = ANY (r."RequiredAssociate_Ints")
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalAssociate_Id", r."OptionalAssociate_Int", r."OptionalAssociate_Ints", r."OptionalAssociate_Name", r."OptionalAssociate_String", r."OptionalAssociate_OptionalNestedAssociate_Id", r."OptionalAssociate_OptionalNestedAssociate_Int", r."OptionalAssociate_OptionalNestedAssociate_Ints", r."OptionalAssociate_OptionalNestedAssociate_Name", r."OptionalAssociate_OptionalNestedAssociate_String", r."OptionalAssociate_RequiredNestedAssociate_Id", r."OptionalAssociate_RequiredNestedAssociate_Int", r."OptionalAssociate_RequiredNestedAssociate_Ints", r."OptionalAssociate_RequiredNestedAssociate_Name", r."OptionalAssociate_RequiredNestedAssociate_String", r."RequiredAssociate_Id", r."RequiredAssociate_Int", r."RequiredAssociate_Ints", r."RequiredAssociate_Name", r."RequiredAssociate_String", r."RequiredAssociate_OptionalNestedAssociate_Id", r."RequiredAssociate_OptionalNestedAssociate_Int", r."RequiredAssociate_OptionalNestedAssociate_Ints", r."RequiredAssociate_OptionalNestedAssociate_Name", r."RequiredAssociate_OptionalNestedAssociate_String", r."RequiredAssociate_RequiredNestedAssociate_Id", r."RequiredAssociate_RequiredNestedAssociate_Int", r."RequiredAssociate_RequiredNestedAssociate_Ints", r."RequiredAssociate_RequiredNestedAssociate_Name", r."RequiredAssociate_RequiredNestedAssociate_String"
FROM "RootEntity" AS r
WHERE cardinality(r."RequiredAssociate_RequiredNestedAssociate_Ints") = 3
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(sum(r1.value), 0)::int
    FROM unnest(r."RequiredAssociate_Ints") AS r1(value))
FROM "RootEntity" AS r
WHERE (
    SELECT COALESCE(sum(r0.value), 0)::int
    FROM unnest(r."RequiredAssociate_Ints") AS r0(value)) >= 6
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
