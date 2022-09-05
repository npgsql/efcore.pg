using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;

/// <summary>
/// Provides translation services for <see cref="NodaTime"/> members.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/functions-datetime.html
/// </remarks>
public class NpgsqlNodaTimeMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlNodaTimeMethodCallTranslatorPlugin(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        Translators = new IMethodCallTranslator[]
        {
            new NpgsqlNodaTimeMethodCallTranslator(typeMappingSource, (NpgsqlSqlExpressionFactory)sqlExpressionFactory),
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlNodaTimeMethodCallTranslator : IMethodCallTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

    private static readonly MethodInfo SystemClock_GetCurrentInstant =
        typeof(SystemClock).GetRuntimeMethod(nameof(SystemClock.GetCurrentInstant), Type.EmptyTypes)!;
    private static readonly MethodInfo Instant_InUtc =
        typeof(Instant).GetRuntimeMethod(nameof(Instant.InUtc), Type.EmptyTypes)!;
    private static readonly MethodInfo Instant_InZone =
        typeof(Instant).GetRuntimeMethod(nameof(Instant.InZone), new[] { typeof(DateTimeZone)})!;
    private static readonly MethodInfo Instant_ToDateTimeUtc =
        typeof(Instant).GetRuntimeMethod(nameof(Instant.ToDateTimeUtc), Type.EmptyTypes)!;

    private static readonly MethodInfo ZonedDateTime_ToInstant =
        typeof(ZonedDateTime).GetRuntimeMethod(nameof(ZonedDateTime.ToInstant), Type.EmptyTypes)!;

    private static readonly MethodInfo LocalDateTime_InZoneLeniently =
        typeof(LocalDateTime).GetRuntimeMethod(nameof(LocalDateTime.InZoneLeniently), new[] { typeof(DateTimeZone) })!;

    private static readonly MethodInfo Period_FromYears   = typeof(Period).GetRuntimeMethod(nameof(Period.FromYears),        new[] { typeof(int) })!;
    private static readonly MethodInfo Period_FromMonths  = typeof(Period).GetRuntimeMethod(nameof(Period.FromMonths),       new[] { typeof(int) })!;
    private static readonly MethodInfo Period_FromWeeks   = typeof(Period).GetRuntimeMethod(nameof(Period.FromWeeks),        new[] { typeof(int) })!;
    private static readonly MethodInfo Period_FromDays    = typeof(Period).GetRuntimeMethod(nameof(Period.FromDays),         new[] { typeof(int) })!;
    private static readonly MethodInfo Period_FromHours   = typeof(Period).GetRuntimeMethod(nameof(Period.FromHours),        new[] { typeof(long) })!;
    private static readonly MethodInfo Period_FromMinutes = typeof(Period).GetRuntimeMethod(nameof(Period.FromMinutes),      new[] { typeof(long) })!;
    private static readonly MethodInfo Period_FromSeconds = typeof(Period).GetRuntimeMethod(nameof(Period.FromSeconds),      new[] { typeof(long) })!;

    private static readonly MethodInfo Interval_Contains
        = typeof(Interval).GetRuntimeMethod(nameof(Interval.Contains), new[] { typeof(Instant) })!;

    private static readonly MethodInfo DateInterval_Contains_LocalDate
        = typeof(DateInterval).GetRuntimeMethod(nameof(DateInterval.Contains), new[] { typeof(LocalDate) })!;
    private static readonly MethodInfo DateInterval_Contains_DateInterval
        = typeof(DateInterval).GetRuntimeMethod(nameof(DateInterval.Contains), new[] { typeof(DateInterval) })!;
    private static readonly MethodInfo DateInterval_Intersection
        = typeof(DateInterval).GetRuntimeMethod(nameof(DateInterval.Intersection), new[] { typeof(DateInterval) })!;
    private static readonly MethodInfo DateInterval_Union
        = typeof(DateInterval).GetRuntimeMethod(nameof(DateInterval.Union), new[] { typeof(DateInterval) })!;

    private static readonly MethodInfo IDateTimeZoneProvider_get_Item
        = typeof(IDateTimeZoneProvider).GetRuntimeMethod("get_Item", new[] { typeof(string) })!;

    private static readonly bool[][] TrueArrays =
    {
        Array.Empty<bool>(),
        new[] { true },
        new[] { true, true },
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
            // Instant -> ZonedDateTime is a no-op (different types in .NET but both mapped to timestamptz in PG)
            return instance;
        }

        if (method == ZonedDateTime_ToInstant)
        {
            // We get here with the expression localDateTime.InZoneLeniently(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).ToInstant()
            if (instance is PendingZonedDateTimeExpression pendingZonedDateTime)
            {
                return _sqlExpressionFactory.AtTimeZone(
                    pendingZonedDateTime.Operand,
                    pendingZonedDateTime.TimeZoneId,
                    typeof(Instant),
                    _typeMappingSource.FindMapping(typeof(Instant)));
            }

            // Otherwise, ZonedDateTime -> ToInstant is a no-op (different types in .NET but both mapped to timestamptz in PG)
            return instance;
        }

        if (method == IDateTimeZoneProvider_get_Item && instance is PendingDateTimeZoneProviderExpression)
        {
            // We're translating an expression such as 'DateTimeZoneProviders.Tzdb["Europe/Berlin"]'.
            // Note that the .NET ype of that expression is DateTimeZone, but we just return the string ID for the time zone.
            return arguments[0];
        }

        if (method == Instant_InZone)
        {
            return new PendingZonedDateTimeExpression(instance!, arguments[0]);
        }

        if (method == LocalDateTime_InZoneLeniently)
        {
            return new PendingZonedDateTimeExpression(instance!, arguments[0]);
        }

        if (method == Instant_ToDateTimeUtc)
        {
            return _sqlExpressionFactory.Convert(
                instance!,
                typeof(DateTime),
                _typeMappingSource.FindMapping(typeof(DateTime), "timestamp with time zone"));
        }

        var declaringType = method.DeclaringType;

        if (declaringType == typeof(Period))
        {
            return TranslatePeriod(instance, method, arguments, logger);
        }

        if (declaringType == typeof(Interval))
        {
            return TranslateInterval(instance, method, arguments, logger);
        }

        if (declaringType == typeof(DateInterval))
        {
            return TranslateDateInterval(instance, method, arguments, logger);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? TranslatePeriod(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
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
            return IntervalPart(
                "secs", _sqlExpressionFactory.Convert(arguments[0], typeof(double), _typeMappingSource.FindMapping(typeof(double))));
        }

        return null;

        static PostgresFunctionExpression IntervalPart(string datePart, SqlExpression parameter)
            => PostgresFunctionExpression.CreateWithNamedArguments(
                "make_interval",
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

            return IntervalPart(
                datePart, _sqlExpressionFactory.Convert(parameter, typeof(int), _typeMappingSource.FindMapping(typeof(int))));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? TranslateInterval(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null)
        {
            return null;
        }

        if (method == Interval_Contains)
        {
            return _sqlExpressionFactory.Contains(instance, arguments[0]);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? TranslateDateInterval(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null)
        {
            return null;
        }

        if (method == DateInterval_Contains_LocalDate
            || method == DateInterval_Contains_DateInterval)
        {
            return _sqlExpressionFactory.Contains(instance, arguments[0]);
        }

        if (method == DateInterval_Intersection)
        {
            return _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIntersect, instance, arguments[0]);
        }

        if (method == DateInterval_Union)
        {
            return _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeUnion, instance, arguments[0]);
        }

        return null;
    }
#pragma warning restore EF1001
}
