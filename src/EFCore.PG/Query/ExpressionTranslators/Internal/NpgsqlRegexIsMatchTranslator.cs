using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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

        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        public NpgsqlRegexIsMatchTranslator([NotNull] NpgsqlSqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        /// <inheritdoc />
        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (method != IsMatch && method != IsMatchWithRegexOptions)
                return null;

            var (input, pattern) = (arguments[0], arguments[1]);
            var typeMapping = ExpressionExtensions.InferTypeMapping(input, pattern);

            RegexOptions options;

            if (method == IsMatch)
                options = RegexOptions.None;
            else if (arguments[2] is SqlConstantExpression constantOptionsExpr)
                options = (RegexOptions)constantOptionsExpr.Value;
            else
                return null;  // We don't support non-constant regex options

            return (options & UnsupportedRegexOptions) == 0
                ? _sqlExpressionFactory.RegexMatch(
                    _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping),
                    options)
                : null;
        }
    }
}
