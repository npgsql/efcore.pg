using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for operators and functions of PostgreSQL network typess (cidr, inet, macaddr, macaddr8).
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-net.html
    /// </remarks>
    public class NpgsqlNetworkTranslator : IMethodCallTranslator
    {
        [NotNull] static readonly MethodInfo IPAddressParse =
            typeof(IPAddress).GetRuntimeMethod(nameof(IPAddress.Parse), new[] { typeof(string) });

        [NotNull] static readonly MethodInfo PhysicalAddressParse =
            typeof(PhysicalAddress).GetRuntimeMethod(nameof(PhysicalAddress.Parse), new[] { typeof(string) });

        [NotNull]
        readonly ISqlExpressionFactory _sqlExpressionFactory;

        readonly RelationalTypeMapping _boolMapping;
        readonly RelationalTypeMapping _inetMapping;
        readonly RelationalTypeMapping _cidrMapping;
        readonly RelationalTypeMapping _macaddr8Mapping;

        public NpgsqlNetworkTranslator(ISqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _boolMapping = typeMappingSource.FindMapping(typeof(bool));
            _inetMapping = typeMappingSource.FindMapping("inet");
            _cidrMapping = typeMappingSource.FindMapping("cidr");
            _macaddr8Mapping = typeMappingSource.FindMapping("macaddr8");
        }

        /// <inheritdoc />
        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (method == IPAddressParse)
                return _sqlExpressionFactory.Convert(arguments[0], typeof(IPAddress), _sqlExpressionFactory.FindMapping(typeof(IPAddress)));

            if (method == PhysicalAddressParse)
                return _sqlExpressionFactory.Convert(arguments[0], typeof(PhysicalAddress), _sqlExpressionFactory.FindMapping(typeof(PhysicalAddress)));

            if (method.DeclaringType != typeof(NpgsqlNetworkDbFunctionsExtensions))
                return null;

            return method.Name switch
            {
            nameof(NpgsqlNetworkDbFunctionsExtensions.LessThan)              => _sqlExpressionFactory.LessThan(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.LessThanOrEqual)       => _sqlExpressionFactory.LessThanOrEqual(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.GreaterThanOrEqual)    => _sqlExpressionFactory.GreaterThanOrEqual(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.GreaterThan)           => _sqlExpressionFactory.GreaterThan(arguments[1], arguments[2]),

            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainedBy)           => BoolReturningOnTwoNetworkTypes("<<"),
            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainedByOrEqual)    => BoolReturningOnTwoNetworkTypes("<<="),
            nameof(NpgsqlNetworkDbFunctionsExtensions.Contains)              => BoolReturningOnTwoNetworkTypes(">>"),
            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainsOrEqual)       => BoolReturningOnTwoNetworkTypes(">>="),
            nameof(NpgsqlNetworkDbFunctionsExtensions.ContainsOrContainedBy) => BoolReturningOnTwoNetworkTypes("&&"),

            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseNot)            => new SqlUnaryExpression(ExpressionType.Not,
                arguments[1],
                arguments[1].Type,
                arguments[1].TypeMapping),

            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseAnd) => _sqlExpressionFactory.And(arguments[1], arguments[2]),
            nameof(NpgsqlNetworkDbFunctionsExtensions.BitwiseOr)  => _sqlExpressionFactory.Or(arguments[1], arguments[2]),

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
                    _sqlExpressionFactory.FindMapping(typeof(long))),

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

            _ => (SqlExpression)null
            };

            SqlFunctionExpression NullPropagatingFunction(
                string name,
                SqlExpression[] arguments,
                Type returnType,
                RelationalTypeMapping typeMapping = null)
                => _sqlExpressionFactory.Function(
                    name,
                    arguments,
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[arguments.Length],
                    returnType,
                    typeMapping);

            SqlCustomBinaryExpression BoolReturningOnTwoNetworkTypes(string @operator)
                => new SqlCustomBinaryExpression(
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    @operator,
                    typeof(bool),
                    _boolMapping);
        }
    }
}
