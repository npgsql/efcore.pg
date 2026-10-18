using System.Text.Json;
using System.Text.Json.Serialization;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Primitive_parameter_uses_json_serialization(bool jsonb)
    {
        var serializerOptions = new JsonSerializerOptions { Converters = { new IntAsStringConverter() } };
        using var dataSource = new NpgsqlDataSourceBuilder("Host=localhost")
            .ConfigureJsonOptions(serializerOptions)
            .EnableDynamicJson()
            .Build();
        using var context = new JsonContext(new DbContextOptionsBuilder().UseNpgsql(dataSource).Options);
        var age = 25;
        var query = jsonb
            ? context.Set<JsonEntity>().Where(e => e.Jsonb["Age"] == (object)EF.Parameter(age))
            : context.Set<JsonEntity>().Where(e => e.Json["Age"] == (object)EF.Parameter(age));
        using var command = query.CreateDbCommand();

        var parameter = Assert.IsType<NpgsqlParameter>(Assert.Single(command.Parameters.Cast<object>()));
        Assert.Equal(NpgsqlDbType.Jsonb, parameter.NpgsqlDbType);
        Assert.Equal(age, Assert.IsType<int>(parameter.Value));
        Assert.Equal("\"25\"", JsonSerializer.Serialize(parameter.Value, serializerOptions));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Concat_rejects_json_operand(bool jsonb, bool reverse)
    {
        using var context = new JsonContext(new DbContextOptionsBuilder().UseNpgsql("Host=localhost").Options);
        var factory = context.GetService<ISqlExpressionFactory>();
        var native = factory.Constant(new[] { 1 }, context.GetService<IRelationalTypeMappingSource>().FindMapping(typeof(int[])));
        var json = factory.Constant(new[] { 1 }, new NpgsqlJsonTypeMapping(jsonb ? "jsonb" : "json", typeof(int[])));
        var method = typeof(Enumerable).GetMethods()
            .Single(m => m.Name == nameof(Enumerable.Concat) && m.GetParameters().Length == 2).MakeGenericMethod(typeof(int));

        Assert.Null(context.GetService<IMethodCallTranslatorProvider>().Translate(
            context.Model,
            null,
            method,
            reverse ? [json, native] : [native, json],
            context.GetService<IDiagnosticsLogger<DbLoggerCategory.Query>>()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Object_coalesce_operands_are_visited_once(bool json)
    {
        using var context = new JsonContext(
            new DbContextOptionsBuilder().UseNpgsql("Host=localhost")
                .ReplaceService<IRelationalSqlTranslatingExpressionVisitorFactory, CountingVisitorFactory>().Options);
        var parameter = Expression.Parameter(typeof(JsonEntity), "e");
        Expression value = json
            ? Expression.Call(
                Expression.Property(parameter, nameof(JsonEntity.Jsonb)),
                typeof(Dictionary<string, object>).GetMethod("get_Item")!,
                Expression.Constant("Missing"))
            : Expression.Convert(Expression.Property(parameter, nameof(JsonEntity.Name)), typeof(object));
        var body = value;
        for (var i = 0; i < 7; i++)
        {
            body = Expression.Coalesce(value, body);
        }

        var factory = (CountingVisitorFactory)context.GetService<IRelationalSqlTranslatingExpressionVisitorFactory>();
        _ = context.Set<JsonEntity>().Select(Expression.Lambda<Func<JsonEntity, object>>(body, parameter)).ToQueryString();

        Assert.Equal(7, factory.CoalesceVisits);
    }

    [Fact]
    public void Non_null_literal_can_serialize_to_json_null()
    {
        using var context = new JsonContext(new DbContextOptionsBuilder().UseNpgsql("Host=localhost").Options);
        using var command = context.Set<JsonEntity>().Where(e => e.NullPayload == new NullPayload()).CreateDbCommand();

        Assert.Contains("NULLIF", command.CommandText);
    }

    [JsonConverter(typeof(NullPayloadConverter))]
    private sealed class NullPayload;

    private sealed class NullPayloadConverter : JsonConverter<NullPayload>
    {
        public override NullPayload Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => new();

        public override void Write(Utf8JsonWriter writer, NullPayload value, JsonSerializerOptions options)
            => writer.WriteNullValue();
    }

    private sealed class CountingVisitorFactory(RelationalSqlTranslatingExpressionVisitorDependencies dependencies)
        : IRelationalSqlTranslatingExpressionVisitorFactory
    {
        public int CoalesceVisits { get; set; }

        public RelationalSqlTranslatingExpressionVisitor Create(
            QueryCompilationContext queryCompilationContext,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            => new CountingVisitor(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor, this);
    }

    private sealed class CountingVisitor(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
        CountingVisitorFactory factory)
        : NpgsqlSqlTranslatingExpressionVisitor(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
    {
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Coalesce)
            {
                factory.CoalesceVisits++;
            }

            return base.VisitBinary(binaryExpression);
        }
    }

    private sealed class IntAsStringConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => int.Parse(reader.GetString()!);

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }

    private class JsonContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonEntity>().Property(e => e.Json).HasColumnType("json");
            modelBuilder.Entity<JsonEntity>().Property(e => e.Jsonb).HasColumnType("jsonb");
            modelBuilder.Entity<JsonEntity>().Property(e => e.NullPayload).HasColumnType("jsonb");
        }
    }

    private class JsonEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, object> Json { get; set; } = [];
        public Dictionary<string, object> Jsonb { get; set; } = [];
        public NullPayload NullPayload { get; set; }
    }

    private enum Mood
    {
        Happy,
        Sad
    }
}
