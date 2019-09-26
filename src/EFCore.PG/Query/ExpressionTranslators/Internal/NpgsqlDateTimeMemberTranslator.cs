using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for <see cref="DateTime"/> members.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-datetime.html
    /// </remarks>
    public class NpgsqlDateTimeMemberTranslator : IMemberTranslator
    {
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        public NpgsqlDateTimeMemberTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        /// <inheritdoc />
        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            var type = member.DeclaringType;
            if (type != typeof(DateTime) && type != typeof(NpgsqlDateTime) && type != typeof(NpgsqlDate))
                return null;

            return member.Name switch
            {
                nameof(DateTime.Now)       => _sqlExpressionFactory.Function("NOW", Array.Empty<SqlExpression>(), returnType),
                nameof(DateTime.UtcNow)    =>
                    _sqlExpressionFactory.AtTimeZone(
                        _sqlExpressionFactory.Function("NOW", Array.Empty<SqlExpression>(), returnType),
                        _sqlExpressionFactory.Constant("UTC"),
                        returnType),

                nameof(DateTime.Today)     => _sqlExpressionFactory.Function(
                    "DATE_TRUNC",
                    new SqlExpression[]
                    {
                        _sqlExpressionFactory.Constant("day"),
                        _sqlExpressionFactory.Function("NOW", Array.Empty<SqlExpression>(), returnType)
                    },
                    returnType),

                nameof(DateTime.Year)      => GetDatePartExpression(instance, "year"),
                nameof(DateTime.Month)     => GetDatePartExpression(instance, "month"),
                nameof(DateTime.DayOfYear) => GetDatePartExpression(instance, "doy"),
                nameof(DateTime.Day)       => GetDatePartExpression(instance, "day"),
                nameof(DateTime.Hour)      => GetDatePartExpression(instance, "hour"),
                nameof(DateTime.Minute)    => GetDatePartExpression(instance, "minute"),
                nameof(DateTime.Second)    => GetDatePartExpression(instance, "second"),

                nameof(DateTime.Millisecond) => null, // Too annoying

                // .NET's DayOfWeek is an enum, but its int values happen to correspond to PostgreSQL
                nameof(DateTime.DayOfWeek) => GetDatePartExpression(instance, "dow", true),

                nameof(DateTime.Date) => _sqlExpressionFactory.Function("DATE_TRUNC", new[] { _sqlExpressionFactory.Constant("day"), instance }, returnType),

                // TODO: Technically possible simply via casting to PG time, should be better in EF Core 3.0
                // but ExplicitCastExpression only allows casting to PG types that
                // are default-mapped from CLR types (timespan maps to interval,
                // which timestamp cannot be cast into)
                nameof(DateTime.TimeOfDay) => null,

                // TODO: Should be possible
                nameof(DateTime.Ticks) => null,

                _ => null
            };
        }

        /// <summary>
        /// Constructs the DATE_PART expression.
        /// </summary>
        /// <param name="e">The member expression.</param>
        /// <param name="partName">The name of the DATE_PART to construct.</param>
        /// <param name="floor">True if the result should be wrapped with FLOOR(...); otherwise, false.</param>
        /// <returns>
        /// The DATE_PART expression.
        /// </returns>
        /// <remarks>
        /// DATE_PART returns doubles, which we floor and cast into ints
        /// This also gets rid of sub-second components when retrieving seconds.
        /// </remarks>
        [NotNull]
        SqlExpression GetDatePartExpression(
            [NotNull] SqlExpression instance,
            [NotNull] string partName,
            bool floor = false)
        {
            var result = _sqlExpressionFactory.Function(
                "DATE_PART",
                new[]
                {
                    _sqlExpressionFactory.Constant(partName),
                    instance
                }, typeof(double));

            if (floor)
                result = _sqlExpressionFactory.Function("FLOOR", new[] { result }, typeof(double));

            return _sqlExpressionFactory.Convert(result, typeof(int));
        }
    }
}
