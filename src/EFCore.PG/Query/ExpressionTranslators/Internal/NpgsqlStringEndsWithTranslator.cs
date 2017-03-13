using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringEndsWithTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => ReferenceEquals(methodCallExpression.Method, _methodInfo)
                ? Expression.Equal(
                    new SqlFunctionExpression(
                        "RIGHT",
                        // ReSharper disable once PossibleNullReferenceException
                        methodCallExpression.Object.Type,
                        new[]
                        {
                            methodCallExpression.Object,
                            new SqlFunctionExpression("LENGTH", typeof(int), new[] { methodCallExpression.Arguments[0] })
                        }
                    ),
                    methodCallExpression.Arguments[0]
                )
                : null;
    }
}
