using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL UUID functions.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/datatype-uuid.html
    /// </remarks>
    public class NpgsqlGuidTranslator : IMethodCallTranslator
    {
        [NotNull] static readonly MethodInfo NewGuid = typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Type.EmptyTypes);

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression e)
            => e.Method == NewGuid ? new SqlFunctionExpression("uuid_generate_v4", typeof(Guid)) : null;
    }
}
