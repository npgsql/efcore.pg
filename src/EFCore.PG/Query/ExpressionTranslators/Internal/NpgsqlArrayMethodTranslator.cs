using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

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
        readonly NpgsqlJsonTranslator _jsonTranslator;

        public NpgsqlArrayMethodTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory, NpgsqlJsonTranslator jsonTranslator)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _jsonTranslator = jsonTranslator;
        }

        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            // TODO: Fully support List<>

            if (arguments.Count == 0 || !arguments[0].Type.IsArray &&
                (!arguments[0].Type.IsGenericType || arguments[0].Type.GetGenericTypeDefinition() != typeof(List<>)))
                return null;

            var arrayOperand = arguments[0];

            if (method.IsClosedFormOf(SequenceEqual) && arguments[1].Type.IsArray)
                return _sqlExpressionFactory.Equal(arrayOperand, arguments[1]);

            // Predicate-less Any - translate to a simple length check.
            if (method.IsClosedFormOf(EnumerableAnyWithoutPredicate))
            {
                return
                    _sqlExpressionFactory.GreaterThan(
                        _jsonTranslator.TranslateArrayLength(arrayOperand) ??
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

            // TODO: The following does not work correctly if there are any nulls in a parameterized array, because
            // of null semantics (test: Contains_on_nullable_array_produces_correct_sql).
            // https://github.com/aspnet/EntityFrameworkCore/issues/15892 tracks caching based on parameter values,
            // which should allow us to enable this and have correct behavior.

            // We still apply this translation when it's on a column expression, since that can't work anyway with
            // EF Core's parameter to constant expansion

            if (method.IsClosedFormOf(Contains) &&
                arrayOperand is ColumnExpression &&
                //!(arrayOperand is SqlConstantExpression) &&   // When the null semantics issue is resolved
                _sqlExpressionFactory.FindMapping(arrayOperand.Type) != null)
            {
                return _sqlExpressionFactory.ArrayAnyAll(arguments[1], arrayOperand, ArrayComparisonType.Any, "=");
            }

            // Note: we also translate .Where(e => new[] { "a", "b", "c" }.Any(p => EF.Functions.Like(e.SomeText, p)))
            // to LIKE ANY (...). See NpgsqlSqlTranslatingExpressionVisitor.VisitMethodCall.

            return null;
        }
    }
}
