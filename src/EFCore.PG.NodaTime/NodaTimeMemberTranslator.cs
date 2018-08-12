#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime
{
    /// <summary>
    /// Provides translation services for <see cref="NodaTime"/> members.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-datetime.html
    /// </remarks>
    public class NodaTimeMemberTranslator : NpgsqlDateTimeMemberTranslator
    {
        /// <summary>
        /// The static member info for <see cref="T:SystemClock.Instance"/>.
        /// </summary>
        [NotNull] static readonly MemberInfo Instance =
            typeof(SystemClock).GetRuntimeProperty(nameof(SystemClock.Instance));

        /// <inheritdoc />
        public override Expression Translate(MemberExpression e)
        {
            if (e.Member == Instance)
                return e;

            var declaringType = e.Member.DeclaringType;
            if (declaringType == typeof(LocalDateTime) ||
                declaringType == typeof(LocalDate) ||
                declaringType == typeof(LocalTime) ||
                declaringType == typeof(Period))
                return TranslateDateTime(e);

            return null;
        }

        /// <summary>
        /// Translates date and time members.
        /// </summary>
        /// <param name="e">The member expression.</param>
        /// <returns>
        /// The translated expression or null.
        /// </returns>
        [CanBeNull]
        static Expression TranslateDateTime([NotNull] MemberExpression e)
        {
            switch (e.Member.Name)
            {
            case "Year":
            case "Years":
                return GetDatePartExpression(e, "year");

            case "Month":
            case "Months":
                return GetDatePartExpression(e, "month");

            case "DayOfYear":
                return GetDatePartExpression(e, "doy");

            case "Day":
            case "Days":
                return GetDatePartExpression(e, "day");

            case "Hour":
            case "Hours":
                return GetDatePartExpression(e, "hour");

            case "Minute":
            case "Minutes":
                return GetDatePartExpression(e, "minute");

            case "Second":
            case "Seconds":
                return GetDatePartExpression(e, "second", true);

            case "Millisecond":
            case "Milliseconds":
                return null; // Too annoying

            case "DayOfWeek":
                // Unlike DateTime.DayOfWeek, NodaTime's IsoDayOfWeek enum doesn't exactly correspond to PostgreSQL's
                // values returned by DATE_PART('dow', ...): in NodaTime Sunday is 7 and not 0, which is None.
                // So we generate a CASE WHEN expression to translate PostgreSQL's 0 to 7.
                var getValueExpression = GetDatePartExpression(e, "dow", true);
                return
                    Expression.Condition(
                        Expression.Equal(
                            getValueExpression,
                            Expression.Constant(0)),
                        Expression.Constant(7),
                        getValueExpression);

            case "Date":
                return new SqlFunctionExpression("DATE_TRUNC", e.Type, new[] { Expression.Constant("day"), e.Expression });

            case "TimeOfDay":
                // TODO: Technically possible simply via casting to PG time,
                // but ExplicitCastExpression only allows casting to PG types that
                // are default-mapped from CLR types (timespan maps to interval,
                // which timestamp cannot be cast into)
                return null;

            default:
                return null;
            }
        }
    }
}
