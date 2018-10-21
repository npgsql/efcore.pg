using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates Regex.IsMatch calls into PostgreSQL regex expressions for database-side processing.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/functions-matching.html
    /// </remarks>
    public class NpgsqlRegexIsMatchTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo IsMatch =
            typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string) });

        static readonly MethodInfo IsMatchWithRegexOptions =
            typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string), typeof(RegexOptions) });

        const RegexOptions UnsupportedRegexOptions = RegexOptions.RightToLeft | RegexOptions.ECMAScript;

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression e)
        {
            // Regex.IsMatch(string, string)
            if (e.Method.Equals(IsMatch))
                return new RegexMatchExpression(e.Arguments[0], e.Arguments[1], RegexOptions.None);

            // Regex.IsMatch(string, string, RegexOptions)
            if (!e.Method.Equals(IsMatchWithRegexOptions))
                return null;

            if (!(e.Arguments[2] is ConstantExpression constantExpr))
                return null;

            var options = (RegexOptions)constantExpr.Value;

            return
                (options & UnsupportedRegexOptions) == 0
                    ? new RegexMatchExpression(e.Arguments[0], e.Arguments[1], options)
                    : null;
        }
    }
}
