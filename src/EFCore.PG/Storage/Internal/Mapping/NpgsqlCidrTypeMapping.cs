using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for the PostgreSQL cidr type.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/datatype-net-types.html#DATATYPE-CIDR
/// </remarks>
public class NpgsqlCidrTypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlCidrTypeMapping()
        : base("cidr", typeof(NpgsqlCidr), NpgsqlDbType.Cidr, JsonCidrReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlCidrTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Cidr) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlCidrTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var cidr = (NpgsqlCidr)value;
        return $"CIDR '{cidr.Address}/{cidr.Netmask}'";
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        var cidr = (NpgsqlCidr)value;
        return Expression.New(
            NpgsqlCidrConstructor,
            Expression.Call(ParseMethod, Expression.Constant(cidr.Address.ToString())),
            Expression.Constant(cidr.Netmask));
    }

    private static readonly MethodInfo ParseMethod = typeof(IPAddress).GetMethod("Parse", new[] { typeof(string) })!;

    private static readonly ConstructorInfo NpgsqlCidrConstructor =
        typeof(NpgsqlCidr).GetConstructor(new[] { typeof(IPAddress), typeof(byte) })!;

    private sealed class JsonCidrReaderWriter : JsonValueReaderWriter<NpgsqlCidr>
    {
        public static JsonCidrReaderWriter Instance { get; } = new();

        public override NpgsqlCidr FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => new(manager.CurrentReader.GetString()!);

        public override void ToJsonTyped(Utf8JsonWriter writer, NpgsqlCidr value)
            => writer.WriteStringValue(value.ToString());
    }
}
