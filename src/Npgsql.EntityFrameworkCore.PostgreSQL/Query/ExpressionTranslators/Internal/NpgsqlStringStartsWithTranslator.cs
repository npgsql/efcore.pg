using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringStartsWithTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => ReferenceEquals(methodCallExpression.Method, _methodInfo)
                ? Expression.Equal(
                    new SqlFunctionExpression("STRPOS", typeof(int), new[]
                    {
                        methodCallExpression.Object,
                        methodCallExpression.Arguments[0]
                    }), Expression.Constant(1))
                : null;
    }
}
