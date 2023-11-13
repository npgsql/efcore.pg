using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class JsonDomQueryTest : IClassFixture<JsonDomQueryTest.JsonDomQueryFixture>
{
    private JsonDomQueryFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public JsonDomQueryTest(JsonDomQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Roundtrip(bool jsonb, bool viaDocument)
    {
        using var ctx = CreateContext();

        var customer = jsonb
            ? (viaDocument
                ? ctx.JsonbEntities.Single(e => e.Id == 1).CustomerDocument.RootElement
                : ctx.JsonbEntities.Single(e => e.Id == 1).CustomerElement)
            : (viaDocument
                ? ctx.JsonEntities.Single(e => e.Id == 1).CustomerDocument.RootElement
                : ctx.JsonEntities.Single(e => e.Id == 1).CustomerElement);

        var types = customer.GetProperty("VariousTypes");

        Assert.Equal("foo", types.GetProperty("String").GetString());
        Assert.Equal(8, types.GetProperty("Int16").GetInt16());
        Assert.Equal(8, types.GetProperty("Int32").GetInt32());
        Assert.Equal(8, types.GetProperty("Int64").GetInt64());
        Assert.Equal(10m, types.GetProperty("Decimal").GetDecimal());
        Assert.Equal(new DateTime(2020, 1, 1, 10, 30, 45), types.GetProperty("DateTime").GetDateTime());
        Assert.Equal(
            new DateTimeOffset(2020, 1, 1, 10, 30, 45, TimeSpan.FromHours(2)),
            types.GetProperty("DateTimeOffset").GetDateTimeOffset());
    }

    [Fact]
    public void Literal_document()
    {
        using var ctx = CreateContext();

        Assert.Empty(
            ctx.JsonbEntities.Where(
                e => e.CustomerDocument
                    == JsonDocument.Parse(
                        @"
{ ""Name"": ""Test customer"", ""Age"": 80 }", default)));

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE j."CustomerDocument" = '{"Name":"Test customer","Age":80}'
""");
    }

    [Fact]
    public void Parameter_document()
    {
        using var ctx = CreateContext();

        var expected = ctx.JsonbEntities.Find(1).CustomerDocument;
        var actual = ctx.JsonbEntities.Single(e => e.CustomerDocument == expected).CustomerDocument;

        Assert.Equal(actual, expected);

        AssertSql(
            """
@__p_0='1'

SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE j."Id" = @__p_0
LIMIT 1
""",
            //
            """
@__expected_0='System.Text.Json.JsonDocument' (DbType = Object)

SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE j."CustomerDocument" = @__expected_0
LIMIT 2
""");
    }

    [Fact]
    public void Parameter_element()
    {
        using var ctx = CreateContext();

        var expected = ctx.JsonbEntities.Find(1).CustomerElement;
        var actual = ctx.JsonbEntities.Single(e => e.CustomerElement.Equals(expected)).CustomerElement;

        Assert.Equal(actual, expected);

        AssertSql(
            """
@__p_0='1'

SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE j."Id" = @__p_0
LIMIT 1
""",
            //
            """
@__expected_0='{"ID": "00000000-0000-0000-0000-000000000000", "Age": 25, "Name": "Joe", "IsVip": false, "Orders": [{"Price": 99.5, "ShippingAddress": "Some address 1"}, {"Price": 23, "ShippingAddress": "Some address 2"}], "Statistics": {"Nested": {"IntList": [3, 4], "IntArray": [3, 4], "SomeProperty": 10, "SomeNullableInt": 20, "SomeNullableGuid": "d5f2685d-e5c4-47e5-97aa-d0266154eb2d"}, "Visits": 4, "Purchases": 3}, "VariousTypes": {"Bool": "false", "Int16": 8, "Int32": 8, "Int64": 8, "String": "foo", "Decimal": 10, "DateTime": "2020-01-01T10:30:45", "DateTimeOffset": "2020-01-01T10:30:45+02:00"}}' (DbType = Object)

SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE j."CustomerElement" = @__expected_0
LIMIT 2
""");
    }

    [Fact]
    public void Text_output_on_document()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.CustomerDocument.RootElement.GetProperty("Name").GetString() == "Joe");

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE j."CustomerDocument" ->> 'Name' = 'Joe'
LIMIT 2
""");
    }

    [Fact]
    public void Text_output()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.CustomerElement.GetProperty("Name").GetString() == "Joe");

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE j."CustomerElement" ->> 'Name' = 'Joe'
LIMIT 2
""");
    }

    [Fact]
    public void Text_output_json()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonEntities.Single(e => e.CustomerElement.GetProperty("Name").GetString() == "Joe");

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonEntities" AS j
WHERE j."CustomerElement" ->> 'Name' = 'Joe'
LIMIT 2
""");
    }

    [Fact]
    public void Integer_output()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.CustomerElement.GetProperty("Age").GetInt32() < 30);

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE CAST(j."CustomerElement" ->> 'Age' AS integer) < 30
LIMIT 2
""");
    }

    [Fact]
    public void Guid_output()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.CustomerElement.GetProperty("ID").GetGuid() == Guid.Empty);

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE CAST(j."CustomerElement" ->> 'ID' AS uuid) = '00000000-0000-0000-0000-000000000000'
LIMIT 2
""");
    }

    [Fact]
    public void Bool_output()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.CustomerElement.GetProperty("IsVip").GetBoolean());

        Assert.Equal("Moe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE CAST(j."CustomerElement" ->> 'IsVip' AS boolean)
LIMIT 2
""");
    }

    [Fact]
    public void Nested()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.CustomerElement.GetProperty("Statistics").GetProperty("Visits").GetInt64() == 4);

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE CAST(j."CustomerElement" #>> '{Statistics,Visits}' AS bigint) = 4
LIMIT 2
""");
    }

    [Fact]
    public void Nested_twice()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(
            e => e.CustomerElement.GetProperty("Statistics").GetProperty("Nested").GetProperty("SomeProperty").GetInt32() == 10);

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());
        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE CAST(j."CustomerElement" #>> '{Statistics,Nested,SomeProperty}' AS integer) = 10
LIMIT 2
""");
    }

    [Fact]
    public void Array_of_objects()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.CustomerElement.GetProperty("Orders")[0].GetProperty("Price").GetDecimal() == 99.5m);

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE CAST(j."CustomerElement" #>> '{Orders,0,Price}' AS numeric) = 99.5
LIMIT 2
""");
    }

    [Fact]
    public void Array_nested()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(
            e =>
                e.CustomerElement.GetProperty("Statistics").GetProperty("Nested").GetProperty("IntArray")[1].GetInt32() == 4);

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE CAST(j."CustomerElement" #>> '{Statistics,Nested,IntArray,1}' AS integer) = 4
LIMIT 2
""");
    }

    [Fact]
    public void Array_parameter_index()
    {
        using var ctx = CreateContext();

        var i = 1;
        var x = ctx.JsonbEntities.Single(
            e =>
                e.CustomerElement.GetProperty("Statistics").GetProperty("Nested").GetProperty("IntArray")[i].GetInt32() == 4);

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
@__i_0='1'

SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE CAST(j."CustomerElement" #>> ARRAY['Statistics','Nested','IntArray',@__i_0]::text[] AS integer) = 4
LIMIT 2
""");
    }

    [Fact]
    public void GetArrayLength()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.CustomerElement.GetProperty("Orders").GetArrayLength() == 2);

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE jsonb_array_length(j."CustomerElement" -> 'Orders') = 2
LIMIT 2
""");
    }

    [Fact]
    public void GetArrayLength_json()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonEntities.Single(e => e.CustomerElement.GetProperty("Orders").GetArrayLength() == 2);

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonEntities" AS j
WHERE json_array_length(j."CustomerElement" -> 'Orders') = 2
LIMIT 2
""");
    }

    [Fact]
    public void Like()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(e => e.CustomerElement.GetProperty("Name").GetString().StartsWith("J"));

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE j."CustomerElement" ->> 'Name' LIKE 'J%'
LIMIT 2
""");
    }

    [Fact] // #1363
    public void Where_nullable_guid()
    {
        using var ctx = CreateContext();

        var x = ctx.JsonbEntities.Single(
            e =>
                e.CustomerElement.GetProperty("Statistics").GetProperty("Nested").GetProperty("SomeNullableGuid").GetGuid()
                == Guid.Parse("d5f2685d-e5c4-47e5-97aa-d0266154eb2d"));

        Assert.Equal("Joe", x.CustomerElement.GetProperty("Name").GetString());

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE CAST(j."CustomerElement" #>> '{Statistics,Nested,SomeNullableGuid}' AS uuid) = 'd5f2685d-e5c4-47e5-97aa-d0266154eb2d'
LIMIT 2
""");
    }

    [Fact] // #1415
    public void Where_root_value()
    {
        using var ctx = CreateContext();

        _ = ctx.JsonbEntities.Single(e => e.CustomerElement.GetString() == "foo");

        AssertSql(
            """
SELECT j."Id", j."CustomerDocument", j."CustomerElement"
FROM "JsonbEntities" AS j
WHERE j."CustomerElement" #>> '{}' = 'foo'
LIMIT 2
""");
    }

    #region Functions

    [Fact]
    public void JsonContains_with_json_element()
    {
        using var ctx = CreateContext();
        var element = JsonDocument.Parse(@"{""Name"": ""Joe"", ""Age"": 25}").RootElement;
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonContains(e.CustomerElement, element));

        Assert.Equal(1, count);
        AssertSql(
            """
@__element_1='{"Name": "Joe", "Age": 25}' (DbType = Object)

SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE j."CustomerElement" @> @__element_1
""");
    }

    [Fact]
    public void JsonContains_with_string()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonContains(e.CustomerElement, @"{""Name"": ""Joe"", ""Age"": 25}"));

        Assert.Equal(1, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE j."CustomerElement" @> '{"Name": "Joe", "Age": 25}'
""");
    }

    [Fact]
    public void JsonContained_with_json_element()
    {
        using var ctx = CreateContext();
        var element = JsonDocument.Parse(@"{""Name"": ""Joe"", ""Age"": 25}").RootElement;
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonContained(element, e.CustomerElement));

        Assert.Equal(1, count);
        AssertSql(
            """
@__element_1='{"Name": "Joe", "Age": 25}' (DbType = Object)

SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE @__element_1 <@ j."CustomerElement"
""");
    }

    [Fact]
    public void JsonContained_with_string()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonContained(@"{""Name"": ""Joe"", ""Age"": 25}", e.CustomerElement));

        // Assert.Equal(1, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE '{"Name": "Joe", "Age": 25}' <@ j."CustomerElement"
""");
    }

    [Fact]
    public void JsonExists()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonExists(e.CustomerElement.GetProperty("Statistics"), "Visits"));

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE j."CustomerElement" -> 'Statistics' ? 'Visits'
""");
    }

    [Fact]
    public void JsonExistAny()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonExistAny(e.CustomerElement.GetProperty("Statistics"), "foo", "Visits"));

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE j."CustomerElement" -> 'Statistics' ?| ARRAY['foo','Visits']::text[]
""");
    }

    [Fact]
    public void JsonExistAll()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonExistAll(e.CustomerElement.GetProperty("Statistics"), "foo", "Visits"));

        Assert.Equal(0, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE j."CustomerElement" -> 'Statistics' ?& ARRAY['foo','Visits']::text[]
""");
    }

    [Fact]
    public void JsonTypeof()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonbEntities.Count(
            e =>
                EF.Functions.JsonTypeof(e.CustomerElement.GetProperty("Statistics").GetProperty("Visits")) == "number");

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonbEntities" AS j
WHERE jsonb_typeof(j."CustomerElement" #> '{Statistics,Visits}') = 'number'
""");
    }

    [Fact]
    public void JsonTypeof_json()
    {
        using var ctx = CreateContext();
        var count = ctx.JsonEntities.Count(
            e =>
                EF.Functions.JsonTypeof(e.CustomerElement.GetProperty("Statistics").GetProperty("Visits")) == "number");

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "JsonEntities" AS j
WHERE json_typeof(j."CustomerElement" #> '{Statistics,Visits}') = 'number'
""");
    }

    #endregion Functions

    #region Support

    protected JsonDomQueryContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class JsonDomQueryContext : PoolableDbContext
    {
        public DbSet<JsonbEntity> JsonbEntities { get; set; }
        public DbSet<JsonEntity> JsonEntities { get; set; }

        public JsonDomQueryContext(DbContextOptions options)
            : base(options)
        {
        }

        public static void Seed(JsonDomQueryContext context)
        {
            var (customer1, customer2, customer3) = (CreateCustomer1(), CreateCustomer2(), CreateCustomer3());

            context.JsonbEntities.AddRange(
                new JsonbEntity
                {
                    Id = 1,
                    CustomerDocument = customer1,
                    CustomerElement = customer1.RootElement
                },
                new JsonbEntity
                {
                    Id = 2,
                    CustomerDocument = customer2,
                    CustomerElement = customer2.RootElement
                },
                new JsonbEntity
                {
                    Id = 3,
                    CustomerDocument = customer3,
                    CustomerElement = customer3.RootElement
                });
            context.JsonEntities.AddRange(
                new JsonEntity
                {
                    Id = 1,
                    CustomerDocument = customer1,
                    CustomerElement = customer1.RootElement
                },
                new JsonEntity
                {
                    Id = 2,
                    CustomerDocument = customer2,
                    CustomerElement = customer2.RootElement
                },
                new JsonEntity
                {
                    Id = 3,
                    CustomerDocument = customer3,
                    CustomerElement = customer3.RootElement
                });
            context.SaveChanges();

            static JsonDocument CreateCustomer1()
                => JsonDocument.Parse(
                    """
{
    "Name": "Joe",
    "Age": 25,
    "ID": "00000000-0000-0000-0000-000000000000",
    "IsVip": false,
    "Statistics":
    {
        "Visits": 4,
        "Purchases": 3,
        "Nested":
        {
            "SomeProperty": 10,
            "SomeNullableInt": 20,
            "SomeNullableGuid": "d5f2685d-e5c4-47e5-97aa-d0266154eb2d",
            "IntArray": [3, 4],
            "IntList": [3, 4]
        }
    },
    "Orders":
    [
        {
            "Price": 99.5,
            "ShippingAddress": "Some address 1"
        },
        {
            "Price": 23,
            "ShippingAddress": "Some address 2"
        }
    ],
    "VariousTypes":
    {
        "String": "foo",
        "Int16": 8,
        "Int32": 8,
        "Int64": 8,
        "Bool": "false",
        "Decimal": 10,
        "DateTime": "2020-01-01T10:30:45",
        "DateTimeOffset": "2020-01-01T10:30:45+02:00"
    }
}
""");

            static JsonDocument CreateCustomer2()
                => JsonDocument.Parse(
                    """
{
    "Name": "Moe",
    "Age": 35,
    "ID": "3272b593-bfe2-4ecf-81ae-4242b0632465",
    "IsVip": true,
    "Statistics":
    {
        "Visits": 20,
        "Purchases": 25,
        "Nested":
        {
            "SomeProperty": 20,
            "SomeNullableInt": null,
            "SomeNullableGuid": null,
            "IntArray": [5, 6, 7],
            "IntArray": [5, 6, 7]
        }
    },
    "Orders":
    [
        {
            "Price": 5,
            "ShippingAddress": "Moe's address"
        }
    ],
    "VariousTypes":
    {
        "String": "bar",
        "Int16": 9,
        "Int32": 9,
        "Int64": 9,
        "Bool": "true",
        "Decimal": 20.3,
        "DateTime": "1990-03-03T17:10:15",
        "DateTimeOffset": "1990-03-03 17:10:15+10:00"
    }
}
""");

            static JsonDocument CreateCustomer3()
                => JsonDocument.Parse(@"""foo""");
        }
    }

    public class JsonbEntity
    {
        public int Id { get; set; }

        public JsonDocument CustomerDocument { get; set; }
        public JsonElement CustomerElement { get; set; }
    }

    public class JsonEntity
    {
        public int Id { get; set; }

        [Column(TypeName = "json")]
        public JsonDocument CustomerDocument { get; set; }

        [Column(TypeName = "json")]
        public JsonElement CustomerElement { get; set; }
    }

    public class JsonDomQueryFixture : SharedStoreFixtureBase<JsonDomQueryContext>
    {
        protected override string StoreName
            => "JsonDomQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void Seed(JsonDomQueryContext context)
            => JsonDomQueryContext.Seed(context);
    }

    #endregion
}
