using System.Text.Json;
using System.Text.Json.Serialization;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Types;

public partial class LTreeTest
{
    [ConditionalFact]
    public void ToString_works()
        => Assert.Equal("Top.Sub", ((LTree)"Top.Sub").ToString());

    [ConditionalTheory]
    [InlineData("Top.Sub", "\"Top.Sub\"")]
    [InlineData("", "\"\"")]
    [InlineData(null, "null")]
    public void Json_roundtrips(string value, string json)
    {
        LTree ltree = value;

        Assert.Equal(json, JsonSerializer.Serialize(ltree));
        Assert.Equal(ltree, JsonSerializer.Deserialize<LTree>(json));
    }

    [ConditionalFact]
    public void Json_roundtrips_property()
    {
        var node = new Node("Top.Sub");
        var json = JsonSerializer.Serialize(node);

        Assert.Equal("""{"Path":"Top.Sub"}""", json);
        Assert.Equal(node, JsonSerializer.Deserialize<Node>(json));
    }

    [ConditionalFact]
    public void Json_roundtrips_nullable()
    {
        LTree? ltree = "Top.Sub";

        Assert.Equal("\"Top.Sub\"", JsonSerializer.Serialize(ltree));
        Assert.Equal(ltree, JsonSerializer.Deserialize<LTree?>("\"Top.Sub\""));
        Assert.Equal("null", JsonSerializer.Serialize((LTree?)null));
        Assert.Null(JsonSerializer.Deserialize<LTree?>("null"));
    }

    [ConditionalFact]
    public void Json_roundtrips_with_source_generation()
    {
        LTree ltree = "Top.Sub";
        var typeInfo = LTreeJsonContext.Default.LTree;

        Assert.Equal("\"Top.Sub\"", JsonSerializer.Serialize(ltree, typeInfo));
        Assert.Equal(ltree, JsonSerializer.Deserialize("\"Top.Sub\"", typeInfo));
    }

    [ConditionalFact]
    public void NLevel_throws_on_client()
        => Assert.Throws<InvalidOperationException>(() => ((LTree)"Top.Sub").NLevel);

    private record Node(LTree Path);

    [JsonSerializable(typeof(LTree))]
    private partial class LTreeJsonContext : JsonSerializerContext;
}
