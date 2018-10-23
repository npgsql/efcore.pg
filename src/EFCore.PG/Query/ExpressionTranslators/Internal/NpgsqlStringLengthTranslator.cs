using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates <see cref="M:string.Length"/> to 'LENGTH(text)'.
    /// </summary>
    public class NpgsqlStringLengthTranslator : IMemberTranslator
    {
        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MemberExpression e)
            => e.Expression != null &&
               e.Expression.Type == typeof(string) &&
               e.Member.Name == nameof(string.Length)
                ? new SqlFunctionExpression("LENGTH", e.Type, new[] { e.Expression })
                : null;
    }
}
