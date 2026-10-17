using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonStringQueryTest : IClassFixture<JsonStringQueryTest.JsonStringQueryFixture>
{
    private JsonStringQueryFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public JsonStringQueryTest(JsonStringQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public void Roundtrip()
    {
        using var ctx = CreateContext();
        var entity = ctx.JsonEntities.Single(e => e.Id == 1);
        PerformAsserts(entity.CustomerJsonb);
        PerformAsserts(entity.CustomerJson);

        static void PerformAsserts(string customerText)
        {
            var customer = JsonDocument.Parse(customerText).RootElement;

            Assert.Equal("Joe", customer.GetProperty("Name").GetString());
            Assert.Equal(25, customer.GetProperty("Age").GetInt32());

            var order1 = customer.GetProperty("Orders")[0];

            Assert.Equal(99.5m, order1.GetProperty("Price").GetDecimal());
            Assert.Equal("Some address 1", order1.GetProperty("ShippingAddress").GetString());
            Assert.Equal(new DateTime(2019, 10, 1), order1.GetProperty("ShippingDate").GetDateTime());

            var order2 = customer.GetProperty("Orders")[1];

            Assert.Equal(23, order2.GetProperty("Price").GetDecimal());
            Assert.Equal("Some address 2", order2.GetProperty("ShippingAddress").GetString());
            Assert.Equal(new DateTime(2019, 10, 10), order2.GetProperty("ShippingDate").GetDateTime());
        }
    }

    [Fact]
    public void Literal()
    {
        using var ctx = CreateContext();

        Assert.Empty(
            ctx.JsonEntities.Where(
                e => e.CustomerJsonb == """{"Name":"Test customer","Age":80,"IsVip":false,"Statistics":null,"Orders":null}"""));

        AssertSql(
            """
SELECT j."Id", j."CustomerJson", j."CustomerJsonb", j."SomeString"
FROM "JsonEntities" AS j
WHERE j."CustomerJsonb" = '{"Name":"Test customer","Age":80,"IsVip":false,"Statistics":null,"Orders":null}'
""");
    }

    [Fact]
    public void Parameter()
    {
        using var ctx = CreateContext();

        var expected = ctx.JsonEntities.Find(1)!.CustomerJsonb;
        var actual = ctx.JsonEntities.Single(e => e.CustomerJsonb == expected).CustomerJsonb;

        Assert.Equal(actual, expected);

        AssertSql(
            """
@p='1'

SELECT j."Id", j."CustomerJson", j."CustomerJsonb", j."SomeString"
FROM "JsonEntities" AS j
WHERE j."Id" = @p
LIMIT 1
""",
            //
            """
@expected='{"Age": 25, "Name": "Joe", "IsVip": false, "Orders": [{"Price": 99.5, "ShippingDate": "2019-10-01", "ShippingAddress": "Some address 1"}, {"Price": 23, "ShippingDate": "2019-10-10", "ShippingAddress": "Some address 2"}], "Statistics": {"Nested": {"IntArray": [3, 4], "SomeProperty": 10}, "Visits": 4, "Purchases": 3}}' (DbType = Object)

SELECT j."Id", j."CustomerJson", j."CustomerJsonb", j."SomeString"
FROM "JsonEntities" AS j
WHERE j."CustomerJsonb" = @expected
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Conditional_serialized_json(bool jsonb, bool constant)
    {
        using var ctx = CreateContext();
        var serialized = """{"Age":25}""";
        var query = (jsonb, constant) switch
        {
            (true, false) => ctx.JsonEntities.OrderBy(e => e.Id)
                .Select(e => e.Id == 1 ? e.CustomerJsonb : EF.Parameter(serialized)),
            (false, false) => ctx.JsonEntities.OrderBy(e => e.Id)
                .Select(e => e.Id == 1 ? e.CustomerJson : EF.Parameter(serialized)),
            (true, true) => ctx.JsonEntities.OrderBy(e => e.Id)
                .Select(e => e.Id == 1 ? e.CustomerJsonb : EF.Constant(serialized)),
            (false, true) => ctx.JsonEntities.OrderBy(e => e.Id)
                .Select(e => e.Id == 1 ? e.CustomerJson : EF.Constant(serialized))
        };

        foreach (var value in query.ToArray())
        {
            using var document = JsonDocument.Parse(value);
            Assert.Equal(25, document.RootElement.GetProperty("Age").GetInt32());
        }

        var fallback = $"""{(constant ? """'{"Age":25}'""" : "@serialized")}::{(jsonb ? "jsonb" : "json")}""";
        AssertSql(
            (constant ? "" : "@serialized='{\"Age\":25}'\n\n") + $"""
SELECT CASE
    WHEN j."Id" = 1 THEN j."{(jsonb ? "CustomerJsonb" : "CustomerJson")}"
    ELSE {fallback}
END
FROM "JsonEntities" AS j
ORDER BY j."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Boxed_serialized_json_composition(bool jsonb, bool coalesce, bool nullable)
    {
        using var ctx = CreateContext();
        var serialized = """{"Age":25}""";
        var entities = ctx.JsonEntities.Where(e => e.Id <= 2).OrderBy(e => e.Id);
        var query = (jsonb, coalesce, nullable) switch
        {
            (true, false, _) => entities.Select(e => e.Id == 1 ? (object)e.CustomerJsonb : (object)EF.Parameter(serialized)),
            (false, false, _) => entities.Select(e => e.Id == 1 ? (object)e.CustomerJson : (object)EF.Parameter(serialized)),
            (true, true, false) => entities.Select(e => (object)e.CustomerJsonb ?? (object)EF.Parameter(serialized)),
            (false, true, false) => entities.Select(e => (object)e.CustomerJson ?? (object)EF.Parameter(serialized)),
            (true, true, true) => entities.Select(e => (object?)(e.Id == 1 ? e.CustomerJsonb : null) ?? (object)EF.Parameter(serialized)),
            (false, true, true) => entities.Select(e => (object?)(e.Id == 1 ? e.CustomerJson : null) ?? (object)EF.Parameter(serialized))
        };

        var values = query.ToArray();
        Assert.All(values, value => Assert.IsType<string>(value));
        using var first = JsonDocument.Parse((string)values[0]);
        using var second = JsonDocument.Parse((string)values[1]);
        Assert.Equal(25, first.RootElement.GetProperty("Age").GetInt32());
        Assert.Equal(coalesce && !nullable ? 35 : 25, second.RootElement.GetProperty("Age").GetInt32());

        var column = $"j.\"{(jsonb ? "CustomerJsonb" : "CustomerJson")}\"";
        var fallback = $"@serialized::{(jsonb ? "jsonb" : "json")}";
        var conditional = $"""
CASE
    WHEN j."Id" = 1 THEN {column}
{(nullable ? "" : $"    ELSE {fallback}\n")}END
""";
        var projection = (coalesce, nullable) switch
        {
            (false, _) => conditional,
            (true, false) => column,
            (true, true) => $"COALESCE({conditional}, @serialized)"
        };
        AssertSql(
            (coalesce && !nullable ? "" : $"@serialized='{{\"Age\":25}}'{(coalesce ? " (DbType = Object)" : "")}\n\n") + $"""
SELECT {projection}
FROM "JsonEntities" AS j
WHERE j."Id" <= 2
ORDER BY j."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Boxed_serialized_json_conditional_null(bool jsonb, bool reversed)
    {
        using var ctx = CreateContext();
        var entities = ctx.JsonEntities.Where(e => e.Id <= 2).OrderBy(e => e.Id);
        var query = (jsonb, reversed) switch
        {
            (true, false) => entities.Select(e => e.Id == 1 ? (object)e.CustomerJsonb : null),
            (false, false) => entities.Select(e => e.Id == 1 ? (object)e.CustomerJson : null),
            (true, true) => entities.Select(e => e.Id == 2 ? null : (object)e.CustomerJsonb),
            (false, true) => entities.Select(e => e.Id == 2 ? null : (object)e.CustomerJson)
        };

        var values = query.ToArray();
        using var document = JsonDocument.Parse(Assert.IsType<string>(values[0]));
        Assert.Equal(25, document.RootElement.GetProperty("Age").GetInt32());
        Assert.Null(values[1]);

        var column = $"j.\"{(jsonb ? "CustomerJsonb" : "CustomerJson")}\"";
        AssertSql(
            $"""
SELECT CASE
    WHEN j."Id" = {(reversed ? 2 : 1)} THEN {(reversed ? "NULL" : column)}
{(reversed ? $"    ELSE {column}\n" : "")}END
FROM "JsonEntities" AS j
WHERE j."Id" <= 2
ORDER BY j."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, "\"foo\"")]
    [InlineData(true, "\"foo\"")]
    [InlineData(false, "null")]
    [InlineData(true, "null")]
    public void Boxed_serialized_json_cast_back(bool jsonb, string serialized)
    {
        using var ctx = CreateContext();
        ctx.Database.CreateExecutionStrategy().Execute(
            () =>
            {
                using var transaction = ctx.Database.BeginTransaction();
                ctx.Database.ExecuteSqlInterpolated(
                    $"""UPDATE "JsonEntities" SET "CustomerJsonb" = {serialized}::jsonb, "CustomerJson" = {serialized}::json WHERE "Id" = 1""");
                Fixture.TestSqlLoggerFactory.Clear();

                var query = jsonb
                    ? ctx.JsonEntities.Where(e => e.Id == 1).Select(e => (string)(e.Id == 1 ? (object)e.CustomerJsonb : null)!)
                    : ctx.JsonEntities.Where(e => e.Id == 1).Select(e => (string)(e.Id == 1 ? (object)e.CustomerJson : null)!);

                Assert.Equal(serialized, query.Single());

                AssertSql(
                    $"""
SELECT CASE
    WHEN j."Id" = 1 THEN j."{(jsonb ? "CustomerJsonb" : "CustomerJson")}"
END
FROM "JsonEntities" AS j
WHERE j."Id" = 1
LIMIT 2
""");
            });
    }

    #region Functions

    [Fact]
    public void JsonContains_with_json_element()
    {
        using var ctx = CreateContext();

        var element = JsonDocument.Parse("""{"Name": "Joe", "Age": 25}""").RootElement;
        var count = ctx.JsonEntities.Count(e => EF.Functions.JsonContains(e.CustomerJsonb, element));

        Assert.Equal(1, count);

        AssertSql(
            """
@element='{"Name": "Joe", "Age": 25}' (DbType = Object)

SELECT count(*)::int
FROM "JsonEntities" AS j
WHERE j."CustomerJsonb" @> @element
""");
    }

    [Fact]
    public void JsonContains_with_string()
    {
        using var ctx = CreateContext();

        var count = ctx.JsonEntities.Count(e => EF.Functions.JsonContains(e.CustomerJsonb, """{"Name": "Joe", "Age": 25}"""));

        Assert.Equal(1, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "JsonEntities" AS j
WHERE j."CustomerJsonb" @> '{"Name": "Joe", "Age": 25}'
""");
    }

    [Fact]
    public void JsonContains_with_string_column()
    {
        using var ctx = CreateContext();

        var count = ctx.JsonEntities.Count(
            e => EF.Functions.JsonContains(e.CustomerJsonb, """
                {"Name": "
                """ + e.SomeString + """
                    ", "Age": 25}
                    """));

        Assert.Equal(1, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "JsonEntities" AS j
WHERE j."CustomerJsonb" @> CAST('{"Name": "' || COALESCE(j."SomeString", '') || '", "Age": 25}' AS jsonb)
""");
    }

    [Fact]
    public void JsonContained_with_json_element()
    {
        using var ctx = CreateContext();

        var element = JsonDocument.Parse("""{"Name": "Joe", "Age": 25}""").RootElement;
        var count = ctx.JsonEntities.Count(e => EF.Functions.JsonContained(element, e.CustomerJsonb));

        Assert.Equal(1, count);

        AssertSql(
            """
@element='{"Name": "Joe", "Age": 25}' (DbType = Object)

SELECT count(*)::int
FROM "JsonEntities" AS j
WHERE @element <@ j."CustomerJsonb"
""");
    }

    [Fact]
    public void JsonContained_with_string()
    {
        using var ctx = CreateContext();

        var count = ctx.JsonEntities.Count(e => EF.Functions.JsonContained("""{"Name": "Joe", "Age": 25}""", e.CustomerJsonb));

        Assert.Equal(1, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "JsonEntities" AS j
WHERE '{"Name": "Joe", "Age": 25}' <@ j."CustomerJsonb"
""");
    }

    [Fact]
    public void JsonExists()
    {
        using var ctx = CreateContext();

        var count = ctx.JsonEntities.Count(e => EF.Functions.JsonExists(e.CustomerJsonb, "Age"));

        Assert.Equal(2, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "JsonEntities" AS j
WHERE j."CustomerJsonb" ? 'Age'
""");
    }

    [Fact]
    public void JsonExistAny()
    {
        using var ctx = CreateContext();

        var count = ctx.JsonEntities.Count(e => EF.Functions.JsonExistAny(e.CustomerJsonb, "foo", "Age"));

        Assert.Equal(2, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "JsonEntities" AS j
WHERE j."CustomerJsonb" ?| ARRAY['foo','Age']::text[]
""");
    }

    [Fact]
    public void JsonExistAll()
    {
        using var ctx = CreateContext();

        var count = ctx.JsonEntities.Count(e => EF.Functions.JsonExistAll(e.CustomerJsonb, "foo", "Age"));

        Assert.Equal(0, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "JsonEntities" AS j
WHERE j."CustomerJsonb" ?& ARRAY['foo','Age']::text[]
""");
    }

    #endregion Functions

    #region Support

    protected JsonStringQueryContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class JsonStringQueryContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<JsonEntity> JsonEntities { get; set; }

        public static async Task SeedAsync(JsonStringQueryContext context)
        {
            const string customer1 = """

                {
                    "Name": "Joe",
                    "Age": 25,
                    "IsVip": false,
                    "Statistics":
                    {
                        "Visits": 4,
                        "Purchases": 3,
                        "Nested":
                        {
                            "SomeProperty": 10,
                            "IntArray": [3, 4]
                        }
                    },
                    "Orders":
                    [
                        {
                            "Price": 99.5,
                            "ShippingAddress": "Some address 1",
                            "ShippingDate": "2019-10-01"
                        },
                        {
                            "Price": 23,
                            "ShippingAddress": "Some address 2",
                            "ShippingDate": "2019-10-10"
                        }
                    ]
                }
""";

            const string customer2 = """

                {
                    "Name": "Moe",
                    "Age": 35,
                    "IsVip": true,
                    "Statistics":
                    {
                        "Visits": 20,
                        "Purchases": 25,
                        "Nested":
                        {
                            "SomeProperty": 20,
                            "IntArray": [5, 6]
                        }
                    },
                    "Orders":
                    [
                        {
                            "Price": 5,
                            "ShippingAddress": "Moe's address",
                            "ShippingDate": "2019-11-03"
                        }
                    ]
                }
""";

            const string array = "[1, 2, 3]";

            context.JsonEntities.AddRange(
                new JsonEntity
                {
                    Id = 1,
                    CustomerJsonb = customer1,
                    CustomerJson = customer1,
                    SomeString = "Joe"
                },
                new JsonEntity
                {
                    Id = 2,
                    CustomerJsonb = customer2,
                    CustomerJson = customer2,
                    SomeString = "Moe"
                },
                new JsonEntity
                {
                    Id = 3,
                    CustomerJsonb = array,
                    CustomerJson = array
                });

            await context.SaveChangesAsync();
        }
    }

    public class JsonEntity
    {
        public int Id { get; set; }

        [Column(TypeName = "jsonb")]
        public string CustomerJsonb { get; set; } = null!;

        [Column(TypeName = "json")]
        public string CustomerJson { get; set; } = null!;

        public string? SomeString { get; set; }
    }

    public class JsonStringQueryFixture : SharedStoreFixtureBase<JsonStringQueryContext>
    {
        protected override string StoreName
            => "JsonStringQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(JsonStringQueryContext context)
            => JsonStringQueryContext.SeedAsync(context);
    }

    #endregion
}
