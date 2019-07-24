using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
#pragma warning disable EF1001
    public class NpgsqlEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
#pragma warning restore EF1001
    {
        [NotNull] static readonly MethodInfo TsQueryParse =
            typeof(NpgsqlTsQuery).GetRuntimeMethod(nameof(NpgsqlTsQuery.Parse), new[] { typeof(string) });

        [NotNull] static readonly MethodInfo TsVectorParse =
            typeof(NpgsqlTsVector).GetRuntimeMethod(nameof(NpgsqlTsVector.Parse), new[] { typeof(string) });

        public NpgsqlEvaluatableExpressionFilter([NotNull] IModel model) : base(model) {}

        public override bool IsEvaluatableExpression(Expression expression)
        {
            // Full text search
            if (expression is MethodCallExpression e && (
                e.Method == TsQueryParse || e.Method == TsVectorParse ||
                e.Method.DeclaringType == typeof(NpgsqlFullTextSearchDbFunctionsExtensions) ||
                e.Method.DeclaringType == typeof(NpgsqlFullTextSearchLinqExtensions)))
            {
                return false;
            }

            // NodaTime
            // TODO: This is a hack until https://github.com/aspnet/EntityFrameworkCore/issues/13454 is done
            if (expression is MethodCallExpression methodExpr && (
                    methodExpr.Method.DeclaringType?.FullName == "NodaTime.SystemClock" ||
                    methodExpr.Method.Name == "GetCurrentInstant") ||
                expression is MemberExpression memberExpr && (
                    memberExpr.Member.DeclaringType?.FullName == "NodaTime.SystemClock" ||
                    memberExpr.Member.Name == "Instance"))
            {
                return false;
            }

#pragma warning disable EF1001
            return base.IsEvaluatableExpression(expression);
#pragma warning restore EF1001
        }
    }
}
