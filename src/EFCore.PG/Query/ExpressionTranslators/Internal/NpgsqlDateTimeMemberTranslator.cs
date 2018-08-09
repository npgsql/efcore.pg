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
                return GetDatePartExpression(e, "dow");

            case nameof(DateTime.Date):
                return new SqlFunctionExpression("DATE_TRUNC", e.Type, new[]
                {
                    Expression.Constant("day"),
                    e.Expression
                });

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
        /// Constructs the DATE_PART expression.
        /// </summary>
        /// <param name="e">The member expression.</param>
        /// <param name="partName">The name of the DATE_PART to construct.</param>
        /// <returns>
        /// The DATE_PART expression.
        /// </returns>
        [NotNull]
        static Expression GetDatePartExpression([NotNull] MemberExpression e, [NotNull] string partName)
            =>
                // DATE_PART returns doubles, which we floor and cast into ints
                // This also gets rid of sub-second components when retrieving seconds
                new ExplicitCastExpression(
                    new SqlFunctionExpression("FLOOR", typeof(double), new[]
                    {
                        new SqlFunctionExpression("DATE_PART", typeof(double), new[]
                        {
                            Expression.Constant(partName),
                            e.Expression
                        })
                    }),
                    typeof(int));

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
            return null;
        }
    }
}
