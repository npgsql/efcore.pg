using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime
{
    public class NodaTimeMemberTranslator : IMemberTranslator
    {
        public Expression Translate(MemberExpression e)
        {
            var declaringType = e.Member.DeclaringType;
            if (declaringType == typeof(LocalDateTime) ||
                declaringType == typeof(LocalDate) ||
                declaringType == typeof(LocalTime) ||
                declaringType == typeof(Period))
            {
                return TranslateDateTime(e);
            }

            return null;
        }

        Expression TranslateDateTime(MemberExpression e)
        {
            var n = e.Member.Name;

            if (n == "Year" || n == "Years")
                return GetDatePartExpression(e, "year");
            if (n == "Month" || n == "Months")
                return GetDatePartExpression(e, "month");
            if (n == "DayOfYear")
                return GetDatePartExpression(e, "doy");
            if (n == "Day" || n == "Days")
                return GetDatePartExpression(e, "day");

            if (n == "Hour" || n == "Hours")
                return GetDatePartExpression(e, "hour");
            if (n == "Minute" || n == "Minutes")
                return GetDatePartExpression(e, "minute");
            if (n == "Second" || n == "Seconds")
                return GetDatePartExpression(e, "second", true);
            if (n == "Millisecond" || n == "Milliseconds")
                return null;  // Too annoying

            if (n == "DayOfWeek")
            {
                // Unlike DateTime.DayOfWeek, NodaTime's IsoDayOfWeek enum doesn't exactly correspond to PostgreSQL's
                // values returned by DATE_PART('dow', ...): in NodaTime Sunday is 7 and not 0, which is None.
                // So we generate a CASE WHEN expression to translate PostgreSQL's 0 to 7.
                var getValueExpression = GetDatePartExpression(e, "dow");
                return Expression.Condition(Expression.Equal(getValueExpression, Expression.Constant(0)),
                    Expression.Constant(7), getValueExpression);
            }

            if (n == "Date")
            {
                return new SqlFunctionExpression("DATE_TRUNC", e.Type, new[]
                {
                    Expression.Constant("day"),
                    e.Expression
                });
            }

            if (n == "TimeOfDay")
            {
                // TODO: Technically possible simply via casting to PG time,
                // but ExplicitCastExpression only allows casting to PG types that
                // are default-mapped from CLR types (timespan maps to interval,
                // which timestamp cannot be cast into)
                return null;
            }

            return null;
        }

        static Expression GetDatePartExpression(MemberExpression e, string partName, bool needsFloor=false)
        {
            // DATE_PART returns doubles, which we floor and cast into ints
            // This also gets rid of sub-second components when retrieving seconds

            var result = new SqlFunctionExpression("DATE_PART", typeof(double), new[]
            {
                Expression.Constant(partName),
                e.Expression
            });

            if (needsFloor)
                result = new SqlFunctionExpression("FLOOR", typeof(double), new[] { result });

            return new ExplicitCastExpression(result, typeof(int));
        }
    }
}
