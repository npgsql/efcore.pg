using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for static <see cref="Math"/> methods..
    /// </summary>
    /// <remarks>
    /// See:
    ///   - https://www.postgresql.org/docs/current/static/functions-math.html
    ///   - https://www.postgresql.org/docs/current/static/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
    /// </remarks>
    public class NpgsqlMathTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull] readonly Version _postgresVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlMathTranslator"/> class.
        /// </summary>
        /// <param name="postgresVersion">The backend version to target.</param>
        public NpgsqlMathTranslator([CanBeNull] Version postgresVersion) => _postgresVersion = postgresVersion;

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression expression)
        {
            var method = expression.Method;

            if (!method.IsStatic || method.DeclaringType != typeof(Math))
                return null;

            switch (method.Name)
            {
            case nameof(Math.Abs):
            case nameof(Math.Acos):
            case nameof(Math.Asin):
            case nameof(Math.Atan):
            case nameof(Math.Atan2):
            case nameof(Math.Ceiling):
            case nameof(Math.Cos):
            case nameof(Math.Exp):
            case nameof(Math.Floor):
            case nameof(Math.Sign):
            case nameof(Math.Sin):
            case nameof(Math.Sqrt):
            case nameof(Math.Tan):
                return new SqlFunctionExpression(method.Name.ToUpper(), expression.Type, expression.Arguments);

            case nameof(Math.Max) when VersionAtLeast(8, 1):
                return new SqlFunctionExpression("GREATEST", expression.Type, expression.Arguments);

            case nameof(Math.Min) when VersionAtLeast(8, 1):
                return new SqlFunctionExpression("LEAST", expression.Type, expression.Arguments);

            case nameof(Math.Log) when expression.Arguments.Count == 1:
                return new SqlFunctionExpression("LN", expression.Type, expression.Arguments);

            case nameof(Math.Log10):
                return new SqlFunctionExpression("LOG", expression.Type, expression.Arguments);

            case nameof(Math.Pow):
                return new SqlFunctionExpression("POWER", expression.Type, expression.Arguments);

            case nameof(Math.Round):
            {
                var firstArgument = expression.Arguments[0];
                if (firstArgument.NodeType == ExpressionType.Convert)
                    firstArgument = new ExplicitCastExpression(firstArgument, firstArgument.Type);

                return new SqlFunctionExpression(
                    "ROUND",
                    expression.Type,
                    expression.Arguments.Count == 1
                        ? new[] { firstArgument }
                        : new[] { firstArgument, expression.Arguments[1] });
            }

            case nameof(Math.Truncate):
            {
                var firstArgument = expression.Arguments[0];
                if (firstArgument.NodeType == ExpressionType.Convert)
                    firstArgument = new ExplicitCastExpression(firstArgument, firstArgument.Type);

                return new SqlFunctionExpression("TRUNC", expression.Type, new[] { firstArgument });
            }

            default:
                return null;
            }
        }

        #region Helpers

        /// <summary>
        /// True if <see cref="_postgresVersion"/> is null, greater than, or equal to the specified version.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <returns>
        /// True if <see cref="_postgresVersion"/> is null, greater than, or equal to the specified version.
        /// </returns>
        bool VersionAtLeast(int major, int minor)
            => _postgresVersion is null || new Version(major, minor) <= _postgresVersion;

        #endregion
    }
}
