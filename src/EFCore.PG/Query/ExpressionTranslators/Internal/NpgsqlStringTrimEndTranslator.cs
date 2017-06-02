using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringTrimEndTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _trimEnd
            = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char[]) });

        // The following exists as an optimization in netcoreapp20
        static readonly MethodInfo _trimEndSingleChar
            = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (!methodCallExpression.Method.Equals(_trimEnd) &&
                !methodCallExpression.Method.Equals(_trimEndSingleChar))
            {
                return null;
            }

            var constantTrimChars = methodCallExpression.Arguments[0] as ConstantExpression;
            if (constantTrimChars == null)
                return null;

            if (methodCallExpression.Method.Equals(_trimEndSingleChar))
            {
                var trimChar = (char)constantTrimChars.Value;
                return new SqlFunctionExpression(
                    "RTRIM",
                    typeof(string),
                    new[]
                    {
                        methodCallExpression.Object,
                        Expression.Constant(new string(trimChar, 1))
                    });
            }

            var trimChars = (char[])constantTrimChars.Value;

            if (trimChars == null || trimChars.Length == 0)
            {
                // Trim whitespace
                return new SqlFunctionExpression(
                    "REGEXP_REPLACE",
                    typeof(string),
                    new[]
                    {
                    methodCallExpression.Object,
                    Expression.Constant(@"\s*$"),
                    Expression.Constant(string.Empty)
                });
            }

            return new SqlFunctionExpression(
                "RTRIM",
                typeof(string),
                new[]
                {
                    methodCallExpression.Object,
                    Expression.Constant(new string(trimChars))
                });
        }
    }
}
