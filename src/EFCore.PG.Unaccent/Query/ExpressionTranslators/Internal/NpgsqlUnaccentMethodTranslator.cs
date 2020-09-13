using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlUnaccentMethodTranslator : IMethodCallTranslator
    {
        static readonly Dictionary<MethodInfo, string> Functions = new Dictionary<MethodInfo, string>
        {
            [GetRuntimeMethod(nameof(NpgsqlUnaccentDbFunctionsExtensions.Unaccent), new[] { typeof(DbFunctions), typeof(string) })] = "unaccent",
            [GetRuntimeMethod(nameof(NpgsqlUnaccentDbFunctionsExtensions.Unaccent), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "unaccent",
        };

        static MethodInfo GetRuntimeMethod(string name, params Type[] parameters)
            => typeof(NpgsqlUnaccentDbFunctionsExtensions).GetRuntimeMethod(name, parameters);

        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        static readonly bool[][] TrueArrays =
        {
            Array.Empty<bool>(),
            new[] { true },
            new[] { true, true }
        };
        public NpgsqlUnaccentMethodTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

#pragma warning disable EF1001
        /// <inheritdoc />
        public SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (Functions.TryGetValue(method, out var function))
                return _sqlExpressionFactory.Function(
                    function,
                    arguments.Skip(1),
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[arguments.Count - 1],
                    method.ReturnType);

            return null;
        }
#pragma warning restore EF1001
    }
}
