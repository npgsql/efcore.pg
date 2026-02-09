using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;

/// <summary>
///     Provides translation services for <see cref="NodaTime" /> members.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/static/functions-datetime.html
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
        Translators =
        [
            new NpgsqlNodaTimeMethodCallTranslator(typeMappingSource, (NpgsqlSqlExpressionFactory)sqlExpressionFactory)
        ];
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
public class NpgsqlNodaTimeMethodCallTranslator(
    IRelationalTypeMappingSource typeMappingSource,
    NpgsqlSqlExpressionFactory sqlExpressionFactory)
    : IMethodCallTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource = typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory = sqlExpressionFactory;

    private static readonly bool[][] TrueArrays = [[], [true], [true, true]];

#pragma warning disable EF1001
    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var translated =
            TranslateInstant(instance, method, arguments)
            ?? TranslateZonedDateTime(instance, method, arguments)
            ?? TranslateLocalDateTime(instance, method, arguments)
            ?? TranslateLocalDate(instance, method, arguments)
            ?? TranslatePeriod(instance, method, arguments, logger)
            ?? TranslateInterval(instance, method, arguments, logger)
            ?? TranslateDateInterval(instance, method, arguments, logger);

        if (translated is not null)
        {
            return translated;
        }

        if (method.DeclaringType == typeof(IDateTimeZoneProvider) && method.Name == "get_Item" && instance is PendingDateTimeZoneProviderExpression)
        {
            // We're translating an expression such as 'DateTimeZoneProviders.Tzdb["Europe/Berlin"]'.
            // Note that the .NET type of that expression is DateTimeZone, but we just return the string ID for the time zone.
            return arguments[0];
        }

        return null;
    }

    private SqlExpression? TranslateInstant(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            nameof(SystemClock.GetCurrentInstant) when method.DeclaringType == typeof(SystemClock)
                => NpgsqlNodaTimeTypeMappingSourcePlugin.LegacyTimestampBehavior
                    ? _sqlExpressionFactory.AtTimeZone(
                        _sqlExpressionFactory.Function(
                            "NOW",
                            [],
                            nullable: false,
                            argumentsPropagateNullability: [],
                            method.ReturnType),
                        _sqlExpressionFactory.Constant("UTC"),
                        method.ReturnType)
                    : _sqlExpressionFactory.Function(
                        "NOW",
                        [],
                        nullable: false,
                        argumentsPropagateNullability: [],
                        method.ReturnType,
                        _typeMappingSource.FindMapping(typeof(Instant), "timestamp with time zone")),

            // Instant -> ZonedDateTime is a no-op (different types in .NET but both mapped to timestamptz in PG)
            nameof(Instant.InUtc) when method.DeclaringType == typeof(Instant)
                => instance,

            // When InZone is called, we have a mismatch: on the .NET NodaTime side, we have a ZonedDateTime; but on the PostgreSQL side,
            // the AT TIME ZONE expression returns a 'timestamp without time zone' (when applied to a 'timestamp with time zone', which is
            // what ZonedDateTime is mapped to).
            nameof(Instant.InZone) when method.DeclaringType == typeof(Instant)
                => new PendingZonedDateTimeExpression(instance!, arguments[0]),

            nameof(Instant.ToDateTimeUtc) when method.DeclaringType == typeof(Instant)
                => _sqlExpressionFactory.Convert(
                    instance!,
                    typeof(DateTime),
                    _typeMappingSource.FindMapping(typeof(DateTime), "timestamp with time zone")),

            nameof(NpgsqlNodaTimeDbFunctionsExtensions.Distance)
                when method.DeclaringType == typeof(NpgsqlNodaTimeDbFunctionsExtensions)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.Distance, arguments[1], arguments[2]),

            _ => null
        };

    private SqlExpression? TranslateZonedDateTime(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            // We get here with the expression localDateTime.InZoneLeniently(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).ToInstant()
            nameof(ZonedDateTime.ToInstant) when method.DeclaringType == typeof(ZonedDateTime)
                => instance is PendingZonedDateTimeExpression pendingZonedDateTime
                    ? _sqlExpressionFactory.AtTimeZone(
                        pendingZonedDateTime.Operand,
                        pendingZonedDateTime.TimeZoneId,
                        typeof(Instant),
                        _typeMappingSource.FindMapping(typeof(Instant)))
                    // Otherwise, ZonedDateTime -> ToInstant is a no-op (different types in .NET but both mapped to timestamptz in PG)
                    : instance,

            nameof(NpgsqlNodaTimeDbFunctionsExtensions.Distance)
                when method.DeclaringType == typeof(NpgsqlNodaTimeDbFunctionsExtensions)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.Distance, arguments[1], arguments[2]),

            _ => null
        };

    private SqlExpression? TranslateLocalDateTime(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            nameof(LocalDateTime.InZoneLeniently) when method.DeclaringType == typeof(LocalDateTime)
                => new PendingZonedDateTimeExpression(instance!, arguments[0]),

            nameof(NpgsqlNodaTimeDbFunctionsExtensions.Distance)
                when method.DeclaringType == typeof(NpgsqlNodaTimeDbFunctionsExtensions)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.Distance, arguments[1], arguments[2]),

            _ => null
        };

    private SqlExpression? TranslateLocalDate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (method.DeclaringType == typeof(NpgsqlNodaTimeDbFunctionsExtensions) && method.Name == nameof(NpgsqlNodaTimeDbFunctionsExtensions.Distance))
        {
            return _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.Distance, arguments[1], arguments[2]);
        }

        if (method.DeclaringType == typeof(LocalDate))
        {
            return method.Name switch
            {
                nameof(LocalDate.At) => new SqlBinaryExpression(
                    ExpressionType.Add,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(instance!),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
                    typeof(LocalDateTime),
                    _typeMappingSource.FindMapping(typeof(LocalDateTime))),

                nameof(LocalDate.AtMidnight) => new SqlBinaryExpression(
                    ExpressionType.Add,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(instance!),
                    new SqlConstantExpression(new LocalTime(0, 0, 0), _typeMappingSource.FindMapping(typeof(LocalTime))),
                    typeof(LocalDateTime),
                    _typeMappingSource.FindMapping(typeof(LocalDateTime))),

                _ => null
            };
        }
        return null;
    }

    private SqlExpression? TranslatePeriod(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(Period))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(Period.FromYears) => IntervalPart("years", arguments[0]),
            nameof(Period.FromMonths) => IntervalPart("months", arguments[0]),
            nameof(Period.FromWeeks) => IntervalPart("weeks", arguments[0]),
            nameof(Period.FromDays) => IntervalPart("days", arguments[0]),
            nameof(Period.FromHours) => IntervalPartOverBigInt("hours", arguments[0]),
            nameof(Period.FromMinutes) => IntervalPartOverBigInt("mins", arguments[0]),
            nameof(Period.FromSeconds) => IntervalPart(
                "secs", _sqlExpressionFactory.Convert(arguments[0], typeof(double), _typeMappingSource.FindMapping(typeof(double)))),
            _ => null
        };

        static PgFunctionExpression IntervalPart(string datePart, SqlExpression parameter)
            => PgFunctionExpression.CreateWithNamedArguments(
                "make_interval",
                [parameter],
                [datePart],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                builtIn: true,
                typeof(Period),
                typeMapping: null);

        PgFunctionExpression IntervalPartOverBigInt(string datePart, SqlExpression parameter)
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

    private SqlExpression? TranslateInterval(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType == typeof(Interval) && method.Name == nameof(Interval.Contains))
        {
            return _sqlExpressionFactory.Contains(instance!, arguments[0]);
        }

        return null;
    }

    private SqlExpression? TranslateDateInterval(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType == typeof(DateInterval) && method.Name == nameof(DateInterval.Contains))
        {
            return _sqlExpressionFactory.Contains(instance!, arguments[0]);
        }

        if (method.DeclaringType == typeof(DateInterval) && method.Name == nameof(DateInterval.Intersection))
        {
            return _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.RangeIntersect, instance!, arguments[0]);
        }

        if (method.DeclaringType == typeof(DateInterval) && method.Name == nameof(DateInterval.Union))
        {
            return _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.RangeUnion, instance!, arguments[0]);
        }

        return null;
    }
#pragma warning restore EF1001
}
