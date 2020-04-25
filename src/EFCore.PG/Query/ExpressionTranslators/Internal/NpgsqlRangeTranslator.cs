using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

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

        public NpgsqlRangeTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _boolMapping = sqlExpressionFactory.FindMapping(typeof(bool));
        }

        /// <inheritdoc />
        [CanBeNull]
        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (method.DeclaringType != typeof(NpgsqlRangeDbFunctionsExtensions))
                return null;

            if (method.Name == nameof(NpgsqlRangeDbFunctionsExtensions.Merge))
            {
                var inferredMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);
                return _sqlExpressionFactory.Function(
                    "range_merge",
                    new[] {
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[0], inferredMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[1], inferredMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    method.ReturnType,
                    inferredMapping);
            }

            return method.Name switch
            {
                nameof(NpgsqlRangeDbFunctionsExtensions.Contains) when arguments[0].Type == arguments[1].Type => BoolReturningOnTwoRanges("@>"),

                // Default to element contained in range
                nameof(NpgsqlRangeDbFunctionsExtensions.Contains) =>
                    new SqlCustomBinaryExpression(
                        _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]), // TODO: Infer the range mapping from the subtype's...
                        arguments[0].TypeMapping is NpgsqlRangeTypeMapping rangeMapping
                            ? _sqlExpressionFactory.ApplyTypeMapping(arguments[1], rangeMapping.SubtypeMapping)
                            : _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                        "@>",
                        typeof(bool),
                        _boolMapping),

                nameof(NpgsqlRangeDbFunctionsExtensions.ContainedBy)          => BoolReturningOnTwoRanges("<@"),
                nameof(NpgsqlRangeDbFunctionsExtensions.Overlaps)             => BoolReturningOnTwoRanges("&&"),
                nameof(NpgsqlRangeDbFunctionsExtensions.IsStrictlyLeftOf)     => BoolReturningOnTwoRanges("<<"),
                nameof(NpgsqlRangeDbFunctionsExtensions.IsStrictlyRightOf)    => BoolReturningOnTwoRanges(">>"),
                nameof(NpgsqlRangeDbFunctionsExtensions.DoesNotExtendRightOf) => BoolReturningOnTwoRanges("&<"),
                nameof(NpgsqlRangeDbFunctionsExtensions.DoesNotExtendLeftOf)  => BoolReturningOnTwoRanges("&>"),
                nameof(NpgsqlRangeDbFunctionsExtensions.IsAdjacentTo)         => BoolReturningOnTwoRanges("-|-"),

                nameof(NpgsqlRangeDbFunctionsExtensions.Union)                => RangeReturningOnTwoRanges("+"),
                nameof(NpgsqlRangeDbFunctionsExtensions.Intersect)            => RangeReturningOnTwoRanges("*"),
                nameof(NpgsqlRangeDbFunctionsExtensions.Except)               => RangeReturningOnTwoRanges("-"),

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
        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
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
                var accessor = _sqlExpressionFactory.Function(
                    accessorName,
                    new[] { instance },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    returnType,
                    typeMapping);

                return returnType.IsNullableType()
                    ? accessor
                    : _sqlExpressionFactory.Coalesce(
                        accessor,
                        _sqlExpressionFactory.Constant(GetDefaultValue(returnType)),
                        typeMapping);
            }

            return member.Name switch
            {
            nameof(NpgsqlRange<int>.IsEmpty)               => SingleArgBoolFunction("isempty", instance),
            nameof(NpgsqlRange<int>.LowerBoundIsInclusive) => SingleArgBoolFunction("lower_inc", instance),
            nameof(NpgsqlRange<int>.UpperBoundIsInclusive) => SingleArgBoolFunction("upper_inc", instance),
            nameof(NpgsqlRange<int>.LowerBoundInfinite)    => SingleArgBoolFunction("lower_inf", instance),
            nameof(NpgsqlRange<int>.UpperBoundInfinite)    => SingleArgBoolFunction("upper_inf", instance),

            _ => null
            };

            SqlFunctionExpression SingleArgBoolFunction(string name, SqlExpression argument)
                => _sqlExpressionFactory.Function(
                    name,
                    new[] { argument },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(bool));
        }

        static readonly ConcurrentDictionary<Type, object> _defaults = new ConcurrentDictionary<Type, object>();

        static object GetDefaultValue(Type type)
            => type.IsValueType ? _defaults.GetOrAdd(type, Activator.CreateInstance) : null;
    }
}
