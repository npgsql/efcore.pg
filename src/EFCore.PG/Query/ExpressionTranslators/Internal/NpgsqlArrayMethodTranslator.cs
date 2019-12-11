using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates functions on arrays into their corresponding PostgreSQL operations.
    /// </summary>
    /// <remarks>
    /// https://www.postgresql.org/docs/current/static/functions-array.html
    /// </remarks>
    public class NpgsqlArrayMethodTranslator : IMethodCallTranslator
    {
        [NotNull] static readonly MethodInfo SequenceEqual =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(m => m.Name == nameof(Enumerable.SequenceEqual) && m.GetParameters().Length == 2);

        [NotNull] static readonly MethodInfo Contains =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

        [NotNull] static readonly MethodInfo EnumerableAnyWithoutPredicate =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);


        [NotNull]
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        [NotNull]
        readonly NpgsqlJsonPocoTranslator _jsonPocoTranslator;

        public NpgsqlArrayMethodTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory, NpgsqlJsonPocoTranslator jsonPocoTranslator)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _jsonPocoTranslator = jsonPocoTranslator;
        }

        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            // TODO: Fully support List<>

            if (arguments.Count == 0)
                return null;

            var operand = arguments[0];

            var operandElementType = operand.Type.IsArray
                ? operand.Type.GetElementType()
                : operand.Type.IsGenericType && operand.Type.GetGenericTypeDefinition() == typeof(List<>)
                    ? operand.Type.GetGenericArguments()[0]
                    : null;

            if (operandElementType == null) // Not an array/list
                return null;

            // Even if the CLR type is an array/list, it may be mapped to a non-array database type (e.g. via value converters).
            if (operand.TypeMapping is RelationalTypeMapping typeMapping &&
                !(typeMapping is NpgsqlArrayTypeMapping) && !(typeMapping is NpgsqlJsonTypeMapping))
            {
                return null;
            }

            if (method.IsClosedFormOf(SequenceEqual) && arguments[1].Type.IsArray)
                return _sqlExpressionFactory.Equal(operand, arguments[1]);

            // Predicate-less Any - translate to a simple length check.
            if (method.IsClosedFormOf(EnumerableAnyWithoutPredicate))
            {
                return _sqlExpressionFactory.GreaterThan(
                    _jsonPocoTranslator.TranslateArrayLength(operand) ??
                    _sqlExpressionFactory.Function("cardinality", arguments, typeof(int?)),
                    _sqlExpressionFactory.Constant(0));
            }

            // Note that .Where(e => new[] { "a", "b", "c" }.Any(p => e.SomeText == p)))
            // is pattern-matched in AllAnyToContainsRewritingExpressionVisitor, which transforms it to
            // new[] { "a", "b", "c" }.Contains(e.SomeText).
            // Here we go further, and translate that to the PostgreSQL-specific construct e.SomeText = ANY (@p) -
            // this is superior to the general solution which will expand parameters to constants,
            // since non-PG SQL does not support arrays. If the list is a constant we leave it for regular IN
            // (functionality the same but more familiar).

            // Note: we exclude constant array expressions from this PG-specific optimization since the general
            // EF Core mechanism is fine for that case. After https://github.com/aspnet/EntityFrameworkCore/issues/16375
            // is done we may not need the check any more.
            // Note: we exclude arrays/lists over Nullable<T> since the ADO layer doesn't handle them (but will in 5.0)

            if (method.IsClosedFormOf(Contains) &&
                _sqlExpressionFactory.FindMapping(operand.Type) != null &&
                !(operand is SqlConstantExpression) &&
                Nullable.GetUnderlyingType(operandElementType) == null)
            {
                var item = arguments[1];
                // TODO: no null semantics is implemented here (see https://github.com/npgsql/efcore.pg/issues/1142)
                // We require a null semantics check in case the item is null and the array contains a null.
                // Advanced parameter sniffing would help here: https://github.com/aspnet/EntityFrameworkCore/issues/17598
                // We need to coalesce to false since 'x' = ANY ({'y', NULL}) returns null, not false
                // (and so will be null when negated too)
                return _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.ArrayAnyAll(item, operand, ArrayComparisonType.Any, "="),
                    _sqlExpressionFactory.AndAlso(
                        _sqlExpressionFactory.IsNull(item),
                        _sqlExpressionFactory.IsNotNull(
                            _sqlExpressionFactory.Function(
                                "array_position",
                                new[] { operand, _sqlExpressionFactory.Fragment("NULL") },
                                typeof(int)))));
            }

            // Note: we also translate .Where(e => new[] { "a", "b", "c" }.Any(p => EF.Functions.Like(e.SomeText, p)))
            // to LIKE ANY (...). See NpgsqlSqlTranslatingExpressionVisitor.VisitMethodCall.

            return null;
        }
    }
}
