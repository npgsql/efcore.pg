using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlTimeSpanMemberTranslator : IMemberTranslator
    {
        readonly ISqlExpressionFactory _sqlExpressionFactory;

        public NpgsqlTimeSpanMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        static readonly bool[] FalseTrueArray = { false, true };

        public virtual SqlExpression Translate(SqlExpression instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));

            if (member.DeclaringType == typeof(TimeSpan))
            {
                return member.Name switch
                {
                    nameof(TimeSpan.Days)         => Floor(DatePart("day", instance)),
                    nameof(TimeSpan.Hours)        => Floor(DatePart("hour", instance)),
                    nameof(TimeSpan.Minutes)      => Floor(DatePart("minute", instance)),
                    nameof(TimeSpan.Seconds)      => Floor(DatePart("second", instance)),
                    nameof(TimeSpan.Milliseconds) => _sqlExpressionFactory.Modulo(
                        Floor(DatePart("millisecond", instance)),
                        _sqlExpressionFactory.Constant(1000)),
                    _ => null
                };
            }

            return null;

            SqlExpression Floor(SqlExpression value)
                => _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function(
                        "floor",
                        new[] { value },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        typeof(double)),
                    typeof(int));

            SqlFunctionExpression DatePart(string part, SqlExpression value)
                => _sqlExpressionFactory.Function("date_part", new[]
                    {
                        _sqlExpressionFactory.Constant(part),
                        value
                    },
                    nullable: true,
                    argumentsPropagateNullability: FalseTrueArray,
                    returnType);
        }
    }
}
