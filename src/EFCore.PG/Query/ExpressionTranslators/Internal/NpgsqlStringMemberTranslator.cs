using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates <see cref="M:string.Length"/> to 'LENGTH(text)'.
    /// </summary>
    public class NpgsqlStringMemberTranslator : IMemberTranslator
    {
        readonly ISqlExpressionFactory _sqlExpressionFactory;

        public NpgsqlStringMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
            => member.Name == nameof(string.Length) && instance?.Type == typeof(string)
                ? _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function("LENGTH", new[] { instance }, typeof(long)),
                    returnType)
                : null;
    }
}
