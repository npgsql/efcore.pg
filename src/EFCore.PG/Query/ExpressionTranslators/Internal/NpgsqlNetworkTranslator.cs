using System.Net;
using System.Net.NetworkInformation;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Provides translation services for operators and functions of PostgreSQL network typess (cidr, inet, macaddr, macaddr8).
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/static/functions-net.html
/// </remarks>
public class NpgsqlNetworkTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IPAddressParse =
        typeof(IPAddress).GetRuntimeMethod(nameof(IPAddress.Parse), [typeof(string)])!;

    private static readonly MethodInfo PhysicalAddressParse =
        typeof(PhysicalAddress).GetRuntimeMethod(nameof(PhysicalAddress.Parse), [typeof(string)])!;

    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

    private readonly RelationalTypeMapping _inetMapping;
    private readonly RelationalTypeMapping _cidrMapping;
    private readonly RelationalTypeMapping _macaddr8Mapping;
    private readonly RelationalTypeMapping _longAddressMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlNetworkTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _inetMapping = typeMappingSource.FindMapping("inet")!;
        _cidrMapping = typeMappingSource.FindMapping("cidr")!;
        _macaddr8Mapping = typeMappingSource.FindMapping("macaddr8")!;
        _longAddressMapping = typeMappingSource.FindMapping(typeof(long), model)!;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == IPAddressParse)
        {
            return _sqlExpressionFactory.Convert(arguments[0], typeof(IPAddress));
        }

        if (method == PhysicalAddressParse)
        {
            return _sqlExpressionFactory.Convert(arguments[0], typeof(PhysicalAddress));
        }

        if (method.DeclaringType == typeof(NpgsqlNetworkDbFunctionsExtensions))
        {
            var paramType = method.GetParameters()[1].ParameterType;

            if (paramType == typeof(NpgsqlInet))
            {
                return TranslateInetExtensionMethod(method, arguments);
            }

            if (paramType == typeof(NpgsqlCidr))
            {
                return TranslateCidrExtensionMethod(method, arguments);
            }

            if (paramType == typeof(PhysicalAddress))
            {
                return TranslateMacaddrExtensionMethod(method, arguments);
            }
        }

        return null;
    }

    private SqlExpression? TranslateInetExtensionMethod(MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            nameof(NpgsqlNetworkDbFunctionsExtensions.LessThan)
                => new SqlBinaryExpression(
                    ExpressionType.LessThan,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(NpgsqlInet),
                    _inetMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.LessThanOrEqual)
                => new SqlBinaryExpression(
                    ExpressionType.LessThanOrEqual,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(NpgsqlInet),
                    _inetMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.GreaterThanOrEqual)
                => new SqlBinaryExpression(
                    ExpressionType.GreaterThanOrEqual,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(NpgsqlInet),
                    _inetMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.GreaterThan)
                => new SqlBinaryExpression(
                    ExpressionType.GreaterThan,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(NpgsqlInet),
                    _inetMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainedBy)
                => _sqlExpressionFactory.ContainedBy(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainedByOrEqual)
                => _sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.NetworkContainedByOrEqual, arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Contains)
                => _sqlExpressionFactory.Contains(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainsOrEqual)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.NetworkContainsOrEqual, arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainsOrContainedBy)
                => _sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.NetworkContainsOrContainedBy, arguments[1], arguments[2]),

            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseNot)
                => new SqlUnaryExpression(
                    ExpressionType.Not,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    typeof(NpgsqlInet),
                    _inetMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseAnd)
                => new SqlBinaryExpression(
                    ExpressionType.And,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(NpgsqlInet),
                    _inetMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseOr)
                => new SqlBinaryExpression(
                    ExpressionType.Or,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(NpgsqlInet),
                    _inetMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.Add)
                => new SqlBinaryExpression(
                    ExpressionType.Add,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(NpgsqlInet),
                    _inetMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.Subtract) when arguments[2].Type == typeof(long)
                => new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(NpgsqlInet),
                    _inetMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.Subtract)
                => new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(long),
                    _longAddressMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.Abbreviate)
                => NullPropagatingFunction("abbrev", [arguments[1]], typeof(string)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Broadcast)
                => NullPropagatingFunction("broadcast", [arguments[1]], typeof(IPAddress), _inetMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Family)
                => NullPropagatingFunction("family", [arguments[1]], typeof(int)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Host)
                => NullPropagatingFunction("host", [arguments[1]], typeof(string)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.HostMask)
                => NullPropagatingFunction("hostmask", [arguments[1]], typeof(IPAddress), _inetMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.MaskLength)
                => NullPropagatingFunction("masklen", [arguments[1]], typeof(int)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Netmask)
                => NullPropagatingFunction("netmask", [arguments[1]], typeof(IPAddress), _inetMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Network)
                => NullPropagatingFunction("network", [arguments[1]], typeof((IPAddress Address, int Subnet)), _cidrMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.SetMaskLength)
                => NullPropagatingFunction(
                    "set_masklen", [arguments[1], arguments[2]], arguments[1].Type, arguments[1].TypeMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Text)
                => NullPropagatingFunction("text", [arguments[1]], typeof(string)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.SameFamily)
                => NullPropagatingFunction("inet_same_family", [arguments[1], arguments[2]], typeof(bool)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Merge)
                => NullPropagatingFunction(
                    "inet_merge", [arguments[1], arguments[2]], typeof((IPAddress Address, int Subnet)), _cidrMapping),

            _ => null
        };

    private SqlExpression? TranslateCidrExtensionMethod(MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            nameof(NpgsqlNetworkDbFunctionsExtensions.Abbreviate)
                => NullPropagatingFunction("abbrev", [arguments[1]], typeof(string)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.SetMaskLength)
                => NullPropagatingFunction(
                    "set_masklen", [arguments[1], arguments[2]], arguments[1].Type, arguments[1].TypeMapping),

            _ => null
        };

    private SqlExpression? TranslateMacaddrExtensionMethod(MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            nameof(NpgsqlNetworkDbFunctionsExtensions.LessThan)
                => _sqlExpressionFactory.LessThan(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.LessThanOrEqual)
                => _sqlExpressionFactory.LessThanOrEqual(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.GreaterThanOrEqual)
                => _sqlExpressionFactory.GreaterThanOrEqual(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.GreaterThan)
                => _sqlExpressionFactory.GreaterThan(arguments[1], arguments[2]),

            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseNot)
                => _sqlExpressionFactory.Not(arguments[1]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseAnd)
                => _sqlExpressionFactory.And(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseOr)
                => _sqlExpressionFactory.Or(arguments[1], arguments[2]),

            nameof(NpgsqlNetworkDbFunctionsExtensions.Truncate) => NullPropagatingFunction(
                "trunc", [arguments[1]], typeof(PhysicalAddress), arguments[1].TypeMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Set7BitMac8) => NullPropagatingFunction(
                "macaddr8_set7bit", [arguments[1]], typeof(PhysicalAddress), _macaddr8Mapping),

            _ => null
        };

    private SqlExpression NullPropagatingFunction(
        string name,
        SqlExpression[] arguments,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => _sqlExpressionFactory.Function(
            name,
            arguments,
            nullable: true,
            argumentsPropagateNullability: TrueArrays[arguments.Length],
            returnType,
            typeMapping);
}
