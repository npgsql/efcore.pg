using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates method and property calls on arrays/lists into their corresponding PostgreSQL operations.
    /// </summary>
    /// <remarks>
    /// https://www.postgresql.org/docs/current/static/functions-array.html
    /// </remarks>
    public class NpgsqlArrayTranslator : IMethodCallTranslator, IMemberTranslator
    {
        static readonly MethodInfo SequenceEqual =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(m => m.Name == nameof(Enumerable.SequenceEqual) && m.GetParameters().Length == 2);

        static readonly MethodInfo Contains =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

        static readonly MethodInfo EnumerableAnyWithoutPredicate =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);

        readonly IRelationalTypeMappingSource _typeMappingSource;
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        readonly NpgsqlJsonPocoTranslator _jsonPocoTranslator;

        public NpgsqlArrayTranslator(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] NpgsqlSqlExpressionFactory sqlExpressionFactory,
            [NotNull] NpgsqlJsonPocoTranslator jsonPocoTranslator)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
            _jsonPocoTranslator = jsonPocoTranslator;
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (instance != null && instance.Type.IsGenericList() && method.Name == "get_Item" && arguments.Count == 1)
            {
                return
                    // Try translating indexing inside json column
                    _jsonPocoTranslator.TranslateMemberAccess(instance, arguments[0], method.ReturnType) ??
                    // Other types should be subscriptable - but PostgreSQL arrays are 1-based, so adjust the index.
                    _sqlExpressionFactory.ArrayIndex(instance, GenerateOneBasedIndexExpression(arguments[0]));
            }

            if (arguments.Count == 0)
                return null;

            var array = arguments[0];
            if (!array.Type.TryGetElementType(out var elementType))
                return null; // Not an array/list

            // The array/list CLR type may be mapped to a non-array database type (e.g. byte[] to bytea, or just
            // value converters). Make sure we're dealing with an array
            // Regardless of CLR type, we may be dealing with a non-array database type (e.g. via value converters).
            if (array.TypeMapping is RelationalTypeMapping typeMapping &&
                !(typeMapping is NpgsqlArrayTypeMapping) && !(typeMapping is NpgsqlJsonTypeMapping))
            {
                return null;
            }

            if (method.IsClosedFormOf(SequenceEqual) && arguments[1].Type.IsArray)
                return _sqlExpressionFactory.Equal(array, arguments[1]);

            // Predicate-less Any - translate to a simple length check.
            if (method.IsClosedFormOf(EnumerableAnyWithoutPredicate))
            {
                return _sqlExpressionFactory.GreaterThan(
                    _jsonPocoTranslator.TranslateArrayLength(array) ??
                    _sqlExpressionFactory.Function(
                        "cardinality",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        typeof(int)),
                    _sqlExpressionFactory.Constant(0));
            }

            // Note that .Where(e => new[] { "a", "b", "c" }.Any(p => e.SomeText == p)))
            // is pattern-matched in AllAnyToContainsRewritingExpressionVisitor, which transforms it to
            // new[] { "a", "b", "c" }.Contains(e.Some Text).

            if (method.IsClosedFormOf(Contains) &&
                (
                    // Handle either parameters (no mapping but supported CLR type), or array columns. We specifically
                    // don't want to translate if the type mapping is bytea (CLR type is array, but not an array in
                    // the database).
                    array.TypeMapping == null && _typeMappingSource.FindMapping(array.Type) != null ||
                    array.TypeMapping is NpgsqlArrayTypeMapping
                ) &&
                // Exclude arrays/lists over Nullable<T> since the ADO layer doesn't handle them (but will in 5.0)
                Nullable.GetUnderlyingType(elementType) == null)
            {
                var item = arguments[1];

                switch (array)
                {
                // When the array is a column, we translate to array @> ARRAY[item]. GIN indexes
                // on array are used, but null semantics is impossible without preventing index use.
                case ColumnExpression _:
                    if (item is SqlConstantExpression constant && constant.Value is null)
                    {
                        // We special-case null constant item and use array_position instead, since it does
                        // nulls correctly (but doesn't use indexes)
                        // TODO: once lambda-based caching is implemented, move this to NpgsqlSqlNullabilityProcessor
                        // (https://github.com/dotnet/efcore/issues/17598) and do for parameters as well.
                        return _sqlExpressionFactory.IsNotNull(
                            _sqlExpressionFactory.Function(
                                "array_position",
                                new[] { array, item },
                                nullable: true,
                                argumentsPropagateNullability: FalseArrays[2],
                                typeof(int)));
                    }

                    return _sqlExpressionFactory.Contains(array,
                            _sqlExpressionFactory.NewArrayOrConstant(new[] { item }, array.Type));

                // Don't do anything PG-specific for constant arrays since the general EF Core mechanism is fine
                // for that case: item IN (1, 2, 3).
                // After https://github.com/aspnet/EntityFrameworkCore/issues/16375 is done we may not need the
                // check any more.
                case SqlConstantExpression _:
                    return null;

                // For ParameterExpression, and for all other cases - e.g. array returned from some function -
                // translate to e.SomeText = ANY (@p). This is superior to the general solution which will expand
                // parameters to constants, since non-PG SQL does not support arrays.
                // Note that this will allow indexes on the item to be used.
                default:
                    return _sqlExpressionFactory.Any(item, array, PostgresAnyOperatorType.Equal);
                }
            }

            // Note: we also translate .Where(e => new[] { "a", "b", "c" }.Any(p => EF.Functions.Like(e.SomeText, p)))
            // to LIKE ANY (...). See NpgsqlSqlTranslatingExpressionVisitor.VisitArrayMethodCall.

            return null;
        }

        public virtual SqlExpression Translate(SqlExpression instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (instance?.Type.IsGenericList() == true &&
                member.Name == nameof(List<object>.Count) &&
                (instance.TypeMapping is NpgsqlArrayTypeMapping || instance.TypeMapping is null))
            {
                return _jsonPocoTranslator.TranslateArrayLength(instance) ??
                       _sqlExpressionFactory.Function(
                           "cardinality",
                           new[] { instance },
                           nullable: true,
                           argumentsPropagateNullability: TrueArrays[1],
                           typeof(int));
            }

            return null;
        }

        /// <summary>
        /// PostgreSQL array indexing is 1-based. If the index happens to be a constant,
        /// just increment it. Otherwise, append a +1 in the SQL.
        /// </summary>
        SqlExpression GenerateOneBasedIndexExpression([NotNull] SqlExpression expression)
            => expression is SqlConstantExpression constant
                ? _sqlExpressionFactory.Constant(Convert.ToInt32(constant.Value) + 1, constant.TypeMapping)
                : (SqlExpression)_sqlExpressionFactory.Add(expression, _sqlExpressionFactory.Constant(1));
    }
}
