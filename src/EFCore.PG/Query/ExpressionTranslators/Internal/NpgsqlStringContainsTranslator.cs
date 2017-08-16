using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringContainsTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => methodCallExpression.Method.Equals(_methodInfo)
                ? Expression.GreaterThan(
                    new SqlFunctionExpression("STRPOS", typeof(int), new[]
                    {
                        methodCallExpression.Object,
                        methodCallExpression.Arguments[0]
                    }), Expression.Constant(0))
                : null;
    }
}
