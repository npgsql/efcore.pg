using System.Net;
using System.Net.NetworkInformation;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for the PostgreSQL macaddr type.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/datatype-net-types.html#DATATYPE-MACADDR
/// </remarks>
public class NpgsqlMacaddrTypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlMacaddrTypeMapping() : base("macaddr", typeof(PhysicalAddress), NpgsqlDbType.MacAddr) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlMacaddrTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.MacAddr) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlMacaddrTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"MACADDR '{(PhysicalAddress)value}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
        => Expression.Call(ParseMethod, Expression.Constant(((PhysicalAddress)value).ToString()));

    private static readonly MethodInfo ParseMethod = typeof(PhysicalAddress).GetMethod("Parse", new[] { typeof(string) })!;
}

/// <summary>
/// The type mapping for the PostgreSQL macaddr8 type.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/datatype-net-types.html#DATATYPE-MACADDR8
/// </remarks>
public class NpgsqlMacaddr8TypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlMacaddr8TypeMapping() : base("macaddr8", typeof(PhysicalAddress), NpgsqlDbType.MacAddr8) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlMacaddr8TypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.MacAddr8) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlMacaddr8TypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"MACADDR8 '{(PhysicalAddress)value}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
        => Expression.Call(ParseMethod, Expression.Constant(((PhysicalAddress)value).ToString()));

    private static readonly MethodInfo ParseMethod = typeof(PhysicalAddress).GetMethod("Parse", new[] { typeof(string) })!;
}

/// <summary>
/// The type mapping for the PostgreSQL inet and cidr types.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/datatype-net-types.html#DATATYPE-INET
/// </remarks>
public class NpgsqlInetTypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlInetTypeMapping(string storeType, Type clrType, NpgsqlDbType npgsqlDbType) : base(storeType, clrType, npgsqlDbType) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlInetTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
        : base(parameters, npgsqlDbType) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlInetTypeMapping(parameters, NpgsqlDbType);

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
            case (IPAddress address, int subnet):
                return $"{StoreType.ToUpperInvariant()} '{address}/{subnet}'";
            
            case IPAddress address:
                return $"INET '{address}'";
            
            default:
                throw new InvalidCastException(
                    $"Attempted to generate {StoreType} literal for type {value.GetType()}");
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        if (value is (IPAddress address, int subnet))
        {
            return Expression.New(
                MaskConstructor,
                Expression.Call(ParseMethod, Expression.Constant(address.ToString())),
                Expression.Constant(subnet));
        }
        
        return Expression.Call(ParseMethod, Expression.Constant(((IPAddress)value).ToString()));
    }

    private static readonly MethodInfo ParseMethod = typeof(IPAddress).GetMethod("Parse", new[] { typeof(string) })!;
    private static readonly ConstructorInfo MaskConstructor =
        typeof((IPAddress, int)).GetConstructor(new[] { typeof(IPAddress), typeof(int) })!;
}
