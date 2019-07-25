using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline;

// ReSharper disable once CheckNamespace
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
        public NpgsqlNodaTimeMethodCallTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory)
        {
            Translators = new IMethodCallTranslator[]
            {
                new NpgsqlNodaTimeMethodCallTranslator((NpgsqlSqlExpressionFactory)sqlExpressionFactory),
            };
        }

        public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
    }

    /// <summary>
    /// Provides translation services for NodaTime method calls.
    /// </summary>
    public class NpgsqlNodaTimeMethodCallTranslator : IMethodCallTranslator
    {
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        /// The static method info for <see cref="T:SystemClock.GetCurrentInstant()"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo GetCurrentInstant =
            typeof(SystemClock).GetRuntimeMethod(nameof(SystemClock.GetCurrentInstant), Type.EmptyTypes);

        /// <summary>
        /// The mapping of supported method translations.
        /// </summary>
        [NotNull] static readonly Dictionary<MethodInfo, string> PeriodMethodMap = new Dictionary<MethodInfo, string>
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

        public NpgsqlNodaTimeMethodCallTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

#pragma warning disable EF1001
        /// <inheritdoc />
        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (method == GetCurrentInstant)
            {
                return _sqlExpressionFactory.AtTimeZone(
                    _sqlExpressionFactory.Function("NOW", Array.Empty<SqlExpression>(), method.ReturnType),
                    _sqlExpressionFactory.Constant("UTC"),
                    method.ReturnType);
            }

            var declaringType = method.DeclaringType;
            if (declaringType == typeof(Period))
            {
                return PeriodMethodMap.TryGetValue(method, out var datePart)
                    ? new PgFunctionExpression(
                        "MAKE_INTERVAL",
                        new Dictionary<string, SqlExpression> { [datePart] = _sqlExpressionFactory.Constant(arguments[0]) },
                        typeof(Period))
                    : null;
            }
            return null;
        }
#pragma warning restore EF1001
    }
}
