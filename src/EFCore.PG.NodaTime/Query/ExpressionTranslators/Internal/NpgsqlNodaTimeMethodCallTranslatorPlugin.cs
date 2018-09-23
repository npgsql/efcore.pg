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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime
{
    /// <summary>
    /// Provides translation services for <see cref="NodaTime"/> members.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-datetime.html
    /// </remarks>
    public class NpgsqlNodaTimeMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        public virtual IEnumerable<IMethodCallTranslator> Translators { get; } = new IMethodCallTranslator[]
        {
            new NpgsqlNodaTimeMethodCallTranslator()
        };
    }

    /// <summary>
    /// Provides translation services for NodaTime method calls.
    /// </summary>
    public class NpgsqlNodaTimeMethodCallTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// The static method info for <see cref="T:SystemClock.GetCurrentInstant()"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo GetCurrentInstant =
            typeof(SystemClock).GetRuntimeMethod(nameof(SystemClock.GetCurrentInstant), Type.EmptyTypes);

        /// <summary>
        /// The mapping of supported method translations.
        /// </summary>
        [NotNull] static readonly Dictionary<MethodInfo, string> PeriodPethodMap = new Dictionary<MethodInfo, string>
        {
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromYears),        new[] { typeof(int) }),  "years" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromMonths),       new[] { typeof(int) }),  "months" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromWeeks),        new[] { typeof(int) }),  "weeks" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromDays),         new[] { typeof(int) }),  "days" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromHours),        new[] { typeof(long) }), "hours" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromMinutes),      new[] { typeof(long) }), "mins" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromSeconds),      new[] { typeof(long) }), "secs" },
            //{ typeof(Period).GetRuntimeMethod(nameof(Period.FromMilliseconds), new[] { typeof(long) }), "" },
            //{ typeof(Period).GetRuntimeMethod(nameof(Period.FromNanoseconds),  new[] { typeof(long) }), "" },
        };

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression e)
        {
            if (e.Method == GetCurrentInstant)
                return new AtTimeZoneExpression(new SqlFunctionExpression("NOW", e.Type), "UTC", e.Type);

            // TODO: Version compat? See DateTime.Add* translator
            var declaringType = e.Method.DeclaringType;
            if (declaringType == typeof(Period))
            {
                return PeriodPethodMap.TryGetValue(e.Method, out var datePart)
                    ? new PgFunctionExpression("MAKE_INTERVAL", typeof(Period), new Dictionary<string, Expression> {
                          [datePart] = e.Arguments[0]
                      })
                    : null;
            }
            return null;
        }
    }
}
