namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public class OwnedJsonPrimitiveCollectionNpgsqlTest(OwnedJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedJsonPrimitiveCollectionRelationalTestBase<OwnedJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE cardinality((ARRAY(SELECT CAST(element AS integer) FROM jsonb_array_elements_text(r."RequiredRelated" -> 'Ints') WITH ORDINALITY AS t(element) ORDER BY ordinality))) = 3
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE ((ARRAY(SELECT CAST(element AS integer) FROM jsonb_array_elements_text(r."RequiredRelated" -> 'Ints') WITH ORDINALITY AS t(element) ORDER BY ordinality)))[1] = 1
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE 3 = ANY ((ARRAY(SELECT CAST(element AS integer) FROM jsonb_array_elements_text(r."RequiredRelated" -> 'Ints') WITH ORDINALITY AS t(element) ORDER BY ordinality)))
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE 2 = ANY ((ARRAY(SELECT CAST(element AS integer) FROM jsonb_array_elements_text(r."RequiredRelated" -> 'Ints') WITH ORDINALITY AS t(element) ORDER BY ordinality)))
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE cardinality((ARRAY(SELECT CAST(element AS integer) FROM jsonb_array_elements_text(r."RequiredRelated" #> '{RequiredNested,Ints}') WITH ORDINALITY AS t(element) ORDER BY ordinality))) = 3
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(sum(i0.value), 0)::int
    FROM unnest((ARRAY(SELECT CAST(element AS integer) FROM jsonb_array_elements_text(r."RequiredRelated" -> 'Ints') WITH ORDINALITY AS t(element) ORDER BY ordinality))) AS i0(value))
FROM "RootEntity" AS r
WHERE (
    SELECT COALESCE(sum(i.value), 0)::int
    FROM unnest((ARRAY(SELECT CAST(element AS integer) FROM jsonb_array_elements_text(r."RequiredRelated" -> 'Ints') WITH ORDINALITY AS t(element) ORDER BY ordinality))) AS i(value)) >= 6
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
