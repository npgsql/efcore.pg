using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL json operators.
    /// </summary>
    public class NpgsqlJsonTranslator : IMethodCallTranslator
    {
        [NotNull]
        readonly ISqlExpressionFactory _sqlExpressionFactory;
        [NotNull]
        readonly RelationalTypeMapping _boolMapping;

        public NpgsqlJsonTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _boolMapping = sqlExpressionFactory.FindMapping(typeof(bool));
        }

        /// <inheritdoc />
        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (method.DeclaringType != typeof(NpgsqlJsonExtensions))
                return null;

            return method.Name switch
            {
                nameof(NpgsqlJsonExtensions.KeyExists) => GetCustomBinaryExpression("?"),
                nameof(NpgsqlJsonExtensions.GetValue) => GetCustomBinaryExpression("->"),

                _ => null
            };

            SqlCustomBinaryExpression GetCustomBinaryExpression(string @operator)
            {
                var inferredMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);
                return new SqlCustomBinaryExpression(
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[0], inferredMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[1], inferredMapping),
                    @operator,
                    method.ReturnType,
                    inferredMapping);
            }
        }
    }
}
