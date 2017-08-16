using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringTrimStartTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo TrimStartWithChars
            = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char[]) });

        // The following exists as an optimization in netcoreapp20
        static readonly MethodInfo TrimStartNoParam
            = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new Type[0]);

        // The following exists as an optimization in netcoreapp20
        static readonly MethodInfo TrimStartSingleChar
            = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (!methodCallExpression.Method.Equals(TrimStartWithChars) &&
                !methodCallExpression.Method.Equals(TrimStartNoParam) &&
                !methodCallExpression.Method.Equals(TrimStartSingleChar))
            {
                return null;
            }

            char[] trimChars;

            if (methodCallExpression.Method.Equals(TrimStartSingleChar))
            {
                var constantTrimChars = methodCallExpression.Arguments[0] as ConstantExpression;
                if (constantTrimChars == null)
                    return null;  // Don't translate if trim chars isn't a constant
                var trimChar = (char)constantTrimChars.Value;
                return new SqlFunctionExpression(
                    "LTRIM",
                    typeof(string),
                    new[]
                    {
                        methodCallExpression.Object,
                        Expression.Constant(new string(trimChar, 1))
                    });
            }

            if (methodCallExpression.Method.Equals(TrimStartNoParam))
            {
                trimChars = null;
            }
            else if (methodCallExpression.Method.Equals(TrimStartWithChars))
            {
                var constantTrimChars = methodCallExpression.Arguments[0] as ConstantExpression;
                if (constantTrimChars == null)
                    return null; // Don't translate if trim chars isn't a constant
                trimChars = (char[])constantTrimChars.Value;
            }
            else
                throw new Exception($"{nameof(NpgsqlStringTrimStartTranslator)} does not support {methodCallExpression}");

            if (trimChars == null || trimChars.Length == 0)
            {
                // Trim whitespace
                return new SqlFunctionExpression(
                    "REGEXP_REPLACE",
                    typeof(string),
                    new[]
                    {
                    methodCallExpression.Object,
                    Expression.Constant(@"^\s*"),
                    Expression.Constant(string.Empty)
                });
            }

            return new SqlFunctionExpression(
                "LTRIM",
                typeof(string),
                new[]
                {
                    methodCallExpression.Object,
                    Expression.Constant(new string(trimChars))
                });
        }
    }
}
