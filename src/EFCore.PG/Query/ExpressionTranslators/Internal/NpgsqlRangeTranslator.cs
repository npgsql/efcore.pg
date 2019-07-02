using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL range operators.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    public class NpgsqlRangeTranslator : IMethodCallTranslator, IMemberTranslator
    {
        [NotNull]
        readonly ISqlExpressionFactory _sqlExpressionFactory;
        [NotNull]
        readonly RelationalTypeMapping _boolMapping;

        public NpgsqlRangeTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _boolMapping = sqlExpressionFactory.FindMapping(typeof(bool));
        }

        /// <inheritdoc />
        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (method.DeclaringType != typeof(NpgsqlRangeExtensions))
                return null;

            if (method.Name == nameof(NpgsqlRangeExtensions.Merge))
            {
                var inferredMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);
                return new SqlFunctionExpression(
                    "range_merge",
                    new[] {
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[0], inferredMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[1], inferredMapping)
                    },
                    method.ReturnType,
                    inferredMapping);
            }

            return method.Name switch
            {
                nameof(NpgsqlRangeExtensions.Contains) when arguments[0].Type == arguments[1].Type => BoolReturningOnTwoRanges("@>"),

                // Default to element contained in range
                nameof(NpgsqlRangeExtensions.Contains) =>
                    new SqlCustomBinaryExpression(
                        _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]), // TODO: Infer the range mapping from the subtype's...
                        arguments[0].TypeMapping is NpgsqlRangeTypeMapping rangeMapping
                            ? _sqlExpressionFactory.ApplyTypeMapping(arguments[1], rangeMapping.SubtypeMapping)
                            : _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                        "@>",
                        typeof(bool),
                        _boolMapping),

                nameof(NpgsqlRangeExtensions.ContainedBy)          => BoolReturningOnTwoRanges("<@"),
                nameof(NpgsqlRangeExtensions.Overlaps)             => BoolReturningOnTwoRanges("&&"),
                nameof(NpgsqlRangeExtensions.IsStrictlyLeftOf)     => BoolReturningOnTwoRanges("<<"),
                nameof(NpgsqlRangeExtensions.IsStrictlyRightOf)    => BoolReturningOnTwoRanges(">>"),
                nameof(NpgsqlRangeExtensions.DoesNotExtendRightOf) => BoolReturningOnTwoRanges("&<"),
                nameof(NpgsqlRangeExtensions.DoesNotExtendLeftOf)  => BoolReturningOnTwoRanges("&>"),
                nameof(NpgsqlRangeExtensions.IsAdjacentTo)         => BoolReturningOnTwoRanges("-|-"),

                nameof(NpgsqlRangeExtensions.Union)                => RangeReturningOnTwoRanges("+"),
                nameof(NpgsqlRangeExtensions.Intersect)            => RangeReturningOnTwoRanges("*"),
                nameof(NpgsqlRangeExtensions.Except)               => RangeReturningOnTwoRanges("-"),

                _ => null
            };

            SqlCustomBinaryExpression BoolReturningOnTwoRanges(string @operator)
            {
                var inferredMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);
                return new SqlCustomBinaryExpression(
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[0], inferredMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[1], inferredMapping),
                    @operator,
                    typeof(bool),
                    _boolMapping);
            }

            SqlCustomBinaryExpression RangeReturningOnTwoRanges(string @operator)
            {
                var inferredMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);
                return new SqlCustomBinaryExpression(
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[0], inferredMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[1], inferredMapping),
                    @operator,
                    method.ReturnType,
                    inferredMapping);
            }
        }

        /// <inheritdoc />
        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            var type = member.DeclaringType;
            if (type == null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(NpgsqlRange<>))
                return null;

            if (member.Name == nameof(NpgsqlRange<int>.LowerBound) || member.Name == nameof(NpgsqlRange<int>.UpperBound))
            {
                var typeMapping = instance.TypeMapping is NpgsqlRangeTypeMapping rangeMapping
                    ? rangeMapping.SubtypeMapping
                    : _sqlExpressionFactory.FindMapping(returnType);

                var accessorName = member.Name == nameof(NpgsqlRange<int>.LowerBound) ? "lower" : "upper";
                var accessor = _sqlExpressionFactory.Function(accessorName, new[] { instance }, returnType, typeMapping);

                return returnType.IsNullableType()
                    ? accessor
                    : _sqlExpressionFactory.Function(
                        "COALESCE",
                        new SqlExpression[] { accessor, _sqlExpressionFactory.Constant(GetDefaultValue(returnType)) },
                        returnType,
                        typeMapping);
            }

            return member.Name switch
            {
            nameof(NpgsqlRange<int>.IsEmpty)               => _sqlExpressionFactory.Function("isempty",   new[] { instance }, returnType, _boolMapping),
            nameof(NpgsqlRange<int>.LowerBoundIsInclusive) => _sqlExpressionFactory.Function("lower_inc", new[] { instance }, returnType, _boolMapping),
            nameof(NpgsqlRange<int>.UpperBoundIsInclusive) => _sqlExpressionFactory.Function("upper_inc", new[] { instance }, returnType, _boolMapping),
            nameof(NpgsqlRange<int>.LowerBoundInfinite)    => _sqlExpressionFactory.Function("lower_inf", new[] { instance }, returnType, _boolMapping),
            nameof(NpgsqlRange<int>.UpperBoundInfinite)    => _sqlExpressionFactory.Function("upper_inf", new[] { instance }, returnType, _boolMapping),

            _ => null
            };
        }

        static readonly ConcurrentDictionary<Type, object> _defaults = new ConcurrentDictionary<Type, object>();

        static object GetDefaultValue(Type type)
            => type.IsValueType ? _defaults.GetOrAdd(type, Activator.CreateInstance) : null;
    }
}
