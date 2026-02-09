using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlDateTimeMethodTranslator(
    IRelationalTypeMappingSource typeMappingSource,
    NpgsqlSqlExpressionFactory sqlExpressionFactory)
    : IMethodCallTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource = typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory = sqlExpressionFactory;
    private readonly RelationalTypeMapping _timestampMapping = typeMappingSource.FindMapping(typeof(DateTime), "timestamp without time zone")!;
    private readonly RelationalTypeMapping _timestampTzMapping = typeMappingSource.FindMapping(typeof(DateTime), "timestamp with time zone")!;
    private readonly RelationalTypeMapping _intervalMapping = typeMappingSource.FindMapping(typeof(TimeSpan), "interval")!;
    private readonly RelationalTypeMapping _textMapping = typeMappingSource.FindMapping("text")!;

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => TranslateDateTime(instance, method, arguments)
           ?? TranslateDateOnly(instance, method, arguments)
           ?? TranslateTimeOnly(instance, method, arguments)
           ?? TranslateTimeZoneInfo(method, arguments)
           ?? TranslateDatePart(instance, method, arguments);

    private SqlExpression? TranslateDatePart(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (instance is null
            || (method.DeclaringType != typeof(DateTime) && method.DeclaringType != typeof(DateTimeOffset) && method.DeclaringType != typeof(TimeOnly)))
        {
            return null;
        }

        var datePart = method.Name switch
        {
            nameof(DateTime.AddYears) => "years",
            nameof(DateTime.AddMonths) => "months",
            nameof(DateTime.AddDays) => "days",
            nameof(DateTime.AddHours) => "hours",
            nameof(DateTime.AddMinutes) => "mins",
            nameof(DateTime.AddSeconds) => "secs",
            _ => null
        };

        return datePart is not null && CreateIntervalExpression(arguments[0], datePart) is SqlExpression interval
            ? _sqlExpressionFactory.Add(instance, interval, instance.TypeMapping)
            : null;
    }

    private SqlExpression? TranslateDateTime(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (instance is null)
        {
            if (method.DeclaringType == typeof(DateTime) && method.Name == nameof(DateTime.SpecifyKind))
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

            if (method.DeclaringType == typeof(NpgsqlDbFunctionsExtensions)
                && method.Name == nameof(NpgsqlDbFunctionsExtensions.Distance)
                && arguments is [_, var dateTime1, var dateTime2]
                && dateTime1.Type == typeof(DateTime))
            {
                return _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.Distance, dateTime1, dateTime2);
            }
        }
        else
        {
            if (method.DeclaringType == typeof(DateTime) && method.Name == nameof(DateTime.ToUniversalTime))
            {
                return _sqlExpressionFactory.Convert(instance, method.ReturnType, _timestampTzMapping);
            }

            if (method.DeclaringType == typeof(DateTime) && method.Name == nameof(DateTime.ToLocalTime))
            {
                return _sqlExpressionFactory.Convert(instance, method.ReturnType, _timestampMapping);
            }
        }

        return null;
    }

    private SqlExpression? TranslateDateOnly(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (instance is null)
        {
            if (method.DeclaringType == typeof(DateOnly) && method.Name == nameof(DateOnly.FromDateTime))
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

            if (method.DeclaringType == typeof(NpgsqlDbFunctionsExtensions)
                && method.Name == nameof(NpgsqlDbFunctionsExtensions.Distance)
                && arguments is [_, var dateOnly1, var dateOnly2]
                && dateOnly1.Type == typeof(DateOnly))
            {
                return _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.Distance, dateOnly1, dateOnly2);
            }

            if (method.DeclaringType == typeof(DateOnly) && method.Name == nameof(DateOnly.FromDayNumber))
            {
                // We use fragment rather than a DateOnly constant, since 0001-01-01 gets rendered as -infinity by default.
                // TODO: Set the right type/type mapping after https://github.com/dotnet/efcore/pull/34995 is merged
                return new SqlBinaryExpression(
                    ExpressionType.Add,
                    _sqlExpressionFactory.Fragment("DATE '0001-01-01'"),
                    arguments[0],
                    typeof(DateOnly),
                    _typeMappingSource.FindMapping(typeof(DateOnly)));
            }
        }
        else
        {
            if (method.DeclaringType == typeof(DateOnly) && method.Name == nameof(DateOnly.ToDateTime))
            {
                return new SqlBinaryExpression(
                    ExpressionType.Add,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(instance),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
                    typeof(DateTime),
                    _timestampMapping);
            }

            // In PG, date + int = date (int interpreted as days)
            if (method.DeclaringType == typeof(DateOnly) && method.Name == nameof(DateOnly.AddDays))
            {
                return _sqlExpressionFactory.Add(instance, arguments[0]);
            }

            // For months and years, date + interval yields a timestamp (since interval could have a time component), so we need to cast
            // the results back to date
            if (method.DeclaringType == typeof(DateOnly) && method.Name == nameof(DateOnly.AddMonths)
                && CreateIntervalExpression(arguments[0], "months") is SqlExpression interval1)
            {
                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Add(instance, interval1, instance.TypeMapping), typeof(DateOnly));
            }

            if (method.DeclaringType == typeof(DateOnly) && method.Name == nameof(DateOnly.AddYears)
                && CreateIntervalExpression(arguments[0], "years") is SqlExpression interval2)
            {
                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Add(instance, interval2, instance.TypeMapping), typeof(DateOnly));
            }
        }

        return null;
    }

    private SqlExpression? TranslateTimeOnly(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (method.DeclaringType == typeof(TimeOnly) && method.Name == nameof(TimeOnly.FromDateTime))
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

        if (method.DeclaringType == typeof(TimeOnly) && method.Name == nameof(TimeOnly.FromTimeSpan))
        {
            return _sqlExpressionFactory.Convert(arguments[0], typeof(TimeOnly), _typeMappingSource.FindMapping(typeof(TimeOnly)));
        }

        if (instance is not null)
        {
            if (method.DeclaringType == typeof(TimeOnly) && method.Name == nameof(TimeOnly.ToTimeSpan))
            {
                return _sqlExpressionFactory.Convert(instance, typeof(TimeSpan), _typeMappingSource.FindMapping(typeof(TimeSpan)));
            }

            if (method.DeclaringType == typeof(TimeOnly) && method.Name == nameof(TimeOnly.IsBetween))
            {
                return _sqlExpressionFactory.And(
                    _sqlExpressionFactory.GreaterThanOrEqual(instance, arguments[0]),
                    _sqlExpressionFactory.LessThan(instance, arguments[1]));
            }

            if (method.DeclaringType == typeof(TimeOnly) && method.Name == nameof(TimeOnly.Add)
                && arguments is [var timeSpan]
                && timeSpan.Type == typeof(TimeSpan))
            {
                return _sqlExpressionFactory.Add(instance, timeSpan);
            }
        }

        return null;
    }

    private SqlExpression? TranslateTimeZoneInfo(
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (method.DeclaringType == typeof(TimeZoneInfo) && method.Name == nameof(TimeZoneInfo.ConvertTimeBySystemTimeZoneId)
            && arguments is [var convertDateTime, var timeZoneId]
            && convertDateTime.Type == typeof(DateTime))
        {
            var typeMapping = convertDateTime.TypeMapping;
            if (typeMapping is null
                || (typeMapping.StoreType != "timestamp with time zone" && typeMapping.StoreType != "timestamptz"))
            {
                throw new InvalidOperationException(
                    "TimeZoneInfo.ConvertTimeBySystemTimeZoneId is only supported on columns with type 'timestamp with time zone'");
            }

            return _sqlExpressionFactory.AtTimeZone(convertDateTime, timeZoneId, typeof(DateTime), _timestampMapping);
        }

        if (method.DeclaringType == typeof(TimeZoneInfo) && method.Name == nameof(TimeZoneInfo.ConvertTimeToUtc)
            && arguments is [var utcDateTime]
            && utcDateTime.Type == typeof(DateTime))
        {
            var typeMapping = utcDateTime.TypeMapping;
            if (typeMapping is null
                || (typeMapping.StoreType != "timestamp without time zone" && typeMapping.StoreType != "timestamp"))
            {
                throw new InvalidOperationException(
                    "TimeZoneInfo.ConvertTimeToUtc) is only supported on columns with type 'timestamp without time zone'");
            }

            return _sqlExpressionFactory.Convert(utcDateTime, utcDateTime.Type, _timestampTzMapping);
        }

        return null;
    }

    private SqlExpression? CreateIntervalExpression(SqlExpression intervalNum, string datePart)
    {
        // Note: ideally we'd simply generate a PostgreSQL interval expression, but the .NET mapping of that is TimeSpan,
        // which does not work for months, years, etc. So we generate special fragments instead.
        if (intervalNum is SqlConstantExpression constantExpression)
        {
            // We generate constant intervals as INTERVAL '1 days'
            if (constantExpression.Type == typeof(double)
                && ((double)constantExpression.Value! >= int.MaxValue || (double)constantExpression.Value <= int.MinValue))
            {
                return null;
            }

            return _sqlExpressionFactory.Fragment(FormattableString.Invariant($"INTERVAL '{constantExpression.Value} {datePart}'"));
        }

        // For non-constants, we can't parameterize INTERVAL '1 days'. Instead, we use CAST($1 || ' days' AS interval).
        // Note that a make_interval() function also exists, but accepts only int (for all fields except for
        // seconds), so we don't use it.
        // Note: we instantiate SqlBinaryExpression manually rather than via sqlExpressionFactory because
        // of the non-standard Add expression (concatenate int with text)
        return _sqlExpressionFactory.Convert(
            new SqlBinaryExpression(
                ExpressionType.Add,
                _sqlExpressionFactory.Convert(intervalNum, typeof(string), _textMapping),
                _sqlExpressionFactory.Constant(' ' + datePart, _textMapping),
                typeof(string),
                _textMapping),
            typeof(TimeSpan),
            _intervalMapping);
    }
}
