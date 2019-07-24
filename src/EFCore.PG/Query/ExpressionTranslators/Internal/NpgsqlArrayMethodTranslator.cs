using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline;

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
        static readonly MethodInfo SequenceEqualMethodInfo = typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(nameof(Enumerable.SequenceEqual)).Single(m =>
                m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 2);

        static readonly MethodInfo ContainsMethodInfo = typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(nameof(Enumerable.Contains)).Single(m =>
            m.IsGenericMethodDefinition &&
            m.GetParameters().Length == 2);

        [NotNull]
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        public NpgsqlArrayMethodTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            // TODO: Fully support List<>

            if (arguments.Count == 0 || !arguments[0].Type.IsArray &&
                (!arguments[0].Type.IsGenericType || arguments[0].Type.GetGenericTypeDefinition() != typeof(List<>)))
                return null;

            var arrayOperand = arguments[0];

            if (method.IsClosedFormOf(SequenceEqualMethodInfo) && arguments[1].Type.IsArray)
                return _sqlExpressionFactory.Equal(arrayOperand, arguments[1]);

            // Predicate-less Any - translate to a simple length check.
            if (method.IsClosedFormOf(LinqMethodHelpers.EnumerableAnyMethodInfo))
            {
                return _sqlExpressionFactory.GreaterThan(
                    _sqlExpressionFactory.Function(
                        "array_length",
                        new[] { arrayOperand, _sqlExpressionFactory.Constant(1) },
                        typeof(int)),
                    _sqlExpressionFactory.Constant(1));
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

            if (method.IsClosedFormOf(ContainsMethodInfo) &&
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
