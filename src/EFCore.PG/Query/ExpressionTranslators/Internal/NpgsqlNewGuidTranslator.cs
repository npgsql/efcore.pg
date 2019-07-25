using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL UUID functions.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/datatype-uuid.html
    /// </remarks>
    public class NpgsqlNewGuidTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo MethodInfo = typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Array.Empty<Type>());

        readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <inheritdoc />
        public NpgsqlNewGuidTranslator(ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
            => MethodInfo.Equals(method)
                ? _sqlExpressionFactory.Function("uuid_generate_v4", Array.Empty<SqlExpression>(), method.ReturnType)
                : null;
    }
}
