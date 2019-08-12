using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides services casting PostgreSQL json queries to correct type.
    /// </summary>
    public class NpgsqlCastTranslator : IMethodCallTranslator
    {
        [NotNull]
        readonly ISqlExpressionFactory _sqlExpressionFactory;
        [NotNull]
        readonly RelationalTypeMapping _boolMapping;

        public NpgsqlCastTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _boolMapping = sqlExpressionFactory.FindMapping(typeof(bool));
        }

        /// <inheritdoc />
        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (method.IsStatic && method.Name == nameof(double.Parse))
            {
                return _sqlExpressionFactory.Convert(
                    arguments[0],
                    typeof(double),
                    _sqlExpressionFactory.FindMapping(typeof(double))
                );
            }

            return null;
        }
    }
}
