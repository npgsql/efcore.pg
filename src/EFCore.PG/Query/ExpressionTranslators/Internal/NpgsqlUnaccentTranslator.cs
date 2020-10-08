using System.Collections.Generic;
using System.Linq;
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
    /// Translates <see cref="T:DbFunctionsExtensions.Unaccent"/> methods into PostgreSQL unaccent functions.
    /// </summary>
    public class NpgsqlUnaccentTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo Unaccent =
            typeof(NpgsqlUnaccentDbFunctionsExtensions).GetRuntimeMethod(
                nameof(NpgsqlUnaccentDbFunctionsExtensions.Unaccent),
                new[] { typeof(DbFunctions), typeof(string) });

        // ReSharper disable once InconsistentNaming
        static readonly MethodInfo UnaccentWithRegDictionary =
            typeof(NpgsqlUnaccentDbFunctionsExtensions).GetRuntimeMethod(
                nameof(NpgsqlUnaccentDbFunctionsExtensions.Unaccent),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        [NotNull]
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlUnaccentTranslator"/> class.
        /// </summary>
        /// <param name="sqlExpressionFactory">The SQL expression factory to use when generating expressions..</param>
        public NpgsqlUnaccentTranslator([NotNull] NpgsqlSqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        /// <inheritdoc />
        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (method == Unaccent)
                return _sqlExpressionFactory.Function(
                    "unaccent",
                    arguments.Skip(1),
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    method.ReturnType);

            if (method == UnaccentWithRegDictionary)
                return _sqlExpressionFactory.Function(
                    "unaccent",
                    arguments.Skip(1),
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    method.ReturnType);

            return null;
        }
    }
}
