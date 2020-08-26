using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

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

        public NpgsqlNewGuidTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => MethodInfo.Equals(method)
                ? _sqlExpressionFactory.Function(
                    "uuid_generate_v4",
                    Array.Empty<SqlExpression>(),
                    nullable: false,
                    argumentsPropagateNullability: FalseArrays[0],
                    method.ReturnType)
                : null;
    }
}
