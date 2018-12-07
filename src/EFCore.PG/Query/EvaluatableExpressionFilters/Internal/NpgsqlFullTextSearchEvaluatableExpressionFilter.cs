using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NpgsqlTypes;

// TODO: https://github.com/aspnet/EntityFrameworkCore/issues/11466 may provide a better way for implementing
// this (we're currently replacing an internal service
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.EvaluatableExpressionFilters.Internal
{
    /// <summary>
    /// Represents an Npgsql-specific filter for full-text search to identify expressions that are evaluatable.
    /// </summary>
    public class NpgsqlFullTextSearchEvaluatableExpressionFilter : EvaluatableExpressionFilterBase
    {
        /// <summary>
        /// The static method info for <see cref="NpgsqlTsQuery.Parse(string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo TsQueryParse =
            typeof(NpgsqlTsQuery).GetRuntimeMethod(nameof(NpgsqlTsQuery.Parse), new[] { typeof(string) });

        /// <summary>
        /// The static method info for <see cref="NpgsqlTsVector.Parse(string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo TsVectorParse =
            typeof(NpgsqlTsVector).GetRuntimeMethod(nameof(NpgsqlTsVector.Parse), new[] { typeof(string) });

        /// <inheritdoc />
        public override bool IsEvaluatableExpression(Expression expression)
            => !(expression is MethodCallExpression e && (
                   e.Method == TsQueryParse ||
                   e.Method == TsVectorParse ||
                   e.Method.DeclaringType == typeof(NpgsqlFullTextSearchDbFunctionsExtensions) ||
                   e.Method.DeclaringType == typeof(NpgsqlFullTextSearchLinqExtensions)
               ));
    }
}
