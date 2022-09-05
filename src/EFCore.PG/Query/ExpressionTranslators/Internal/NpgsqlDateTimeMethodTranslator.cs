using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlDateTimeMethodTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, string> MethodInfoDatePartMapping = new()
    {
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), new[] { typeof(int) })!, "years" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), new[] { typeof(int) })!, "months" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), new[] { typeof(double) })!, "days" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), new[] { typeof(double) })!, "hours" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), new[] { typeof(double) })!, "mins" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), new[] { typeof(double) })!, "secs" },
        //{ typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), new[] { typeof(double) })!, "milliseconds" },

        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddYears), new[] { typeof(int) })!, "years" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMonths), new[] { typeof(int) })!, "months" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddDays), new[] { typeof(double) })!, "days" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddHours), new[] { typeof(double) })!, "hours" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMinutes), new[] { typeof(double) })!, "mins" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddSeconds), new[] { typeof(double) })!, "secs" },
        //{ typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMilliseconds), new[] { typeof(double) })!, "milliseconds" }

        { typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddYears), new[] { typeof(int) })!, "years" },
        { typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddMonths), new[] { typeof(int) })!, "months" },
        { typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddDays), new[] { typeof(int) })!, "days" },

        { typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.AddHours), new[] { typeof(int) })!, "hours" },
        { typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.AddMinutes), new[] { typeof(int) })!, "mins" },
    };

    // ReSharper disable InconsistentNaming
    private static readonly MethodInfo DateTime_ToUniversalTime
        = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.ToUniversalTime), Array.Empty<Type>())!;
    private static readonly MethodInfo DateTime_ToLocalTime
        = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.ToLocalTime), Array.Empty<Type>())!;
    private static readonly MethodInfo DateTime_SpecifyKind
        = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.SpecifyKind), new[] { typeof(DateTime), typeof(DateTimeKind) })!;

    private static readonly MethodInfo DateOnly_FromDateTime
        = typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.FromDateTime), new[] { typeof(DateTime) })!;
    private static readonly MethodInfo DateOnly_ToDateTime
        = typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.ToDateTime), new[] { typeof(TimeOnly) })!;

    private static readonly MethodInfo TimeOnly_FromDateTime
        = typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.FromDateTime), new[] { typeof(DateTime) })!;
    private static readonly MethodInfo TimeOnly_FromTimeSpan
        = typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.FromTimeSpan), new[] { typeof(TimeSpan) })!;
    private static readonly MethodInfo TimeOnly_ToTimeSpan
        = typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.ToTimeSpan), Type.EmptyTypes)!;
    private static readonly MethodInfo TimeOnly_IsBetween
        = typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.IsBetween), new[] { typeof(TimeOnly), typeof(TimeOnly) })!;
    private static readonly MethodInfo TimeOnly_Add_TimeSpan
        = typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.Add), new[] { typeof(TimeSpan) })!;

    private static readonly MethodInfo TimeZoneInfo_ConvertTimeBySystemTimeZoneId
        = typeof(TimeZoneInfo).GetRuntimeMethod(
            nameof(TimeZoneInfo.ConvertTimeBySystemTimeZoneId), new[] { typeof(DateTime), typeof(string) })!;

    private static readonly MethodInfo TimeZoneInfo_ConvertTimeToUtc
        = typeof(TimeZoneInfo).GetRuntimeMethod(nameof(TimeZoneInfo.ConvertTimeToUtc), new[] { typeof(DateTime) })!;
    // ReSharper restore InconsistentNaming

    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _timestampMapping;
    private readonly RelationalTypeMapping _timestampTzMapping;
    private readonly RelationalTypeMapping _intervalMapping;
    private readonly RelationalTypeMapping _textMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlDateTimeMethodTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory sqlExpressionFactory)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
        _timestampMapping = typeMappingSource.FindMapping("timestamp without time zone")!;
        _timestampTzMapping = typeMappingSource.FindMapping("timestamp with time zone")!;
        _intervalMapping = typeMappingSource.FindMapping("interval")!;
        _textMapping = typeMappingSource.FindMapping("text")!;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => TranslateDatePart(instance, method, arguments)
            ?? TranslateDateTime(instance, method, arguments)
            ?? TranslateDateOnly(instance, method, arguments)
            ?? TranslateTimeOnly(instance, method, arguments)
            ?? TranslateTimeZoneInfo(method, arguments);

    private SqlExpression? TranslateDatePart(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (instance is null || !MethodInfoDatePartMapping.TryGetValue(method, out var datePart))
        {
            return null;
        }

        if (arguments[0] is not { } interval)
        {
            return null;
        }

        // Note: ideally we'd simply generate a PostgreSQL interval expression, but the .NET mapping of that is TimeSpan,
        // which does not work for months, years, etc. So we generate special fragments instead.
        if (interval is SqlConstantExpression constantExpression)
        {
            // We generate constant intervals as INTERVAL '1 days'
            if (constantExpression.Type == typeof(double) &&
                ((double)constantExpression.Value! >= int.MaxValue ||
                    (double)constantExpression.Value <= int.MinValue))
            {
                return null;
            }

            interval = _sqlExpressionFactory.Fragment(FormattableString.Invariant($"INTERVAL '{constantExpression.Value} {datePart}'"));
        }
        else
        {
            // For non-constants, we can't parameterize INTERVAL '1 days'. Instead, we use CAST($1 || ' days' AS interval).
            // Note that a make_interval() function also exists, but accepts only int (for all fields except for
            // seconds), so we don't use it.
            // Note: we instantiate SqlBinaryExpression manually rather than via sqlExpressionFactory because
            // of the non-standard Add expression (concatenate int with text)
            interval = _sqlExpressionFactory.Convert(
                new SqlBinaryExpression(
                    ExpressionType.Add,
                    _sqlExpressionFactory.Convert(interval, typeof(string), _textMapping),
                    _sqlExpressionFactory.Constant(' ' + datePart, _textMapping),
                    typeof(string),
                    _textMapping),
                typeof(TimeSpan),
                _intervalMapping);
        }

        return _sqlExpressionFactory.Add(instance, interval, instance.TypeMapping);
    }

    private SqlExpression? TranslateDateTime(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (instance is not null)
        {
            if (method == DateTime_ToUniversalTime)
            {
                return _sqlExpressionFactory.Convert(instance, method.ReturnType, _timestampTzMapping);
            }

            if (method == DateTime_ToLocalTime)
            {
                return _sqlExpressionFactory.Convert(instance, method.ReturnType, _timestampMapping);
            }
        }

        if (method == DateTime_SpecifyKind)
        {
            if (arguments[1] is not SqlConstantExpression { Value: DateTimeKind kind })
            {
                throw new InvalidOperationException("Translating SpecifyKind is only supported with a constant Kind argument");
            }

            var typeMapping = arguments[0].TypeMapping;

            if (typeMapping is not NpgsqlTimestampTypeMapping and not NpgsqlTimestampTzTypeMapping)
            {
                throw new InvalidOperationException("Translating SpecifyKind is only supported on timestamp/timestamptz columns");
            }

            if (kind == DateTimeKind.Utc)
            {
                return typeMapping is NpgsqlTimestampTypeMapping
                    ? _sqlExpressionFactory.AtUtc(arguments[0])
                    : arguments[0];
            }

            if (kind is DateTimeKind.Unspecified or DateTimeKind.Local)
            {
                return typeMapping is NpgsqlTimestampTzTypeMapping
                    ? _sqlExpressionFactory.AtUtc(arguments[0])
                    : arguments[0];
            }
        }

        return null;
    }

    private SqlExpression? TranslateDateOnly(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (method == DateOnly_FromDateTime)
        {
            // Note: converting timestamptz to date performs a timezone conversion, which is not what .NET DateOnly.FromDateTime does.
            // So if our operand is a timestamptz, we first change the type to timestamp with AT TIME ZONE 'UTC' (returns the same value
            // but as a timestamptz).
            // If our operand is already timestamp, no need to do anything. We throw for anything else to avoid accidentally applying
            // AT TIME ZONE to a non-timestamptz, which would do a timezone conversion
            var dateTime = arguments[0].TypeMapping switch
            {
                NpgsqlTimestampTypeMapping => arguments[0],
                NpgsqlTimestampTzTypeMapping => _sqlExpressionFactory.AtUtc(arguments[0]),
                _ => throw new NotSupportedException("Can only apply TimeOnly.FromDateTime on a timestamp or timestamptz column")
            };

            return _sqlExpressionFactory.Convert(dateTime, typeof(DateOnly), _typeMappingSource.FindMapping(typeof(DateOnly)));
        }

        if (instance is not null)
        {
            if (method == DateOnly_ToDateTime)
            {
                return new SqlBinaryExpression(
                    ExpressionType.Add,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(instance),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
                    typeof(DateTime),
                    _timestampMapping);
            }
        }

        return null;
    }


    private SqlExpression? TranslateTimeOnly(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (method == TimeOnly_FromDateTime)
        {
            // Note: converting timestamptz to time performs a timezone conversion, which is not what .NET TimeOnly.FromDateTime does.
            // So if our operand is a timestamptz, we first change the type to timestamp with AT TIME ZONE 'UTC' (returns the same value
            // but as a timestamptz).
            // If our operand is already timestamp, no need to do anything. We throw for anything else to avoid accidentally applying
            // AT TIME ZONE to a non-timestamptz, which would do a timezone conversion
            var dateTime = arguments[0].TypeMapping switch
            {
                NpgsqlTimestampTypeMapping => arguments[0],
                NpgsqlTimestampTzTypeMapping => _sqlExpressionFactory.AtUtc(arguments[0]),
                _ => throw new NotSupportedException("Can only apply TimeOnly.FromDateTime on a timestamp or timestamptz column")
            };

            return _sqlExpressionFactory.Convert(
                dateTime,
                typeof(TimeOnly),
                _typeMappingSource.FindMapping(typeof(TimeOnly)));
        }

        if (method == TimeOnly_FromTimeSpan)
        {
            return _sqlExpressionFactory.Convert(arguments[0], typeof(TimeOnly), _typeMappingSource.FindMapping(typeof(TimeOnly)));
        }

        if (instance is not null)
        {
            if (method == TimeOnly_ToTimeSpan)
            {
                return _sqlExpressionFactory.Convert(instance, typeof(TimeSpan), _typeMappingSource.FindMapping(typeof(TimeSpan)));
            }

            if (method == TimeOnly_IsBetween)
            {
                return _sqlExpressionFactory.And(
                    _sqlExpressionFactory.GreaterThanOrEqual(instance, arguments[0]),
                    _sqlExpressionFactory.LessThan(instance, arguments[1]));
            }

            if (method == TimeOnly_Add_TimeSpan)
            {
                return _sqlExpressionFactory.Add(instance, arguments[0]);
            }
        }

        return null;
    }

    private SqlExpression? TranslateTimeZoneInfo(
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (method == TimeZoneInfo_ConvertTimeBySystemTimeZoneId)
        {
            var typeMapping = arguments[0].TypeMapping;
            if (typeMapping is null
                || (typeMapping.StoreType != "timestamp with time zone" && typeMapping.StoreType != "timestamptz"))
            {
                throw new InvalidOperationException(
                    "TimeZoneInfo.ConvertTimeBySystemTimeZoneId is only supported on columns with type 'timestamp with time zone'");
            }

            return _sqlExpressionFactory.AtTimeZone(arguments[0], arguments[1], typeof(DateTime), _timestampMapping);
        }

        if (method == TimeZoneInfo_ConvertTimeToUtc)
        {
            var typeMapping = arguments[0].TypeMapping;
            if (typeMapping is null
                || (typeMapping.StoreType != "timestamp without time zone" && typeMapping.StoreType != "timestamp"))
            {
                throw new InvalidOperationException(
                    "TimeZoneInfo.ConvertTimeToUtc) is only supported on columns with type 'timestamp without time zone'");
            }

            return _sqlExpressionFactory.Convert(arguments[0], arguments[0].Type, _timestampTzMapping);
        }

        return null;
    }
}
