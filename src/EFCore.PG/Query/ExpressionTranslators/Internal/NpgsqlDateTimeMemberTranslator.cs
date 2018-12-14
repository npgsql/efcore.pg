using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
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
        /// <summary>
        /// The static <see cref="PropertyInfo"/> for <see cref="T:DateTime.Now"/>.
        /// </summary>
        [NotNull] static readonly PropertyInfo Now = typeof(DateTime).GetRuntimeProperty(nameof(DateTime.Now));

        /// <summary>
        /// The static <see cref="PropertyInfo"/> for <see cref="T:DateTime.UtcNow"/>.
        /// </summary>
        [NotNull] static readonly PropertyInfo UtcNow = typeof(DateTime).GetRuntimeProperty(nameof(DateTime.UtcNow));

        /// <summary>
        /// The static <see cref="PropertyInfo"/> for <see cref="T:DateTime.Today"/>.
        /// </summary>
        [NotNull] static readonly PropertyInfo Today = typeof(DateTime).GetRuntimeProperty(nameof(DateTime.Today));

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MemberExpression e)
        {
            if (e.Expression == null)
                return TranslateStatic(e);

            var type = e.Expression?.Type;
            if (type != typeof(DateTime) &&
                type != typeof(NpgsqlDateTime) &&
                type != typeof(NpgsqlDate))
                return null;

            switch (e.Member.Name)
            {
            case nameof(DateTime.Year):
                return GetDatePartExpression(e, "year");
            case nameof(DateTime.Month):
                return GetDatePartExpression(e, "month");
            case nameof(DateTime.DayOfYear):
                return GetDatePartExpression(e, "doy");
            case nameof(DateTime.Day):
                return GetDatePartExpression(e, "day");
            case nameof(DateTime.Hour):
                return GetDatePartExpression(e, "hour");
            case nameof(DateTime.Minute):
                return GetDatePartExpression(e, "minute");
            case nameof(DateTime.Second):
                return GetDatePartExpression(e, "second");

            case nameof(DateTime.Millisecond):
                // Too annoying
                return null;

            case nameof(DateTime.DayOfWeek):
                // .NET's DayOfWeek is an enum, but its int values happen to correspond to PostgreSQL
                return GetDatePartExpression(e, "dow", true);

            case nameof(DateTime.Date):
                return new SqlFunctionExpression("DATE_TRUNC", e.Type, new[] { Expression.Constant("day"), e.Expression });

            case nameof(DateTime.TimeOfDay):
                // TODO: Technically possible simply via casting to PG time,
                // but ExplicitCastExpression only allows casting to PG types that
                // are default-mapped from CLR types (timespan maps to interval,
                // which timestamp cannot be cast into)
                return null;

            case nameof(DateTime.Ticks):
                // TODO: Should be possible
                return null;

            default:
                return null;
            }
        }

        /// <summary>
        /// Translates static members of <see cref="DateTime"/>.
        /// </summary>
        /// <param name="e">The member expression.</param>
        /// <returns>
        /// The translated expression or null.
        /// </returns>
        [CanBeNull]
        static Expression TranslateStatic([NotNull] MemberExpression e)
        {
            if (e.Member.Equals(Now))
                return new SqlFunctionExpression("NOW", e.Type);
            if (e.Member.Equals(UtcNow))
                return new AtTimeZoneExpression(new SqlFunctionExpression("NOW", e.Type), "UTC", e.Type);
            if (e.Member.Equals(Today))
                return new SqlFunctionExpression("DATE_TRUNC", e.Type, new Expression[] { Expression.Constant("day"), new SqlFunctionExpression("NOW", e.Type) });
            return null;
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
        static Expression GetDatePartExpression(
            [NotNull] MemberExpression e,
            [NotNull] string partName,
            bool floor = false)
        {
            var result =
                new SqlFunctionExpression("DATE_PART", typeof(double), new[] { Expression.Constant(partName), e.Expression });

            if (floor)
                result = new SqlFunctionExpression("FLOOR", typeof(double), new[] { result });

            return new ExplicitStoreTypeCastExpression(result, typeof(int), "int");
        }
    }
}
