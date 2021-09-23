using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal
{
    /// <summary>
    /// Provides translation services for <see cref="NodaTime"/> members.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-datetime.html
    /// </remarks>
    public class NpgsqlNodaTimeMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        public NpgsqlNodaTimeMethodCallTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            Translators = new IMethodCallTranslator[]
            {
                new NpgsqlNodaTimeMethodCallTranslator(typeMappingSource, (NpgsqlSqlExpressionFactory)sqlExpressionFactory),
            };
        }

        public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
    }

    public class NpgsqlNodaTimeMethodCallTranslator : IMethodCallTranslator
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        private static readonly MethodInfo SystemClock_GetCurrentInstant =
            typeof(SystemClock).GetRuntimeMethod(nameof(SystemClock.GetCurrentInstant), Type.EmptyTypes)!;
        private static readonly MethodInfo Instant_InUtc =
            typeof(Instant).GetRuntimeMethod(nameof(Instant.InUtc), Type.EmptyTypes)!;

        private static readonly MethodInfo Period_FromYears   = typeof(Period).GetRuntimeMethod(nameof(Period.FromYears),        new[] { typeof(int) })!;
        private static readonly MethodInfo Period_FromMonths  = typeof(Period).GetRuntimeMethod(nameof(Period.FromMonths),       new[] { typeof(int) })!;
        private static readonly MethodInfo Period_FromWeeks   = typeof(Period).GetRuntimeMethod(nameof(Period.FromWeeks),        new[] { typeof(int) })!;
        private static readonly MethodInfo Period_FromDays    = typeof(Period).GetRuntimeMethod(nameof(Period.FromDays),         new[] { typeof(int) })!;
        private static readonly MethodInfo Period_FromHours   = typeof(Period).GetRuntimeMethod(nameof(Period.FromHours),        new[] { typeof(long) })!;
        private static readonly MethodInfo Period_FromMinutes = typeof(Period).GetRuntimeMethod(nameof(Period.FromMinutes),      new[] { typeof(long) })!;
        private static readonly MethodInfo Period_FromSeconds = typeof(Period).GetRuntimeMethod(nameof(Period.FromSeconds),      new[] { typeof(long) })!;

        /// <summary>
        /// The mapping of supported method translations.
        /// </summary>
        private static readonly Dictionary<MethodInfo, string> PeriodMethodMap = new()
        {
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromYears),        new[] { typeof(int) })!,  "years" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromMonths),       new[] { typeof(int) })!,  "months" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromWeeks),        new[] { typeof(int) })!,  "weeks" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromDays),         new[] { typeof(int) })!,  "days" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromHours),        new[] { typeof(long) })!, "hours" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromMinutes),      new[] { typeof(long) })!, "mins" },
            { typeof(Period).GetRuntimeMethod(nameof(Period.FromSeconds),      new[] { typeof(long) })!, "secs" },
        };

        private static readonly bool[][] TrueArrays =
        {
            Array.Empty<bool>(),
            new[] { true },
            new[] { true, true },
        };

        public NpgsqlNodaTimeMethodCallTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            NpgsqlSqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

#pragma warning disable EF1001
        /// <inheritdoc />
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (method == SystemClock_GetCurrentInstant)
            {
                return NpgsqlNodaTimeTypeMappingSourcePlugin.LegacyTimestampBehavior
                    ? _sqlExpressionFactory.AtTimeZone(
                        _sqlExpressionFactory.Function(
                            "NOW",
                            Array.Empty<SqlExpression>(),
                            nullable: false,
                            argumentsPropagateNullability: Array.Empty<bool>(),
                            method.ReturnType),
                        _sqlExpressionFactory.Constant("UTC"),
                        method.ReturnType)
                    : _sqlExpressionFactory.Function(
                        "NOW",
                        Array.Empty<SqlExpression>(),
                        nullable: false,
                        argumentsPropagateNullability: Array.Empty<bool>(),
                        method.ReturnType,
                        _typeMappingSource.FindMapping(typeof(Instant), "timestamp with time zone"));
            }

            if (method == Instant_InUtc)
            {
                return instance;
            }

            var declaringType = method.DeclaringType;
            if (declaringType == typeof(Period))
            {
                if (method == Period_FromYears)
                {
                    return IntervalPart("years", arguments[0]);
                }
                if (method == Period_FromMonths)
                {
                    return IntervalPart("months", arguments[0]);
                }
                if (method == Period_FromWeeks)
                {
                    return IntervalPart("weeks", arguments[0]);
                }
                if (method == Period_FromDays)
                {
                    return IntervalPart("days", arguments[0]);
                }
                if (method == Period_FromHours)
                {
                    return IntervalPartOverBigInt("hours", arguments[0]);
                }
                if (method == Period_FromMinutes)
                {
                    return IntervalPartOverBigInt("mins", arguments[0]);
                }
                if (method == Period_FromSeconds)
                {
                    return IntervalPart("secs", _sqlExpressionFactory.Convert(arguments[0], typeof(double), _typeMappingSource.FindMapping(typeof(double))));
                }

                static PostgresFunctionExpression IntervalPart(string datePart, SqlExpression parameter)
                    => PostgresFunctionExpression.CreateWithNamedArguments(
                        "MAKE_INTERVAL",
                        new[] { parameter },
                        new[] { datePart },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        builtIn: true,
                        typeof(Period),
                        typeMapping: null);

                PostgresFunctionExpression IntervalPartOverBigInt(string datePart, SqlExpression parameter)
                {
                    parameter = _sqlExpressionFactory.ApplyDefaultTypeMapping(parameter);

                    // NodaTime Period.FromHours/Minutes/Seconds accept a long parameter, but PG interval_part accepts an int.
                    // If the parameter happens to be an int cast up to a long, just unwrap it, otherwise downcast from bigint to int
                    // (this will throw on the PG side if the bigint is out of int range)
                    if (parameter is SqlUnaryExpression { OperatorType: ExpressionType.Convert } convertExpression
                        && convertExpression.TypeMapping!.StoreType == "bigint"
                        && convertExpression.Operand.TypeMapping!.StoreType == "integer")
                    {
                        return IntervalPart(datePart, convertExpression.Operand);
                    }

                    return IntervalPart(datePart, _sqlExpressionFactory.Convert(parameter, typeof(int), _typeMappingSource.FindMapping(typeof(int))));
                }
            }
            return null;
        }
#pragma warning restore EF1001
    }
}
