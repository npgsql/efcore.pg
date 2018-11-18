using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL NULLIF functions.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/functions-conditional.html#FUNCTIONS-NULLIF
    /// </remarks>
    public class NpgsqlNullIfTranslator : IMethodCallTranslator
    {
        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression e)
        {
            if (e.Method.DeclaringType != typeof(NpgsqlDbFunctionsExtensions))
                return null;

            if (e.Method.Name == nameof(NpgsqlDbFunctionsExtensions.NullIf))
                return new SqlFunctionExpression("NULLIF", e.Arguments[1].Type, e.Arguments.Skip(1));

            return null;
        }
    }
}
