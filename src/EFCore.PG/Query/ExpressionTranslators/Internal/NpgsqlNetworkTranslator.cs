using System.Net;
using System.Net.NetworkInformation;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
/// Provides translation services for operators and functions of PostgreSQL network typess (cidr, inet, macaddr, macaddr8).
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/functions-net.html
/// </remarks>
public class NpgsqlNetworkTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IPAddressParse =
        typeof(IPAddress).GetRuntimeMethod(nameof(IPAddress.Parse), new[] { typeof(string) })!;

    private static readonly MethodInfo PhysicalAddressParse =
        typeof(PhysicalAddress).GetRuntimeMethod(nameof(PhysicalAddress.Parse), new[] { typeof(string) })!;

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

        if (method.DeclaringType != typeof(NpgsqlNetworkDbFunctionsExtensions))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(NpgsqlNetworkDbFunctionsExtensions.LessThan)
                => _sqlExpressionFactory.LessThan(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.LessThanOrEqual)
                => _sqlExpressionFactory.LessThanOrEqual(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.GreaterThanOrEqual)
                => _sqlExpressionFactory.GreaterThanOrEqual(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.GreaterThan)
                => _sqlExpressionFactory.GreaterThan(arguments[1], arguments[2]),

            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainedBy)
                => _sqlExpressionFactory.ContainedBy(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainedByOrEqual)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.NetworkContainedByOrEqual, arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Contains)
                => _sqlExpressionFactory.Contains(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainsOrEqual)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.NetworkContainsOrEqual, arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainsOrContainedBy)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.NetworkContainsOrContainedBy, arguments[1], arguments[2]),

            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseNot)
                => new SqlUnaryExpression(ExpressionType.Not, arguments[1], arguments[1].Type, arguments[1].TypeMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseAnd)
                => _sqlExpressionFactory.And(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseOr)
                => _sqlExpressionFactory.Or(arguments[1], arguments[2]),

            // Add/Subtract accept inet + int, so we can't use the default type mapping inference logic which assumes
            // same-typed operands
            nameof(NpgsqlNetworkDbFunctionsExtensions.Add)
                => new SqlBinaryExpression(
                    ExpressionType.Add,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    arguments[1].Type,
                    arguments[1].TypeMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.Subtract) when arguments[2].Type == typeof(int)
                => new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    arguments[1].Type,
                    arguments[1].TypeMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.Subtract)
                when arguments[2].Type == typeof(IPAddress) || arguments[2].Type == typeof((IPAddress, int))
                => new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[1], ExpressionExtensions.InferTypeMapping(arguments[1], arguments[2])),
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[2], ExpressionExtensions.InferTypeMapping(arguments[1], arguments[2])),
                    arguments[1].Type,
                    _longAddressMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.Abbreviate)    => NullPropagatingFunction("abbrev",           new[] { arguments[1] }, typeof(string)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Broadcast)     => NullPropagatingFunction("broadcast",        new[] { arguments[1] }, typeof(IPAddress), _inetMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Family)        => NullPropagatingFunction("family",           new[] { arguments[1] }, typeof(int)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Host)          => NullPropagatingFunction("host",             new[] { arguments[1] }, typeof(string)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.HostMask)      => NullPropagatingFunction("hostmask",         new[] { arguments[1] }, typeof(IPAddress), _inetMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.MaskLength)    => NullPropagatingFunction("masklen",          new[] { arguments[1] }, typeof(int)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Netmask)       => NullPropagatingFunction("netmask",          new[] { arguments[1] }, typeof(IPAddress), _inetMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Network)       => NullPropagatingFunction("network",          new[] { arguments[1] }, typeof((IPAddress Address, int Subnet)), _cidrMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.SetMaskLength) => NullPropagatingFunction("set_masklen",      new[] { arguments[1], arguments[2] }, arguments[1].Type, arguments[1].TypeMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Text)          => NullPropagatingFunction("text",             new[] { arguments[1] }, typeof(string)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.SameFamily)    => NullPropagatingFunction("inet_same_family", new[] { arguments[1], arguments[2] }, typeof(bool)),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Merge)         => NullPropagatingFunction("inet_merge",       new[] { arguments[1], arguments[2] }, typeof((IPAddress Address, int Subnet)), _cidrMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Truncate)      => NullPropagatingFunction("trunc",            new[] { arguments[1] }, typeof(PhysicalAddress), arguments[1].TypeMapping),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Set7BitMac8)   => NullPropagatingFunction("macaddr8_set7bit", new[] { arguments[1] }, typeof(PhysicalAddress), _macaddr8Mapping),

            _ => null
        };

        SqlFunctionExpression NullPropagatingFunction(
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
}
