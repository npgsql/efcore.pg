using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage;

public class JsonNodeTypeMappingTest
{
    [Theory]
    [InlineData(typeof(JsonNode))]
    [InlineData(typeof(JsonObject))]
    [InlineData(typeof(JsonArray))]
    [InlineData(typeof(JsonValue))]
    public void Can_map_JsonNode_types_to_jsonb(Type clrType)
    {
        var typeMappingSource = CreateTypeMappingSource();
        var mapping = typeMappingSource.FindMapping(clrType);

        Assert.NotNull(mapping);
        Assert.Equal("jsonb", mapping.StoreType);
        Assert.IsType<NpgsqlJsonTypeMapping>(mapping);
        Assert.Equal(clrType, mapping.ClrType);
    }

    [Theory]
    [InlineData("jsonb", typeof(JsonNode))]
    [InlineData("jsonb", typeof(JsonObject))]
    [InlineData("jsonb", typeof(JsonArray))]
    [InlineData("jsonb", typeof(JsonValue))]
    [InlineData("json", typeof(JsonNode))]
    [InlineData("json", typeof(JsonObject))]
    [InlineData("json", typeof(JsonArray))]
    [InlineData("json", typeof(JsonValue))]
    public void Can_map_store_types_to_JsonNode_types(string storeType, Type clrType)
    {
        var typeMappingSource = CreateTypeMappingSource();
        var mapping = typeMappingSource.FindMapping(clrType, storeType);

        Assert.NotNull(mapping);
        Assert.Equal(storeType, mapping.StoreType);
        Assert.IsType<NpgsqlJsonTypeMapping>(mapping);
        Assert.Equal(clrType, mapping.ClrType);
    }

    [Fact]
    public void JsonNode_mappings_generate_correct_sql_literals()
    {
        var mapping = new NpgsqlJsonTypeMapping("jsonb", typeof(JsonNode));

        var jsonNode = JsonNode.Parse("""{"name": "test", "value": 42}""");
        var literal = mapping.GenerateSqlLiteral(jsonNode);

        Assert.Contains("name", literal);
        Assert.Contains("test", literal);
        Assert.Contains("value", literal);
        Assert.Contains("42", literal);
    }

    [Fact]
    public void JsonObject_mappings_generate_correct_sql_literals()
    {
        var mapping = new NpgsqlJsonTypeMapping("jsonb", typeof(JsonObject));

        var jsonObject = new JsonObject
        {
            ["category"] = "books",
            ["count"] = 5
        };
        var literal = mapping.GenerateSqlLiteral(jsonObject);

        Assert.Contains("category", literal);
        Assert.Contains("books", literal);
        Assert.Contains("count", literal);
        Assert.Contains("5", literal);
    }

    [Fact]
    public void JsonArray_mappings_generate_correct_sql_literals()
    {
        var mapping = new NpgsqlJsonTypeMapping("jsonb", typeof(JsonArray));

        var jsonArray = new JsonArray { "item1", "item2", 42 };
        var literal = mapping.GenerateSqlLiteral(jsonArray);

        Assert.Contains("item1", literal);
        Assert.Contains("item2", literal);
        Assert.Contains("42", literal);
    }

    [Fact]
    public void JsonValue_mappings_generate_correct_sql_literals()
    {
        var mapping = new NpgsqlJsonTypeMapping("jsonb", typeof(JsonValue));

        var jsonValue = JsonValue.Create("test value");
        var literal = mapping.GenerateSqlLiteral(jsonValue);

        Assert.Contains("test value", literal);
    }

    [Fact]
    public void JsonNode_mappings_handle_null_values()
    {
        var mapping = new NpgsqlJsonTypeMapping("jsonb", typeof(JsonNode));

        var literal = mapping.GenerateSqlLiteral(null);

        Assert.Equal("NULL", literal);
    }

    private static NpgsqlTypeMappingSource CreateTypeMappingSource()
    {
        var builder = new DbContextOptionsBuilder();
        var npgsqlBuilder = new NpgsqlDbContextOptionsBuilder(builder);
        npgsqlBuilder.SetPostgresVersion(new Version(13, 0));

        var options = new NpgsqlSingletonOptions();
        options.Initialize(builder.Options);

        return new NpgsqlTypeMappingSource(
            new TypeMappingSourceDependencies(
                new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                new JsonValueReaderWriterSource(new JsonValueReaderWriterSourceDependencies()),
                []),
            new RelationalTypeMappingSourceDependencies([]),
            new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
            options);
    }
}
