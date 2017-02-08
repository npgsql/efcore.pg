using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringStartsWithTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        static readonly MethodInfo _concat
            = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        public virtual Expression Translate(MethodCallExpression e)
        {
            if (e.Method != _methodInfo || e.Object == null)
                return null;

            var constantPatternExpr = e.Arguments[0] as ConstantExpression;
            if (constantPatternExpr != null)
            {
                // The pattern is constant. Escape all special characters (%, _, \) in C# and send
                // a simple LIKE
                return new LikeExpression(
                    e.Object,
                    Expression.Constant(Regex.Replace((string)constantPatternExpr.Value, @"([%_\\])", @"\$1") + '%')
                );
            }

            // The pattern isn't a constant (i.e. parameter, database column...).
            // First run LIKE against the *unescaped* pattern (which will efficiently use indices),
            // but then add another test to filter out false positives.
            var pattern = e.Arguments[0];
            return Expression.AndAlso(
                new LikeExpression(e.Object, Expression.Add(pattern, Expression.Constant("%"), _concat)),
                Expression.Equal(
                    new SqlFunctionExpression("LEFT", typeof(string), new[]
                    {
                        e.Object,
                        new SqlFunctionExpression("LENGTH", typeof(int), new[] { pattern }),
                    }),
                    pattern
                )
            );
        }
    }
}
