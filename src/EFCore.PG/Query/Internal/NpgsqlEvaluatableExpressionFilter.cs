using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NpgsqlTypes;

// TODO: https://github.com/aspnet/EntityFrameworkCore/issues/11466 may provide a better way for implementing
// this (we're currently replacing an internal service
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
    {
        static readonly MethodInfo _tsQueryParse = typeof(NpgsqlTsQuery).GetMethod(
            nameof(NpgsqlTsQuery.Parse),
            BindingFlags.Public | BindingFlags.Static);

        static readonly MethodInfo _tsVectorParse = typeof(NpgsqlTsVector).GetMethod(
            nameof(NpgsqlTsVector.Parse),
            BindingFlags.Public | BindingFlags.Static);

        public NpgsqlEvaluatableExpressionFilter([NotNull] IModel model) : base(model) { }

        public override bool IsEvaluatableMethodCall(MethodCallExpression methodCallExpression) =>
            methodCallExpression.Method != _tsQueryParse
            && methodCallExpression.Method != _tsVectorParse
            && methodCallExpression.Method.DeclaringType != typeof(NpgsqlFullTextSearchDbFunctionsExtensions)
            && methodCallExpression.Method.DeclaringType != typeof(NpgsqlFullTextSearchLinqExtensions)
            && base.IsEvaluatableMethodCall(methodCallExpression);
    }
}
