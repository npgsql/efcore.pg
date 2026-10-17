using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonPocoQueryTest : IClassFixture<JsonPocoQueryTest.JsonPocoQueryFixture>
{
    private JsonPocoQueryFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public JsonPocoQueryTest(JsonPocoQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Roundtrip(bool jsonb)
    {
        using var ctx = CreateContext();
        var customer = jsonb
            ? ctx.JsonbEntities.Single(e => e.Id == 1).Customer
            : ctx.JsonEntities.Single(e => e.Id == 1).Customer;
        var types = customer.VariousTypes;

        Assert.Equal("foo", types.String);
        Assert.Equal(8, types.Int16);
        Assert.Equal(8, types.Int32);
        Assert.Equal(8, types.Int64);
        Assert.Equal(10m, types.Decimal);
        Assert.Equal(new DateTime(2020, 1, 1, 10, 30, 45), types.DateTime);
        Assert.Equal(new DateTimeOffset(2020, 1, 1, 10, 30, 45, TimeSpan.Zero), types.DateTimeOffset);
    }

    [Fact]
    public void Literal()
    {
        using var ctx = CreateContext();

        Assert.Empty(ctx.JsonbEntities.Where(e => e.Customer == new Customer { Name = "Test customer", Age = 80 }));

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE j."Customer" = '{"Name":"Test customer","Age":80,"ID":"00000000-0000-0000-0000-000000000000","is_vip":false,"Statistics":null,"Orders":null,"VariousTypes":null}'
""");
    }

    [Fact]
    public void Parameter()
    {
        using var ctx = CreateContext();

        var expected = ctx.JsonbEntities.Find(1)!.Customer;
        var actual = ctx.JsonbEntities.Single(e => e.Customer == expected).Customer;

        Assert.Equal(actual.Name, expected.Name);

        AssertSql(
            """
@p='1'

SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE j."Id" = @p
LIMIT 1
""",
            //
            """
@expected='Microsoft.EntityFrameworkCore.Query.JsonPocoQueryTest+Customer' (DbType = Object)

SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CASE
    WHEN j."Customer" #>> '{}' IS NULL THEN NULL
    ELSE j."Customer"
END = @expected
LIMIT 2
""");
    }

    #region Output

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Typed_poco_json_null(bool jsonb)
    {
        using var ctx = CreateContext();
        var count = jsonb
            ? ctx.JsonbEntities.FromSqlRaw(
                    """
SELECT 3 AS "Id", jsonb_build_object('Statistics', NULL) AS "Customer"
""")
                .Count(e => e.Customer.Statistics == null)
            : ctx.JsonEntities.FromSqlRaw(
                    """
SELECT 3 AS "Id", json_build_object('Statistics', NULL) AS "Customer"
""")
                .Count(e => e.Customer.Statistics == null);

        Assert.Equal(1, count);

        AssertSql(
            $"""
SELECT count(*)::int
FROM (
    SELECT 3 AS "Id", {(jsonb ? "jsonb_build_object" : "json_build_object")}('Statistics', NULL) AS "Customer"
) AS m
WHERE m."Customer" ->> 'Statistics' IS NULL
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Output_json_fragments(bool jsonb)
    {
        using var ctx = CreateContext();
        var query = jsonb
            ? ctx.JsonbEntities.OrderBy(e => e.Id)
                .Select(e => new { e.Customer.Statistics, e.Customer.Statistics.Nested.IntArray, e.Customer.Orders })
            : ctx.JsonEntities.OrderBy(e => e.Id)
                .Select(e => new { e.Customer.Statistics, e.Customer.Statistics.Nested.IntArray, e.Customer.Orders });

        var values = query.ToArray();
        Assert.Equal([3, 25], values.Select(v => v.Statistics.Purchases));
        Assert.Equal([3, 4], values[0].IntArray);
        Assert.Equal([5, 6, 7], values[1].IntArray);
        Assert.Equal([99.5m, 23m], values[0].Orders.Select(o => o.Price));
        Assert.Equal(5m, Assert.Single(values[1].Orders).Price);

        AssertSql(
            $$"""
SELECT j."Customer" -> 'Statistics' AS "Statistics", j."Customer" #> '{Statistics,Nested,IntArray}' AS "IntArray", j."Customer" -> 'Orders' AS "Orders"
FROM "{{(jsonb ? "JsonbEntities" : "JsonEntities")}}" AS j
ORDER BY j."Id" NULLS FIRST
""");
    }

    [Fact]
    public void Output_string_with_jsonb()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Name == "Joe");

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE j."Customer" ->> 'Name' = 'Joe'
LIMIT 2
""");
    }

    [Fact]
    public void Output_string_with_json()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonEntities.Single(e => e.Customer.Name == "Joe");

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonEntities" AS j
WHERE j."Customer" ->> 'Name' = 'Joe'
LIMIT 2
""");
    }

    [Fact]
    public void Output_int()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Age < 30);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" ->> 'Age' AS integer) < 30
LIMIT 2
""");
    }

    [Fact]
    public void Output_Guid()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.ID == Guid.Empty);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" ->> 'ID' AS uuid) = '00000000-0000-0000-0000-000000000000'
LIMIT 2
""");
    }

    [Fact]
    public void Output_bool()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.VariousTypes.Bool);

        Assert.Equal("Moe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" #>> '{VariousTypes,Bool}' AS boolean)
LIMIT 2
""");
    }

    [Fact]
    public void Output_DateTime()
    {
        using var ctx = CreateContext();

        var p = new DateTime(1990, 3, 3, 17, 10, 15, DateTimeKind.Utc);
        var x = ctx.JsonbEntities.Single(e => e.Customer.VariousTypes.DateTime == p);

        Assert.Equal("Moe", x.Customer.Name);

        AssertSql(
            """
@p='1990-03-03T17:10:15.0000000Z' (DbType = DateTime)

SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" #>> '{VariousTypes,DateTime}' AS timestamp with time zone) = @p
LIMIT 2
""");
    }

    [Fact]
    public void Output_DateTimeOffset()
    {
        using var ctx = CreateContext();

        var p = new DateTimeOffset(1990, 3, 3, 17, 10, 15, TimeSpan.Zero);
        var x = ctx.JsonbEntities.Single(e => e.Customer.VariousTypes.DateTimeOffset == p);

        Assert.Equal("Moe", x.Customer.Name);

        AssertSql(
            """
@p='1990-03-03T17:10:15.0000000+00:00' (DbType = DateTime)

SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" #>> '{VariousTypes,DateTimeOffset}' AS timestamp with time zone) = @p
LIMIT 2
""");
    }

    #endregion Output

    [Fact]
    public void Nullable()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Statistics.Nested.SomeNullableInt == 20);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" #>> '{Statistics,Nested,SomeNullableInt}' AS integer) = 20
LIMIT 2
""");
    }

    [Fact] // #1674
    public void Compare_to_null()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Statistics.Nested.SomeNullableInt == null);

        Assert.Equal("Moe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE j."Customer" #>> '{Statistics,Nested,SomeNullableInt}' IS NULL
LIMIT 2
""");
    }

    [Fact]
    public void Nested()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Statistics.Visits == 4);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" #>> '{Statistics,Visits}' AS bigint) = 4
LIMIT 2
""");
    }

    [Fact]
    public void Nested_twice()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Statistics.Nested.SomeProperty == 10);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" #>> '{Statistics,Nested,SomeProperty}' AS integer) = 10
LIMIT 2
""");
    }

    [Fact(Skip = "https://github.com/dotnet/efcore/issues/30386")]
    public void Array_of_objects()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Orders[0].Price == 99.5m);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer"#>>'{Orders,0,Price}' AS numeric) = 99.5
LIMIT 2
""");
    }

    [Fact]
    public void Array_toplevel()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.ToplevelArray[1] == "two");

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE (j."ToplevelArray" ->> 1) = 'two'
LIMIT 2
""");
    }

    [Fact]
    public void Array_nested()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Statistics.Nested.IntArray[1] == 4);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" #>> '{Statistics,Nested,IntArray,1}' AS integer) = 4
LIMIT 2
""");
    }

    [Fact]
    public void Array_parameter_index()
    {
        using var ctx = CreateContext();

        var i = 1;
        var x = ctx.JsonbEntities.Single(e => e.Customer.Statistics.Nested.IntArray[i] == 4);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
@i='1'

SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" #>> ARRAY['Statistics','Nested','IntArray',@i]::text[] AS integer) = 4
LIMIT 2
""");
    }

    [Fact]
    public void Array_Length()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Statistics.Nested.IntArray.Length == 2);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE jsonb_array_length(j."Customer" #> '{Statistics,Nested,IntArray}') = 2
LIMIT 2
""");
    }

    [Fact]
    public void Array_Length_json()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonEntities.Single(e => e.Customer.Statistics.Nested.IntArray.Length == 2);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonEntities" AS j
WHERE json_array_length(j."Customer" #> '{Statistics,Nested,IntArray}') = 2
LIMIT 2
""");
    }

    [Fact]
    public void List_Count()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Statistics.Nested.IntList.Count == 2);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE jsonb_array_length(j."Customer" #> '{Statistics,Nested,IntList}') = 2
LIMIT 2
""");
    }

    [Fact]
    public void List_Count_json()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonEntities.Single(e => e.Customer.Statistics.Nested.IntList.Count == 2);

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonEntities" AS j
WHERE json_array_length(j."Customer" #> '{Statistics,Nested,IntList}') = 2
LIMIT 2
""");
    }

    [Fact]
    public void Array_Any_toplevel()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.ToplevelArray.Any());

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE jsonb_array_length(j."ToplevelArray") > 0
LIMIT 2
""");
    }

    [Fact]
    public void Like()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.Customer.Name.StartsWith("J"));

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE j."Customer" ->> 'Name' LIKE 'J%'
LIMIT 2
""");
    }

    [Fact] // #1363
    public void Where_nullable_guid()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(
            e =>
                e.Customer.Statistics.Nested.SomeNullableGuid == Guid.Parse("d5f2685d-e5c4-47e5-97aa-d0266154eb2d"));

        Assert.Equal("Joe", x.Customer.Name);

        AssertSql(
            """
SELECT j."Id", j."Customer", j."ToplevelArray"
FROM "JsonbEntities" AS j
WHERE CAST(j."Customer" #>> '{Statistics,Nested,SomeNullableGuid}' AS uuid) = 'd5f2685d-e5c4-47e5-97aa-d0266154eb2d'
LIMIT 2
""");
    }

    #region Functions

    [Fact]
    public void JsonContains_with_json_element()
    {
        using var ctx = CreateContext();
        var element = JsonDocument.Parse("""{"Name": "Joe", "Age": 25}""").RootElement;
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonContains(e.Customer, element));

        Assert.Equal(1, count);
        AssertSql(
            """
@element='{"Name": "Joe", "Age": 25}' (DbType = Object)

SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE j."Customer" @> @element
""");
    }

    [Fact]
    public void JsonContains_with_string_literal()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonContains(e.Customer, """{"Name": "Joe", "Age": 25}"""));

        Assert.Equal(1, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE j."Customer" @> '{"Name": "Joe", "Age": 25}'
""");
    }

    [Fact]
    public void JsonContains_with_string_parameter()
    {
        using var ctx = CreateContext();
        var someJson = """{"Name": "Joe", "Age": 25}""";
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonContains(e.Customer, someJson));

        Assert.Equal(1, count);
        AssertSql(
            """
@someJson='{"Name": "Joe", "Age": 25}' (DbType = Object)

SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE j."Customer" @> @someJson
""");
    }

    [Fact]
    public void JsonContained_with_json_element()
    {
        using var ctx = CreateContext();
        var element = JsonDocument.Parse("""{"Name": "Joe", "Age": 25}""").RootElement;
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonContained(element, e.Customer));

        Assert.Equal(1, count);
        AssertSql(
            """
@element='{"Name": "Joe", "Age": 25}' (DbType = Object)

SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE @element <@ j."Customer"
""");
    }

    [Fact]
    public void JsonContained_with_string_literal()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonContained("""{"Name": "Joe", "Age": 25}""", e.Customer));

        Assert.Equal(1, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE '{"Name": "Joe", "Age": 25}' <@ j."Customer"
""");
    }

    [Fact]
    public void JsonContained_with_string_parameter()
    {
        using var ctx = CreateContext();
        var someJson = """{"Name": "Joe", "Age": 25}""";
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonContained(someJson, e.Customer));

        Assert.Equal(1, count);
        AssertSql(
            """
@someJson='{"Name": "Joe", "Age": 25}' (DbType = Object)

SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE @someJson <@ j."Customer"
""");
    }

    [Fact]
    public void JsonExists()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e => EF.Functions.JsonExists(e.Customer.Statistics, "Visits"));

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE (j."Customer" -> 'Statistics') ? 'Visits'
""");
    }

    [Fact]
    public void JsonExistAny()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonExistAny(e.Customer.Statistics, "foo", "Visits"));

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE (j."Customer" -> 'Statistics') ?| ARRAY['foo','Visits']::text[]
""");
    }

    [Fact]
    public void JsonExistAll()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonExistAll(e.Customer.Statistics, "foo", "Visits"));

        Assert.Equal(0, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE (j."Customer" -> 'Statistics') ?& ARRAY['foo','Visits']::text[]
""");
    }

    [Fact]
    public void JsonTypeof()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonTypeof(e.Customer.Statistics.Visits) == "number");

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE jsonb_typeof(j."Customer" #> '{Statistics,Visits}') = 'number'
""");
    }

    [Fact]
    public void JsonTypeof_json()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonEntities.Count(
            e =>
                EF.Functions.JsonTypeof(e.Customer.Statistics.Visits) == "number");

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonEntities" AS j
WHERE json_typeof(j."Customer" #> '{Statistics,Visits}') = 'number'
""");
    }

    #endregion Functions

    #region Dictionaries

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_string(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => (string)e.JsonbDictionary["Name"] == "Joe")
            : ctx.DictionaryEntities.Where(e => (string)e.JsonDictionary["Name"] == "Joe");

        Assert.Equal(1, query.Select(e => e.Id).Single());

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'Name' = 'Joe'
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_object_comparison(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => e.JsonbDictionary["Name"] == (object)"Joe")
            : ctx.DictionaryEntities.Where(e => e.JsonDictionary["Name"] == (object)"Joe");

        Assert.Equal(1, query.Select(e => e.Id).Single());

        var traversal = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name'""";
        traversal = $"NULLIF({(jsonb ? traversal : $"CAST({traversal} AS jsonb)")}, 'null'::jsonb)";
        const string value = """NULLIF('"Joe"'::jsonb, 'null'::jsonb)""";

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE {traversal} = {value} OR ({traversal} IS NULL AND {value} IS NULL)
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_object_parameter_comparison(bool jsonb, bool reversed)
    {
        using var ctx = CreateContext();
        var name = "Joe";
        var query = (jsonb, reversed) switch
        {
            (true, false) => ctx.DictionaryEntities.Where(e => e.JsonbDictionary["Name"] == (object)EF.Parameter(name)),
            (true, true) => ctx.DictionaryEntities.Where(e => (object)EF.Parameter(name) == e.JsonbDictionary["Name"]),
            (false, false) => ctx.DictionaryEntities.Where(e => e.JsonDictionary["Name"] == (object)EF.Parameter(name)),
            (false, true) => ctx.DictionaryEntities.Where(e => (object)EF.Parameter(name) == e.JsonDictionary["Name"])
        };

        Assert.Equal(1, query.Select(e => e.Id).Single());

        var traversal = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name'""";
        traversal = $"NULLIF({(jsonb ? traversal : $"CAST({traversal} AS jsonb)")}, 'null'::jsonb)";
        const string parameter = "NULLIF(to_jsonb(@name), 'null'::jsonb)";
        var left = reversed ? parameter : traversal;
        var right = reversed ? traversal : parameter;

        AssertSql(
            $"""
@name='Joe'

SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE {left} = {right} OR ({left} IS NULL AND {right} IS NULL)
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_object_forced_constant_comparison(bool jsonb)
    {
        using var ctx = CreateContext();
        var name = "Joe";
        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => e.JsonbDictionary["Name"] == (object)EF.Constant(name))
            : ctx.DictionaryEntities.Where(e => e.JsonDictionary["Name"] == (object)EF.Constant(name));

        Assert.Equal(1, query.Select(e => e.Id).Single());

        var left = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name'""";
        left = $"NULLIF({(jsonb ? left : $"CAST({left} AS jsonb)")}, 'null'::jsonb)";
        const string right = "NULLIF(to_jsonb('Joe'::text), 'null'::jsonb)";
        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE {left} = {right} OR ({left} IS NULL AND {right} IS NULL)
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_object_int_comparison(bool jsonb)
    {
        using var ctx = CreateContext();
        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => e.JsonbDictionary["Age"] == (object)25)
            : ctx.DictionaryEntities.Where(e => e.JsonDictionary["Age"] == (object)25);

        Assert.Equal(1, query.Select(e => e.Id).Single());

        var traversal = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Age'""";
        traversal = $"NULLIF({(jsonb ? traversal : $"CAST({traversal} AS jsonb)")}, 'null'::jsonb)";
        const string value = "NULLIF('25'::jsonb, 'null'::jsonb)";

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE {traversal} = {value} OR ({traversal} IS NULL AND {value} IS NULL)
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false, false, "Age")]
    [InlineData(false, true, "Age")]
    [InlineData(true, false, "Age")]
    [InlineData(true, true, "Age")]
    [InlineData(false, false, "Name")]
    [InlineData(false, true, "Name")]
    [InlineData(true, false, "Name")]
    [InlineData(true, true, "Name")]
    public void Dictionary_indexer_object_mixed_kind_comparison(bool jsonb, bool notEqual, string key)
    {
        using var ctx = CreateContext();
        var query = (jsonb, notEqual, key) switch
        {
            (true, false, "Age") => ctx.DictionaryEntities.Where(e => e.JsonbDictionary["Age"] == (object)"25"),
            (true, true, "Age") => ctx.DictionaryEntities.Where(e => e.JsonbDictionary["Age"] != (object)"25"),
            (false, false, "Age") => ctx.DictionaryEntities.Where(e => e.JsonDictionary["Age"] == (object)"25"),
            (false, true, "Age") => ctx.DictionaryEntities.Where(e => e.JsonDictionary["Age"] != (object)"25"),
            (true, false, "Name") => ctx.DictionaryEntities.Where(e => e.JsonbDictionary["Name"] == (object)25),
            (true, true, "Name") => ctx.DictionaryEntities.Where(e => e.JsonbDictionary["Name"] != (object)25),
            (false, false, "Name") => ctx.DictionaryEntities.Where(e => e.JsonDictionary["Name"] == (object)25),
            (false, true, "Name") => ctx.DictionaryEntities.Where(e => e.JsonDictionary["Name"] != (object)25),
            _ => throw new UnreachableException()
        };

        int[] expected = notEqual ? [1, 2] : [];
        Assert.Equal(expected, query.OrderBy(e => e.Id).Select(e => e.Id).ToArray());

        var left = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> '{key}'""";
        left = $"NULLIF({(jsonb ? left : $"CAST({left} AS jsonb)")}, 'null'::jsonb)";
        var right = key == "Age"
            ? """NULLIF('"25"'::jsonb, 'null'::jsonb)"""
            : "NULLIF('25'::jsonb, 'null'::jsonb)";
        var predicate = notEqual
            ? $"({left} <> {right} OR {left} IS NULL OR {right} IS NULL) AND ({left} IS NOT NULL OR {right} IS NOT NULL)"
            : $"{left} = {right} OR ({left} IS NULL AND {right} IS NULL)";

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE {predicate}
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_int(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => (int)e.JsonbDictionary["Age"] < 30)
            : ctx.DictionaryEntities.Where(e => (int)e.JsonDictionary["Age"] < 30);

        Assert.Equal(1, query.Select(e => e.Id).Single());

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE CAST(d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'Age' AS integer) < 30
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_binary_projection(bool jsonb, bool objectValue)
    {
        using var ctx = CreateContext();
        var query = (jsonb, objectValue) switch
        {
            (true, false) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonbBinaryDictionary["Payload"]),
            (true, true) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => (byte[])e.JsonbDictionary["Payload"]),
            (false, false) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonBinaryDictionary["Payload"]),
            (false, true) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => (byte[])e.JsonDictionary["Payload"])
        };

        var values = query.ToArray();
        Assert.Equal<byte>([1, 2, 3], values[0]);
        Assert.Equal<byte>([4, 5, 6], values[1]);

        var column = $"{(jsonb ? "Jsonb" : "Json")}{(objectValue ? "" : "Binary")}Dictionary";
        AssertSql(
            $"""
SELECT decode(d."{column}" ->> 'Payload', 'base64')
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_binary_parameter_comparison(bool jsonb, bool longPayload)
    {
        using var ctx = CreateContext();
        byte[] payload = longPayload ? [.. Enumerable.Range(0, 80).Select(i => (byte)i)] : [1, 2, 3];
        var key = longPayload ? "LongPayload" : "Payload";
        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => e.JsonbDictionary[key] == (object)EF.Parameter(payload))
            : ctx.DictionaryEntities.Where(e => e.JsonDictionary[key] == (object)EF.Parameter(payload));

        Assert.Equal(1, query.Select(e => e.Id).Single());

        var left = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> '{key}'""";
        left = $"NULLIF({(jsonb ? left : $"CAST({left} AS jsonb)")}, 'null'::jsonb)";
        const string right =
            """
NULLIF(to_jsonb(replace(encode(@payload, 'base64'), '
', '')), 'null'::jsonb)
""";
        var loggedPayload = longPayload ? "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F..." : "010203";

        AssertSql(
            $"""
@payload='0x{loggedPayload}'

SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE {left} = {right} OR ({left} IS NULL AND {right} IS NULL)
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_binary_composed_projection(bool jsonb, bool coalesce)
    {
        using var ctx = CreateContext();
        byte[] payload = [.. Enumerable.Range(0, 80).Select(i => (byte)i)];
        var query = (jsonb, coalesce) switch
        {
            (true, false) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                .Select(e => (byte[])(e.Id == 1 ? e.JsonbDictionary["Payload"] : (object)EF.Parameter(payload))),
            (false, false) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                .Select(e => (byte[])(e.Id == 1 ? e.JsonDictionary["Payload"] : (object)EF.Parameter(payload))),
            (true, true) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                .Select(e => (byte[])(e.JsonbDictionary["Missing"] ?? (object)EF.Parameter(payload))),
            (false, true) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                .Select(e => (byte[])(e.JsonDictionary["Missing"] ?? (object)EF.Parameter(payload)))
        };

        var values = query.ToArray();
        Assert.Equal<byte>(coalesce ? payload : [1, 2, 3], values[0]);
        Assert.Equal(payload, values[1]);

        var scalar = $"""
{(jsonb ? "to_jsonb" : "to_json")}(replace(encode(@payload, 'base64'), '
', ''))
""";
        var projection = coalesce
            ? $"""
COALESCE(CASE
    WHEN d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'Missing' IS NULL THEN NULL
    ELSE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Missing'
END, {scalar})
"""
            : $"""
CASE
    WHEN d."Id" = 1 THEN d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Payload'
    ELSE {scalar}
END
""";
        AssertSql(
            $$"""
@payload='0x000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F...'

SELECT decode({{projection}} #>> '{}', 'base64')
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_typed_cast_predicate(bool jsonb, bool poco)
    {
        using var ctx = CreateContext();
        var query = (jsonb, poco) switch
        {
            (true, false) => ctx.DictionaryEntities.Where(e => ((Dictionary<string, int>)e.JsonbDictionary["Customer"])["Age"] < 30),
            (true, true) => ctx.DictionaryEntities.Where(e => ((Customer)e.JsonbDictionary["Customer"]).Age < 30),
            (false, false) => ctx.DictionaryEntities.Where(e => ((Dictionary<string, int>)e.JsonDictionary["Customer"])["Age"] < 30),
            (false, true) => ctx.DictionaryEntities.Where(e => ((Customer)e.JsonDictionary["Customer"]).Age < 30)
        };

        Assert.Equal(1, query.Select(e => e.Id).Single());

        AssertSql(
            $$"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE CAST(d."{{(jsonb ? "JsonbDictionary" : "JsonDictionary")}}" #>> '{Customer,Age}' AS integer) < 30
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_JsonDocument_predicate(bool jsonb)
    {
        using var ctx = CreateContext();
        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => ((JsonDocument)e.JsonbDictionary["Name"]).RootElement.GetString() == "Joe")
            : ctx.DictionaryEntities.Where(e => ((JsonDocument)e.JsonDictionary["Name"]).RootElement.GetString() == "Joe");

        Assert.Equal(1, query.Select(e => e.Id).Single());

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'Name' = 'Joe'
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_parameter(bool jsonb)
    {
        using var ctx = CreateContext();
        var key = "Name";

        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => (string)e.JsonbDictionary[EF.Parameter(key)] == "Joe")
            : ctx.DictionaryEntities.Where(e => (string)e.JsonDictionary[EF.Parameter(key)] == "Joe");

        Assert.Equal(1, query.Select(e => e.Id).Single());

        AssertSql(
            $"""
@key='Name'

SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> @key = 'Joe'
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_missing_key(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => (string)e.JsonbDictionary["Missing"] == null)
            : ctx.DictionaryEntities.Where(e => (string)e.JsonDictionary["Missing"] == null);

        Assert.Equal(2, query.Select(e => e.Id).Count());

        AssertSql(
            $"""
SELECT count(*)::int
FROM "DictionaryEntities" AS d
WHERE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'Missing' IS NULL
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_projection(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => (int)e.JsonbDictionary["Age"])
            : ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => (int)e.JsonDictionary["Age"]);

        Assert.Equal([25, 35], query.ToArray());

        AssertSql(
            $"""
SELECT CAST(d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'Age' AS integer)
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_timestamp_comparison(bool jsonb, bool constant)
    {
        using var ctx = CreateContext();
        var timestamp = new DateTime(2020, 1, 1, 10, 30, 45, DateTimeKind.Utc).AddTicks(1234567);
        var query = (jsonb, constant) switch
        {
            (true, false) => ctx.DictionaryEntities.Where(e => e.JsonbDictionary["Timestamp"] == (object)EF.Parameter(timestamp)),
            (false, false) => ctx.DictionaryEntities.Where(e => e.JsonDictionary["Timestamp"] == (object)EF.Parameter(timestamp)),
            (true, true) => ctx.DictionaryEntities.Where(e => e.JsonbDictionary["Timestamp"] == (object)EF.Constant(timestamp)),
            (false, true) => ctx.DictionaryEntities.Where(e => e.JsonDictionary["Timestamp"] == (object)EF.Constant(timestamp))
        };

        Assert.Equal(1, query.Select(e => e.Id).Single());

        var traversal = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Timestamp'""";
        var left = $"NULLIF({(jsonb ? traversal : $"CAST({traversal} AS jsonb)")}, 'null'::jsonb)";
        var right = $"""NULLIF({(constant ? """'"2020-01-01T10:30:45.1234567Z"'""" : "@timestamp")}, 'null'::jsonb)""";
        AssertSql(
            (constant ? "" : "@timestamp='2020-01-01T10:30:45.1234567Z' (DbType = Object)\n\n") + $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE {left} = {right} OR ({left} IS NULL AND {right} IS NULL)
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_timestamp_expression_projection(bool jsonb, bool missing)
    {
        using var ctx = CreateContext();
        var key = missing ? "Missing" : "Timestamp";
        var query = jsonb
            ? ctx.DictionaryEntities.OrderBy(e => e.Id)
                .Select(e => e.Id == 1 ? e.JsonbDictionary["Timestamp"] : (object)((DateTime)e.JsonbDictionary[key]).AddSeconds(1))
            : ctx.DictionaryEntities.OrderBy(e => e.Id)
                .Select(e => e.Id == 1 ? e.JsonDictionary["Timestamp"] : (object)((DateTime)e.JsonDictionary[key]).AddSeconds(1));

        Assert.Equal(
            ["2020-01-01T10:30:45.1234567Z", missing ? null : "1990-03-03T17:10:16.123456Z"],
            query.ToArray().Select(v => v is null ? null : Assert.IsType<JsonElement>(v).GetString()));

        var column = $"d.\"{(jsonb ? "JsonbDictionary" : "JsonDictionary")}\"";
        AssertSql(
            $$"""
SELECT CASE
    WHEN d."Id" = 1 THEN {{column}} -> 'Timestamp'
    ELSE {{(jsonb ? "to_jsonb" : "to_json")}}(regexp_replace(to_jsonb((CAST({{column}} ->> '{{key}}' AS timestamp with time zone) + INTERVAL '1 secs') AT TIME ZONE 'UTC') #>> '{}', '$', 'Z'))
END
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_document_root_projection(bool jsonb)
    {
        using var ctx = CreateContext();
        var query = jsonb
            ? ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => ((JsonDocument)e.JsonbDictionary["Name"]).RootElement)
            : ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => ((JsonDocument)e.JsonDictionary["Name"]).RootElement);

        Assert.Equal(["Joe", "Moe"], query.ToArray().Select(v => v.GetString()));

        AssertSql(
            $"""
SELECT d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name'
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Dictionary_indexer_nullable_collection_projection(bool jsonb, bool typed, bool list)
    {
        using var ctx = CreateContext();
        if (list)
        {
            var query = (jsonb, typed) switch
            {
                (true, false) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => (List<int>)e.JsonbDictionary["NullableArray"]),
                (false, false) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => (List<int>)e.JsonDictionary["NullableArray"]),
                (true, true) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                    .Select(e => ((Dictionary<string, List<int>>)e.JsonbDictionary["Collections"])["Array"]),
                (false, true) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                    .Select(e => ((Dictionary<string, List<int>>)e.JsonDictionary["Collections"])["Array"])
            };
            var values = query.ToArray();
            Assert.Null(values[0]);
            Assert.Equal([3, 4], values[1]);
        }
        else
        {
            var query = (jsonb, typed) switch
            {
                (true, false) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => (int[])e.JsonbDictionary["NullableArray"]),
                (false, false) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => (int[])e.JsonDictionary["NullableArray"]),
                (true, true) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                    .Select(e => ((Dictionary<string, int[]>)e.JsonbDictionary["Collections"])["Array"]),
                (false, true) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                    .Select(e => ((Dictionary<string, int[]>)e.JsonDictionary["Collections"])["Array"])
            };
            var values = query.ToArray();
            Assert.Null(values[0]);
            Assert.Equal([3, 4], values[1]);
        }

        var path = typed ? """#> '{Collections,Array}'""" : """-> 'NullableArray'""";
        AssertSql(
            $"""
SELECT d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" {path}
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Fact]
    public void Dictionary_indexer_interface()
    {
        using var ctx = CreateContext();

        Assert.Equal(1, ctx.DictionaryEntities.Where(e => e.DictionaryInterface["Name"] == "Joe").Select(e => e.Id).Single());

        AssertSql(
            """
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE d."DictionaryInterface" ->> 'Name' = 'Joe'
LIMIT 2
""");
    }

    [Fact]
    public void Dictionary_indexer_readonly_interface()
    {
        using var ctx = CreateContext();

        Assert.Equal(1, ctx.DictionaryEntities.Where(e => e.ReadOnlyDictionary["Age"] < 30).Select(e => e.Id).Single());

        AssertSql(
            """
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE CAST(d."ReadOnlyDictionary" ->> 'Age' AS integer) < 30
LIMIT 2
""");
    }

    [Fact]
    public void Dictionary_indexer_nested()
    {
        using var ctx = CreateContext();

        Assert.Equal(1, ctx.DictionaryEntities.Where(e => e.NestedDictionary["Customer"]["Age"] < 30).Select(e => e.Id).Single());

        AssertSql(
            """
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE CAST(d."NestedDictionary" #>> '{Customer,Age}' AS integer) < 30
LIMIT 2
""");
    }

    [Fact]
    public void Dictionary_indexer_nested_string()
    {
        using var ctx = CreateContext();

        Assert.Equal(1, ctx.DictionaryEntities.Where(e => e.NestedStringDictionary["Customer"]["Name"] == "Joe").Select(e => e.Id).Single());

        AssertSql(
            """
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE d."NestedStringDictionary" #>> '{Customer,Name}' = 'Joe'
LIMIT 2
""");
    }

    [Fact]
    public void Dictionary_indexer_nested_empty_key()
    {
        using var ctx = CreateContext();

        Assert.Equal(1, ctx.DictionaryEntities.Where(e => e.NestedStringDictionary["Customer"][""] == "Joe").Select(e => e.Id).Single());

        AssertSql(
            """
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE d."NestedStringDictionary" #>> ARRAY['Customer','']::text[] = 'Joe'
LIMIT 2
""");
    }

    [Theory]
    [InlineData("NULL")]
    [InlineData("null")]
    [InlineData("NuLl")]
    public void Dictionary_indexer_nested_null_key(string key)
    {
        using var ctx = CreateContext();

        Assert.Equal(1, ctx.DictionaryEntities.Where(e => e.NestedStringDictionary["Customer"][key] == "Joe").Select(e => e.Id).Single());

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE d."NestedStringDictionary" #>> ARRAY['Customer','{key}']::text[] = 'Joe'
LIMIT 2
""");
    }

    [Fact]
    public void Dictionary_indexer_dictionary_projection()
    {
        using var ctx = CreateContext();

        var dictionaries = ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.NestedDictionary["Customer"]).ToArray();

        Assert.Equal([25, 35], dictionaries.Select(d => d["Age"]));

        AssertSql(
            """
SELECT d."NestedDictionary" -> 'Customer'
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Fact]
    public void Dictionary_indexer_string_dictionary_projection()
    {
        using var ctx = CreateContext();

        var dictionaries = ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.NestedStringDictionary["Customer"]).ToArray();

        Assert.Equal(["Joe", "Moe"], dictionaries.Select(d => d["Name"]));

        AssertSql(
            """
SELECT d."NestedStringDictionary" -> 'Customer'
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_object_projection(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => new { Name = e.JsonbDictionary["Name"], Age = e.JsonbDictionary["Age"] })
            : ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => new { Name = e.JsonDictionary["Name"], Age = e.JsonDictionary["Age"] });

        var values = query.ToArray();

        Assert.Equal(["Joe", "Moe"], values.Select(v => Assert.IsType<JsonElement>(v.Name).GetString()));
        Assert.Equal([25, 35], values.Select(v => Assert.IsType<JsonElement>(v.Age).GetInt32()));

        AssertSql(
            $"""
SELECT d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name' AS "Name", d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Age' AS "Age"
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_conditional_object_projection(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.Id == 1 ? e.JsonbDictionary["Age"] : e.JsonbDictionary["Name"])
            : ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.Id == 1 ? e.JsonDictionary["Age"] : e.JsonDictionary["Name"]);

        var values = query.ToArray();

        Assert.Equal(25, Assert.IsType<JsonElement>(values[0]).GetInt32());
        Assert.Equal("Moe", Assert.IsType<JsonElement>(values[1]).GetString());

        AssertSql(
            $"""
SELECT CASE
    WHEN d."Id" = 1 THEN d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Age'
    ELSE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name'
END
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_coalesce_object_projection(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonbDictionary["NullableName"] ?? e.JsonbDictionary["Name"])
            : ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonDictionary["NullableName"] ?? e.JsonDictionary["Name"]);

        Assert.Equal(["Joe", "Moe"], query.ToArray().Select(v => Assert.IsType<JsonElement>(v).GetString()));

        AssertSql(
            $"""
SELECT COALESCE(CASE
    WHEN d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'NullableName' IS NULL THEN NULL
    ELSE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'NullableName'
END, d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name')
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_coalesce_scalar_projection(bool jsonb, bool boxed)
    {
        using var ctx = CreateContext();
        var query = (jsonb, boxed) switch
        {
            (true, true) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonbDictionary["NullableName"] ?? (object)e.Name),
            (true, false) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonbDictionary["NullableName"] ?? e.Name),
            (false, true) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonDictionary["NullableName"] ?? (object)e.Name),
            (false, false) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonDictionary["NullableName"] ?? e.Name)
        };

        Assert.Equal(["Joe", "Moe"], query.ToArray().Select(v => Assert.IsType<JsonElement>(v).GetString()));

        AssertSql(
            $"""
SELECT COALESCE(CASE
    WHEN d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'NullableName' IS NULL THEN NULL
    ELSE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'NullableName'
END, {(jsonb ? "to_jsonb" : "to_json")}(d."Name"))
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_coalesce_constant_projection(bool jsonb)
    {
        using var ctx = CreateContext();
        var query = jsonb
            ? ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonbDictionary["NullableName"] ?? "fallback")
            : ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonDictionary["NullableName"] ?? "fallback");

        Assert.Equal(["fallback", "Moe"], query.ToArray().Select(v => Assert.IsType<JsonElement>(v).GetString()));

        AssertSql(
            $"""
SELECT COALESCE(CASE
    WHEN d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'NullableName' IS NULL THEN NULL
    ELSE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'NullableName'
END, '"fallback"')
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_conditional_scalar_projection(bool jsonb, bool reversed)
    {
        using var ctx = CreateContext();
        var query = (jsonb, reversed) switch
        {
            (true, false) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.Id == 1 ? e.JsonbDictionary["Name"] : (object)e.Name),
            (true, true) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.Id == 1 ? (object)e.Name : e.JsonbDictionary["Name"]),
            (false, false) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.Id == 1 ? e.JsonDictionary["Name"] : (object)e.Name),
            (false, true) => ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.Id == 1 ? (object)e.Name : e.JsonDictionary["Name"])
        };

        Assert.Equal(["Joe", "Moe"], query.ToArray().Select(v => Assert.IsType<JsonElement>(v).GetString()));

        var traversal = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name'""";
        var scalar = $"""{(jsonb ? "to_jsonb" : "to_json")}(d."Name")""";
        AssertSql(
            $"""
SELECT CASE
    WHEN d."Id" = 1 THEN {(reversed ? scalar : traversal)}
    ELSE {(reversed ? traversal : scalar)}
END
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_forced_constant_projection(bool jsonb, bool coalesce)
    {
        using var ctx = CreateContext();
        var guid = Guid.Empty;
        var query = (jsonb, coalesce) switch
        {
            (true, false) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                .Select(e => e.Id == 1 ? e.JsonbDictionary["Name"] : (object)EF.Constant(guid)),
            (false, false) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                .Select(e => e.Id == 1 ? e.JsonDictionary["Name"] : (object)EF.Constant(guid)),
            (true, true) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                .Select(e => e.JsonbDictionary["Missing"] ?? (object)EF.Constant(guid)),
            (false, true) => ctx.DictionaryEntities.OrderBy(e => e.Id)
                .Select(e => e.JsonDictionary["Missing"] ?? (object)EF.Constant(guid))
        };

        string[] expected = coalesce ? [guid.ToString(), guid.ToString()] : ["Joe", guid.ToString()];
        Assert.Equal(expected, query.ToArray().Select(v => Assert.IsType<JsonElement>(v).GetString()));

        var scalar = $"""{(jsonb ? "to_jsonb" : "to_json")}('00000000-0000-0000-0000-000000000000'::uuid)""";
        var projection = coalesce
            ? $"""
COALESCE(CASE
    WHEN d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'Missing' IS NULL THEN NULL
    ELSE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Missing'
END, {scalar})
"""
            : $"""
CASE
    WHEN d."Id" = 1 THEN d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name'
    ELSE {scalar}
END
""";
        AssertSql(
            $"""
SELECT {projection}
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false, "Name", "NullableName")]
    [InlineData(false, true, "Name", "NullableName")]
    [InlineData(true, false, "Name", "NullableName")]
    [InlineData(true, true, "Name", "NullableName")]
    [InlineData(false, false, "NullableName", "Missing")]
    [InlineData(false, true, "NullableName", "Missing")]
    [InlineData(true, false, "NullableName", "Missing")]
    [InlineData(true, true, "NullableName", "Missing")]
    [InlineData(false, false, "Age", "StringAge")]
    [InlineData(false, true, "Age", "StringAge")]
    [InlineData(true, false, "Age", "StringAge")]
    [InlineData(true, true, "Age", "StringAge")]
    public void Dictionary_indexer_compare_indexers(bool jsonb, bool notEqual, string leftKey, string rightKey)
    {
        using var ctx = CreateContext();
        var query = (jsonb, notEqual) switch
        {
            (true, false) => ctx.DictionaryEntities.Where(e => e.JsonbDictionary[leftKey] == e.JsonbDictionary[rightKey]),
            (true, true) => ctx.DictionaryEntities.Where(e => e.JsonbDictionary[leftKey] != e.JsonbDictionary[rightKey]),
            (false, false) => ctx.DictionaryEntities.Where(e => e.JsonDictionary[leftKey] == e.JsonDictionary[rightKey]),
            (false, true) => ctx.DictionaryEntities.Where(e => e.JsonDictionary[leftKey] != e.JsonDictionary[rightKey])
        };
        int[] expected = (leftKey, notEqual) switch
        {
            ("Name", false) or ("NullableName", true) => [2],
            ("Name", true) or ("NullableName", false) => [1],
            ("Age", false) => [],
            ("Age", true) => [1, 2],
            _ => throw new UnreachableException()
        };

        Assert.Equal(expected, query.OrderBy(e => e.Id).Select(e => e.Id).ToArray());

        var left = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> '{leftKey}'""";
        var right = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> '{rightKey}'""";
        if (!jsonb)
        {
            left = $"CAST({left} AS jsonb)";
            right = $"CAST({right} AS jsonb)";
        }

        left = $"NULLIF({left}, 'null'::jsonb)";
        right = $"NULLIF({right}, 'null'::jsonb)";
        var predicate = notEqual
            ? $"({left} <> {right} OR {left} IS NULL OR {right} IS NULL) AND ({left} IS NOT NULL OR {right} IS NOT NULL)"
            : $"{left} = {right} OR ({left} IS NULL AND {right} IS NULL)";

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE {predicate}
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false, "Name", "NullableName")]
    [InlineData(false, true, "Name", "NullableName")]
    [InlineData(true, false, "Name", "NullableName")]
    [InlineData(true, true, "Name", "NullableName")]
    [InlineData(false, false, "NullableName", "Missing")]
    [InlineData(false, true, "NullableName", "Missing")]
    [InlineData(true, false, "NullableName", "Missing")]
    [InlineData(true, true, "NullableName", "Missing")]
    public void Dictionary_indexer_object_Equals(bool jsonb, bool notEqual, string leftKey, string rightKey)
    {
        using var ctx = CreateContext();
        var query = (jsonb, notEqual) switch
        {
            (true, false) => ctx.DictionaryEntities.Where(e => object.Equals(e.JsonbDictionary[leftKey], e.JsonbDictionary[rightKey])),
            (true, true) => ctx.DictionaryEntities.Where(e => !object.Equals(e.JsonbDictionary[leftKey], e.JsonbDictionary[rightKey])),
            (false, false) => ctx.DictionaryEntities.Where(e => object.Equals(e.JsonDictionary[leftKey], e.JsonDictionary[rightKey])),
            (false, true) => ctx.DictionaryEntities.Where(e => !object.Equals(e.JsonDictionary[leftKey], e.JsonDictionary[rightKey]))
        };

        Assert.Equal((leftKey == "Name") == notEqual ? 1 : 2, query.Select(e => e.Id).Single());

        var left = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> '{leftKey}'""";
        var right = $"""d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> '{rightKey}'""";
        left = $"NULLIF({(jsonb ? left : $"CAST({left} AS jsonb)")}, 'null'::jsonb)";
        right = $"NULLIF({(jsonb ? right : $"CAST({right} AS jsonb)")}, 'null'::jsonb)";
        var predicate = notEqual
            ? $"({left} <> {right} OR {left} IS NULL OR {right} IS NULL) AND ({left} IS NOT NULL OR {right} IS NOT NULL)"
            : $"{left} = {right} OR ({left} IS NULL AND {right} IS NULL)";

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE {predicate}
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_coalesce_typed_container(bool jsonb)
    {
        using var ctx = CreateContext();
        var query = jsonb
            ? ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.NestedDictionary["NullableCustomer"] ?? e.NestedDictionary["Customer"])
            : ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonNestedDictionary["NullableCustomer"] ?? e.JsonNestedDictionary["Customer"]);

        Assert.Equal([25, 35], query.ToArray().Select(d => d["Age"]));

        AssertSql(
            $"""
SELECT COALESCE(CASE
    WHEN d."{(jsonb ? "NestedDictionary" : "JsonNestedDictionary")}" ->> 'NullableCustomer' IS NULL THEN NULL
    ELSE d."{(jsonb ? "NestedDictionary" : "JsonNestedDictionary")}" -> 'NullableCustomer'
END, d."{(jsonb ? "NestedDictionary" : "JsonNestedDictionary")}" -> 'Customer')
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dictionary_indexer_compare_typed_container_null(bool jsonb, bool notEqual)
    {
        using var ctx = CreateContext();
        var query = (jsonb, notEqual) switch
        {
            (true, false) => ctx.DictionaryEntities.Where(e => e.NestedDictionary["NullableCustomer"] == null),
            (true, true) => ctx.DictionaryEntities.Where(e => e.NestedDictionary["NullableCustomer"] != null),
            (false, false) => ctx.DictionaryEntities.Where(e => e.JsonNestedDictionary["NullableCustomer"] == null),
            (false, true) => ctx.DictionaryEntities.Where(e => e.JsonNestedDictionary["NullableCustomer"] != null)
        };

        Assert.Equal(notEqual ? 2 : 1, query.Select(e => e.Id).Single());

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE d."{(jsonb ? "NestedDictionary" : "JsonNestedDictionary")}" ->> 'NullableCustomer' IS {(notEqual ? "NOT NULL" : "NULL")}
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void Dictionary_indexer_compare_typed_container_parameter(bool jsonb, bool notEqual, bool nonNull)
    {
        using var ctx = CreateContext();
        Dictionary<string, int>? expected = nonNull ? new() { ["Age"] = 35 } : null;
        var query = (jsonb, notEqual) switch
        {
            (true, false) => ctx.DictionaryEntities.Where(e => e.NestedDictionary["NullableCustomer"] == EF.Parameter(expected)),
            (true, true) => ctx.DictionaryEntities.Where(e => e.NestedDictionary["NullableCustomer"] != EF.Parameter(expected)),
            (false, false) => ctx.DictionaryEntities.Where(e => e.JsonNestedDictionary["NullableCustomer"] == EF.Parameter(expected)),
            (false, true) => ctx.DictionaryEntities.Where(e => e.JsonNestedDictionary["NullableCustomer"] != EF.Parameter(expected))
        };

        Assert.Equal(nonNull == notEqual ? 1 : 2, query.Select(e => e.Id).Single());

        var left = $"""
CASE
    WHEN d."{(jsonb ? "NestedDictionary" : "JsonNestedDictionary")}" ->> 'NullableCustomer' IS NULL THEN NULL
    ELSE d."{(jsonb ? "NestedDictionary" : "JsonNestedDictionary")}" -> 'NullableCustomer'
END
""";
        var predicate = (nonNull, notEqual) switch
        {
            (false, false) => $"{left} IS NULL",
            (false, true) => $"{left} IS NOT NULL",
            (true, false) => $"{left} = @expected",
            (true, true) => $"{left} <> @expected OR {left} IS NULL"
        };
        var parameter = nonNull
            ? "@expected='System.Collections.Generic.Dictionary`2[System.String,System.Int32]' (DbType = Object)\n\n"
            : "";

        AssertSql(
            $"""
{parameter}SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE {predicate}
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_JsonDom_null_projection(bool jsonb)
    {
        using var ctx = CreateContext();
        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => (JsonDocument)e.JsonbDictionary["NullableName"] != null).OrderBy(e => e.Id).Select(e => new
            {
                Element = (JsonElement)e.JsonbDictionary["NullableName"],
                Document = (JsonDocument)e.JsonbDictionary["NullableName"]
            })
            : ctx.DictionaryEntities.Where(e => (JsonDocument)e.JsonDictionary["NullableName"] != null).OrderBy(e => e.Id).Select(e => new
            {
                Element = (JsonElement)e.JsonDictionary["NullableName"],
                Document = (JsonDocument)e.JsonDictionary["NullableName"]
            });

        var values = query.ToArray();
        Assert.Equal(JsonValueKind.Null, values[0].Element.ValueKind);
        Assert.Equal(JsonValueKind.Null, values[0].Document.RootElement.ValueKind);
        Assert.Equal("Moe", values[1].Element.GetString());
        Assert.Equal("Moe", values[1].Document.RootElement.GetString());
        foreach (var value in values)
        {
            value.Document.Dispose();
        }

        AssertSql(
            $"""
SELECT d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'NullableName' AS "Element", d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'NullableName' AS "Document"
FROM "DictionaryEntities" AS d
WHERE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'NullableName' IS NOT NULL
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_JsonElement_projection(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => (JsonElement)e.JsonbDictionary["Name"])
            : ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => (JsonElement)e.JsonDictionary["Name"]);

        Assert.Equal(["Joe", "Moe"], query.ToArray().Select(v => v.GetString()));

        AssertSql(
            $"""
SELECT d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name'
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_json_null(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonbDictionary["NullableName"])
            : ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonDictionary["NullableName"]);

        var values = query.ToArray();
        Assert.Null(values[0]);
        Assert.Equal("Moe", Assert.IsType<JsonElement>(values[1]).GetString());

        AssertSql(
            $"""
SELECT d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'NullableName'
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_indexer_compare_json_null(bool jsonb)
    {
        using var ctx = CreateContext();

        var query = jsonb
            ? ctx.DictionaryEntities.Where(e => e.JsonbDictionary["NullableName"] == null)
            : ctx.DictionaryEntities.Where(e => e.JsonDictionary["NullableName"] == null);

        Assert.Equal(1, query.Select(e => e.Id).Single());

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'NullableName' IS NULL
LIMIT 2
""");
    }

    [Fact]
    public void String_to_object_projection_union()
    {
        using var ctx = CreateContext();
        var query = ctx.DictionaryEntities.Select(e => (object)e.Name);

        Assert.Equal(["Joe", "Moe"], query.Union(query).ToArray().Cast<string>().Order());

        AssertSql(
            """
SELECT d."Name"
FROM "DictionaryEntities" AS d
UNION
SELECT d0."Name"
FROM "DictionaryEntities" AS d0
""");
    }

    [Fact]
    public void Group_by_string_to_object_projection()
    {
        using var ctx = CreateContext();

        var groups = ctx.DictionaryEntities.GroupBy(e => (object)e.Name)
            .Select(g => new { g.Key, Count = g.Count() }).OrderBy(g => g.Key).ToArray();

        Assert.Equal(["Joe", "Moe"], groups.Select(g => Assert.IsType<string>(g.Key)));
        Assert.All(groups, g => Assert.Equal(1, g.Count));

        AssertSql(
            """
SELECT d."Name" AS "Key", count(*)::int AS "Count"
FROM "DictionaryEntities" AS d
GROUP BY d."Name"
ORDER BY d."Name" NULLS FIRST
""");
    }

    [Fact]
    public void Dictionary_indexer_JsonTypeof()
    {
        using var ctx = CreateContext();

        Assert.Equal(2, ctx.DictionaryEntities.Count(e => EF.Functions.JsonTypeof(e.NestedStringDictionary["Customer"]) == "object"));

        AssertSql(
            """
SELECT count(*)::int
FROM "DictionaryEntities" AS d
WHERE jsonb_typeof(d."NestedStringDictionary" -> 'Customer') = 'object'
""");
    }

    [Fact]
    [MinimumPostgresVersion(17, 0)]
    public void Dictionary_indexer_JsonPathExists()
    {
        using var ctx = CreateContext();

        Assert.Equal(2, ctx.DictionaryEntities.Count(e => EF.Functions.JsonPathExists(e.NestedDictionary["Customer"], "$.Age")));

        AssertSql(
            """
SELECT count(*)::int
FROM "DictionaryEntities" AS d
WHERE JSON_EXISTS(d."NestedDictionary" -> 'Customer', '$.Age')
""");
    }

    [Theory]
    [InlineData(false, "Name")]
    [InlineData(true, "Name")]
    [InlineData(false, "NullableName")]
    [InlineData(true, "NullableName")]
    [MinimumPostgresVersion(17, 0)]
    public void Dictionary_indexer_JsonPathExists_object(bool jsonb, string key)
    {
        using var ctx = CreateContext();

        var count = jsonb
            ? ctx.DictionaryEntities.Count(e => EF.Functions.JsonPathExists(e.JsonbDictionary[key], "$"))
            : ctx.DictionaryEntities.Count(e => EF.Functions.JsonPathExists(e.JsonDictionary[key], "$"));

        Assert.Equal(2, count);

        AssertSql(
            $"""
SELECT count(*)::int
FROM "DictionaryEntities" AS d
WHERE JSON_EXISTS(d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> '{key}', '$')
""");
    }

    [Fact]
    [MinimumPostgresVersion(17, 0)]
    public void Dictionary_indexer_JsonPathExists_serialized_string()
    {
        using var ctx = CreateContext();

        Assert.Equal(2, ctx.DictionaryEntities.Count(e => EF.Functions.JsonPathExists(e.DictionaryInterface["Serialized"], "$.Age")));

        AssertSql(
            """
SELECT count(*)::int
FROM "DictionaryEntities" AS d
WHERE JSON_EXISTS(d."DictionaryInterface" ->> 'Serialized', '$.Age')
""");
    }

    [Fact]
    [MinimumPostgresVersion(17, 0)]
    public void Dictionary_indexer_JsonPathExists_int()
    {
        using var ctx = CreateContext();

        Assert.Equal(2, ctx.DictionaryEntities.Count(e => EF.Functions.JsonPathExists(e.ReadOnlyDictionary["Age"], "$")));

        AssertSql(
            """
SELECT count(*)::int
FROM "DictionaryEntities" AS d
WHERE JSON_EXISTS(to_jsonb(CAST(d."ReadOnlyDictionary" ->> 'Age' AS integer)), '$')
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(17, 0)]
    public void JsonPathExists_guid_constant()
    {
        using var ctx = CreateContext();

        Assert.Equal(2, ctx.DictionaryEntities.Count(e => EF.Functions.JsonPathExists(EF.Constant(Guid.Empty), "$")));

        AssertSql(
            """
SELECT count(*)::int
FROM "DictionaryEntities" AS d
WHERE JSON_EXISTS(to_jsonb('00000000-0000-0000-0000-000000000000'::uuid), '$')
""");
    }

    [Fact]
    [MinimumPostgresVersion(17, 0)]
    public void Dictionary_indexer_JsonPathExists_conditional()
    {
        using var ctx = CreateContext();

        Assert.Equal(
            2,
            ctx.DictionaryEntities.Count(
                e => EF.Functions.JsonPathExists(e.Id == 1 ? e.JsonbDictionary["Age"] : e.JsonbDictionary["Name"], "$")));

        AssertSql(
            """
SELECT count(*)::int
FROM "DictionaryEntities" AS d
WHERE JSON_EXISTS(CASE
    WHEN d."Id" = 1 THEN d."JsonbDictionary" -> 'Age'
    ELSE d."JsonbDictionary" -> 'Name'
END, '$')
""");
    }

    [Fact]
    [MinimumPostgresVersion(17, 0)]
    public void Dictionary_indexer_JsonPathExists_coalesce()
    {
        using var ctx = CreateContext();

        Assert.Equal(
            2,
            ctx.DictionaryEntities.Count(
                e => EF.Functions.JsonPathExists(e.JsonbDictionary["NullableName"] ?? e.JsonbDictionary["Name"], "$")));

        AssertSql(
            """
SELECT count(*)::int
FROM "DictionaryEntities" AS d
WHERE JSON_EXISTS(COALESCE(CASE
    WHEN d."JsonbDictionary" ->> 'NullableName' IS NULL THEN NULL
    ELSE d."JsonbDictionary" -> 'NullableName'
END, d."JsonbDictionary" -> 'Name'), '$')
""");
    }

    #endregion Dictionaries

    #region Support

    protected JsonPocoQueryContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class JsonPocoQueryContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<JsonbEntity> JsonbEntities { get; set; }
        public DbSet<JsonEntity> JsonEntities { get; set; }
        public DbSet<DictionaryEntity> DictionaryEntities { get; set; }

        public static async Task SeedAsync(JsonPocoQueryContext context)
        {
            byte[] longPayload = [.. Enumerable.Range(0, 80).Select(i => (byte)i)];
            context.JsonbEntities.AddRange(
                new JsonbEntity
                {
                    Id = 1,
                    Customer = CreateCustomer1(),
                    ToplevelArray = ["one", "two", "three"]
                },
                new JsonbEntity { Id = 2, Customer = CreateCustomer2(), ToplevelArray = [] });
            context.JsonEntities.AddRange(
                new JsonEntity
                {
                    Id = 1,
                    Customer = CreateCustomer1(),
                    ToplevelArray = ["one", "two", "three"]
                },
                new JsonEntity { Id = 2, Customer = CreateCustomer2(), ToplevelArray = [] });
            context.DictionaryEntities.AddRange(
                new DictionaryEntity
                {
                    Id = 1,
                    Name = "Joe",
                    JsonbDictionary = new()
                    {
                        ["Name"] = "Joe",
                        ["Age"] = 25,
                        ["StringAge"] = "25",
                        ["Timestamp"] = new DateTime(2020, 1, 1, 10, 30, 45, DateTimeKind.Utc).AddTicks(1234567),
                        ["NullableArray"] = null!,
                        ["Collections"] = new Dictionary<string, int[]> { ["Array"] = null! },
                        ["NullableName"] = null!,
                        ["Payload"] = (byte[])[1, 2, 3],
                        ["LongPayload"] = longPayload,
                        ["Customer"] = new Dictionary<string, int> { ["Age"] = 25 }
                    },
                    JsonDictionary = new()
                    {
                        ["Name"] = "Joe",
                        ["Age"] = 25,
                        ["StringAge"] = "25",
                        ["Timestamp"] = new DateTime(2020, 1, 1, 10, 30, 45, DateTimeKind.Utc).AddTicks(1234567),
                        ["NullableArray"] = null!,
                        ["Collections"] = new Dictionary<string, int[]> { ["Array"] = null! },
                        ["NullableName"] = null!,
                        ["Payload"] = (byte[])[1, 2, 3],
                        ["LongPayload"] = longPayload,
                        ["Customer"] = new Dictionary<string, int> { ["Age"] = 25 }
                    },
                    JsonbBinaryDictionary = new() { ["Payload"] = [1, 2, 3] },
                    JsonBinaryDictionary = new() { ["Payload"] = [1, 2, 3] },
                    DictionaryInterface = new Dictionary<string, string> { ["Name"] = "Joe", ["Serialized"] = """{"Age":25}""" },
                    ReadOnlyDictionary = new Dictionary<string, int> { ["Age"] = 25 },
                    NestedDictionary = new() { ["Customer"] = new() { ["Age"] = 25 }, ["NullableCustomer"] = null! },
                    JsonNestedDictionary = new() { ["Customer"] = new() { ["Age"] = 25 }, ["NullableCustomer"] = null! },
                    NestedStringDictionary = new()
                    {
                        ["Customer"] = new() { ["Name"] = "Joe", [""] = "Joe", ["NULL"] = "Joe", ["null"] = "Joe", ["NuLl"] = "Joe" }
                    }
                },
                new DictionaryEntity
                {
                    Id = 2,
                    Name = "Moe",
                    JsonbDictionary = new()
                    {
                        ["Name"] = "Moe",
                        ["Age"] = 35,
                        ["StringAge"] = "35",
                        ["Timestamp"] = new DateTime(1990, 3, 3, 17, 10, 15, DateTimeKind.Utc).AddTicks(1234560),
                        ["NullableArray"] = (int[])[3, 4],
                        ["Collections"] = new Dictionary<string, int[]> { ["Array"] = [3, 4] },
                        ["NullableName"] = "Moe",
                        ["Payload"] = (byte[])[4, 5, 6],
                        ["LongPayload"] = (byte[])[],
                        ["Customer"] = new Dictionary<string, int> { ["Age"] = 35 }
                    },
                    JsonDictionary = new()
                    {
                        ["Name"] = "Moe",
                        ["Age"] = 35,
                        ["StringAge"] = "35",
                        ["Timestamp"] = new DateTime(1990, 3, 3, 17, 10, 15, DateTimeKind.Utc).AddTicks(1234560),
                        ["NullableArray"] = (int[])[3, 4],
                        ["Collections"] = new Dictionary<string, int[]> { ["Array"] = [3, 4] },
                        ["NullableName"] = "Moe",
                        ["Payload"] = (byte[])[4, 5, 6],
                        ["LongPayload"] = (byte[])[],
                        ["Customer"] = new Dictionary<string, int> { ["Age"] = 35 }
                    },
                    JsonbBinaryDictionary = new() { ["Payload"] = [4, 5, 6] },
                    JsonBinaryDictionary = new() { ["Payload"] = [4, 5, 6] },
                    DictionaryInterface = new Dictionary<string, string> { ["Name"] = "Moe", ["Serialized"] = """{"Age":35}""" },
                    ReadOnlyDictionary = new Dictionary<string, int> { ["Age"] = 35 },
                    NestedDictionary = new() { ["Customer"] = new() { ["Age"] = 35 }, ["NullableCustomer"] = new() { ["Age"] = 35 } },
                    JsonNestedDictionary = new() { ["Customer"] = new() { ["Age"] = 35 }, ["NullableCustomer"] = new() { ["Age"] = 35 } },
                    NestedStringDictionary = new()
                    {
                        ["Customer"] = new() { ["Name"] = "Moe", [""] = "Moe", ["NULL"] = "Moe", ["null"] = "Moe", ["NuLl"] = "Moe" }
                    }
                });

            await context.SaveChangesAsync();

            static Customer CreateCustomer1()
                => new()
                {
                    Name = "Joe",
                    Age = 25,
                    ID = Guid.Empty,
                    IsVip = false,
                    Statistics = new Statistics
                    {
                        Visits = 4,
                        Purchases = 3,
                        Nested = new NestedStatistics
                        {
                            SomeProperty = 10,
                            SomeNullableInt = 20,
                            SomeNullableGuid = Guid.Parse("d5f2685d-e5c4-47e5-97aa-d0266154eb2d"),
                            IntArray = [3, 4],
                            IntList = [3, 4]
                        }
                    },
                    Orders =
                    [
                        new Order
                        {
                            Price = 99.5m, ShippingAddress = "Some address 1",
                        },
                        new Order
                        {
                            Price = 23, ShippingAddress = "Some address 2",
                        }
                    ],
                    VariousTypes = new VariousTypes
                    {
                        String = "foo",
                        Int16 = 8,
                        Int32 = 8,
                        Int64 = 8,
                        Bool = false,
                        Decimal = 10m,
                        DateTime = new DateTime(2020, 1, 1, 10, 30, 45, DateTimeKind.Utc),
                        DateTimeOffset = new DateTimeOffset(2020, 1, 1, 10, 30, 45, TimeSpan.Zero)
                    }
                };

            static Customer CreateCustomer2()
                => new()
                {
                    Name = "Moe",
                    Age = 35,
                    ID = Guid.Parse("3272b593-bfe2-4ecf-81ae-4242b0632465"),
                    IsVip = true,
                    Statistics = new Statistics
                    {
                        Visits = 20,
                        Purchases = 25,
                        Nested = new NestedStatistics
                        {
                            SomeProperty = 20,
                            SomeNullableInt = null,
                            SomeNullableGuid = null,
                            IntArray = [5, 6, 7],
                            IntList =
                            [
                                5,
                                6,
                                7
                            ]
                        }
                    },
                    Orders =
                    [
                        new Order
                        {
                            Price = 5, ShippingAddress = "Moe's address",
                        }
                    ],
                    VariousTypes = new VariousTypes
                    {
                        String = "bar",
                        Int16 = 9,
                        Int32 = 9,
                        Int64 = 9,
                        Bool = true,
                        Decimal = 20.3m,
                        DateTime = new DateTime(1990, 3, 3, 17, 10, 15, DateTimeKind.Utc),
                        DateTimeOffset = new DateTimeOffset(1990, 3, 3, 17, 10, 15, TimeSpan.Zero)
                    }
                };
        }
    }

    public class DictionaryEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        [Column(TypeName = "jsonb")]
        public required Dictionary<string, object> JsonbDictionary { get; set; }

        [Column(TypeName = "json")]
        public required Dictionary<string, object> JsonDictionary { get; set; }

        [Column(TypeName = "jsonb")]
        public required Dictionary<string, byte[]> JsonbBinaryDictionary { get; set; }

        [Column(TypeName = "json")]
        public required Dictionary<string, byte[]> JsonBinaryDictionary { get; set; }

        [Column(TypeName = "jsonb")]
        public required IDictionary<string, string> DictionaryInterface { get; set; }

        [Column(TypeName = "jsonb")]
        public required IReadOnlyDictionary<string, int> ReadOnlyDictionary { get; set; }

        [Column(TypeName = "jsonb")]
        public required Dictionary<string, Dictionary<string, int>> NestedDictionary { get; set; }

        [Column(TypeName = "json")]
        public required Dictionary<string, Dictionary<string, int>> JsonNestedDictionary { get; set; }

        [Column(TypeName = "jsonb")]
        public required Dictionary<string, Dictionary<string, string>> NestedStringDictionary { get; set; }
    }

    public class JsonbEntity
    {
        public int Id { get; set; }

        [Column(TypeName = "jsonb")]
        public required Customer Customer { get; set; }

        [Column(TypeName = "jsonb")]
        public required string[] ToplevelArray { get; set; }
    }

    public class JsonEntity
    {
        public int Id { get; set; }

        [Column(TypeName = "json")]
        public required Customer Customer { get; set; }

        [Column(TypeName = "json")]
        public required string[] ToplevelArray { get; set; }
    }

    public class JsonPocoQueryFixture : SharedStoreFixtureBase<JsonPocoQueryContext>
    {
        static JsonPocoQueryFixture()
        {
            // TODO: Switch to using NpgsqlDataSource
#pragma warning disable CS0618 // Type or member is obsolete
            NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        protected override string StoreName
            => "JsonPocoQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(JsonPocoQueryContext context)
            => JsonPocoQueryContext.SeedAsync(context);
    }

    public class Customer
    {
        public required string Name { get; set; }
        public int Age { get; set; }
        public Guid ID { get; set; }

        [JsonPropertyName("is_vip")]
        public bool IsVip { get; set; }

        public Statistics Statistics { get; set; } = null!;
        public Order[] Orders { get; set; } = null!;
        public VariousTypes VariousTypes { get; set; } = null!;
    }

    public class Statistics
    {
        public long Visits { get; set; }
        public int Purchases { get; set; }
        public required NestedStatistics Nested { get; set; }
    }

    public class NestedStatistics
    {
        public int SomeProperty { get; set; }
        public int? SomeNullableInt { get; set; }
        public int[] IntArray { get; set; } = null!;
        public List<int> IntList { get; set; } = null!;
        public Guid? SomeNullableGuid { get; set; }
    }

    public class Order
    {
        public decimal Price { get; set; }
        public string ShippingAddress { get; set; } = null!;
    }

    public class VariousTypes
    {
        public string String { get; set; } = null!;
        public int Int16 { get; set; }
        public int Int32 { get; set; }
        public int Int64 { get; set; }
        public bool Bool { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
    }

    #endregion
}
