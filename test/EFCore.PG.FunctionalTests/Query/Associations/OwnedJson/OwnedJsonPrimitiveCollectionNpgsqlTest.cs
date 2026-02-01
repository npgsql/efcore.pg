namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public class OwnedJsonPrimitiveCollectionNpgsqlTest(OwnedJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedJsonPrimitiveCollectionRelationalTestBase<OwnedJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE jsonb_array_length(r."RequiredAssociate" -> 'Ints') = 3
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (CAST(r."RequiredAssociate" #>> '{Ints,0}' AS integer)) = 1
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate" -> 'Ints') @> to_jsonb(3)
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (r."RequiredAssociate" -> 'Ints') @> to_jsonb(2)
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE jsonb_array_length(r."RequiredAssociate" #> '{RequiredNestedAssociate,Ints}') = 3
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(sum(i0.element::int), 0)::int
    FROM jsonb_array_elements_text(r."RequiredAssociate" -> 'Ints') WITH ORDINALITY AS i0(element))
FROM "RootEntity" AS r
WHERE (
    SELECT COALESCE(sum(i.element::int), 0)::int
    FROM jsonb_array_elements_text(r."RequiredAssociate" -> 'Ints') WITH ORDINALITY AS i(element)) >= 6
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
