using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit.Sdk;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update;

public class JsonUpdateNpgsqlTest : JsonUpdateTestBase<JsonUpdateNpgsqlTest.JsonUpdateNpgsqlFixture>
{
    public JsonUpdateNpgsqlTest(JsonUpdateNpgsqlFixture fixture)
        : base(fixture)
    {
        ClearLog();
    }

    public override async Task Add_element_to_json_collection_branch()
    {
        await base.Add_element_to_json_collection_branch();

        AssertSql(
            """
@p0='[{"Date":"2101-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":10.1,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":10.2,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}},{"Date":"2010-10-10T00:00:00","Enum":2,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}]' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedCollectionBranch}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Add_element_to_json_collection_leaf()
    {
        await base.Add_element_to_json_collection_leaf();

        AssertSql(
            """
@p0='[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"},{"SomethingSomething":"ss1"}]' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedReferenceBranch,OwnedCollectionLeaf}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Add_element_to_json_collection_on_derived()
    {
        await base.Add_element_to_json_collection_on_derived();

        AssertSql(
            """
@p0='[{"Date":"2221-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":221.1,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"d2_r_c1"},{"SomethingSomething":"d2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"d2_r_r"}},{"Date":"2222-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":222.1,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"d2_r_c1"},{"SomethingSomething":"d2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"d2_r_r"}},{"Date":"2010-10-10T00:00:00","Enum":2,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}]' (Nullable = false) (DbType = Object)
@p1='2'

UPDATE "JsonEntitiesInheritance" SET "CollectionOnDerived" = @p0
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."Discriminator", j."Name", j."Fraction", j."CollectionOnBase", j."ReferenceOnBase", j."CollectionOnDerived", j."ReferenceOnDerived"
FROM "JsonEntitiesInheritance" AS j
WHERE j."Discriminator" = 'JsonEntityInheritanceDerived'
LIMIT 2
""");
    }

    public override async Task Add_element_to_json_collection_root()
    {
        await base.Add_element_to_json_collection_root();

        AssertSql(
            """
@p0='[{"Name":"e1_c1","Names":["e1_c11","e1_c12"],"Number":11,"Numbers":[-1000,0,1000],"OwnedCollectionBranch":[{"Date":"2111-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":11.1,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":11.2,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01T00:00:00","Enum":0,"Enums":[0,-1,1],"Fraction":11.0,"NullableEnum":null,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"e1_c2","Names":["e1_c21","e1_c22"],"Number":12,"Numbers":[-1001,0,1001],"OwnedCollectionBranch":[{"Date":"2121-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":12.1,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01T00:00:00","Enum":0,"Enums":[0,-1,1],"Fraction":12.2,"NullableEnum":null,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":12.0,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}},{"Name":"new Name","Names":null,"Number":142,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":2,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}]' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = @p0
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Add_element_to_json_collection_root_null_navigations()
    {
        await base.Add_element_to_json_collection_root_null_navigations();

        AssertSql(
            """
@p0='[{"Name":"e1_c1","Names":["e1_c11","e1_c12"],"Number":11,"Numbers":[-1000,0,1000],"OwnedCollectionBranch":[{"Date":"2111-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":11.1,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":11.2,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01T00:00:00","Enum":0,"Enums":[0,-1,1],"Fraction":11.0,"NullableEnum":null,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"e1_c2","Names":["e1_c21","e1_c22"],"Number":12,"Numbers":[-1001,0,1001],"OwnedCollectionBranch":[{"Date":"2121-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":12.1,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01T00:00:00","Enum":0,"Enums":[0,-1,1],"Fraction":12.2,"NullableEnum":null,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":12.0,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}},{"Name":"new Name","Names":null,"Number":142,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":2,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":null,"OwnedReferenceLeaf":null}}]' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = @p0
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Add_entity_with_json()
    {
        await base.Add_entity_with_json();

        AssertSql(
            """
@p0='{"Name":"RootName","Names":null,"Number":42,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":2,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}' (Nullable = false) (DbType = Object)
@p1='[]' (Nullable = false) (DbType = Object)
@p2='2'
@p3=NULL (DbType = Int32)
@p4='NewEntity'

INSERT INTO "JsonEntitiesBasic" ("OwnedReferenceRoot", "OwnedCollectionRoot", "Id", "EntityBasicId", "Name")
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Add_entity_with_json_null_navigations()
    {
        await base.Add_entity_with_json_null_navigations();

        AssertSql(
            """
@p0='{"Name":"RootName","Names":null,"Number":42,"Numbers":null,"OwnedCollectionBranch":null,"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":2,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":null}}' (Nullable = false) (DbType = Object)
@p1='2'
@p2=NULL (DbType = Int32)
@p3='NewEntity'

INSERT INTO "JsonEntitiesBasic" ("OwnedReferenceRoot", "Id", "EntityBasicId", "Name")
VALUES (@p0, @p1, @p2, @p3);
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Add_json_reference_leaf()
    {
        await base.Add_json_reference_leaf();

        AssertSql(
            """
@p0='{"SomethingSomething":"ss3"}' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedCollectionBranch,0,OwnedReferenceLeaf}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Add_json_reference_root()
    {
        await base.Add_json_reference_root();

        AssertSql(
            """
@p0='{"Name":"RootName","Names":null,"Number":42,"Numbers":null,"OwnedCollectionBranch":[],"OwnedReferenceBranch":{"Date":"2010-10-10T00:00:00","Enum":2,"Enums":null,"Fraction":42.42,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":[{"SomethingSomething":"ss1"},{"SomethingSomething":"ss2"}],"OwnedReferenceLeaf":{"SomethingSomething":"ss3"}}}' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = @p0
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Delete_entity_with_json()
    {
        await base.Delete_entity_with_json();

        AssertSql(
            """
@p0='1'

DELETE FROM "JsonEntitiesBasic"
WHERE "Id" = @p0;
""",
            //
            """
SELECT count(*)::int
FROM "JsonEntitiesBasic" AS j
""");
    }

    public override async Task Delete_json_collection_branch()
    {
        await base.Delete_json_collection_branch();

        AssertSql(
            """
@p0='null' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedCollectionBranch}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Delete_json_collection_root()
    {
        await base.Delete_json_collection_root();

        AssertSql(
            """
@p0=NULL (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = @p0
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Delete_json_reference_leaf()
    {
        await base.Delete_json_reference_leaf();

        AssertSql(
            """
@p0='null' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedReferenceBranch,OwnedReferenceLeaf}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Delete_json_reference_root()
    {
        await base.Delete_json_reference_root();

        AssertSql(
            """
@p0=NULL (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = @p0
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_element_in_json_collection_branch()
    {
        await base.Edit_element_in_json_collection_branch();

        AssertSql(
            """
@p0='"2111-11-11T00:00:00"' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = jsonb_set("OwnedCollectionRoot", '{0,OwnedCollectionBranch,0,Date}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_element_in_json_collection_root1()
    {
        await base.Edit_element_in_json_collection_root1();

        AssertSql(
            """
@p0='"Modified"' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = jsonb_set("OwnedCollectionRoot", '{0,Name}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_element_in_json_collection_root2()
    {
        await base.Edit_element_in_json_collection_root2();

        AssertSql(
            """
@p0='"Modified"' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = jsonb_set("OwnedCollectionRoot", '{1,Name}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_element_in_json_multiple_levels_partial_update()
    {
        await base.Edit_element_in_json_multiple_levels_partial_update();

        AssertSql(
            """
@p0='[{"Date":"2111-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":11.1,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"...and another"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":11.2,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"yet another change"},{"SomethingSomething":"and another"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}]' (Nullable = false) (DbType = Object)
@p1='{"Name":"edit","Names":["e1_r1","e1_r2"],"Number":10,"Numbers":[-2147483648,-1,0,1,2147483647],"OwnedCollectionBranch":[{"Date":"2101-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":10.1,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":10.2,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}}],"OwnedReferenceBranch":{"Date":"2111-11-11T00:00:00","Enum":0,"Enums":[0,-1,1],"Fraction":10.0,"NullableEnum":null,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_r_r"}}}' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = jsonb_set("OwnedCollectionRoot", '{0,OwnedCollectionBranch}', @p0), "OwnedReferenceRoot" = @p1
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_element_in_json_branch_collection_and_add_element_to_the_same_collection()
    {
        await base.Edit_element_in_json_branch_collection_and_add_element_to_the_same_collection();

        AssertSql(
            """
@p0='[{"Date":"2101-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":4321.3,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c1_c1"},{"SomethingSomething":"e1_r_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c1_r"}},{"Date":"2102-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":10.2,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_c2_c1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_c2_r"}},{"Date":"2222-11-11T00:00:00","Enum":2,"Enums":null,"Fraction":45.32,"NullableEnum":null,"NullableEnums":null,"OwnedCollectionLeaf":null,"OwnedReferenceLeaf":{"SomethingSomething":"cc"}}]' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedCollectionBranch}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_two_elements_in_the_same_json_collection()
    {
        await base.Edit_two_elements_in_the_same_json_collection();

        AssertSql(
            """
@p0='[{"SomethingSomething":"edit1"},{"SomethingSomething":"edit2"}]' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedCollectionBranch,0,OwnedCollectionLeaf}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_two_elements_in_the_same_json_collection_at_the_root()
    {
        await base.Edit_two_elements_in_the_same_json_collection_at_the_root();

        AssertSql(
            """
@p0='[{"Name":"edit1","Names":["e1_c11","e1_c12"],"Number":11,"Numbers":[-1000,0,1000],"OwnedCollectionBranch":[{"Date":"2111-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":11.1,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c1_c1"},{"SomethingSomething":"e1_c1_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c1_r"}},{"Date":"2112-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":11.2,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_c2_c1"},{"SomethingSomething":"e1_c1_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_c2_r"}}],"OwnedReferenceBranch":{"Date":"2110-01-01T00:00:00","Enum":0,"Enums":[0,-1,1],"Fraction":11.0,"NullableEnum":null,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c1_r_c1"},{"SomethingSomething":"e1_c1_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c1_r_r"}}},{"Name":"edit2","Names":["e1_c21","e1_c22"],"Number":12,"Numbers":[-1001,0,1001],"OwnedCollectionBranch":[{"Date":"2121-01-01T00:00:00","Enum":1,"Enums":[0,-1,1],"Fraction":12.1,"NullableEnum":0,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c1_c1"},{"SomethingSomething":"e1_c2_c1_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c1_r"}},{"Date":"2122-01-01T00:00:00","Enum":0,"Enums":[0,-1,1],"Fraction":12.2,"NullableEnum":null,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_c2_c1"},{"SomethingSomething":"e1_c2_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_c2_r"}}],"OwnedReferenceBranch":{"Date":"2120-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":12.0,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_c2_r_c1"},{"SomethingSomething":"e1_c2_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_c2_r_r"}}}]' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = @p0
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_collection_element_and_reference_at_once()
    {
        await base.Edit_collection_element_and_reference_at_once();

        AssertSql(
            """
@p0='{"Date":"2102-01-01T00:00:00","Enum":2,"Enums":[0,-1,1],"Fraction":10.2,"NullableEnum":1,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"edit1"},{"SomethingSomething":"e1_r_c2_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit2"}}' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedCollectionBranch,1}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_single_enum_property()
    {
        await base.Edit_single_enum_property();

        AssertSql(
            """
@p0='1' (Nullable = false) (DbType = Object)
@p1='1' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = jsonb_set("OwnedCollectionRoot", '{1,OwnedCollectionBranch,1,Enum}', @p0), "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedReferenceBranch,Enum}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_single_numeric_property()
    {
        await base.Edit_single_numeric_property();

        AssertSql(
            """
@p0='1024' (Nullable = false) (DbType = Object)
@p1='999' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = jsonb_set("OwnedCollectionRoot", '{1,Number}', @p0), "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{Number}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_single_property_bool()
    {
        await base.Edit_single_property_bool();

        AssertSql(
            """
@p0='true' (Nullable = false) (DbType = Object)
@p1='false' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestBoolean}', @p0), "Reference" = jsonb_set("Reference", '{TestBoolean}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_byte()
    {
        await base.Edit_single_property_byte();

        AssertSql(
            """
@p0='14' (Nullable = false) (DbType = Object)
@p1='25' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestByte}', @p0), "Reference" = jsonb_set("Reference", '{TestByte}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_char()
    {
        await base.Edit_single_property_char();

        AssertSql(
            """
@p0='"t"' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesAllTypes" SET "Reference" = jsonb_set("Reference", '{TestCharacter}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_datetime()
    {
        await base.Edit_single_property_datetime();

        AssertSql(
            """
@p0='"3000-01-01T12:34:56"' (Nullable = false) (DbType = Object)
@p1='"3000-01-01T12:34:56"' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestDateTime}', @p0), "Reference" = jsonb_set("Reference", '{TestDateTime}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_datetimeoffset()
    {
        await base.Edit_single_property_datetimeoffset();

        AssertSql(
            """
@p0='"3000-01-01T12:34:56-04:00"' (Nullable = false) (DbType = Object)
@p1='"3000-01-01T12:34:56-04:00"' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestDateTimeOffset}', @p0), "Reference" = jsonb_set("Reference", '{TestDateTimeOffset}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_decimal()
    {
        await base.Edit_single_property_decimal();

        AssertSql(
            """
@p0='-13579.01' (Nullable = false) (DbType = Object)
@p1='-13579.01' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestDecimal}', @p0), "Reference" = jsonb_set("Reference", '{TestDecimal}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_double()
    {
        await base.Edit_single_property_double();

        AssertSql(
            """
@p0='-1.23579' (Nullable = false) (DbType = Object)
@p1='-1.23579' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestDouble}', @p0), "Reference" = jsonb_set("Reference", '{TestDouble}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_guid()
    {
        await base.Edit_single_property_guid();

        AssertSql(
            """
@p0='"12345678-1234-4321-5555-987654321000"' (Nullable = false) (DbType = Object)
@p1='"12345678-1234-4321-5555-987654321000"' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestGuid}', @p0), "Reference" = jsonb_set("Reference", '{TestGuid}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_int16()
    {
        await base.Edit_single_property_int16();

        AssertSql(
            """
@p0='-3234' (Nullable = false) (DbType = Object)
@p1='-3234' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestInt16}', @p0), "Reference" = jsonb_set("Reference", '{TestInt16}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_int32()
    {
        await base.Edit_single_property_int32();

        AssertSql(
            """
@p0='-3234' (Nullable = false) (DbType = Object)
@p1='-3234' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestInt32}', @p0), "Reference" = jsonb_set("Reference", '{TestInt32}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_int64()
    {
        await base.Edit_single_property_int64();

        AssertSql(
            """
@p0='-3234' (Nullable = false) (DbType = Object)
@p1='-3234' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestInt64}', @p0), "Reference" = jsonb_set("Reference", '{TestInt64}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_signed_byte()
    {
        await base.Edit_single_property_signed_byte();

        AssertSql(
            """
@p0='-108' (Nullable = false) (DbType = Object)
@p1='-108' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestSignedByte}', @p0), "Reference" = jsonb_set("Reference", '{TestSignedByte}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_single()
    {
        await base.Edit_single_property_single();

        AssertSql(
            """
@p0='-7.234' (Nullable = false) (DbType = Object)
@p1='-7.234' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestSingle}', @p0), "Reference" = jsonb_set("Reference", '{TestSingle}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_timespan()
    {
        await base.Edit_single_property_timespan();

        AssertSql(
            """
@p0='"10:01:01.007"' (Nullable = false) (DbType = Object)
@p1='"10:01:01.007"' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestTimeSpan}', @p0), "Reference" = jsonb_set("Reference", '{TestTimeSpan}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_uint16()
    {
        await base.Edit_single_property_uint16();

        AssertSql(
            """
@p0='1534' (Nullable = false) (DbType = Object)
@p1='1534' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestUnsignedInt16}', @p0), "Reference" = jsonb_set("Reference", '{TestUnsignedInt16}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_uint32()
    {
        await base.Edit_single_property_uint32();

        AssertSql(
            """
@p0='1237775789' (Nullable = false) (DbType = Object)
@p1='1237775789' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestUnsignedInt32}', @p0), "Reference" = jsonb_set("Reference", '{TestUnsignedInt32}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_uint64()
    {
        await base.Edit_single_property_uint64();

        AssertSql(
            """
@p0='1234555555123456789' (Nullable = false) (DbType = Object)
@p1='1234555555123456789' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestUnsignedInt64}', @p0), "Reference" = jsonb_set("Reference", '{TestUnsignedInt64}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_int32()
    {
        await base.Edit_single_property_nullable_int32();

        AssertSql(
            """
@p0='122354' (Nullable = false) (DbType = Object)
@p1='64528' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableInt32}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableInt32}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_int32_set_to_null()
    {
        await base.Edit_single_property_nullable_int32_set_to_null();

        AssertSql(
            """
@p0='null' (Nullable = false) (DbType = Object)
@p1='null' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableInt32}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableInt32}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_enum()
    {
        await base.Edit_single_property_enum();

        AssertSql(
            """
@p0='2' (Nullable = false) (DbType = Object)
@p1='2' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestEnum}', @p0), "Reference" = jsonb_set("Reference", '{TestEnum}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_enum_with_int_converter()
    {
        await base.Edit_single_property_enum_with_int_converter();

        AssertSql(
            """
@p0='2' (Nullable = false) (DbType = Object)
@p1='2' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestEnumWithIntConverter}', @p0), "Reference" = jsonb_set("Reference", '{TestEnumWithIntConverter}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum()
    {
        await base.Edit_single_property_nullable_enum();

        AssertSql(
            """
@p0='2' (Nullable = false) (DbType = Object)
@p1='2' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestEnum}', @p0), "Reference" = jsonb_set("Reference", '{TestEnum}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum_set_to_null()
    {
        await base.Edit_single_property_nullable_enum_set_to_null();

        AssertSql(
            """
@p0='null' (Nullable = false) (DbType = Object)
@p1='null' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableEnum}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableEnum}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_int_converter()
    {
        await base.Edit_single_property_nullable_enum_with_int_converter();

        AssertSql(
            """
@p0='0' (Nullable = false) (DbType = Object)
@p1='2' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableEnumWithIntConverter}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableEnumWithIntConverter}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_int_converter_set_to_null()
    {
        await base.Edit_single_property_nullable_enum_with_int_converter_set_to_null();

        AssertSql(
            """
@p0='null' (Nullable = false) (DbType = Object)
@p1='null' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableEnumWithIntConverter}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableEnumWithIntConverter}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_converter_that_handles_nulls()
    {
        await base.Edit_single_property_nullable_enum_with_converter_that_handles_nulls();

        AssertSql(
            """
@p0='"Three"' (Nullable = false) (DbType = Object)
@p1='"One"' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableEnumWithConverterThatHandlesNulls}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableEnumWithConverterThatHandlesNulls}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_nullable_enum_with_converter_that_handles_nulls_set_to_null()
    {
        await base.Edit_single_property_nullable_enum_with_converter_that_handles_nulls_set_to_null();

        AssertSql(
            """
@p0='null' (Nullable = false) (DbType = Object)
@p1='null' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableEnumWithConverterThatHandlesNulls}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableEnumWithConverterThatHandlesNulls}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_two_properties_on_same_entity_updates_the_entire_entity()
    {
        await base.Edit_two_properties_on_same_entity_updates_the_entire_entity();

        AssertSql(
            """
@p0='{"TestBoolean":false,"TestBooleanCollection":[true,false],"TestByte":25,"TestByteCollection":null,"TestCharacter":"h","TestCharacterCollection":["A","B","\u0022"],"TestDateOnly":"2323-04-03","TestDateOnlyCollection":["3234-01-23","4331-01-21"],"TestDateTime":"2100-11-11T12:34:56","TestDateTimeCollection":["2000-01-01T12:34:56","3000-01-01T12:34:56"],"TestDateTimeOffset":"2200-11-11T12:34:56-05:00","TestDateTimeOffsetCollection":["2000-01-01T12:34:56-08:00"],"TestDecimal":-123450.01,"TestDecimalCollection":[-1234567890.01],"TestDefaultString":"MyDefaultStringInCollection1","TestDefaultStringCollection":["S1","\u0022S2\u0022","S3"],"TestDouble":-1.2345,"TestDoubleCollection":[-1.23456789,1.23456789,0],"TestEnum":0,"TestEnumCollection":[0,2,-7],"TestEnumWithIntConverter":1,"TestEnumWithIntConverterCollection":[0,2,-7],"TestGuid":"00000000-0000-0000-0000-000000000000","TestGuidCollection":["12345678-1234-4321-7777-987654321000"],"TestInt16":-12,"TestInt16Collection":[-32768,0,32767],"TestInt32":32,"TestInt32Collection":[-2147483648,0,2147483647],"TestInt64":64,"TestInt64Collection":[-9223372036854775808,0,9223372036854775807],"TestMaxLengthString":"Baz","TestMaxLengthStringCollection":["S1","S2","S3"],"TestNullableEnum":0,"TestNullableEnumCollection":[0,null,2,-7],"TestNullableEnumWithConverterThatHandlesNulls":"Two","TestNullableEnumWithConverterThatHandlesNullsCollection":[0,null,-7],"TestNullableEnumWithIntConverter":2,"TestNullableEnumWithIntConverterCollection":[0,null,2,-7],"TestNullableInt32":90,"TestNullableInt32Collection":[null,-2147483648,0,null,2147483647,null],"TestSignedByte":-18,"TestSignedByteCollection":[-128,0,127],"TestSingle":-1.4,"TestSingleCollection":[-1.234,0,-1.234],"TestTimeOnly":"05:07:08.0000000","TestTimeOnlyCollection":["13:42:23.0000000","07:17:25.0000000"],"TestTimeSpan":"06:05:04.003","TestTimeSpanCollection":["10:09:08.007","-09:50:51.993"],"TestUnsignedInt16":12,"TestUnsignedInt16Collection":[0,0,65535],"TestUnsignedInt32":12345,"TestUnsignedInt32Collection":[0,0,4294967295],"TestUnsignedInt64":1234567867,"TestUnsignedInt64Collection":[0,0,18446744073709551615]}' (Nullable = false) (DbType = Object)
@p1='{"TestBoolean":true,"TestBooleanCollection":[true,false],"TestByte":255,"TestByteCollection":null,"TestCharacter":"a","TestCharacterCollection":["A","B","\u0022"],"TestDateOnly":"2023-10-10","TestDateOnlyCollection":["1234-01-23","4321-01-21"],"TestDateTime":"2000-01-01T12:34:56","TestDateTimeCollection":["2000-01-01T12:34:56","3000-01-01T12:34:56"],"TestDateTimeOffset":"2000-01-01T12:34:56-08:00","TestDateTimeOffsetCollection":["2000-01-01T12:34:56-08:00"],"TestDecimal":-1234567890.01,"TestDecimalCollection":[-1234567890.01],"TestDefaultString":"MyDefaultStringInReference1","TestDefaultStringCollection":["S1","\u0022S2\u0022","S3"],"TestDouble":-1.23456789,"TestDoubleCollection":[-1.23456789,1.23456789,0],"TestEnum":0,"TestEnumCollection":[0,2,-7],"TestEnumWithIntConverter":1,"TestEnumWithIntConverterCollection":[0,2,-7],"TestGuid":"12345678-1234-4321-7777-987654321000","TestGuidCollection":["12345678-1234-4321-7777-987654321000"],"TestInt16":-1234,"TestInt16Collection":[-32768,0,32767],"TestInt32":32,"TestInt32Collection":[-2147483648,0,2147483647],"TestInt64":64,"TestInt64Collection":[-9223372036854775808,0,9223372036854775807],"TestMaxLengthString":"Foo","TestMaxLengthStringCollection":["S1","S2","S3"],"TestNullableEnum":0,"TestNullableEnumCollection":[0,null,2,-7],"TestNullableEnumWithConverterThatHandlesNulls":"Three","TestNullableEnumWithConverterThatHandlesNullsCollection":[0,null,-7],"TestNullableEnumWithIntConverter":1,"TestNullableEnumWithIntConverterCollection":[0,null,2,-7],"TestNullableInt32":78,"TestNullableInt32Collection":[null,-2147483648,0,null,2147483647,null],"TestSignedByte":-128,"TestSignedByteCollection":[-128,0,127],"TestSingle":-1.234,"TestSingleCollection":[-1.234,0,-1.234],"TestTimeOnly":"11:12:13.0000000","TestTimeOnlyCollection":["11:42:23.0000000","07:17:27.0000000"],"TestTimeSpan":"10:09:08.007","TestTimeSpanCollection":["10:09:08.007","-09:50:51.993"],"TestUnsignedInt16":1234,"TestUnsignedInt16Collection":[0,0,65535],"TestUnsignedInt32":1234565789,"TestUnsignedInt32Collection":[0,0,4294967295],"TestUnsignedInt64":1234567890123456789,"TestUnsignedInt64Collection":[0,0,18446744073709551615]}' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0}', @p0), "Reference" = @p1
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_a_scalar_property_and_reference_navigation_on_the_same_entity()
    {
        await base.Edit_a_scalar_property_and_reference_navigation_on_the_same_entity();

        AssertSql(
            """
@p0='{"Date":"2100-01-01T00:00:00","Enum":0,"Enums":[0,-1,1],"Fraction":523.532,"NullableEnum":null,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit"}}' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedReferenceBranch}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_a_scalar_property_and_collection_navigation_on_the_same_entity()
    {
        await base.Edit_a_scalar_property_and_collection_navigation_on_the_same_entity();

        AssertSql(
            """
@p0='{"Date":"2100-01-01T00:00:00","Enum":0,"Enums":[0,-1,1],"Fraction":523.532,"NullableEnum":null,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"edit"}],"OwnedReferenceLeaf":{"SomethingSomething":"e1_r_r_r"}}' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedReferenceBranch}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_a_scalar_property_and_another_property_behind_reference_navigation_on_the_same_entity()
    {
        await base.Edit_a_scalar_property_and_another_property_behind_reference_navigation_on_the_same_entity();

        AssertSql(
            """
@p0='{"Date":"2100-01-01T00:00:00","Enum":0,"Enums":[0,-1,1],"Fraction":523.532,"NullableEnum":null,"NullableEnums":[null,-1,1],"OwnedCollectionLeaf":[{"SomethingSomething":"e1_r_r_c1"},{"SomethingSomething":"e1_r_r_c2"}],"OwnedReferenceLeaf":{"SomethingSomething":"edit"}}' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesBasic" SET "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{OwnedReferenceBranch}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_single_property_with_converter_bool_to_int_zero_one()
    {
        await base.Edit_single_property_with_converter_bool_to_int_zero_one();

        AssertSql(
            """
@p0='0' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = jsonb_set("Reference", '{BoolConvertedToIntZeroOne}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_with_converter_bool_to_string_True_False()
    {
        await base.Edit_single_property_with_converter_bool_to_string_True_False();

        AssertSql(
            """
@p0='"True"' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = jsonb_set("Reference", '{BoolConvertedToStringTrueFalse}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_with_converter_bool_to_string_Y_N()
    {
        await base.Edit_single_property_with_converter_bool_to_string_Y_N();

        AssertSql(
            """
@p0='"N"' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = jsonb_set("Reference", '{BoolConvertedToStringYN}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_with_converter_int_zero_one_to_bool()
    {
        await base.Edit_single_property_with_converter_int_zero_one_to_bool();

        AssertSql(
            """
@p0='true' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = jsonb_set("Reference", '{IntZeroOneConvertedToBool}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    [ConditionalFact]
    public override async Task Edit_single_property_with_converter_string_True_False_to_bool()
    {
        await base.Edit_single_property_with_converter_string_True_False_to_bool();

        AssertSql(
            """
@p0='false' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = jsonb_set("Reference", '{StringTrueFalseConvertedToBool}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    [ConditionalFact]
    public override async Task Edit_single_property_with_converter_string_Y_N_to_bool()
    {
        await base.Edit_single_property_with_converter_string_Y_N_to_bool();

        AssertSql(
            """
@p0='true' (Nullable = false) (DbType = Object)
@p1='1'

UPDATE "JsonEntitiesConverters" SET "Reference" = jsonb_set("Reference", '{StringYNConvertedToBool}', @p0)
WHERE "Id" = @p1;
""",
            //
            """
SELECT j."Id", j."Reference"
FROM "JsonEntitiesConverters" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_numeric()
    {
        await base.Edit_single_property_collection_of_numeric();

        AssertSql(
            """
@p0='[1024,2048]' (Nullable = false) (DbType = Object)
@p1='[999,997]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesBasic" SET "OwnedCollectionRoot" = jsonb_set("OwnedCollectionRoot", '{1,Numbers}', @p0), "OwnedReferenceRoot" = jsonb_set("OwnedReferenceRoot", '{Numbers}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."EntityBasicId", j."Name", j."OwnedCollectionRoot", j."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS j
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_bool()
    {
        await base.Edit_single_property_collection_of_bool();

        AssertSql(
            """
@p0='[true,true,true,false]' (Nullable = false) (DbType = Object)
@p1='[true,true,false]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestBooleanCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestBooleanCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_byte()
    {
        await base.Edit_single_property_collection_of_byte();

        AssertSql(
            """
@p0='"Dg=="' (Nullable = false) (DbType = Object)
@p1='"GRo="' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestByteCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestByteCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_char()
    {
        // PostgreSQL does not support the 0 char in text
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => base.Edit_single_property_collection_of_char());
        var pgException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal("22P05", pgException.SqlState); // untranslatable_character
    }

    public override async Task Edit_single_property_collection_of_datetime()
    {
        await base.Edit_single_property_collection_of_datetime();

        AssertSql(
            """
@p0='["2000-01-01T12:34:56","3000-01-01T12:34:56","3000-01-01T12:34:56"]' (Nullable = false) (DbType = Object)
@p1='["2000-01-01T12:34:56","3000-01-01T12:34:56","3000-01-01T12:34:56"]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestDateTimeCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestDateTimeCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_datetimeoffset()
    {
        await base.Edit_single_property_collection_of_datetimeoffset();

        AssertSql(
            """
@p0='["3000-01-01T12:34:56-04:00"]' (Nullable = false) (DbType = Object)
@p1='["3000-01-01T12:34:56-04:00"]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestDateTimeOffsetCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestDateTimeOffsetCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_decimal()
    {
        await base.Edit_single_property_collection_of_decimal();

        AssertSql(
            """
@p0='[-13579.01]' (Nullable = false) (DbType = Object)
@p1='[-13579.01]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestDecimalCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestDecimalCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_double()
    {
        await base.Edit_single_property_collection_of_double();

        AssertSql(
            """
@p0='[-1.23456789,1.23456789,0,-1.23579]' (Nullable = false) (DbType = Object)
@p1='[-1.23456789,1.23456789,0,-1.23579]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestDoubleCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestDoubleCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_guid()
    {
        await base.Edit_single_property_collection_of_guid();

        AssertSql(
            """
@p0='["12345678-1234-4321-5555-987654321000"]' (Nullable = false) (DbType = Object)
@p1='["12345678-1234-4321-5555-987654321000"]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestGuidCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestGuidCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_int16()
    {
        await base.Edit_single_property_collection_of_int16();

        AssertSql(
            """
@p0='[-3234]' (Nullable = false) (DbType = Object)
@p1='[-3234]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestInt16Collection}', @p0), "Reference" = jsonb_set("Reference", '{TestInt16Collection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_int32()
    {
        await base.Edit_single_property_collection_of_int32();

        AssertSql(
            """
@p0='[-3234]' (Nullable = false) (DbType = Object)
@p1='[-3234]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestInt32Collection}', @p0), "Reference" = jsonb_set("Reference", '{TestInt32Collection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_int64()
    {
        await base.Edit_single_property_collection_of_int64();

        AssertSql(
            """
@p0='[]' (Nullable = false) (DbType = Object)
@p1='[]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestInt64Collection}', @p0), "Reference" = jsonb_set("Reference", '{TestInt64Collection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_signed_byte()
    {
        await base.Edit_single_property_collection_of_signed_byte();

        AssertSql(
            """
@p0='[-108]' (Nullable = false) (DbType = Object)
@p1='[-108]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestSignedByteCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestSignedByteCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_single()
    {
        await base.Edit_single_property_collection_of_single();

        AssertSql(
            """
@p0='[-1.234,-1.234]' (Nullable = false) (DbType = Object)
@p1='[0,-1.234]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestSingleCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestSingleCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_timespan()
    {
        await base.Edit_single_property_collection_of_timespan();

        AssertSql(
            """
@p0='["10:09:08.007","10:01:01.007"]' (Nullable = false) (DbType = Object)
@p1='["10:01:01.007","-09:50:51.993"]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestTimeSpanCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestTimeSpanCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_dateonly()
    {
        await base.Edit_single_property_collection_of_dateonly();

        AssertSql(
            """
@p0='["3234-01-23","0001-01-07"]' (Nullable = false) (DbType = Object)
@p1='["0001-01-07","4321-01-21"]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestDateOnlyCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestDateOnlyCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_timeonly()
    {
        await base.Edit_single_property_collection_of_timeonly();

        AssertSql(
            """
@p0='["13:42:23.0000000","01:01:07.0000000"]' (Nullable = false) (DbType = Object)
@p1='["01:01:07.0000000","07:17:27.0000000"]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestTimeOnlyCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestTimeOnlyCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_uint16()
    {
        await base.Edit_single_property_collection_of_uint16();

        AssertSql(
            """
@p0='[1534]' (Nullable = false) (DbType = Object)
@p1='[1534]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestUnsignedInt16Collection}', @p0), "Reference" = jsonb_set("Reference", '{TestUnsignedInt16Collection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_uint32()
    {
        await base.Edit_single_property_collection_of_uint32();

        AssertSql(
            """
@p0='[1237775789]' (Nullable = false) (DbType = Object)
@p1='[1237775789]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestUnsignedInt32Collection}', @p0), "Reference" = jsonb_set("Reference", '{TestUnsignedInt32Collection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_uint64()
    {
        await base.Edit_single_property_collection_of_uint64();

        AssertSql(
            """
@p0='[1234555555123456789]' (Nullable = false) (DbType = Object)
@p1='[1234555555123456789]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestUnsignedInt64Collection}', @p0), "Reference" = jsonb_set("Reference", '{TestUnsignedInt64Collection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_int32()
    {
        await base.Edit_single_property_collection_of_nullable_int32();

        AssertSql(
            """
@p0='[null,77]' (Nullable = false) (DbType = Object)
@p1='[null,-2147483648,0,null,2147483647,null,77,null]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableInt32Collection}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableInt32Collection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_int32_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_int32_set_to_null();

        AssertSql(
            """
@p0='null' (Nullable = false) (DbType = Object)
@p1='null' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableInt32Collection}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableInt32Collection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_enum()
    {
        await base.Edit_single_property_collection_of_enum();

        AssertSql(
            """
@p0='[2]' (Nullable = false) (DbType = Object)
@p1='[2]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestEnumCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestEnumCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_enum_with_int_converter()
    {
        await base.Edit_single_property_collection_of_enum_with_int_converter();

        AssertSql(
            """
@p0='[2]' (Nullable = false) (DbType = Object)
@p1='[2]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestEnumWithIntConverterCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestEnumWithIntConverterCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum()
    {
        await base.Edit_single_property_collection_of_nullable_enum();

        AssertSql(
            """
@p0='[2]' (Nullable = false) (DbType = Object)
@p1='[2]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestEnumCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestEnumCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_enum_set_to_null();

        AssertSql(
            """
@p0='null' (Nullable = false) (DbType = Object)
@p1='null' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableEnumCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableEnumCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_int_converter()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_int_converter();

        AssertSql(
            """
@p0='[0,null,-7,1]' (Nullable = false) (DbType = Object)
@p1='[0,2,-7,1]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableEnumWithIntConverterCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableEnumWithIntConverterCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_int_converter_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_int_converter_set_to_null();

        AssertSql(
            """
@p0='null' (Nullable = false) (DbType = Object)
@p1='null' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableEnumWithIntConverterCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableEnumWithIntConverterCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls();

        AssertSql(
            """
@p0='[2]' (Nullable = false) (DbType = Object)
@p1='[0]' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableEnumWithConverterThatHandlesNullsCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableEnumWithConverterThatHandlesNullsCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    public override async Task Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls_set_to_null()
    {
        await base.Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls_set_to_null();

        AssertSql(
            """
@p0='null' (Nullable = false) (DbType = Object)
@p1='null' (Nullable = false) (DbType = Object)
@p2='1'

UPDATE "JsonEntitiesAllTypes" SET "Collection" = jsonb_set("Collection", '{0,TestNullableEnumWithConverterThatHandlesNullsCollection}', @p0), "Reference" = jsonb_set("Reference", '{TestNullableEnumWithConverterThatHandlesNullsCollection}', @p1)
WHERE "Id" = @p2;
""",
            //
            """
SELECT j."Id", j."TestDateTimeCollection", j."TestDecimalCollection", j."TestDefaultStringCollection", j."TestEnumWithIntConverterCollection", j."TestGuidCollection", j."TestInt32Collection", j."TestInt64Collection", j."TestMaxLengthStringCollection", j."TestNullableEnumWithConverterThatHandlesNullsCollection", j."TestSignedByteCollection", j."TestSingleCollection", j."TestTimeSpanCollection", j."TestUnsignedInt32Collection", j."Collection", j."Reference"
FROM "JsonEntitiesAllTypes" AS j
WHERE j."Id" = 1
LIMIT 2
""");
    }

    // https://github.com/dotnet/efcore/pull/31831/files#r1393411950
    public override Task Add_and_update_top_level_optional_owned_collection_to_JSON(bool? value)
        => Assert.ThrowsAsync<PostgresException>(() => base.Add_and_update_top_level_optional_owned_collection_to_JSON(value));

    public override async Task Add_and_update_nested_optional_primitive_collection(bool? value)
    {
        // PostgreSQL does not support the 0 char in text
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => base.Edit_single_property_collection_of_char());
        var pgException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal("22P05", pgException.SqlState); // untranslatable_character
    }

    #region Skipped tests because of unsupported list type outside of JSON

    // The following tests fail because the properties they access are ignored in the model (see OnModelCreating below).
    // We do not yet support arbitrary list types outside of JSON.
    public override Task Edit_single_property_relational_collection_of_bool()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_bool());

    public override Task Edit_single_property_relational_collection_of_byte()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_byte());

    public override Task Edit_single_property_relational_collection_of_char()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_char());

    public override Task Edit_single_property_relational_collection_of_datetimeoffset()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_datetimeoffset());

    public override Task Edit_single_property_relational_collection_of_double()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_double());

    public override Task Edit_single_property_relational_collection_of_enum()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_enum());

    public override Task Edit_single_property_relational_collection_of_int16()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_int16());

    public override Task Edit_single_property_relational_collection_of_nullable_enum()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_nullable_enum());

    public override Task Edit_single_property_relational_collection_of_nullable_enum_set_to_null()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_nullable_enum_set_to_null());

    public override Task Edit_single_property_relational_collection_of_nullable_enum_with_int_converter()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_nullable_enum_with_int_converter());

    public override Task Edit_single_property_relational_collection_of_nullable_enum_with_int_converter_set_to_null()
        => Assert.ThrowsAsync<EqualException>(
            () => base.Edit_single_property_relational_collection_of_nullable_enum_with_int_converter_set_to_null());

    public override Task Edit_single_property_relational_collection_of_nullable_int32()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_nullable_int32());

    public override Task Edit_single_property_relational_collection_of_nullable_int32_set_to_null()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_nullable_int32_set_to_null());

    public override Task Edit_single_property_relational_collection_of_uint16()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_uint16());

    public override Task Edit_single_property_relational_collection_of_uint64()
        => Assert.ThrowsAsync<EqualException>(() => base.Edit_single_property_relational_collection_of_uint64());

    #endregion

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class JsonUpdateNpgsqlFixture : JsonUpdateFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

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
    }
}
