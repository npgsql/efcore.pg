using System.Text.Json;
using System.Text.Json.Serialization;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NpgsqlJsonQueryTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dictionary_parameter_is_not_preserialized(bool jsonb)
    {
        var serializerOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        using var dataSource = new NpgsqlDataSourceBuilder("Host=localhost")
            .ConfigureJsonOptions(serializerOptions)
            .EnableDynamicJson()
            .MapEnum<Mood>("mood")
            .Build();
        using var context = new JsonContext(
            new DbContextOptionsBuilder().UseNpgsql(dataSource, o => o.MapEnum<Mood>("mood")).Options);
        var mood = Mood.Sad;
        var query = jsonb
            ? context.Set<JsonEntity>().Where(e => e.Jsonb["Mood"] == (object)EF.Parameter(mood))
            : context.Set<JsonEntity>().Where(e => e.Json["Mood"] == (object)EF.Parameter(mood));
        using var command = query.CreateDbCommand();

        var parameter = Assert.IsType<NpgsqlParameter>(Assert.Single(command.Parameters.Cast<object>()));
        Assert.Equal(NpgsqlDbType.Jsonb, parameter.NpgsqlDbType);
        Assert.Equal(mood, Assert.IsType<Mood>(parameter.Value));
        Assert.Equal("\"Sad\"", JsonSerializer.Serialize(parameter.Value, serializerOptions));
    }

    private class JsonContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonEntity>().Property(e => e.Json).HasColumnType("json");
            modelBuilder.Entity<JsonEntity>().Property(e => e.Jsonb).HasColumnType("jsonb");
        }
    }

    private class JsonEntity
    {
        public int Id { get; set; }
        public Dictionary<string, object> Json { get; set; } = [];
        public Dictionary<string, object> Jsonb { get; set; } = [];
    }

    private enum Mood
    {
        Happy,
        Sad
    }
}
