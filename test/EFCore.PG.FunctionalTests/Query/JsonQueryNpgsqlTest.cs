using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class JsonQueryNpgsqlTest : JsonQueryTestBase<JsonQueryNpgsqlTest.JsonQueryNpgsqlFixture>
{
    public JsonQueryNpgsqlTest(JsonQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Basic_json_projection_owner_entity(bool async)
    {
        await base.Basic_json_projection_owner_entity(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Basic_json_projection_owner_entity_NoTracking(bool async)
    {
        await base.Basic_json_projection_owner_entity_NoTracking(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Basic_json_projection_owner_entity_duplicated(bool async)
    {
        await base.Basic_json_projection_owner_entity_duplicated(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Basic_json_projection_owner_entity_duplicated_NoTracking(bool async)
    {
        await base.Basic_json_projection_owner_entity_duplicated_NoTracking(async);

        AssertSql(
            """
SELECT j."Id", j."Name", j."OwnedCollection", j."OwnedCollection"
FROM "JsonEntitiesSingleOwned" AS j
""");
    }

    public override async Task Basic_json_projection_owner_entity_twice(bool async)
    {
        await base.Basic_json_projection_owner_entity_twice(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Basic_json_projection_owner_entity_twice_NoTracking(bool async)
    {
        await base.Basic_json_projection_owner_entity_twice_NoTracking(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Project_json_reference_in_tracking_query_fails(bool async)
    {
        await base.Project_json_reference_in_tracking_query_fails(async);

        AssertSql(
        );
    }

    public override async Task Project_json_collection_in_tracking_query_fails(bool async)
    {
        await base.Project_json_collection_in_tracking_query_fails(async);

        AssertSql(
        );
    }

    public override async Task Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(bool async)
    {
        await base.Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(async);

        AssertSql(
        );
    }

    public override async Task Basic_json_projection_owned_reference_root(bool async)
    {
        await base.Basic_json_projection_owned_reference_root(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot", j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Basic_json_projection_owned_reference_duplicated2(bool async)
    {
        await base.Basic_json_projection_owned_reference_duplicated2(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot", j."Id", j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedReferenceLeaf}', j."OwnedReferenceRoot", j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedReferenceLeaf}'
FROM "JsonEntitiesBasic" AS j
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Basic_json_projection_owned_reference_duplicated(bool async)
    {
        await base.Basic_json_projection_owned_reference_duplicated(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot", j."Id", j."OwnedReferenceRoot" -> 'OwnedReferenceBranch', j."OwnedReferenceRoot", j."OwnedReferenceRoot" -> 'OwnedReferenceBranch'
FROM "JsonEntitiesBasic" AS j
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Basic_json_projection_owned_collection_root(bool async)
    {
        await base.Basic_json_projection_owned_collection_root(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot", j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Basic_json_projection_owned_reference_branch(bool async)
    {
        await base.Basic_json_projection_owned_reference_branch(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot" -> 'OwnedReferenceBranch', j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Basic_json_projection_owned_collection_branch(bool async)
    {
        await base.Basic_json_projection_owned_collection_branch(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot" -> 'OwnedCollectionBranch', j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Basic_json_projection_owned_reference_leaf(bool async)
    {
        await base.Basic_json_projection_owned_reference_leaf(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedReferenceLeaf}', j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Basic_json_projection_owned_collection_leaf(bool async)
    {
        await base.Basic_json_projection_owned_collection_leaf(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf}', j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Basic_json_projection_scalar(bool async)
    {
        await base.Basic_json_projection_scalar(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot" ->> 'Name'
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_scalar_length(bool async)
    {
        await base.Json_scalar_length(async);

        AssertSql(
            """
SELECT j."Name"
FROM "JsonEntitiesBasic" AS j
WHERE length(j."OwnedReferenceRoot" ->> 'Name')::int > 2
""");
    }

    public override async Task Basic_json_projection_enum_inside_json_entity(bool async)
    {
        await base.Basic_json_projection_enum_inside_json_entity(async);

        AssertSql(
            """
SELECT j."Id", CAST(j."OwnedReferenceRoot" #>> '{OwnedReferenceBranch,Enum}' AS integer) AS "Enum"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_projection_enum_with_custom_conversion(bool async)
    {
        await base.Json_projection_enum_with_custom_conversion(async);

        AssertSql(
            """
SELECT j."Id", CAST(j.json_reference_custom_naming ->> 'CustomEnum' AS integer) AS "Enum"
FROM "JsonEntitiesCustomNaming" AS j
""");
    }

    public override async Task Json_projection_with_deduplication(bool async)
    {
        await base.Json_projection_with_deduplication(async);

        AssertSql(
            """
SELECT j."Id", j."OwnedReferenceRoot" -> 'OwnedReferenceBranch', j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedReferenceLeaf}', j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf}', j."OwnedReferenceRoot" -> 'OwnedCollectionBranch', j."OwnedReferenceRoot" #>> '{OwnedReferenceBranch,OwnedReferenceLeaf,SomethingSomething}'
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_projection_with_deduplication_reverse_order(bool async)
    {
        await base.Json_projection_with_deduplication_reverse_order(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedReferenceLeaf}', j."Id", j."OwnedReferenceRoot", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_property_in_predicate(bool async)
    {
        await base.Json_property_in_predicate(async);

        AssertSql(
            """
SELECT j."Id"
FROM "JsonEntitiesBasic" AS j
WHERE (CAST(j."OwnedReferenceRoot" #>> '{OwnedReferenceBranch,Fraction}' AS numeric(18,2))) < 20.5
""");
    }

    public override async Task Json_subquery_property_pushdown_length(bool async)
    {
        await base.Json_subquery_property_pushdown_length(async);

        AssertSql(
            """
@__p_0='3'

SELECT length(t0.c)::int
FROM (
    SELECT DISTINCT t.c
    FROM (
        SELECT j."OwnedReferenceRoot" #>> '{OwnedReferenceBranch,OwnedReferenceLeaf,SomethingSomething}' AS c
        FROM "JsonEntitiesBasic" AS j
        ORDER BY j."Id" NULLS FIRST
        LIMIT @__p_0
    ) AS t
) AS t0
""");
    }

    public override async Task Json_subquery_reference_pushdown_reference(bool async)
    {
        await base.Json_subquery_reference_pushdown_reference(async);

        AssertSql(
            """
@__p_0='10'

SELECT t0.c -> 'OwnedReferenceBranch', t0."Id"
FROM (
    SELECT DISTINCT t.c AS c, t."Id"
    FROM (
        SELECT j."OwnedReferenceRoot" AS c, j."Id"
        FROM "JsonEntitiesBasic" AS j
        ORDER BY j."Id" NULLS FIRST
        LIMIT @__p_0
    ) AS t
) AS t0
""");
    }

    public override async Task Json_subquery_reference_pushdown_reference_anonymous_projection(bool async)
    {
        await base.Json_subquery_reference_pushdown_reference_anonymous_projection(async);

        AssertSql(
            """
@__p_0='10'

SELECT JSON_QUERY([t0].[c], '$.OwnedReferenceSharedBranch'), [t0].[Id], CAST(LEN([t0].[c0]) AS int)
FROM (
    SELECT DISTINCT JSON_QUERY([t].[c],'$') AS [c], [t].[Id], [t].[c0]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([j].[json_reference_shared], '$') AS [c], [j].[Id], CAST(JSON_VALUE([j].[json_reference_shared], '$.OwnedReferenceSharedBranch.OwnedReferenceSharedLeaf.SomethingSomething') AS nvarchar(max)) AS [c0]
        FROM [JsonEntitiesBasic] AS [j]
        ORDER BY [j].[Id]
    ) AS [t]
) AS [t0]
""");
    }

    public override async Task Json_subquery_reference_pushdown_reference_pushdown_anonymous_projection(bool async)
    {
        await base.Json_subquery_reference_pushdown_reference_pushdown_anonymous_projection(async);

        AssertSql(
            """
@__p_0='10'

SELECT JSON_QUERY([t2].[c],'$.OwnedReferenceSharedLeaf'), [t2].[Id], JSON_QUERY([t2].[c], '$.OwnedCollectionSharedLeaf'), [t2].[Length]
FROM (
    SELECT DISTINCT JSON_QUERY([t1].[c],'$') AS [c], [t1].[Id], [t1].[Length]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([t0].[c], '$.OwnedReferenceSharedBranch') AS [c], [t0].[Id], CAST(LEN([t0].[Scalar]) AS int) AS [Length]
        FROM (
            SELECT DISTINCT JSON_QUERY([t].[c],'$') AS [c], [t].[Id], [t].[Scalar]
            FROM (
                SELECT TOP(@__p_0) JSON_QUERY([j].[json_reference_shared], '$') AS [c], [j].[Id], CAST(JSON_VALUE([j].[json_reference_shared], '$.OwnedReferenceSharedBranch.OwnedReferenceSharedLeaf.SomethingSomething') AS nvarchar(max)) AS [Scalar]
                FROM [JsonEntitiesBasic] AS [j]
                ORDER BY [j].[Id]
            ) AS [t]
        ) AS [t0]
        ORDER BY CAST(LEN([t0].[Scalar]) AS int)
    ) AS [t1]
) AS [t2]
""");
    }

    public override async Task Json_subquery_reference_pushdown_reference_pushdown_reference(bool async)
    {
        await base.Json_subquery_reference_pushdown_reference_pushdown_reference(async);

        AssertSql(
            """
@__p_0='10'

SELECT t2.c -> 'OwnedReferenceLeaf', t2."Id"
FROM (
    SELECT DISTINCT t1.c AS c, t1."Id"
    FROM (
        SELECT t0.c -> 'OwnedReferenceBranch' AS c, t0."Id"
        FROM (
            SELECT DISTINCT t.c AS c, t."Id", t.c AS c0
            FROM (
                SELECT j."OwnedReferenceRoot" AS c, j."Id"
                FROM "JsonEntitiesBasic" AS j
                ORDER BY j."Id" NULLS FIRST
                LIMIT @__p_0
            ) AS t
        ) AS t0
        ORDER BY t0.c0 ->> 'Name' NULLS FIRST
        LIMIT @__p_0
    ) AS t1
) AS t2
""");
    }

    public override async Task Json_subquery_reference_pushdown_reference_pushdown_collection(bool async)
    {
        await base.Json_subquery_reference_pushdown_reference_pushdown_collection(async);

        AssertSql(
            """
@__p_0='10'

SELECT t2.c -> 'OwnedCollectionLeaf', t2."Id"
FROM (
    SELECT DISTINCT t1.c AS c, t1."Id"
    FROM (
        SELECT t0.c -> 'OwnedReferenceBranch' AS c, t0."Id"
        FROM (
            SELECT DISTINCT t.c AS c, t."Id", t.c AS c0
            FROM (
                SELECT j."OwnedReferenceRoot" AS c, j."Id"
                FROM "JsonEntitiesBasic" AS j
                ORDER BY j."Id" NULLS FIRST
                LIMIT @__p_0
            ) AS t
        ) AS t0
        ORDER BY t0.c0 ->> 'Name' NULLS FIRST
        LIMIT @__p_0
    ) AS t1
) AS t2
""");
    }

    public override async Task Json_subquery_reference_pushdown_property(bool async)
    {
        await base.Json_subquery_reference_pushdown_property(async);

        AssertSql(
            """
@__p_0='10'

SELECT t0.c ->> 'SomethingSomething'
FROM (
    SELECT DISTINCT t.c AS c, t."Id"
    FROM (
        SELECT j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedReferenceLeaf}' AS c, j."Id"
        FROM "JsonEntitiesBasic" AS j
        ORDER BY j."Id" NULLS FIRST
        LIMIT @__p_0
    ) AS t
) AS t0
""");
    }

    public override async Task Custom_naming_projection_owner_entity(bool async)
    {
        await base.Custom_naming_projection_owner_entity(async);

        AssertSql(
            """
SELECT j."Id", j."Title", j.json_collection_custom_naming, j.json_reference_custom_naming
FROM "JsonEntitiesCustomNaming" AS j
""");
    }

    public override async Task Custom_naming_projection_owned_reference(bool async)
    {
        await base.Custom_naming_projection_owned_reference(async);

        AssertSql(
            """
SELECT j.json_reference_custom_naming -> 'CustomOwnedReferenceBranch', j."Id"
FROM "JsonEntitiesCustomNaming" AS j
""");
    }

    public override async Task Custom_naming_projection_owned_collection(bool async)
    {
        await base.Custom_naming_projection_owned_collection(async);

        AssertSql(
            """
SELECT j.json_collection_custom_naming, j."Id"
FROM "JsonEntitiesCustomNaming" AS j
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Custom_naming_projection_owned_scalar(bool async)
    {
        await base.Custom_naming_projection_owned_scalar(async);

        AssertSql(
            """
SELECT CAST(j.json_reference_custom_naming #>> '{CustomOwnedReferenceBranch,CustomFraction}' AS double precision)
FROM "JsonEntitiesCustomNaming" AS j
""");
    }

    public override async Task Custom_naming_projection_everything(bool async)
    {
        await base.Custom_naming_projection_everything(async);

        AssertSql(
            """
SELECT j."Id", j."Title", j.json_collection_custom_naming, j.json_reference_custom_naming, j.json_reference_custom_naming, j.json_reference_custom_naming -> 'CustomOwnedReferenceBranch', j.json_collection_custom_naming, j.json_reference_custom_naming -> 'CustomOwnedCollectionBranch', j.json_reference_custom_naming ->> 'CustomName', CAST(j.json_reference_custom_naming #>> '{CustomOwnedReferenceBranch,CustomFraction}' AS double precision)
FROM "JsonEntitiesCustomNaming" AS j
""");
    }

    public override async Task Project_entity_with_single_owned(bool async)
    {
        await base.Project_entity_with_single_owned(async);

        AssertSql(
            """
SELECT j."Id", j."Name", j."OwnedCollection"
FROM "JsonEntitiesSingleOwned" AS j
""");
    }

    public override async Task Left_join_json_entities(bool async)
    {
        await base.Left_join_json_entities(async);

        AssertSql(
            """
SELECT j."Id", j."Name", j."OwnedCollection", j0."Id", j0."EntityBasicId", j0."Name", j0."OwnedCollectionRoot", j0."OwnedReferenceRoot"
FROM "JsonEntitiesSingleOwned" AS j
LEFT JOIN "JsonEntitiesBasic" AS j0 ON j."Id" = j0."Id"
""");
    }

    public override async Task Left_join_json_entities_complex_projection(bool async)
    {
        await base.Left_join_json_entities_complex_projection(async);

        AssertSql(
            """
SELECT j."Id", j0."Id", j0."OwnedReferenceRoot", j0."OwnedReferenceRoot" -> 'OwnedReferenceBranch', j0."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedReferenceLeaf}', j0."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf}'
FROM "JsonEntitiesSingleOwned" AS j
LEFT JOIN "JsonEntitiesBasic" AS j0 ON j."Id" = j0."Id"
""");
    }

    public override async Task Left_join_json_entities_json_being_inner(bool async)
    {
        await base.Left_join_json_entities_json_being_inner(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j0."Id", j0."Name", j0."OwnedCollection"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesSingleOwned" AS j0 ON j."Id" = j0."Id"
""");
    }

    public override async Task Left_join_json_entities_complex_projection_json_being_inner(bool async)
    {
        await base.Left_join_json_entities_complex_projection_json_being_inner(async);

        AssertSql(
            """
SELECT j."Id", j0."Id", j."OwnedReferenceRoot", j."OwnedReferenceRoot" -> 'OwnedReferenceBranch', j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedReferenceLeaf}', j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf}', j0."Name"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesSingleOwned" AS j0 ON j."Id" = j0."Id"
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery(async);

        AssertSql(
            """
SELECT t.c, t."Id"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT j0."OwnedReferenceRoot" -> 'OwnedReferenceBranch' AS c, j0."Id"
    FROM "JsonEntitiesBasic" AS j0
    ORDER BY j0."Id" NULLS FIRST
    LIMIT 1
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery_with_binding_on_top(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery_with_binding_on_top(async);

        AssertSql(
            """
SELECT (
    SELECT CAST(j0."OwnedReferenceRoot" #>> '{OwnedReferenceBranch,Date}' AS timestamp without time zone)
    FROM "JsonEntitiesBasic" AS j0
    ORDER BY j0."Id" NULLS FIRST
    LIMIT 1)
FROM "JsonEntitiesBasic" AS j
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery_with_entity_comparison_on_top(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery_with_entity_comparison_on_top(async);

        AssertSql(
            @"");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery_deduplication(async);

        AssertSql(
            """
SELECT t.c, t."Id", t.c0, t."Id0", t.c1, t.c2, t.c3, t.c4
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT j."OwnedReferenceRoot" -> 'OwnedCollectionBranch' AS c, j."Id", j0."OwnedReferenceRoot" AS c0, j0."Id" AS "Id0", j0."OwnedReferenceRoot" -> 'OwnedReferenceBranch' AS c1, j0."OwnedReferenceRoot" ->> 'Name' AS c2, CAST(j."OwnedReferenceRoot" #>> '{OwnedReferenceBranch,Enum}' AS integer) AS c3, 1 AS c4
    FROM "JsonEntitiesBasic" AS j0
    ORDER BY j0."Id" NULLS FIRST
    LIMIT 1
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication_and_outer_reference(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery_deduplication_and_outer_reference(async);

        AssertSql(
            """
SELECT t.c, t."Id", t.c0, t."Id0", t.c1, t.c2, t.c3, t.c4
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT j."OwnedReferenceRoot" -> 'OwnedCollectionBranch' AS c, j."Id", j0."OwnedReferenceRoot" AS c0, j0."Id" AS "Id0", j0."OwnedReferenceRoot" -> 'OwnedReferenceBranch' AS c1, j0."OwnedReferenceRoot" ->> 'Name' AS c2, CAST(j."OwnedReferenceRoot" #>> '{OwnedReferenceBranch,Enum}' AS integer) AS c3, 1 AS c4
    FROM "JsonEntitiesBasic" AS j0
    ORDER BY j0."Id" NULLS FIRST
    LIMIT 1
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication_outer_reference_and_pruning(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery_deduplication_outer_reference_and_pruning(async);

        AssertSql(
            """
SELECT t.c, t."Id", t.c0
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT j."OwnedReferenceRoot" -> 'OwnedCollectionBranch' AS c, j."Id", 1 AS c0
    FROM "JsonEntitiesBasic" AS j0
    ORDER BY j0."Id" NULLS FIRST
    LIMIT 1
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_entity_with_inheritance_basic_projection(bool async)
    {
        await base.Json_entity_with_inheritance_basic_projection(async);

        AssertSql(
            """
SELECT j."Id", j."Discriminator", j."Name", j."Fraction", j."CollectionOnBase", j."ReferenceOnBase", j."CollectionOnDerived", j."ReferenceOnDerived"
FROM "JsonEntitiesInheritance" AS j
""");
    }

    public override async Task Json_entity_with_inheritance_project_derived(bool async)
    {
        await base.Json_entity_with_inheritance_project_derived(async);

        AssertSql(
            """
SELECT j."Id", j."Discriminator", j."Name", j."Fraction", j."CollectionOnBase", j."ReferenceOnBase", j."CollectionOnDerived", j."ReferenceOnDerived"
FROM "JsonEntitiesInheritance" AS j
WHERE j."Discriminator" = 'JsonEntityInheritanceDerived'
""");
    }

    public override async Task Json_entity_with_inheritance_project_navigations(bool async)
    {
        await base.Json_entity_with_inheritance_project_navigations(async);

        AssertSql(
            """
SELECT j."Id", j."ReferenceOnBase", j."CollectionOnBase"
FROM "JsonEntitiesInheritance" AS j
""");
    }

    public override async Task Json_entity_with_inheritance_project_navigations_on_derived(bool async)
    {
        await base.Json_entity_with_inheritance_project_navigations_on_derived(async);

        AssertSql(
            """
SELECT j."Id", j."ReferenceOnBase", j."ReferenceOnDerived", j."CollectionOnBase", j."CollectionOnDerived"
FROM "JsonEntitiesInheritance" AS j
WHERE j."Discriminator" = 'JsonEntityInheritanceDerived'
""");
    }

    public override async Task Json_entity_backtracking(bool async)
    {
        await base.Json_entity_backtracking(async);

        AssertSql(
            @"");
    }

    public override async Task Json_collection_index_in_projection_basic(bool async)
    {
        await base.Json_collection_index_in_projection_basic(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" -> 1, j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_ElementAt_in_projection(bool async)
    {
        await base.Json_collection_ElementAt_in_projection(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" -> 1, j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_ElementAtOrDefault_in_projection(bool async)
    {
        await base.Json_collection_ElementAtOrDefault_in_projection(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" -> 1, j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_project_collection(bool async)
    {
        await base.Json_collection_index_in_projection_project_collection(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" #> '{1,OwnedCollectionBranch}', j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_ElementAt_project_collection(bool async)
    {
        await base.Json_collection_ElementAt_project_collection(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" #> '{1,OwnedCollectionBranch}', j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_ElementAtOrDefault_project_collection(bool async)
    {
        await base.Json_collection_ElementAtOrDefault_project_collection(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" #> '{1,OwnedCollectionBranch}', j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_using_parameter(bool async)
    {
        await base.Json_collection_index_in_projection_using_parameter(async);

        AssertSql(
            """
@__prm_0='0'

SELECT j."OwnedCollectionRoot" -> @__prm_0, j."Id", @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_using_column(bool async)
    {
        await base.Json_collection_index_in_projection_using_column(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" -> j."Id", j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_using_untranslatable_client_method(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Json_collection_index_in_projection_using_untranslatable_client_method(async))).Message;

        Assert.Contains(
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.JsonQueryTestBase<Npgsql.EntityFrameworkCore.PostgreSQL.Query.JsonQueryNpgsqlTest+JsonQueryNpgsqlFixture>",
                "MyMethod"),
            message);
    }

    public override async Task Json_collection_index_in_projection_using_untranslatable_client_method2(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Json_collection_index_in_projection_using_untranslatable_client_method2(async))).Message;

        Assert.Contains(
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.JsonQueryTestBase<Npgsql.EntityFrameworkCore.PostgreSQL.Query.JsonQueryNpgsqlTest+JsonQueryNpgsqlFixture>",
                "MyMethod"),
            message);
    }

    public override async Task Json_collection_index_outside_bounds(bool async)
    {
        await base.Json_collection_index_outside_bounds(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" -> 25, j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_outside_bounds2(bool async)
    {
        await base.Json_collection_index_outside_bounds2(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf,25}', j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_outside_bounds_with_property_access(bool async)
    {
        await base.Json_collection_index_outside_bounds_with_property_access(async);

        AssertSql(
            """
SELECT CAST(j."OwnedCollectionRoot" #>> '{25,Number}' AS integer)
FROM "JsonEntitiesBasic" AS j
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_collection_index_in_projection_nested(bool async)
    {
        await base.Json_collection_index_in_projection_nested(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."OwnedCollectionRoot" #> ARRAY[0,'OwnedCollectionBranch',@__prm_0]::text[], j."Id", @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_nested_project_scalar(bool async)
    {
        await base.Json_collection_index_in_projection_nested_project_scalar(async);

        AssertSql(
            """
@__prm_0='1'

SELECT CAST(j."OwnedCollectionRoot" #>> ARRAY[0,'OwnedCollectionBranch',@__prm_0,'Date']::text[] AS timestamp without time zone)
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_nested_project_reference(bool async)
    {
        await base.Json_collection_index_in_projection_nested_project_reference(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."OwnedCollectionRoot" #> ARRAY[0,'OwnedCollectionBranch',@__prm_0,'OwnedReferenceLeaf']::text[], j."Id", @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_nested_project_collection(bool async)
    {
        await base.Json_collection_index_in_projection_nested_project_collection(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."OwnedCollectionRoot" #> ARRAY[0,'OwnedCollectionBranch',@__prm_0,'OwnedCollectionLeaf']::text[], j."Id", @__prm_0
FROM "JsonEntitiesBasic" AS j
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_collection_index_in_projection_nested_project_collection_anonymous_projection(bool async)
    {
        await base.Json_collection_index_in_projection_nested_project_collection_anonymous_projection(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id", j."OwnedCollectionRoot" #> ARRAY[0,'OwnedCollectionBranch',@__prm_0,'OwnedCollectionLeaf']::text[], @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_predicate_using_constant(bool async)
    {
        await base.Json_collection_index_in_predicate_using_constant(async);

        AssertSql(
            """
SELECT j."Id"
FROM "JsonEntitiesBasic" AS j
WHERE (j."OwnedCollectionRoot" #>> '{0,Name}') <> 'Foo' OR (j."OwnedCollectionRoot" #>> '{0,Name}') IS NULL
""");
    }

    public override async Task Json_collection_index_in_predicate_using_variable(bool async)
    {
        await base.Json_collection_index_in_predicate_using_variable(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id"
FROM "JsonEntitiesBasic" AS j
WHERE (j."OwnedCollectionRoot" #>> ARRAY[@__prm_0,'Name']::text[]) <> 'Foo' OR (j."OwnedCollectionRoot" #>> ARRAY[@__prm_0,'Name']::text[]) IS NULL
""");
    }

    public override async Task Json_collection_index_in_predicate_using_column(bool async)
    {
        await base.Json_collection_index_in_predicate_using_column(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE (j."OwnedCollectionRoot" #>> ARRAY[j."Id",'Name']::text[]) = 'e1_c2'
""");
    }

    public override async Task Json_collection_index_in_predicate_using_complex_expression1(bool async)
    {
        await base.Json_collection_index_in_predicate_using_complex_expression1(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE (j."OwnedCollectionRoot" #>> ARRAY[CASE
    WHEN j."Id" = 1 THEN 0
    ELSE 1
END,'Name']::text[]) = 'e1_c1'
""");
    }

    public override async Task Json_collection_index_in_predicate_using_complex_expression2(bool async)
    {
        await base.Json_collection_index_in_predicate_using_complex_expression2(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE (j."OwnedCollectionRoot" #>> ARRAY[(
    SELECT max(j0."Id")
    FROM "JsonEntitiesBasic" AS j0),'Name']::text[]) = 'e1_c2'
""");
    }

    public override async Task Json_collection_ElementAt_in_predicate(bool async)
    {
        await base.Json_collection_ElementAt_in_predicate(async);

        AssertSql(
            """
SELECT j."Id"
FROM "JsonEntitiesBasic" AS j
WHERE (j."OwnedCollectionRoot" #>> '{1,Name}') <> 'Foo' OR (j."OwnedCollectionRoot" #>> '{1,Name}') IS NULL
""");
    }

    public override async Task Json_collection_index_in_predicate_nested_mix(bool async)
    {
        await base.Json_collection_index_in_predicate_nested_mix(async);

        AssertSql(
            """
@__prm_0='0'

SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE (j."OwnedCollectionRoot" #>> ARRAY[1,'OwnedCollectionBranch',@__prm_0,'OwnedCollectionLeaf',j."Id" - 1,'SomethingSomething']::text[]) = 'e1_c2_c1_c1'
""");
    }

    public override async Task Json_collection_ElementAt_and_pushdown(bool async)
    {
        await base.Json_collection_ElementAt_and_pushdown(async);

        AssertSql(
            """
SELECT j."Id", CAST(j."OwnedCollectionRoot" #>> '{0,Number}' AS integer) AS "CollectionElement"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_Any_with_predicate(bool async)
    {
        await base.Json_collection_Any_with_predicate(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE EXISTS (
    SELECT 1
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedReferenceRoot" -> 'OwnedCollectionBranch') AS (
        "Date" timestamp without time zone,
        "Enum" integer,
        "Enums" integer[],
        "Fraction" numeric(18,2),
        "NullableEnum" integer,
        "NullableEnums" integer[],
        "OwnedCollectionLeaf" jsonb,
        "OwnedReferenceLeaf" jsonb
    )) WITH ORDINALITY AS o
    WHERE (o."OwnedReferenceLeaf" ->> 'SomethingSomething') = 'e1_r_c1_r')
""");
    }

    public override async Task Json_collection_Where_ElementAt(bool async)
    {
        await base.Json_collection_Where_ElementAt(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE (
    SELECT o."OwnedReferenceLeaf" ->> 'SomethingSomething'
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedReferenceRoot" -> 'OwnedCollectionBranch') AS (
        "Date" timestamp without time zone,
        "Enum" integer,
        "Enums" integer[],
        "Fraction" numeric(18,2),
        "NullableEnum" integer,
        "NullableEnums" integer[],
        "OwnedCollectionLeaf" jsonb,
        "OwnedReferenceLeaf" jsonb
    )) WITH ORDINALITY AS o
    WHERE o."Enum" = 2
    LIMIT 1 OFFSET 0) = 'e1_r_c2_r'
""");
    }

    public override async Task Json_collection_Skip(bool async)
    {
        await base.Json_collection_Skip(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE (
    SELECT t.c
    FROM (
        SELECT o."OwnedReferenceLeaf" ->> 'SomethingSomething' AS c, o.ordinality
        FROM ROWS FROM (jsonb_to_recordset(j."OwnedReferenceRoot" -> 'OwnedCollectionBranch') AS (
            "Date" timestamp without time zone,
            "Enum" integer,
            "Enums" integer[],
            "Fraction" numeric(18,2),
            "NullableEnum" integer,
            "NullableEnums" integer[],
            "OwnedCollectionLeaf" jsonb,
            "OwnedReferenceLeaf" jsonb
        )) WITH ORDINALITY AS o
        OFFSET 1
    ) AS t
    LIMIT 1 OFFSET 0) = 'e1_r_c2_r'
""");
    }

    public override async Task Json_collection_OrderByDescending_Skip_ElementAt(bool async)
    {
        await base.Json_collection_OrderByDescending_Skip_ElementAt(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE (
    SELECT t.c
    FROM (
        SELECT o."OwnedReferenceLeaf" ->> 'SomethingSomething' AS c, o.ordinality, o."Date" AS c0
        FROM ROWS FROM (jsonb_to_recordset(j."OwnedReferenceRoot" -> 'OwnedCollectionBranch') AS (
            "Date" timestamp without time zone,
            "Enum" integer,
            "Enums" integer[],
            "Fraction" numeric(18,2),
            "NullableEnum" integer,
            "NullableEnums" integer[],
            "OwnedCollectionLeaf" jsonb,
            "OwnedReferenceLeaf" jsonb
        )) WITH ORDINALITY AS o
        ORDER BY o."Date" DESC NULLS LAST
        OFFSET 1
    ) AS t
    ORDER BY t.c0 DESC NULLS LAST
    LIMIT 1 OFFSET 0) = 'e1_r_c1_r'
""");
    }

    public override async Task Json_collection_Distinct_Count_with_predicate(bool async)
    {
        await base.Json_collection_Distinct_Count_with_predicate(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT DISTINCT j."Id", o."Date", o."Enum", o."Enums", o."Fraction", o."NullableEnum", o."NullableEnums", o."OwnedCollectionLeaf" AS c, o."OwnedReferenceLeaf" AS c0
        FROM ROWS FROM (jsonb_to_recordset(j."OwnedReferenceRoot" -> 'OwnedCollectionBranch') AS (
            "Date" timestamp without time zone,
            "Enum" integer,
            "Enums" integer[],
            "Fraction" numeric(18,2),
            "NullableEnum" integer,
            "NullableEnums" integer[],
            "OwnedCollectionLeaf" jsonb,
            "OwnedReferenceLeaf" jsonb
        )) WITH ORDINALITY AS o
        WHERE (o."OwnedReferenceLeaf" ->> 'SomethingSomething') = 'e1_r_c2_r'
    ) AS t) = 1
""");
    }

    public override async Task Json_collection_within_collection_Count(bool async)
    {
        await base.Json_collection_within_collection_Count(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE EXISTS (
    SELECT 1
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    WHERE (
        SELECT count(*)::int
        FROM ROWS FROM (jsonb_to_recordset(o."OwnedCollectionBranch") AS (
            "Date" timestamp without time zone,
            "Enum" integer,
            "Enums" integer[],
            "Fraction" numeric(18,2),
            "NullableEnum" integer,
            "NullableEnums" integer[],
            "OwnedCollectionLeaf" jsonb,
            "OwnedReferenceLeaf" jsonb
        )) WITH ORDINALITY AS o0) = 2)
""");
    }

    public override async Task Json_collection_in_projection_with_composition_count(bool async)
    {
        await base.Json_collection_in_projection_with_composition_count(async);

        AssertSql(
            """
SELECT (
    SELECT count(*)::int
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o)
FROM "JsonEntitiesBasic" AS j
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_collection_in_projection_with_anonymous_projection_of_scalars(bool async)
    {
        await base.Json_collection_in_projection_with_anonymous_projection_of_scalars(async);

        AssertSql(
            """
SELECT j."Id", o."Name", o."Number", o.ordinality
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
    "Name" text,
    "Names" text[],
    "Number" integer,
    "Numbers" integer[],
    "OwnedCollectionBranch" jsonb,
    "OwnedReferenceBranch" jsonb
)) WITH ORDINALITY AS o ON TRUE
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(bool async)
    {
        await base.Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(async);

        AssertSql(
            """
SELECT j."Id", t."Name", t."Number", t.ordinality
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT o."Name", o."Number", o.ordinality
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    WHERE o."Name" = 'Foo'
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(bool async)
    {
        await base.Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(async);

        AssertSql(
            """
SELECT j."Id", t."Names", t."Numbers", t.ordinality
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT o."Names", o."Numbers", o.ordinality
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    WHERE o."Name" = 'Foo'
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_collection_filter_in_projection(bool async)
    {
        await base.Json_collection_filter_in_projection(async);

        AssertSql(
            """
SELECT j."Id", t."Id", t."Name", t."Names", t."Number", t."Numbers", t.c, t.c0, t.ordinality
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT j."Id", o."Name", o."Names", o."Number", o."Numbers", o."OwnedCollectionBranch" AS c, o."OwnedReferenceBranch" AS c0, o.ordinality
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    WHERE o."Name" <> 'Foo' OR o."Name" IS NULL
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_nested_collection_filter_in_projection(bool async)
    {
        await base.Json_nested_collection_filter_in_projection(async);

        AssertSql(
            """
SELECT j."Id", t0.ordinality, t0."Id", t0."Date", t0."Enum", t0."Enums", t0."Fraction", t0."NullableEnum", t0."NullableEnums", t0.c, t0.c0, t0.ordinality0
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT o.ordinality, t."Id", t."Date", t."Enum", t."Enums", t."Fraction", t."NullableEnum", t."NullableEnums", t.c, t.c0, t.ordinality AS ordinality0
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    LEFT JOIN LATERAL (
        SELECT j."Id", o0."Date", o0."Enum", o0."Enums", o0."Fraction", o0."NullableEnum", o0."NullableEnums", o0."OwnedCollectionLeaf" AS c, o0."OwnedReferenceLeaf" AS c0, o0.ordinality
        FROM ROWS FROM (jsonb_to_recordset(o."OwnedCollectionBranch") AS (
            "Date" timestamp without time zone,
            "Enum" integer,
            "Enums" integer[],
            "Fraction" numeric(18,2),
            "NullableEnum" integer,
            "NullableEnums" integer[],
            "OwnedCollectionLeaf" jsonb,
            "OwnedReferenceLeaf" jsonb
        )) WITH ORDINALITY AS o0
        WHERE o0."Date" <> TIMESTAMP '2000-01-01T00:00:00'
    ) AS t ON TRUE
) AS t0 ON TRUE
ORDER BY j."Id" NULLS FIRST, t0.ordinality NULLS FIRST
""");
    }

    public override async Task Json_nested_collection_anonymous_projection_in_projection(bool async)
    {
        await base.Json_nested_collection_anonymous_projection_in_projection(async);

        AssertSql(
            """
SELECT j."Id", t.ordinality, t.c, t.c0, t.c1, t.c2, t.c3, t."Id", t.c4, t.ordinality0
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT o.ordinality, o0."Date" AS c, o0."Enum" AS c0, o0."Enums" AS c1, o0."Fraction" AS c2, o0."OwnedReferenceLeaf" AS c3, j."Id", o0."OwnedCollectionLeaf" AS c4, o0.ordinality AS ordinality0
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    LEFT JOIN LATERAL ROWS FROM (jsonb_to_recordset(o."OwnedCollectionBranch") AS (
        "Date" timestamp without time zone,
        "Enum" integer,
        "Enums" integer[],
        "Fraction" numeric(18,2),
        "NullableEnum" integer,
        "NullableEnums" integer[],
        "OwnedCollectionLeaf" jsonb,
        "OwnedReferenceLeaf" jsonb
    )) WITH ORDINALITY AS o0 ON TRUE
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST, t.ordinality NULLS FIRST
""");
    }

    public override async Task Json_collection_skip_take_in_projection(bool async)
    {
        await base.Json_collection_skip_take_in_projection(async);

        AssertSql(
            """
SELECT j."Id", t."Id", t."Name", t."Names", t."Number", t."Numbers", t.c, t.c0, t.ordinality
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT j."Id", o."Name", o."Names", o."Number", o."Numbers", o."OwnedCollectionBranch" AS c, o."OwnedReferenceBranch" AS c0, o.ordinality, o."Name" AS c1
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    ORDER BY o."Name" NULLS FIRST
    LIMIT 5 OFFSET 1
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST, t.c1 NULLS FIRST
""");
    }

    public override async Task Json_collection_skip_take_in_projection_project_into_anonymous_type(bool async)
    {
        await base.Json_collection_skip_take_in_projection_project_into_anonymous_type(async);

        AssertSql(
            """
SELECT j."Id", t.c, t.c0, t.c1, t.c2, t.c3, t."Id", t.c4, t.ordinality
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT o."Name" AS c, o."Names" AS c0, o."Number" AS c1, o."Numbers" AS c2, o."OwnedCollectionBranch" AS c3, j."Id", o."OwnedReferenceBranch" AS c4, o.ordinality
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    ORDER BY o."Name" NULLS FIRST
    LIMIT 5 OFFSET 1
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST, t.c NULLS FIRST
""");
    }

    public override async Task Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(bool async)
    {
        await base.Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(async);

        AssertSql(
            """
SELECT j."Id", t.c, t."Id", t.ordinality
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT o."OwnedReferenceBranch" AS c, j."Id", o.ordinality, o."Name" AS c0
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    ORDER BY o."Name" NULLS FIRST
    LIMIT 5 OFFSET 1
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST, t.c0 NULLS FIRST
""");
    }

    public override async Task Json_collection_distinct_in_projection(bool async)
    {
        await base.Json_collection_distinct_in_projection(async);

        AssertSql(
            """
SELECT j."Id", t."Id", t."Name", t."Names", t."Number", t."Numbers", t.c, t.c0
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT DISTINCT j."Id", o."Name", o."Names", o."Number", o."Numbers", o."OwnedCollectionBranch" AS c, o."OwnedReferenceBranch" AS c0
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST, t."Name" NULLS FIRST, t."Names" NULLS FIRST, t."Number" NULLS FIRST
""");
    }

    public override async Task Json_collection_anonymous_projection_distinct_in_projection(bool async)
    {
        await base.Json_collection_anonymous_projection_distinct_in_projection(async);

        AssertSql("");
    }

    public override async Task Json_collection_leaf_filter_in_projection(bool async)
    {
        await base.Json_collection_leaf_filter_in_projection(async);

        AssertSql(
            """
SELECT j."Id", t."Id", t."SomethingSomething", t.ordinality
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT j."Id", o."SomethingSomething", o.ordinality
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf}') AS ("SomethingSomething" text)) WITH ORDINALITY AS o
    WHERE o."SomethingSomething" <> 'Baz' OR o."SomethingSomething" IS NULL
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_multiple_collection_projections(bool async)
    {
        await base.Json_multiple_collection_projections(async);

        AssertSql(
            """
SELECT j."Id", t."Id", t."SomethingSomething", t.ordinality, t0."Id", t0."Name", t0."Names", t0."Number", t0."Numbers", t0.c, t0.c0, t1.ordinality, t1."Id", t1."Date", t1."Enum", t1."Enums", t1."Fraction", t1."NullableEnum", t1."NullableEnums", t1.c, t1.c0, t1.ordinality0, j0."Id", j0."Name", j0."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT j."Id", o."SomethingSomething", o.ordinality
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf}') AS ("SomethingSomething" text)) WITH ORDINALITY AS o
    WHERE o."SomethingSomething" <> 'Baz' OR o."SomethingSomething" IS NULL
) AS t ON TRUE
LEFT JOIN LATERAL (
    SELECT DISTINCT j."Id", o0."Name", o0."Names", o0."Number", o0."Numbers", o0."OwnedCollectionBranch" AS c, o0."OwnedReferenceBranch" AS c0
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o0
) AS t0 ON TRUE
LEFT JOIN LATERAL (
    SELECT o1.ordinality, t2."Id", t2."Date", t2."Enum", t2."Enums", t2."Fraction", t2."NullableEnum", t2."NullableEnums", t2.c, t2.c0, t2.ordinality AS ordinality0
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o1
    LEFT JOIN LATERAL (
        SELECT j."Id", o2."Date", o2."Enum", o2."Enums", o2."Fraction", o2."NullableEnum", o2."NullableEnums", o2."OwnedCollectionLeaf" AS c, o2."OwnedReferenceLeaf" AS c0, o2.ordinality
        FROM ROWS FROM (jsonb_to_recordset(o1."OwnedCollectionBranch") AS (
            "Date" timestamp without time zone,
            "Enum" integer,
            "Enums" integer[],
            "Fraction" numeric(18,2),
            "NullableEnum" integer,
            "NullableEnums" integer[],
            "OwnedCollectionLeaf" jsonb,
            "OwnedReferenceLeaf" jsonb
        )) WITH ORDINALITY AS o2
        WHERE o2."Date" <> TIMESTAMP '2000-01-01T00:00:00'
    ) AS t2 ON TRUE
) AS t1 ON TRUE
LEFT JOIN "JsonEntitiesBasicForCollection" AS j0 ON j."Id" = j0."ParentId"
ORDER BY j."Id" NULLS FIRST, t.ordinality NULLS FIRST, t0."Name" NULLS FIRST, t0."Names" NULLS FIRST, t0."Number" NULLS FIRST, t0."Numbers" NULLS FIRST, t1.ordinality NULLS FIRST, t1.ordinality0 NULLS FIRST
""");
    }

    public override async Task Json_branch_collection_distinct_and_other_collection(bool async)
    {
        await base.Json_branch_collection_distinct_and_other_collection(async);

        AssertSql(
            """
SELECT j."Id", t."Id", t."Date", t."Enum", t."Enums", t."Fraction", t."NullableEnum", t."NullableEnums", t.c, t.c0, j0."Id", j0."Name", j0."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT DISTINCT j."Id", o."Date", o."Enum", o."Enums", o."Fraction", o."NullableEnum", o."NullableEnums", o."OwnedCollectionLeaf" AS c, o."OwnedReferenceLeaf" AS c0
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedReferenceRoot" -> 'OwnedCollectionBranch') AS (
        "Date" timestamp without time zone,
        "Enum" integer,
        "Enums" integer[],
        "Fraction" numeric(18,2),
        "NullableEnum" integer,
        "NullableEnums" integer[],
        "OwnedCollectionLeaf" jsonb,
        "OwnedReferenceLeaf" jsonb
    )) WITH ORDINALITY AS o
) AS t ON TRUE
LEFT JOIN "JsonEntitiesBasicForCollection" AS j0 ON j."Id" = j0."ParentId"
ORDER BY j."Id" NULLS FIRST, t."Date" NULLS FIRST, t."Enum" NULLS FIRST, t."Enums" NULLS FIRST, t."Fraction" NULLS FIRST, t."NullableEnum" NULLS FIRST, t."NullableEnums" NULLS FIRST
""");
    }

    public override async Task Json_leaf_collection_distinct_and_other_collection(bool async)
    {
        await base.Json_leaf_collection_distinct_and_other_collection(async);

        AssertSql(
            """
SELECT j."Id", t."Id", t."SomethingSomething", j0."Id", j0."Name", j0."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT DISTINCT j."Id", o."SomethingSomething"
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf}') AS ("SomethingSomething" text)) WITH ORDINALITY AS o
) AS t ON TRUE
LEFT JOIN "JsonEntitiesBasicForCollection" AS j0 ON j."Id" = j0."ParentId"
ORDER BY j."Id" NULLS FIRST, t."SomethingSomething" NULLS FIRST
""");
    }

    public override async Task Json_collection_SelectMany(bool async)
    {
        await base.Json_collection_SelectMany(async);

        AssertSql(
            """
SELECT j."Id", o."Name", o."Names", o."Number", o."Numbers", o."OwnedCollectionBranch", o."OwnedReferenceBranch"
FROM "JsonEntitiesBasic" AS j
JOIN LATERAL ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
    "Name" text,
    "Names" text[],
    "Number" integer,
    "Numbers" integer[],
    "OwnedCollectionBranch" jsonb,
    "OwnedReferenceBranch" jsonb
)) WITH ORDINALITY AS o ON TRUE
""");
    }

    public override async Task Json_nested_collection_SelectMany(bool async)
    {
        await base.Json_nested_collection_SelectMany(async);

        AssertSql(
            """
SELECT j."Id", o."Date", o."Enum", o."Enums", o."Fraction", o."NullableEnum", o."NullableEnums", o."OwnedCollectionLeaf", o."OwnedReferenceLeaf"
FROM "JsonEntitiesBasic" AS j
JOIN LATERAL ROWS FROM (jsonb_to_recordset(j."OwnedReferenceRoot" -> 'OwnedCollectionBranch') AS (
    "Date" timestamp without time zone,
    "Enum" integer,
    "Enums" integer[],
    "Fraction" numeric(18,2),
    "NullableEnum" integer,
    "NullableEnums" integer[],
    "OwnedCollectionLeaf" jsonb,
    "OwnedReferenceLeaf" jsonb
)) WITH ORDINALITY AS o ON TRUE
""");
    }

    public override async Task Json_collection_of_primitives_SelectMany(bool async)
    {
        await base.Json_collection_of_primitives_SelectMany(async);

        AssertSql("");
    }

    public override async Task Json_collection_of_primitives_index_used_in_predicate(bool async)
    {
        await base.Json_collection_of_primitives_index_used_in_predicate(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE ((ARRAY(SELECT CAST(element AS text) FROM jsonb_array_elements_text(j."OwnedReferenceRoot" -> 'Names') WITH ORDINALITY AS t(element) ORDER BY ordinality)))[1] = 'e1_r1'
""");
    }

    public override async Task Json_collection_of_primitives_index_used_in_projection(bool async)
    {
        await base.Json_collection_of_primitives_index_used_in_projection(async);

        AssertSql(
            """
SELECT ((ARRAY(SELECT CAST(element AS integer) FROM jsonb_array_elements_text(j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,Enums}') WITH ORDINALITY AS t(element) ORDER BY ordinality)))[1]
FROM "JsonEntitiesBasic" AS j
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_collection_of_primitives_index_used_in_orderby(bool async)
    {
        await base.Json_collection_of_primitives_index_used_in_orderby(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
ORDER BY ((ARRAY(SELECT CAST(element AS integer) FROM jsonb_array_elements_text(j."OwnedReferenceRoot" -> 'Numbers') WITH ORDINALITY AS t(element) ORDER BY ordinality)))[1] NULLS FIRST
""");
    }

    public override async Task Json_collection_of_primitives_contains_in_predicate(bool async)
    {
        await base.Json_collection_of_primitives_contains_in_predicate(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
WHERE 'e1_r1' = ANY ((ARRAY(SELECT CAST(element AS text) FROM jsonb_array_elements_text(j."OwnedReferenceRoot" -> 'Names') WITH ORDINALITY AS t(element) ORDER BY ordinality)))
""");
    }

    public override async Task Json_collection_index_with_parameter_Select_ElementAt(bool async)
    {
        await base.Json_collection_index_with_parameter_Select_ElementAt(async);

        AssertSql(
            """
@__prm_0='0'

SELECT j."Id", (
    SELECT 'Foo'
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot" #> ARRAY[@__prm_0,'OwnedCollectionBranch']::text[]) AS (
        "Date" timestamp without time zone,
        "Enum" integer,
        "Enums" integer[],
        "Fraction" numeric(18,2),
        "NullableEnum" integer,
        "NullableEnums" integer[],
        "OwnedCollectionLeaf" jsonb,
        "OwnedReferenceLeaf" jsonb
    )) WITH ORDINALITY AS o
    LIMIT 1 OFFSET 0) AS "CollectionElement"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_with_expression_Select_ElementAt(bool async)
    {
        await base.Json_collection_index_with_expression_Select_ElementAt(async);

        AssertSql(
            """
@__prm_0='0'

SELECT j."OwnedCollectionRoot" #>> ARRAY[@__prm_0 + j."Id",'OwnedCollectionBranch',0,'OwnedReferenceLeaf','SomethingSomething']::text[]
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_Select_entity_collection_ElementAt(bool async)
    {
        await base.Json_collection_Select_entity_collection_ElementAt(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" #> '{0,OwnedCollectionBranch}', j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_Select_entity_ElementAt(bool async)
    {
        await base.Json_collection_Select_entity_ElementAt(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" #> '{0,OwnedReferenceBranch}', j."Id"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_Select_entity_in_anonymous_object_ElementAt(bool async)
    {
        await base.Json_collection_Select_entity_in_anonymous_object_ElementAt(async);

        AssertSql(
            """
SELECT t.c, t."Id", t.c0
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT o."OwnedReferenceBranch" AS c, j."Id", 1 AS c0
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    LIMIT 1 OFFSET 0
) AS t ON TRUE
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_collection_Select_entity_with_initializer_ElementAt(bool async)
    {
        await base.Json_collection_Select_entity_with_initializer_ElementAt(async);

        AssertSql(
            """
SELECT t."Id", t.c
FROM "JsonEntitiesBasic" AS j
LEFT JOIN LATERAL (
    SELECT j."Id", 1 AS c
    FROM ROWS FROM (jsonb_to_recordset(j."OwnedCollectionRoot") AS (
        "Name" text,
        "Names" text[],
        "Number" integer,
        "Numbers" integer[],
        "OwnedCollectionBranch" jsonb,
        "OwnedReferenceBranch" jsonb
    )) WITH ORDINALITY AS o
    LIMIT 1 OFFSET 0
) AS t ON TRUE
""");
    }

    public override async Task Json_projection_deduplication_with_collection_indexer_in_original(bool async)
    {
        await base.Json_projection_deduplication_with_collection_indexer_in_original(async);

        AssertSql(
            """
SELECT j."Id", j."OwnedCollectionRoot" #> '{0,OwnedReferenceBranch}', j."OwnedCollectionRoot" -> 0, j."OwnedCollectionRoot" #> '{0,OwnedReferenceBranch,OwnedCollectionLeaf}'
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_projection_deduplication_with_collection_indexer_in_target(bool async)
    {
        await base.Json_projection_deduplication_with_collection_indexer_in_target(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id", j."OwnedReferenceRoot" #> '{OwnedCollectionBranch,1}', j."OwnedReferenceRoot", j."OwnedReferenceRoot" #> ARRAY['OwnedReferenceBranch','OwnedCollectionLeaf',@__prm_0]::text[], @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_projection_deduplication_with_collection_in_original_and_collection_indexer_in_target(bool async)
    {
        await base.Json_projection_deduplication_with_collection_in_original_and_collection_indexer_in_target(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."OwnedReferenceRoot" #> ARRAY['OwnedCollectionBranch',0,'OwnedCollectionLeaf',@__prm_0]::text[], j."Id", @__prm_0, j."OwnedReferenceRoot" #> ARRAY['OwnedCollectionBranch',@__prm_0]::text[], j."OwnedReferenceRoot" -> 'OwnedCollectionBranch', j."OwnedReferenceRoot" #> '{OwnedCollectionBranch,0}'
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_using_constant_when_owner_is_present(bool async)
    {
        await base.Json_collection_index_in_projection_using_constant_when_owner_is_present(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j."OwnedCollectionRoot" -> 1
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_using_constant_when_owner_is_not_present(bool async)
    {
        await base.Json_collection_index_in_projection_using_constant_when_owner_is_not_present(async);

        AssertSql(
            """
SELECT j."Id", j."OwnedCollectionRoot" -> 1
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_using_parameter_when_owner_is_present(bool async)
    {
        await base.Json_collection_index_in_projection_using_parameter_when_owner_is_present(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j."OwnedCollectionRoot" -> @__prm_0, @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_using_parameter_when_owner_is_not_present(bool async)
    {
        await base.Json_collection_index_in_projection_using_parameter_when_owner_is_not_present(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id", j."OwnedCollectionRoot" -> @__prm_0, @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_present(bool async)
    {
        await base.Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_present(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j."OwnedCollectionRoot" #> '{1,OwnedCollectionBranch}'
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_not_present(bool async)
    {
        await base.Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_not_present(async);

        AssertSql(
            """
SELECT j."Id", j."OwnedCollectionRoot" #> '{1,OwnedCollectionBranch}'
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_present(bool async)
    {
        await base.Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_present(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j."OwnedCollectionRoot" #> ARRAY[@__prm_0,'OwnedCollectionBranch']::text[], @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_not_present(bool async)
    {
        await base.Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_not_present(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id", j."OwnedCollectionRoot" #> ARRAY[@__prm_0,'OwnedCollectionBranch']::text[], @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_when_owner_is_present_misc1(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_present_misc1(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j."OwnedCollectionRoot" #> ARRAY[1,'OwnedCollectionBranch',@__prm_0]::text[], @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_when_owner_is_not_present_misc1(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_not_present_misc1(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id", j."OwnedCollectionRoot" #> ARRAY[1,'OwnedCollectionBranch',@__prm_0]::text[], @__prm_0
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_when_owner_is_present_misc2(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_present_misc2(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf,1}'
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_when_owner_is_not_present_misc2(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_not_present_misc2(async);

        AssertSql(
            """
SELECT j."Id", j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf,1}'
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_when_owner_is_present_multiple(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_present_multiple(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j."OwnedCollectionRoot" #> ARRAY[@__prm_0,'OwnedCollectionBranch',1]::text[], @__prm_0, j."OwnedCollectionRoot" #> '{1,OwnedCollectionBranch,1,OwnedReferenceLeaf}', j."OwnedCollectionRoot" #> '{1,OwnedReferenceBranch}', j."OwnedCollectionRoot" #> ARRAY[@__prm_0,'OwnedReferenceBranch']::text[], j."OwnedCollectionRoot" #> ARRAY[@__prm_0,'OwnedCollectionBranch',j."Id"]::text[], j."OwnedCollectionRoot" #> ARRAY[j."Id",'OwnedCollectionBranch',1,'OwnedReferenceLeaf']::text[], j."OwnedCollectionRoot" #> '{1,OwnedReferenceBranch}', j."OwnedCollectionRoot" #> ARRAY[j."Id",'OwnedReferenceBranch']::text[], j."OwnedCollectionRoot" #> ARRAY[j."Id",'OwnedCollectionBranch',j."Id"]::text[]
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_collection_index_in_projection_when_owner_is_not_present_multiple(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_not_present_multiple(async);

        AssertSql(
            """
@__prm_0='1'

SELECT j."Id", j."OwnedCollectionRoot" #> ARRAY[@__prm_0,'OwnedCollectionBranch',1]::text[], @__prm_0, j."OwnedCollectionRoot" #> '{1,OwnedCollectionBranch,1,OwnedReferenceLeaf}', j."OwnedCollectionRoot" #> '{1,OwnedReferenceBranch}', j."OwnedCollectionRoot" #> ARRAY[@__prm_0,'OwnedReferenceBranch']::text[], j."OwnedCollectionRoot" #> ARRAY[@__prm_0,'OwnedCollectionBranch',j."Id"]::text[], j."OwnedCollectionRoot" #> ARRAY[j."Id",'OwnedCollectionBranch',1,'OwnedReferenceLeaf']::text[], j."OwnedCollectionRoot" #> '{1,OwnedReferenceBranch}', j."OwnedCollectionRoot" #> ARRAY[j."Id",'OwnedReferenceBranch']::text[], j."OwnedCollectionRoot" #> ARRAY[j."Id",'OwnedCollectionBranch',j."Id"]::text[]
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_scalar_required_null_semantics(bool async)
    {
        await base.Json_scalar_required_null_semantics(async);

        AssertSql(
            """
SELECT j."Name"
FROM "JsonEntitiesBasic" AS j
WHERE (CAST(j."OwnedReferenceRoot" ->> 'Number' AS integer)) <> length(j."OwnedReferenceRoot" ->> 'Name')::int OR (j."OwnedReferenceRoot" ->> 'Name') IS NULL
""");
    }

    public override async Task Json_scalar_optional_null_semantics(bool async)
    {
        await base.Json_scalar_optional_null_semantics(async);

        AssertSql(
            """
SELECT j."Name"
FROM "JsonEntitiesBasic" AS j
WHERE ((j."OwnedReferenceRoot" ->> 'Name') <> (j."OwnedReferenceRoot" #>> '{OwnedReferenceBranch,OwnedReferenceLeaf,SomethingSomething}') OR (j."OwnedReferenceRoot" ->> 'Name') IS NULL OR (j."OwnedReferenceRoot" #>> '{OwnedReferenceBranch,OwnedReferenceLeaf,SomethingSomething}') IS NULL) AND ((j."OwnedReferenceRoot" ->> 'Name') IS NOT NULL OR (j."OwnedReferenceRoot" #>> '{OwnedReferenceBranch,OwnedReferenceLeaf,SomethingSomething}') IS NOT NULL)
""");
    }

    public override async Task Group_by_on_json_scalar(bool async)
    {
        await base.Group_by_on_json_scalar(async);

        AssertSql(
            """
SELECT t."Key", count(*)::int AS "Count"
FROM (
    SELECT j."OwnedReferenceRoot" ->> 'Name' AS "Key"
    FROM "JsonEntitiesBasic" AS j
) AS t
GROUP BY t."Key"
""");
    }

    public override async Task Group_by_on_json_scalar_using_collection_indexer(bool async)
    {
        await base.Group_by_on_json_scalar_using_collection_indexer(async);

        AssertSql(
            """
SELECT t."Key", count(*)::int AS "Count"
FROM (
    SELECT j."OwnedCollectionRoot" #>> '{0,Name}' AS "Key"
    FROM "JsonEntitiesBasic" AS j
) AS t
GROUP BY t."Key"
""");
    }

    public override async Task Group_by_First_on_json_scalar(bool async)
    {
        await base.Group_by_First_on_json_scalar(async);

        AssertSql(
            """
SELECT t1."Id", t1."EntityBasicId", t1."Name", t1.c, t1.c0
FROM (
    SELECT t."Key"
    FROM (
        SELECT j."OwnedReferenceRoot" ->> 'Name' AS "Key"
        FROM "JsonEntitiesBasic" AS j
    ) AS t
    GROUP BY t."Key"
) AS t0
LEFT JOIN (
    SELECT t2."Id", t2."EntityBasicId", t2."Name", t2.c AS c, t2.c0 AS c0, t2."Key"
    FROM (
        SELECT t3."Id", t3."EntityBasicId", t3."Name", t3.c AS c, t3.c0 AS c0, t3."Key", ROW_NUMBER() OVER(PARTITION BY t3."Key" ORDER BY t3."Id" NULLS FIRST) AS row
        FROM (
            SELECT j0."Id", j0."EntityBasicId", j0."Name", j0."OwnedCollectionRoot" AS c, j0."OwnedReferenceRoot" AS c0, j0."OwnedReferenceRoot" ->> 'Name' AS "Key"
            FROM "JsonEntitiesBasic" AS j0
        ) AS t3
    ) AS t2
    WHERE t2.row <= 1
) AS t1 ON t0."Key" = t1."Key"
""");
    }

    public override async Task Group_by_FirstOrDefault_on_json_scalar(bool async)
    {
        await base.Group_by_FirstOrDefault_on_json_scalar(async);

        AssertSql(
            """
SELECT t1."Id", t1."EntityBasicId", t1."Name", t1.c, t1.c0
FROM (
    SELECT t."Key"
    FROM (
        SELECT j."OwnedReferenceRoot" ->> 'Name' AS "Key"
        FROM "JsonEntitiesBasic" AS j
    ) AS t
    GROUP BY t."Key"
) AS t0
LEFT JOIN (
    SELECT t2."Id", t2."EntityBasicId", t2."Name", t2.c AS c, t2.c0 AS c0, t2."Key"
    FROM (
        SELECT t3."Id", t3."EntityBasicId", t3."Name", t3.c AS c, t3.c0 AS c0, t3."Key", ROW_NUMBER() OVER(PARTITION BY t3."Key" ORDER BY t3."Id" NULLS FIRST) AS row
        FROM (
            SELECT j0."Id", j0."EntityBasicId", j0."Name", j0."OwnedCollectionRoot" AS c, j0."OwnedReferenceRoot" AS c0, j0."OwnedReferenceRoot" ->> 'Name' AS "Key"
            FROM "JsonEntitiesBasic" AS j0
        ) AS t3
    ) AS t2
    WHERE t2.row <= 1
) AS t1 ON t0."Key" = t1."Key"
""");
    }

    public override async Task Group_by_Skip_Take_on_json_scalar(bool async)
    {
        await base.Group_by_Skip_Take_on_json_scalar(async);

        AssertSql(
            """
SELECT t0."Key", t1."Id", t1."EntityBasicId", t1."Name", t1.c, t1.c0
FROM (
    SELECT t."Key"
    FROM (
        SELECT j."OwnedReferenceRoot" ->> 'Name' AS "Key"
        FROM "JsonEntitiesBasic" AS j
    ) AS t
    GROUP BY t."Key"
) AS t0
LEFT JOIN (
    SELECT t2."Id", t2."EntityBasicId", t2."Name", t2.c, t2.c0, t2."Key"
    FROM (
        SELECT t3."Id", t3."EntityBasicId", t3."Name", t3.c AS c, t3.c0 AS c0, t3."Key", ROW_NUMBER() OVER(PARTITION BY t3."Key" ORDER BY t3."Id" NULLS FIRST) AS row
        FROM (
            SELECT j0."Id", j0."EntityBasicId", j0."Name", j0."OwnedCollectionRoot" AS c, j0."OwnedReferenceRoot" AS c0, j0."OwnedReferenceRoot" ->> 'Name' AS "Key"
            FROM "JsonEntitiesBasic" AS j0
        ) AS t3
    ) AS t2
    WHERE 1 < t2.row AND t2.row <= 6
) AS t1 ON t0."Key" = t1."Key"
ORDER BY t0."Key" NULLS FIRST, t1."Key" NULLS FIRST, t1."Id" NULLS FIRST
""");
    }

    public override async Task Group_by_json_scalar_Orderby_json_scalar_FirstOrDefault(bool async)
    {
        await base.Group_by_json_scalar_Orderby_json_scalar_FirstOrDefault(async);

        AssertSql(
            @"");
    }

    public override async Task Group_by_json_scalar_Skip_First_project_json_scalar(bool async)
    {
        await base.Group_by_json_scalar_Skip_First_project_json_scalar(async);

        AssertSql(
            """
SELECT (
    SELECT CAST(t0.c0 #>> '{OwnedReferenceBranch,Enum}' AS integer)
    FROM (
        SELECT j0."Id", j0."EntityBasicId", j0."Name", j0."OwnedCollectionRoot" AS c, j0."OwnedReferenceRoot" AS c0, j0."OwnedReferenceRoot" ->> 'Name' AS "Key"
        FROM "JsonEntitiesBasic" AS j0
    ) AS t0
    WHERE t."Key" = t0."Key" OR (t."Key" IS NULL AND t0."Key" IS NULL)
    LIMIT 1)
FROM (
    SELECT j."OwnedReferenceRoot" ->> 'Name' AS "Key"
    FROM "JsonEntitiesBasic" AS j
) AS t
GROUP BY t."Key"
""");
    }

    public override async Task Json_with_include_on_json_entity(bool async)
    {
        await base.Json_with_include_on_json_entity(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Json_with_include_on_entity_reference(bool async)
    {
        await base.Json_with_include_on_entity_reference(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j0."Id", j0."Name", j0."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesBasicForReference" AS j0 ON j."Id" = j0."ParentId"
""");
    }

    public override async Task Json_with_include_on_entity_collection(bool async)
    {
        await base.Json_with_include_on_entity_collection(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j0."Id", j0."Name", j0."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesBasicForCollection" AS j0 ON j."Id" = j0."ParentId"
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Entity_including_collection_with_json(bool async)
    {
        await base.Entity_including_collection_with_json(async);

        AssertSql(
            """
SELECT e."Id", e."Name", j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "EntitiesBasic" AS e
LEFT JOIN "JsonEntitiesBasic" AS j ON e."Id" = j."EntityBasicId"
ORDER BY e."Id" NULLS FIRST
""");
    }

    public override async Task Json_with_include_on_entity_collection_and_reference(bool async)
    {
        await base.Json_with_include_on_entity_collection_and_reference(async);

        AssertSql(
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot", j0."Id", j0."Name", j0."ParentId", j1."Id", j1."Name", j1."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesBasicForReference" AS j0 ON j."Id" = j0."ParentId"
LEFT JOIN "JsonEntitiesBasicForCollection" AS j1 ON j."Id" = j1."ParentId"
ORDER BY j."Id" NULLS FIRST, j0."Id" NULLS FIRST
""");
    }

    public override async Task Json_with_projection_of_json_reference_leaf_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_json_reference_leaf_and_entity_collection(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedReferenceLeaf}', j."Id", j0."Id", j0."Name", j0."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesBasicForCollection" AS j0 ON j."Id" = j0."ParentId"
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_with_projection_of_json_reference_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_json_reference_and_entity_collection(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot", j."Id", j0."Id", j0."Name", j0."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesBasicForCollection" AS j0 ON j."Id" = j0."ParentId"
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_with_projection_of_multiple_json_references_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_multiple_json_references_and_entity_collection(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot", j."Id", j."OwnedCollectionRoot" #> '{0,OwnedReferenceBranch}', j0."Id", j0."Name", j0."ParentId", j."OwnedCollectionRoot" #> '{1,OwnedReferenceBranch,OwnedReferenceLeaf}', j."OwnedCollectionRoot" #> '{0,OwnedCollectionBranch,0,OwnedReferenceLeaf}'
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesBasicForCollection" AS j0 ON j."Id" = j0."ParentId"
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_with_projection_of_json_collection_leaf_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_json_collection_leaf_and_entity_collection(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf}', j."Id", j0."Id", j0."Name", j0."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesBasicForCollection" AS j0 ON j."Id" = j0."ParentId"
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_with_projection_of_json_collection_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_json_collection_and_entity_collection(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot", j."Id", j0."Id", j0."Name", j0."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesBasicForCollection" AS j0 ON j."Id" = j0."ParentId"
ORDER BY j."Id" NULLS FIRST
""");
    }

    public override async Task Json_with_projection_of_json_collection_element_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_json_collection_element_and_entity_collection(async);

        AssertSql(
            """
SELECT j."OwnedCollectionRoot" -> 0, j."Id", j0."Id", j0."Name", j0."ParentId", j1."Id", j1."Name", j1."ParentId"
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesBasicForReference" AS j0 ON j."Id" = j0."ParentId"
LEFT JOIN "JsonEntitiesBasicForCollection" AS j1 ON j."Id" = j1."ParentId"
ORDER BY j."Id" NULLS FIRST, j0."Id" NULLS FIRST
""");
    }

    public override async Task Json_with_projection_of_mix_of_json_collections_json_references_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_mix_of_json_collections_json_references_and_entity_collection(async);

        AssertSql(
            """
SELECT j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf}', j."Id", j0."Id", j0."Name", j0."ParentId", j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedReferenceLeaf}', j1."Id", j1."Name", j1."ParentId", j."OwnedReferenceRoot" #> '{OwnedReferenceBranch,OwnedCollectionLeaf,0}', j."OwnedReferenceRoot" -> 'OwnedCollectionBranch', j."OwnedCollectionRoot", j."OwnedCollectionRoot" #> '{0,OwnedReferenceBranch}', j."OwnedCollectionRoot" #> '{0,OwnedCollectionBranch}'
FROM "JsonEntitiesBasic" AS j
LEFT JOIN "JsonEntitiesBasicForReference" AS j0 ON j."Id" = j0."ParentId"
LEFT JOIN "JsonEntitiesBasicForCollection" AS j1 ON j."Id" = j1."ParentId"
ORDER BY j."Id" NULLS FIRST, j0."Id" NULLS FIRST
""");

//        AssertSql(
//"""
//SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf'), [j1].[Id], [j1].[Name], [j1].[ParentId], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch'), [j].[OwnedCollectionRoot]
//FROM [JsonEntitiesBasic] AS [j]
//LEFT JOIN [JsonEntitiesBasicForReference] AS [j0] ON [j].[Id] = [j0].[ParentId]
//LEFT JOIN [JsonEntitiesBasicForCollection] AS [j1] ON [j].[Id] = [j1].[ParentId]
//ORDER BY [j].[Id], [j0].[Id]
//""");
    }

    public override async Task Json_all_types_entity_projection(bool async)
    {
        await base.Json_all_types_entity_projection(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
""");
    }

    public override async Task Json_all_types_projection_from_owned_entity_reference(bool async)
    {
        await base.Json_all_types_projection_from_owned_entity_reference(async);

        AssertSql(
            """
SELECT j."Reference", j."Id"
FROM "JsonEntitiesAllTypes" AS j
""");
    }

    public override async Task Json_all_types_projection_individual_properties(bool async)
    {
        await base.Json_all_types_projection_individual_properties(async);

        AssertSql(
            """
SELECT j."Reference" ->> 'TestDefaultString' AS "TestDefaultString", j."Reference" ->> 'TestMaxLengthString' AS "TestMaxLengthString", CAST(j."Reference" ->> 'TestBoolean' AS boolean) AS "TestBoolean", CAST(j."Reference" ->> 'TestByte' AS smallint) AS "TestByte", CAST(j."Reference" ->> 'TestCharacter' AS character(1)) AS "TestCharacter", CAST(j."Reference" ->> 'TestDateTime' AS timestamp without time zone) AS "TestDateTime", CAST(j."Reference" ->> 'TestDateTimeOffset' AS timestamp with time zone) AS "TestDateTimeOffset", CAST(j."Reference" ->> 'TestDecimal' AS numeric(18,3)) AS "TestDecimal", CAST(j."Reference" ->> 'TestDouble' AS double precision) AS "TestDouble", CAST(j."Reference" ->> 'TestGuid' AS uuid) AS "TestGuid", CAST(j."Reference" ->> 'TestInt16' AS smallint) AS "TestInt16", CAST(j."Reference" ->> 'TestInt32' AS integer) AS "TestInt32", CAST(j."Reference" ->> 'TestInt64' AS bigint) AS "TestInt64", CAST(j."Reference" ->> 'TestSignedByte' AS smallint) AS "TestSignedByte", CAST(j."Reference" ->> 'TestSingle' AS real) AS "TestSingle", CAST(j."Reference" ->> 'TestTimeSpan' AS interval) AS "TestTimeSpan", CAST(j."Reference" ->> 'TestDateOnly' AS date) AS "TestDateOnly", CAST(j."Reference" ->> 'TestTimeOnly' AS time without time zone) AS "TestTimeOnly", CAST(j."Reference" ->> 'TestUnsignedInt16' AS integer) AS "TestUnsignedInt16", CAST(j."Reference" ->> 'TestUnsignedInt32' AS bigint) AS "TestUnsignedInt32", CAST(j."Reference" ->> 'TestUnsignedInt64' AS numeric(20,0)) AS "TestUnsignedInt64", CAST(j."Reference" ->> 'TestEnum' AS integer) AS "TestEnum", CAST(j."Reference" ->> 'TestEnumWithIntConverter' AS integer) AS "TestEnumWithIntConverter", CAST(j."Reference" ->> 'TestNullableEnum' AS integer) AS "TestNullableEnum", CAST(j."Reference" ->> 'TestNullableEnumWithIntConverter' AS integer) AS "TestNullableEnumWithIntConverter", j."Reference" ->> 'TestNullableEnumWithConverterThatHandlesNulls' AS "TestNullableEnumWithConverterThatHandlesNulls"
FROM "JsonEntitiesAllTypes" AS j
""");
    }

    public override async Task Json_boolean_predicate(bool async)
    {
        await base.Json_boolean_predicate(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE CAST(j."Reference" ->> 'TestBoolean' AS boolean)
""");
    }

    public override async Task Json_boolean_predicate_negated(bool async)
    {
        await base.Json_boolean_predicate_negated(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE NOT (CAST(j."Reference" ->> 'TestBoolean' AS boolean))
""");
    }

    public override async Task Json_boolean_projection(bool async)
    {
        await base.Json_boolean_projection(async);

        AssertSql(
            """
SELECT CAST(j."Reference" ->> 'TestBoolean' AS boolean)
FROM "JsonEntitiesAllTypes" AS j
""");
    }

    public override async Task Json_boolean_projection_negated(bool async)
    {
        await base.Json_boolean_projection_negated(async);

        AssertSql(
            """
SELECT NOT (CAST(j."Reference" ->> 'TestBoolean' AS boolean))
FROM "JsonEntitiesAllTypes" AS j
""");
    }

    public override async Task Json_predicate_on_default_string(bool async)
    {
        await base.Json_predicate_on_default_string(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (j."Reference" ->> 'TestDefaultString') <> 'MyDefaultStringInReference1' OR (j."Reference" ->> 'TestDefaultString') IS NULL
""");
    }

    public override async Task Json_predicate_on_max_length_string(bool async)
    {
        await base.Json_predicate_on_max_length_string(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (j."Reference" ->> 'TestMaxLengthString') <> 'Foo' OR (j."Reference" ->> 'TestMaxLengthString') IS NULL
""");
    }

    public override async Task Json_predicate_on_string_condition(bool async)
    {
        await base.Json_predicate_on_string_condition(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE CASE
    WHEN NOT (CAST(j."Reference" ->> 'TestBoolean' AS boolean)) THEN j."Reference" ->> 'TestMaxLengthString'
    ELSE j."Reference" ->> 'TestDefaultString'
END = 'MyDefaultStringInReference1'
""");
    }

    public override async Task Json_predicate_on_byte(bool async)
    {
        await base.Json_predicate_on_byte(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestByte' AS smallint)) <> 3 OR (CAST(j."Reference" ->> 'TestByte' AS smallint)) IS NULL
""");
    }

    public override async Task Json_predicate_on_character(bool async)
    {
        await base.Json_predicate_on_character(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestCharacter' AS character(1))) <> 'z' OR (CAST(j."Reference" ->> 'TestCharacter' AS character(1))) IS NULL
""");
    }

    public override async Task Json_predicate_on_datetime(bool async)
    {
        await base.Json_predicate_on_datetime(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestDateTime' AS timestamp without time zone)) <> TIMESTAMP '2000-01-03T00:00:00' OR (CAST(j."Reference" ->> 'TestDateTime' AS timestamp without time zone)) IS NULL
""");
    }

    public override async Task Json_predicate_on_datetimeoffset(bool async)
    {
        await base.Json_predicate_on_datetimeoffset(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestDateTimeOffset' AS timestamp with time zone)) <> TIMESTAMPTZ '2000-01-04T00:00:00+03:02' OR (CAST(j."Reference" ->> 'TestDateTimeOffset' AS timestamp with time zone)) IS NULL
""");
    }

    public override async Task Json_predicate_on_decimal(bool async)
    {
        await base.Json_predicate_on_decimal(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestDecimal' AS numeric(18,3))) <> 1.35 OR (CAST(j."Reference" ->> 'TestDecimal' AS numeric(18,3))) IS NULL
""");
    }

    public override async Task Json_predicate_on_double(bool async)
    {
        await base.Json_predicate_on_double(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestDouble' AS double precision)) <> 33.25 OR (CAST(j."Reference" ->> 'TestDouble' AS double precision)) IS NULL
""");
    }

    public override async Task Json_predicate_on_enum(bool async)
    {
        await base.Json_predicate_on_enum(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestEnum' AS integer)) <> 1 OR (CAST(j."Reference" ->> 'TestEnum' AS integer)) IS NULL
""");
    }

    public override async Task Json_predicate_on_enumwithintconverter(bool async)
    {
        await base.Json_predicate_on_enumwithintconverter(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestEnumWithIntConverter' AS integer)) <> 2 OR (CAST(j."Reference" ->> 'TestEnumWithIntConverter' AS integer)) IS NULL
""");
    }

    public override async Task Json_predicate_on_guid(bool async)
    {
        await base.Json_predicate_on_guid(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestGuid' AS uuid)) <> '00000000-0000-0000-0000-000000000000' OR (CAST(j."Reference" ->> 'TestGuid' AS uuid)) IS NULL
""");
    }

    public override async Task Json_predicate_on_int16(bool async)
    {
        await base.Json_predicate_on_int16(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestInt16' AS smallint)) <> 3 OR (CAST(j."Reference" ->> 'TestInt16' AS smallint)) IS NULL
""");
    }

    public override async Task Json_predicate_on_int32(bool async)
    {
        await base.Json_predicate_on_int32(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestInt32' AS integer)) <> 33 OR (CAST(j."Reference" ->> 'TestInt32' AS integer)) IS NULL
""");
    }

    public override async Task Json_predicate_on_int64(bool async)
    {
        await base.Json_predicate_on_int64(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestInt64' AS bigint)) <> 333 OR (CAST(j."Reference" ->> 'TestInt64' AS bigint)) IS NULL
""");
    }

    public override async Task Json_predicate_on_nullableenum1(bool async)
    {
        await base.Json_predicate_on_nullableenum1(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestNullableEnum' AS integer)) <> 0 OR (CAST(j."Reference" ->> 'TestNullableEnum' AS integer)) IS NULL
""");
    }

    public override async Task Json_predicate_on_nullableenum2(bool async)
    {
        await base.Json_predicate_on_nullableenum2(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestNullableEnum' AS integer)) IS NOT NULL
""");
    }

    public override async Task Json_predicate_on_nullableenumwithconverter1(bool async)
    {
        await base.Json_predicate_on_nullableenumwithconverter1(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestNullableEnumWithIntConverter' AS integer)) <> 1 OR (CAST(j."Reference" ->> 'TestNullableEnumWithIntConverter' AS integer)) IS NULL
""");
    }

    public override async Task Json_predicate_on_nullableenumwithconverter2(bool async)
    {
        await base.Json_predicate_on_nullableenumwithconverter2(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestNullableEnumWithIntConverter' AS integer)) IS NOT NULL
""");
    }

    public override async Task Json_predicate_on_nullableenumwithconverterthathandlesnulls1(bool async)
    {
        await base.Json_predicate_on_nullableenumwithconverterthathandlesnulls1(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (j."Reference" ->> 'TestNullableEnumWithConverterThatHandlesNulls') <> 'One' OR (j."Reference" ->> 'TestNullableEnumWithConverterThatHandlesNulls') IS NULL
""");
    }

    public override async Task Json_predicate_on_nullableenumwithconverterthathandlesnulls2(bool async)
    {
        await base.Json_predicate_on_nullableenumwithconverterthathandlesnulls2(async);

        AssertSql(
            """
x
""");
    }

    public override async Task Json_predicate_on_nullableint321(bool async)
    {
        await base.Json_predicate_on_nullableint321(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestNullableInt32' AS integer)) <> 100 OR (CAST(j."Reference" ->> 'TestNullableInt32' AS integer)) IS NULL
""");
    }

    public override async Task Json_predicate_on_nullableint322(bool async)
    {
        await base.Json_predicate_on_nullableint322(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestNullableInt32' AS integer)) IS NOT NULL
""");
    }

    public override async Task Json_predicate_on_signedbyte(bool async)
    {
        await base.Json_predicate_on_signedbyte(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestSignedByte' AS smallint)) <> 100 OR (CAST(j."Reference" ->> 'TestSignedByte' AS smallint)) IS NULL
""");
    }

    public override async Task Json_predicate_on_single(bool async)
    {
        await base.Json_predicate_on_single(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestSingle' AS real)) <> 10.4 OR (CAST(j."Reference" ->> 'TestSingle' AS real)) IS NULL
""");
    }

    public override async Task Json_predicate_on_timespan(bool async)
    {
        await base.Json_predicate_on_timespan(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestTimeSpan' AS interval)) <> INTERVAL '03:02:00' OR (CAST(j."Reference" ->> 'TestTimeSpan' AS interval)) IS NULL
""");
    }

    public override async Task Json_predicate_on_dateonly(bool async)
    {
        await base.Json_predicate_on_dateonly(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestDateOnly' AS date)) <> DATE '0003-02-01' OR (CAST(j."Reference" ->> 'TestDateOnly' AS date)) IS NULL
""");
    }

    public override async Task Json_predicate_on_timeonly(bool async)
    {
        await base.Json_predicate_on_timeonly(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestTimeOnly' AS time without time zone)) <> TIME '03:02:00' OR (CAST(j."Reference" ->> 'TestTimeOnly' AS time without time zone)) IS NULL
""");
    }

    public override async Task Json_predicate_on_unisgnedint16(bool async)
    {
        await base.Json_predicate_on_unisgnedint16(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestUnsignedInt16' AS integer)) <> 100 OR (CAST(j."Reference" ->> 'TestUnsignedInt16' AS integer)) IS NULL
""");
    }

    public override async Task Json_predicate_on_unsignedint32(bool async)
    {
        await base.Json_predicate_on_unsignedint32(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestUnsignedInt32' AS bigint)) <> 1000 OR (CAST(j."Reference" ->> 'TestUnsignedInt32' AS bigint)) IS NULL
""");
    }

    public override async Task Json_predicate_on_unsignedint64(bool async)
    {
        await base.Json_predicate_on_unsignedint64(async);

        AssertSql(
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE (CAST(j."Reference" ->> 'TestUnsignedInt64' AS numeric(20,0))) <> 10000.0 OR (CAST(j."Reference" ->> 'TestUnsignedInt64' AS numeric(20,0))) IS NULL
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_int_zero_one(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_int_zero_one(async);

        AssertSql(
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE (CAST(j."Reference" ->> 'BoolConvertedToIntZeroOne' AS integer)) = 1
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_int_zero_one_with_explicit_comparison(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_int_zero_one_with_explicit_comparison(async);

        AssertSql(
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE (CAST(j."Reference" ->> 'BoolConvertedToIntZeroOne' AS integer)) = 0
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_string_True_False(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_string_True_False(async);

        AssertSql(
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE (j."Reference" ->> 'BoolConvertedToStringTrueFalse') = 'True'
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_string_True_False_with_explicit_comparison(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_string_True_False_with_explicit_comparison(async);

        AssertSql(
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE (j."Reference" ->> 'BoolConvertedToStringTrueFalse') = 'True'
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_string_Y_N(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_string_Y_N(async);

        AssertSql(
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE (j."Reference" ->> 'BoolConvertedToStringYN') = 'Y'
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_string_Y_N_with_explicit_comparison(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_string_Y_N_with_explicit_comparison(async);

        AssertSql(
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE (j."Reference" ->> 'BoolConvertedToStringYN') = 'N'
""");
    }

    public override async Task Json_predicate_on_int_zero_one_converted_to_bool(bool async)
    {
        await base.Json_predicate_on_int_zero_one_converted_to_bool(async);

        AssertSql(
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE (CAST(j."Reference" ->> 'IntZeroOneConvertedToBool' AS boolean)) = TRUE
""");
    }

    public override async Task Json_predicate_on_string_True_False_converted_to_bool(bool async)
    {
        await base.Json_predicate_on_string_True_False_converted_to_bool(async);

        AssertSql(
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE (CAST(j."Reference" ->> 'StringTrueFalseConvertedToBool' AS boolean)) = FALSE
""");
    }

    public override async Task Json_predicate_on_string_Y_N_converted_to_bool(bool async)
    {
        await base.Json_predicate_on_string_Y_N_converted_to_bool(async);

        AssertSql(
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE (CAST(j."Reference" ->> 'StringYNConvertedToBool' AS boolean)) = FALSE
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_basic(bool async)
    {
        await base.FromSql_on_entity_with_json_basic(async);

        AssertSql(
            """
SELECT m."Id", m."EntityBasicId", m."Name", m."OwnedCollectionRoot", m."OwnedReferenceRoot"
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j
) AS m
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_project_json_reference(bool async)
    {
        await base.FromSql_on_entity_with_json_project_json_reference(async);

        AssertSql(
            """
SELECT m."OwnedReferenceRoot" -> 'OwnedReferenceBranch', m."Id"
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j
) AS m
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_project_json_collection(bool async)
    {
        await base.FromSql_on_entity_with_json_project_json_collection(async);

        AssertSql(
            """
SELECT m."OwnedReferenceRoot" -> 'OwnedCollectionBranch', m."Id"
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j
) AS m
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_inheritance_on_base(bool async)
    {
        await base.FromSql_on_entity_with_json_inheritance_on_base(async);

        AssertSql(
            """
SELECT m."Id", m."Discriminator", m."Name", m."Fraction", m."CollectionOnBase", m."ReferenceOnBase", m."CollectionOnDerived", m."ReferenceOnDerived"
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS m
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_inheritance_on_derived(bool async)
    {
        await base.FromSql_on_entity_with_json_inheritance_on_derived(async);

        AssertSql(
            """
SELECT m."Id", m."Discriminator", m."Name", m."Fraction", m."CollectionOnBase", m."ReferenceOnBase", m."CollectionOnDerived", m."ReferenceOnDerived"
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS m
WHERE m."Discriminator" = 'JsonEntityInheritanceDerived'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_inheritance_project_reference_on_base(bool async)
    {
        await base.FromSql_on_entity_with_json_inheritance_project_reference_on_base(async);

        AssertSql(
            """
SELECT m."ReferenceOnBase", m."Id"
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS m
ORDER BY m."Id" NULLS FIRST
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_inheritance_project_reference_on_derived(bool async)
    {
        await base.FromSql_on_entity_with_json_inheritance_project_reference_on_derived(async);

        AssertSql(
            """
SELECT m."CollectionOnDerived", m."Id"
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS m
WHERE m."Discriminator" = 'JsonEntityInheritanceDerived'
ORDER BY m."Id" NULLS FIRST
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class JsonQueryNpgsqlFixture : JsonQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        private JsonQueryData _expectedData;

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            // The tests seed Unspecified DateTimes, but our default mapping for DateTime is timestamptz, which requires UTC.
            // Map these properties to "timestamp without time zone".
            configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
            configurationBuilder.Properties<List<DateTime>>().HaveColumnType("timestamp without time zone[]");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // The following are ignored since we do not support mapping IList<T> (as opposed to array/List) on regular properties
            // (since that's not supported at the ADO.NET layer). However, we do support IList<T> inside JSON documents, since that doesn't
            // rely on ADO.NET support.
            modelBuilder.Entity<JsonEntityAllTypes>(
                b =>
                {
                    b.Ignore(j => j.TestEnumCollection);
                    b.Ignore(j => j.TestUnsignedInt16Collection);
                    b.Ignore(j => j.TestNullableEnumCollection);
                    b.Ignore(j => j.TestNullableEnumWithIntConverterCollection);
                    b.Ignore(j => j.TestCharacterCollection);
                    b.Ignore(j => j.TestNullableInt32Collection);
                    b.Ignore(j => j.TestUnsignedInt64Collection);

                    b.Ignore(j => j.TestByteCollection);
                    b.Ignore(j => j.TestBooleanCollection);
                    b.Ignore(j => j.TestDateTimeOffsetCollection);
                    b.Ignore(j => j.TestDoubleCollection);
                    b.Ignore(j => j.TestInt16Collection);
                });
        }

        public override ISetSource GetExpectedData()
        {
            if (_expectedData is null)
            {
                _expectedData = (JsonQueryData)base.GetExpectedData();

                // The test data contains DateTimeOffsets with various offsets, which we don't support. Change these to UTC.
                // Also chop sub-microsecond precision which PostgreSQL does not support.
                foreach (var j in _expectedData.JsonEntitiesAllTypes)
                {
                    j.Reference.TestDateTimeOffset = new DateTimeOffset(
                        j.Reference.TestDateTimeOffset.Ticks
                        - (j.Reference.TestDateTimeOffset.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero);

                    foreach (var j2 in j.Collection)
                    {
                        j2.TestDateTimeOffset = new DateTimeOffset(
                            j2.TestDateTimeOffset.Ticks - (j2.TestDateTimeOffset.Ticks % (TimeSpan.TicksPerMillisecond / 1000)),
                            TimeSpan.Zero);
                    }

                    j.TestDateTimeOffsetCollection = j.TestDateTimeOffsetCollection.Select(
                        dto => new DateTimeOffset(dto.Ticks - (dto.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero)).ToList();
                }
            }

            return _expectedData;
        }

        protected override void Seed(JsonQueryContext context)
        {
            // The test data contains DateTimeOffsets with various offsets, which we don't support. Change these to UTC.
            // Also chop sub-microsecond precision which PostgreSQL does not support.
            // See https://github.com/dotnet/efcore/issues/26068

            var jsonEntitiesBasic = JsonQueryData.CreateJsonEntitiesBasic();
            var entitiesBasic = JsonQueryData.CreateEntitiesBasic();
            var jsonEntitiesBasicForReference = JsonQueryData.CreateJsonEntitiesBasicForReference();
            var jsonEntitiesBasicForCollection = JsonQueryData.CreateJsonEntitiesBasicForCollection();
            JsonQueryData.WireUp(jsonEntitiesBasic, entitiesBasic, jsonEntitiesBasicForReference, jsonEntitiesBasicForCollection);

            var jsonEntitiesCustomNaming = JsonQueryData.CreateJsonEntitiesCustomNaming();
            var jsonEntitiesSingleOwned = JsonQueryData.CreateJsonEntitiesSingleOwned();
            var jsonEntitiesInheritance = JsonQueryData.CreateJsonEntitiesInheritance();
            var jsonEntitiesAllTypes = JsonQueryData.CreateJsonEntitiesAllTypes();
            var jsonEntitiesConverters = JsonQueryData.CreateJsonEntitiesConverters();

            context.JsonEntitiesBasic.AddRange(jsonEntitiesBasic);
            context.EntitiesBasic.AddRange(entitiesBasic);
            context.JsonEntitiesBasicForReference.AddRange(jsonEntitiesBasicForReference);
            context.JsonEntitiesBasicForCollection.AddRange(jsonEntitiesBasicForCollection);
            context.JsonEntitiesCustomNaming.AddRange(jsonEntitiesCustomNaming);
            context.JsonEntitiesSingleOwned.AddRange(jsonEntitiesSingleOwned);
            context.JsonEntitiesInheritance.AddRange(jsonEntitiesInheritance);
            context.JsonEntitiesAllTypes.AddRange(jsonEntitiesAllTypes);
            context.JsonEntitiesConverters.AddRange(jsonEntitiesConverters);

            foreach (var j in jsonEntitiesAllTypes)
            {
                j.Reference.TestDateTimeOffset = new DateTimeOffset(
                    j.Reference.TestDateTimeOffset.Ticks - (j.Reference.TestDateTimeOffset.Ticks % (TimeSpan.TicksPerMillisecond / 1000)),
                    TimeSpan.Zero);

                foreach (var j2 in j.Collection)
                {
                    j2.TestDateTimeOffset = new DateTimeOffset(
                        j2.TestDateTimeOffset.Ticks - (j2.TestDateTimeOffset.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero);
                }

                j.TestDateTimeOffsetCollection = j.TestDateTimeOffsetCollection.Select(
                    dto => new DateTimeOffset(dto.Ticks - (dto.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero)).ToList();
            }

            context.SaveChanges();
        }
    }
}
