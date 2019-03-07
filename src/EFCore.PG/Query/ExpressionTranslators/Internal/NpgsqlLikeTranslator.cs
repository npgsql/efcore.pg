using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates <see cref="T:DbFunctionsExtensions.Like"/> methods into PostgreSQL LIKE expressions.
    /// </summary>
    public class NpgsqlLikeTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo Like =
            typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        static readonly MethodInfo LikeWithEscape =
            typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        // ReSharper disable once InconsistentNaming
        static readonly MethodInfo ILike =
            typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        // ReSharper disable once InconsistentNaming
        static readonly MethodInfo ILikeWithEscape =
            typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression e, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(e, nameof(e));

            if (Equals(e.Method, LikeWithEscape))
                return new LikeExpression(e.Arguments[1], e.Arguments[2], e.Arguments[3]);

            if (Equals(e.Method, ILikeWithEscape))
                return new ILikeExpression(e.Arguments[1], e.Arguments[2], e.Arguments[3]);

            bool sensitive;
            if (Equals(e.Method, Like))
                sensitive = true;
            else if (Equals(e.Method, ILike))
                sensitive = false;
            else
                return null;

            // PostgreSQL has backslash as the default LIKE escape character, but EF Core expects
            // no escape character unless explicitly requested (https://github.com/aspnet/EntityFramework/issues/8696).

            // If we have a constant expression, we check that there are no backslashes in order to render with
            // an ESCAPE clause (better SQL). If we have a constant expression with backslashes or a non-constant
            // expression, we render an ESCAPE clause to disable backslash escaping.

            if (e.Arguments[2] is ConstantExpression constantPattern &&
                constantPattern.Value is string pattern &&
                !pattern.Contains("\\"))
            {
                return sensitive
                    ? new LikeExpression(e.Arguments[1], e.Arguments[2])
                    : (Expression)new ILikeExpression(e.Arguments[1], e.Arguments[2]);
            }

            return sensitive
                ? new LikeExpression(e.Arguments[1], e.Arguments[2], Expression.Constant(string.Empty))
                : (Expression)new ILikeExpression(e.Arguments[1], e.Arguments[2], Expression.Constant(string.Empty));
        }
    }
}
