using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides expression translation for <see cref="DateTime"/> addition methods.
    /// </summary>
    public class NpgsqlDateAddTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// The minimum version of PostgreSQL for which this translator should operate.
        /// </summary>
        [NotNull] static readonly Version MinimumSupportedVersion = new Version("9.4");

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
        /// The version of PostgreSQL to target.
        /// </summary>
        [NotNull] readonly Version _compatibility;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlDateAddTranslator"/> class.
        /// </summary>
        /// <param name="compatibility">The version of PostgreSQL to target.</param>
        public NpgsqlDateAddTranslator([NotNull] Version compatibility)
            => _compatibility = Check.NotNull(compatibility, nameof(compatibility));

        /// <inheritdoc />
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (_compatibility < MinimumSupportedVersion)
                return null;

            if (!MethodInfoDatePartMapping.TryGetValue(methodCallExpression.Method, out var datePart))
                return null;

            // Ideally we would use Expression.Add() to have a simple plus-sign expression generated for us.
            // But to represent an interval we'd need to use a TimeSpan, which does not represent months/years.
            // We could use the provider-specific NpgsqlTimeSpan but it's not currently supported and is somewhat
            // ugly, so we just generate the expression ourselves with CustomBinaryExpression

            if (!(methodCallExpression.Object is Expression instance))
                return null;

            if (methodCallExpression.Arguments.Count < 1)
                return null;

            if (!(methodCallExpression.Arguments[0] is Expression amountToAdd))
                return null;

            if (amountToAdd is ConstantExpression constantExpression &&
                constantExpression.Type == typeof(double) &&
                ((double)constantExpression.Value >= int.MaxValue ||
                 (double)constantExpression.Value <= int.MinValue))
                return null;

            return
                new CustomBinaryExpression(
                    instance,
                    new PgFunctionExpression(
                        "MAKE_INTERVAL",
                        typeof(TimeSpan),
                        new Dictionary<string, Expression> { [datePart] = amountToAdd }),
                    "+",
                    methodCallExpression.Type);
        }
    }
}
