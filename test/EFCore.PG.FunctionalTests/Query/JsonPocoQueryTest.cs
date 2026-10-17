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
WHERE j."Customer" = @expected
LIMIT 2
""");
    }

    #region Output

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

        AssertSql(
            $"""
SELECT d."Id"
FROM "DictionaryEntities" AS d
WHERE d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" ->> 'Name' = 'Joe'::text
LIMIT 2
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

    [Fact]
    public void Dictionary_indexer_dictionary_projection()
    {
        using var ctx = CreateContext();

        var dictionaries = ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.NestedDictionary["Customer"]).ToArray();

        Assert.Equal([25, 35], dictionaries.Select(d => d["Age"]));

        AssertSql(
            """
SELECT d."NestedDictionary"
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
SELECT d."NestedStringDictionary"
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
SELECT d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}"
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
SELECT d."Id" = 1, d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}"
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
            ? ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonbDictionary["Name"] ?? e.JsonbDictionary["Age"])
            : ctx.DictionaryEntities.OrderBy(e => e.Id).Select(e => e.JsonDictionary["Name"] ?? e.JsonDictionary["Age"]);

        Assert.Equal(["Joe", "Moe"], query.ToArray().Select(v => Assert.IsType<JsonElement>(v).GetString()));

        AssertSql(
            $"""
SELECT d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}"
FROM "DictionaryEntities" AS d
ORDER BY d."Id" NULLS FIRST
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
    [InlineData(false)]
    [InlineData(true)]
    [MinimumPostgresVersion(17, 0)]
    public void Dictionary_indexer_JsonPathExists_object_string(bool jsonb)
    {
        using var ctx = CreateContext();

        var count = jsonb
            ? ctx.DictionaryEntities.Count(e => EF.Functions.JsonPathExists(e.JsonbDictionary["Name"], "$"))
            : ctx.DictionaryEntities.Count(e => EF.Functions.JsonPathExists(e.JsonDictionary["Name"], "$"));

        Assert.Equal(2, count);

        AssertSql(
            $"""
SELECT count(*)::int
FROM "DictionaryEntities" AS d
WHERE JSON_EXISTS(d."{(jsonb ? "JsonbDictionary" : "JsonDictionary")}" -> 'Name', '$')
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
                    JsonbDictionary = new() { ["Name"] = "Joe", ["Age"] = 25 },
                    JsonDictionary = new() { ["Name"] = "Joe", ["Age"] = 25 },
                    DictionaryInterface = new Dictionary<string, string> { ["Name"] = "Joe", ["Serialized"] = """{"Age":25}""" },
                    ReadOnlyDictionary = new Dictionary<string, int> { ["Age"] = 25 },
                    NestedDictionary = new() { ["Customer"] = new() { ["Age"] = 25 } },
                    NestedStringDictionary = new() { ["Customer"] = new() { ["Name"] = "Joe", [""] = "Joe" } }
                },
                new DictionaryEntity
                {
                    Id = 2,
                    JsonbDictionary = new() { ["Name"] = "Moe", ["Age"] = 35 },
                    JsonDictionary = new() { ["Name"] = "Moe", ["Age"] = 35 },
                    DictionaryInterface = new Dictionary<string, string> { ["Name"] = "Moe", ["Serialized"] = """{"Age":35}""" },
                    ReadOnlyDictionary = new Dictionary<string, int> { ["Age"] = 35 },
                    NestedDictionary = new() { ["Customer"] = new() { ["Age"] = 35 } },
                    NestedStringDictionary = new() { ["Customer"] = new() { ["Name"] = "Moe", [""] = "Moe" } }
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

        [Column(TypeName = "jsonb")]
        public required Dictionary<string, object> JsonbDictionary { get; set; }

        [Column(TypeName = "json")]
        public required Dictionary<string, object> JsonDictionary { get; set; }

        [Column(TypeName = "jsonb")]
        public required IDictionary<string, string> DictionaryInterface { get; set; }

        [Column(TypeName = "jsonb")]
        public required IReadOnlyDictionary<string, int> ReadOnlyDictionary { get; set; }

        [Column(TypeName = "jsonb")]
        public required Dictionary<string, Dictionary<string, int>> NestedDictionary { get; set; }

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
