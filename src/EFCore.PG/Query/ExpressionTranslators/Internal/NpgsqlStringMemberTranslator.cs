using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

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
                    _sqlExpressionFactory.Function(
                        "LENGTH",
                        new[] { instance },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        typeof(long)),
                    returnType)
                : null;
    }
}
