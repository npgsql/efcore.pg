using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides expression translation for <see cref="DateTime"/> addition methods.
    /// </summary>
    public class NpgsqlDateAddTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// The minimum backend version supported by this translator.
        /// </summary>
        [NotNull] static readonly Version MinimumSupportedVersion = new Version(9, 4);

        /// <summary>
        /// The mapping of supported method translations.
        /// </summary>
        [NotNull] static readonly Dictionary<MethodInfo, string> MethodInfoDatePartMapping = new Dictionary<MethodInfo, string>
        {
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), new[] { typeof(int) }), "years" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), new[] { typeof(int) }), "months" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), new[] { typeof(double) }), "days" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), new[] { typeof(double) }), "hours" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), new[] { typeof(double) }), "mins" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), new[] { typeof(double) }), "secs" },
            //{ typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), new[] { typeof(double) }), "milliseconds" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddYears), new[] { typeof(int) }), "years" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMonths), new[] { typeof(int) }), "months" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddDays), new[] { typeof(double) }), "days" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddHours), new[] { typeof(double) }), "hours" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMinutes), new[] { typeof(double) }), "mins" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddSeconds), new[] { typeof(double) }), "secs" },
            //{ typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMilliseconds), new[] { typeof(double) }), "milliseconds" }
        };

        /// <summary>
        /// The backend version to target.
        /// </summary>
        [NotNull] readonly Version _postgresVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlDateAddTranslator"/> class.
        /// </summary>
        /// <param name="postgresVersion">The backend version to target.</param>
        public NpgsqlDateAddTranslator([CanBeNull] Version postgresVersion)
            => _postgresVersion = postgresVersion ?? MinimumSupportedVersion;

        /// <inheritdoc />
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            // This translation is only supported for PostgreSQL 9.4 or higher.
            if (_postgresVersion < MinimumSupportedVersion)
                return null;

            if (!MethodInfoDatePartMapping.TryGetValue(methodCallExpression.Method, out var datePart))
                return null;

            // When we have a constant, we create the interval via INTERVAL '1 days'. But this expression cannot be
            // parameterized, so we use CAST($1 || ' days' AS interval) instead
            // Note that a make_interval() function also exists, but accepts only int (for all fields except for
            // seconds), so we don't use it.

            Expression interval;

            var instance = methodCallExpression.Object;
            var amountToAdd = methodCallExpression.Arguments[0];

            if (instance is null || amountToAdd is null)
                return null;

            if (amountToAdd is ConstantExpression constantExpression)
            {
                if (constantExpression.Type == typeof(double) &&
                    ((double)constantExpression.Value >= int.MaxValue ||
                     (double)constantExpression.Value <= int.MinValue))
                {
                    return null;
                }

                interval = new SqlFragmentExpression(FormattableString.Invariant($"INTERVAL '{amountToAdd} {datePart}'"));
            }
            else
            {
                interval = new ExplicitCastExpression(
                    new CustomBinaryExpression(amountToAdd, Expression.Constant(' ' + datePart), "||", typeof(string)),
                    typeof(TimeSpan)
                );
            }

            // Ideally we would use Expression.Add() to have a simple plus-sign expression generated for us.
            // But to represent an interval we'd need to use a TimeSpan, which does not represent months/years.
            // We could use the provider-specific NpgsqlTimeSpan but it's not currently supported and is somewhat
            // ugly, so we just generate the expression ourselves with CustomBinaryExpression

            return new CustomBinaryExpression(instance, interval, "+", methodCallExpression.Type);
        }
    }
}
