using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringIsNullOrWhiteSpaceTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => _methodInfo == methodCallExpression.Method
                ? Expression.MakeBinary(
                    ExpressionType.OrElse,
                    new IsNullExpression(methodCallExpression.Arguments[0]),
                    new RegexMatchExpression(
                        methodCallExpression.Arguments[0],
                        Expression.Constant(@"^\s*$"),
                        RegexOptions.Singleline
                    )
                )
                : null;
    }
}
