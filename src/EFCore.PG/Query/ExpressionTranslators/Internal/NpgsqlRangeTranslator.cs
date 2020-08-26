using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        readonly IRelationalTypeMappingSource _typeMappingSource;
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        readonly RelationalTypeMapping _boolMapping;

        public NpgsqlRangeTranslator(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] NpgsqlSqlExpressionFactory npgsqlSqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = npgsqlSqlExpressionFactory;
            _boolMapping = typeMappingSource.FindMapping(typeof(bool));
        }

        /// <inheritdoc />
        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
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
                nameof(NpgsqlRangeDbFunctionsExtensions.Contains)
                => _sqlExpressionFactory.Contains(arguments[0], arguments[1]),
                nameof(NpgsqlRangeDbFunctionsExtensions.ContainedBy)
                => _sqlExpressionFactory.ContainedBy(arguments[0], arguments[1]),
                nameof(NpgsqlRangeDbFunctionsExtensions.Overlaps)
                => _sqlExpressionFactory.Overlaps(arguments[0], arguments[1]),
                nameof(NpgsqlRangeDbFunctionsExtensions.IsStrictlyLeftOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsStrictlyLeftOf, arguments[0], arguments[1]),
                nameof(NpgsqlRangeDbFunctionsExtensions.IsStrictlyRightOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsStrictlyRightOf, arguments[0], arguments[1]),
                nameof(NpgsqlRangeDbFunctionsExtensions.DoesNotExtendRightOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeDoesNotExtendRightOf, arguments[0], arguments[1]),
                nameof(NpgsqlRangeDbFunctionsExtensions.DoesNotExtendLeftOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeDoesNotExtendLeftOf, arguments[0], arguments[1]),
                nameof(NpgsqlRangeDbFunctionsExtensions.IsAdjacentTo)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsAdjacentTo, arguments[0], arguments[1]),
                nameof(NpgsqlRangeDbFunctionsExtensions.Union)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeUnion, arguments[0], arguments[1]),
                nameof(NpgsqlRangeDbFunctionsExtensions.Intersect)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIntersect, arguments[0], arguments[1]),
                nameof(NpgsqlRangeDbFunctionsExtensions.Except)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeExcept, arguments[0], arguments[1]),

                _ => null
            };
        }

        /// <inheritdoc />
        public virtual SqlExpression Translate(SqlExpression instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            var type = member.DeclaringType;
            if (type == null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(NpgsqlRange<>))
                return null;

            if (member.Name == nameof(NpgsqlRange<int>.LowerBound) || member.Name == nameof(NpgsqlRange<int>.UpperBound))
            {
                var typeMapping = instance.TypeMapping is NpgsqlRangeTypeMapping rangeMapping
                    ? rangeMapping.SubtypeMapping
                    : _typeMappingSource.FindMapping(returnType);

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
