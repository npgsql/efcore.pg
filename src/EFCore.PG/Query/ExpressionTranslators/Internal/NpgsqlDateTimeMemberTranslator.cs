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
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlDateTimeMemberTranslator : IMemberTranslator
    {
        static readonly PropertyInfo Now = typeof(DateTime).GetProperty(nameof(DateTime.Now));
        static readonly PropertyInfo UtcNow = typeof(DateTime).GetProperty(nameof(DateTime.UtcNow));

        public virtual Expression Translate(MemberExpression e)
        {
            if (e.Expression == null)
                return TranslateStatic(e);

            var type = e.Expression?.Type;
            if (type != typeof(DateTime) &&
                type != typeof(NpgsqlDateTime) &&
                type != typeof(NpgsqlDate))
            {
                return null;
            }

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

        static Expression GetDatePartExpression(MemberExpression e, string partName)
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
                    typeof(int)
                );

        Expression TranslateStatic(MemberExpression e)
        {
            if (e.Member.Equals(Now))
                return new SqlFunctionExpression("NOW", e.Type);
            if (e.Member.Equals(UtcNow))
                return new AtTimeZoneExpression(new SqlFunctionExpression("NOW", e.Type), "UTC");
            return null;
        }
    }
}
