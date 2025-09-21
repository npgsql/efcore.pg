namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingPrimitiveCollectionNpgsqlTest(ComplexTableSplittingNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingPrimitiveCollectionRelationalTestBase<ComplexTableSplittingNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated_Id", r."OptionalRelated_Int", r."OptionalRelated_Ints", r."OptionalRelated_Name", r."OptionalRelated_String", r."OptionalRelated_OptionalNested_Id", r."OptionalRelated_OptionalNested_Int", r."OptionalRelated_OptionalNested_Ints", r."OptionalRelated_OptionalNested_Name", r."OptionalRelated_OptionalNested_String", r."OptionalRelated_RequiredNested_Id", r."OptionalRelated_RequiredNested_Int", r."OptionalRelated_RequiredNested_Ints", r."OptionalRelated_RequiredNested_Name", r."OptionalRelated_RequiredNested_String", r."RequiredRelated_Id", r."RequiredRelated_Int", r."RequiredRelated_Ints", r."RequiredRelated_Name", r."RequiredRelated_String", r."RequiredRelated_OptionalNested_Id", r."RequiredRelated_OptionalNested_Int", r."RequiredRelated_OptionalNested_Ints", r."RequiredRelated_OptionalNested_Name", r."RequiredRelated_OptionalNested_String", r."RequiredRelated_RequiredNested_Id", r."RequiredRelated_RequiredNested_Int", r."RequiredRelated_RequiredNested_Ints", r."RequiredRelated_RequiredNested_Name", r."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS r
WHERE cardinality(r."RequiredRelated_Ints") = 3
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated_Id", r."OptionalRelated_Int", r."OptionalRelated_Ints", r."OptionalRelated_Name", r."OptionalRelated_String", r."OptionalRelated_OptionalNested_Id", r."OptionalRelated_OptionalNested_Int", r."OptionalRelated_OptionalNested_Ints", r."OptionalRelated_OptionalNested_Name", r."OptionalRelated_OptionalNested_String", r."OptionalRelated_RequiredNested_Id", r."OptionalRelated_RequiredNested_Int", r."OptionalRelated_RequiredNested_Ints", r."OptionalRelated_RequiredNested_Name", r."OptionalRelated_RequiredNested_String", r."RequiredRelated_Id", r."RequiredRelated_Int", r."RequiredRelated_Ints", r."RequiredRelated_Name", r."RequiredRelated_String", r."RequiredRelated_OptionalNested_Id", r."RequiredRelated_OptionalNested_Int", r."RequiredRelated_OptionalNested_Ints", r."RequiredRelated_OptionalNested_Name", r."RequiredRelated_OptionalNested_String", r."RequiredRelated_RequiredNested_Id", r."RequiredRelated_RequiredNested_Int", r."RequiredRelated_RequiredNested_Ints", r."RequiredRelated_RequiredNested_Name", r."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS r
WHERE r."RequiredRelated_Ints"[1] = 1
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated_Id", r."OptionalRelated_Int", r."OptionalRelated_Ints", r."OptionalRelated_Name", r."OptionalRelated_String", r."OptionalRelated_OptionalNested_Id", r."OptionalRelated_OptionalNested_Int", r."OptionalRelated_OptionalNested_Ints", r."OptionalRelated_OptionalNested_Name", r."OptionalRelated_OptionalNested_String", r."OptionalRelated_RequiredNested_Id", r."OptionalRelated_RequiredNested_Int", r."OptionalRelated_RequiredNested_Ints", r."OptionalRelated_RequiredNested_Name", r."OptionalRelated_RequiredNested_String", r."RequiredRelated_Id", r."RequiredRelated_Int", r."RequiredRelated_Ints", r."RequiredRelated_Name", r."RequiredRelated_String", r."RequiredRelated_OptionalNested_Id", r."RequiredRelated_OptionalNested_Int", r."RequiredRelated_OptionalNested_Ints", r."RequiredRelated_OptionalNested_Name", r."RequiredRelated_OptionalNested_String", r."RequiredRelated_RequiredNested_Id", r."RequiredRelated_RequiredNested_Int", r."RequiredRelated_RequiredNested_Ints", r."RequiredRelated_RequiredNested_Name", r."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS r
WHERE 3 = ANY (r."RequiredRelated_Ints")
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated_Id", r."OptionalRelated_Int", r."OptionalRelated_Ints", r."OptionalRelated_Name", r."OptionalRelated_String", r."OptionalRelated_OptionalNested_Id", r."OptionalRelated_OptionalNested_Int", r."OptionalRelated_OptionalNested_Ints", r."OptionalRelated_OptionalNested_Name", r."OptionalRelated_OptionalNested_String", r."OptionalRelated_RequiredNested_Id", r."OptionalRelated_RequiredNested_Int", r."OptionalRelated_RequiredNested_Ints", r."OptionalRelated_RequiredNested_Name", r."OptionalRelated_RequiredNested_String", r."RequiredRelated_Id", r."RequiredRelated_Int", r."RequiredRelated_Ints", r."RequiredRelated_Name", r."RequiredRelated_String", r."RequiredRelated_OptionalNested_Id", r."RequiredRelated_OptionalNested_Int", r."RequiredRelated_OptionalNested_Ints", r."RequiredRelated_OptionalNested_Name", r."RequiredRelated_OptionalNested_String", r."RequiredRelated_RequiredNested_Id", r."RequiredRelated_RequiredNested_Int", r."RequiredRelated_RequiredNested_Ints", r."RequiredRelated_RequiredNested_Name", r."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS r
WHERE 2 = ANY (r."RequiredRelated_Ints")
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated_Id", r."OptionalRelated_Int", r."OptionalRelated_Ints", r."OptionalRelated_Name", r."OptionalRelated_String", r."OptionalRelated_OptionalNested_Id", r."OptionalRelated_OptionalNested_Int", r."OptionalRelated_OptionalNested_Ints", r."OptionalRelated_OptionalNested_Name", r."OptionalRelated_OptionalNested_String", r."OptionalRelated_RequiredNested_Id", r."OptionalRelated_RequiredNested_Int", r."OptionalRelated_RequiredNested_Ints", r."OptionalRelated_RequiredNested_Name", r."OptionalRelated_RequiredNested_String", r."RequiredRelated_Id", r."RequiredRelated_Int", r."RequiredRelated_Ints", r."RequiredRelated_Name", r."RequiredRelated_String", r."RequiredRelated_OptionalNested_Id", r."RequiredRelated_OptionalNested_Int", r."RequiredRelated_OptionalNested_Ints", r."RequiredRelated_OptionalNested_Name", r."RequiredRelated_OptionalNested_String", r."RequiredRelated_RequiredNested_Id", r."RequiredRelated_RequiredNested_Int", r."RequiredRelated_RequiredNested_Ints", r."RequiredRelated_RequiredNested_Name", r."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS r
WHERE cardinality(r."RequiredRelated_RequiredNested_Ints") = 3
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(sum(r1.value), 0)::int
    FROM unnest(r."RequiredRelated_Ints") AS r1(value))
FROM "RootEntity" AS r
WHERE (
    SELECT COALESCE(sum(r0.value), 0)::int
    FROM unnest(r."RequiredRelated_Ints") AS r0(value)) >= 6
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
