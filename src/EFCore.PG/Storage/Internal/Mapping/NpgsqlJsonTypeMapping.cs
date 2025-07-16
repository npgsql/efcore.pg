using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     Supports the older Npgsql-specific JSON mapping, allowing mapping json/jsonb to text, to e.g.
///     <see cref="JsonElement" /> (weakly-typed mapping) or to arbitrary POCOs (but without them being modeled).
///     For the standard EF JSON support, which relies on owned entity modeling, see <see cref="NpgsqlOwnedJsonTypeMapping" />.
/// </summary>
public class NpgsqlJsonTypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static NpgsqlJsonTypeMapping Default { get; } = new("jsonb", typeof(JsonElement));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlJsonTypeMapping(string storeType, Type clrType)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(clrType, comparer: GetValueComparer(clrType)),
                storeType),
            storeType == "jsonb" ? NpgsqlDbType.Jsonb : NpgsqlDbType.Json)
    {
        if (storeType != "json" && storeType != "jsonb")
        {
            throw new ArgumentException($"{nameof(storeType)} must be 'json' or 'jsonb'", nameof(storeType));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlJsonTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
        : base(parameters, npgsqlDbType)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsJsonb
        => StoreType == "jsonb";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlJsonTypeMapping(parameters, NpgsqlDbType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual string EscapeSqlLiteral(string literal)
        => Check.NotNull(literal, nameof(literal)).Replace("'", "''");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        switch (value)
        {
            case JsonDocument _:
            case JsonElement _:
            {
                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream);
                if (value is JsonDocument doc)
                {
                    doc.WriteTo(writer);
                }
                else
                {
                    ((JsonElement)value).WriteTo(writer);
                }

                writer.Flush();
                return $"'{EscapeSqlLiteral(Encoding.UTF8.GetString(stream.ToArray()))}'";
            }
            case JsonNode node:
            {
                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream);
                node.WriteTo(writer);
                writer.Flush();
                return $"'{EscapeSqlLiteral(Encoding.UTF8.GetString(stream.ToArray()))}'";
            }
            case string s:
                return $"'{EscapeSqlLiteral(s)}'";
            default: // User POCO
                return $"'{EscapeSqlLiteral(JsonSerializer.Serialize(value))}'";
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
        => value switch
        {
            JsonDocument document => Expression.Call(
                ParseMethod, Expression.Constant(document.RootElement.ToString()), DefaultJsonDocumentOptions),
            JsonElement element => Expression.Property(
                Expression.Call(ParseMethod, Expression.Constant(element.ToString()), DefaultJsonDocumentOptions),
                nameof(JsonDocument.RootElement)),
            JsonNode node => Expression.Call(
                ParseJsonNodeMethod, Expression.Constant(node.ToString())),
            string s => Expression.Constant(s),
            _ => throw new NotSupportedException("Cannot generate code literals for JSON POCOs")
        };

    private static readonly Expression DefaultJsonDocumentOptions = Expression.New(typeof(JsonDocumentOptions));

    private static readonly MethodInfo ParseMethod =
        typeof(JsonDocument).GetMethod(nameof(JsonDocument.Parse), [typeof(string), typeof(JsonDocumentOptions)])!;

    private static readonly MethodInfo ParseJsonNodeMethod =
        typeof(JsonNode).GetMethod(nameof(JsonNode.Parse), [typeof(string)])!;

    private static ValueComparer? GetValueComparer(Type clrType)
    {
        // JsonNode types are mutable reference types and need custom comparers for change tracking
        if (clrType == typeof(JsonNode))
            return new JsonNodeValueComparer();
        if (clrType == typeof(JsonObject))
            return new JsonObjectValueComparer();
        if (clrType == typeof(JsonArray))
            return new JsonArrayValueComparer();
        if (clrType == typeof(JsonValue))
            return new JsonValueValueComparer();

        // JsonDocument and JsonElement don't need custom comparers
        // JsonDocument is immutable, JsonElement is a value type
        return null;
    }

    private sealed class JsonNodeValueComparer() : ValueComparer<JsonNode>(
        (l, r) => JsonNodeEquals(l, r),
        v => v == null ? 0 : JsonNodeHashCode(v),
        v => CloneJsonNode(v)!)
    {
    }

    private sealed class JsonObjectValueComparer() : ValueComparer<JsonObject>(
        (l, r) => JsonNodeEquals(l, r),
        v => v == null ? 0 : JsonNodeHashCode(v),
        v => CloneJsonObject(v)!)
    {
    }

    private sealed class JsonArrayValueComparer() : ValueComparer<JsonArray>(
        (l, r) => JsonNodeEquals(l, r),
        v => v == null ? 0 : JsonNodeHashCode(v),
        v => CloneJsonArray(v)!)
    {
    }

    private sealed class JsonValueValueComparer() : ValueComparer<JsonValue>(
        (l, r) => JsonNodeEquals(l, r),
        v => v == null ? 0 : JsonNodeHashCode(v),
        v => CloneJsonValue(v)!)
    {
    }

    private static bool JsonNodeEquals(JsonNode? left, JsonNode? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;

        return JsonNode.DeepEquals(left, right);
    }

    private static int JsonNodeHashCode(JsonNode node)
    {
        // Use string representation for hash code since JsonNode doesn't provide a reliable GetHashCode
        return node.ToString().GetHashCode();
    }

    private static JsonNode? CloneJsonNode(JsonNode? node)
        => node?.DeepClone();

    private static JsonObject? CloneJsonObject(JsonObject? obj)
        => obj?.DeepClone().AsObject();

    private static JsonArray? CloneJsonArray(JsonArray? array)
        => array?.DeepClone().AsArray();

    private static JsonValue? CloneJsonValue(JsonValue? value)
        => value?.DeepClone().AsValue();
}
