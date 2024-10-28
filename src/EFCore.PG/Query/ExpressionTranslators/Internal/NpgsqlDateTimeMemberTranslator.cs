using System.Diagnostics.CodeAnalysis;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Provides translation services for <see cref="DateTime" /> members.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/static/functions-datetime.html
/// </remarks>
public class NpgsqlDateTimeMemberTranslator : IMemberTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _timestampMapping;
    private readonly RelationalTypeMapping _timestampTzMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlDateTimeMemberTranslator(IRelationalTypeMappingSource typeMappingSource, NpgsqlSqlExpressionFactory sqlExpressionFactory)
    {
        _typeMappingSource = typeMappingSource;
        _timestampMapping = typeMappingSource.FindMapping(typeof(DateTime), "timestamp without time zone")!;
        _timestampTzMapping = typeMappingSource.FindMapping(typeof(DateTime), "timestamp with time zone")!;
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var declaringType = member.DeclaringType;

        if (declaringType != typeof(DateTime)
            && declaringType != typeof(DateTimeOffset)
            && declaringType != typeof(DateOnly)
            && declaringType != typeof(TimeOnly))
        {
            return null;
        }

        if (declaringType == typeof(DateTimeOffset)
            && instance is not null
            && TranslateDateTimeOffset(instance, member) is { } translated)
        {
            return translated;
        }

        if (declaringType == typeof(DateOnly) && TranslateDateOnly(instance, member) is { } translated2)
        {
            return translated2;
        }

        if (member.Name == nameof(DateTime.Date))
        {
            // Note that DateTime.Date returns a DateTime, not a DateOnly (introduced later); so we convert using date_trunc (which returns
            // a PG timestamp/timestamptz) rather than a conversion to PG date (compare with NodaTime where we want a LocalDate).

            // When given a timestamptz, date_trunc performs the truncation with respect to TimeZone; to avoid that, we use the overload
            // accepting a time zone, and pass UTC. For regular timestamp (or in legacy timestamp mode), we use the simpler overload without
            // a time zone.
            switch (instance)
            {
                case { TypeMapping: NpgsqlTimestampTypeMapping }:
                case { } when NpgsqlTypeMappingSource.LegacyTimestampBehavior:
                    return _sqlExpressionFactory.Function(
                        "date_trunc",
                        [_sqlExpressionFactory.Constant("day"), instance],
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        returnType,
                        instance.TypeMapping);

                case { TypeMapping: NpgsqlTimestampTzTypeMapping }:
                    return _sqlExpressionFactory.Function(
                        "date_trunc",
                        [_sqlExpressionFactory.Constant("day"), instance, _sqlExpressionFactory.Constant("UTC")],
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[3],
                        returnType,
                        instance.TypeMapping);

                // If DateTime.Date is invoked on a PostgreSQL date (or DateOnly, which can only be mapped to datE), simply no-op.
                case { TypeMapping: NpgsqlDateTimeDateTypeMapping }:
                case { Type: var type } when type == typeof(DateOnly):
                    return instance;

                default:
                    return null;
            }
        }

        return member.Name switch
        {
            // Legacy behavior
            nameof(DateTime.Now) when NpgsqlTypeMappingSource.LegacyTimestampBehavior
                => UtcNow(),
            nameof(DateTime.UtcNow) when NpgsqlTypeMappingSource.LegacyTimestampBehavior
                => _sqlExpressionFactory.AtUtc(UtcNow()), // Return a UTC timestamp, but as timestamp without time zone

            // We support getting a local DateTime via DateTime.Now (based on PG TimeZone), but there's no way to get a non-UTC
            // DateTimeOffset.
            nameof(DateTime.Now) => declaringType == typeof(DateTimeOffset)
                ? throw new InvalidOperationException("Cannot translate DateTimeOffset.Now - use UtcNow.")
                : LocalNow(),
            nameof(DateTime.UtcNow) => UtcNow(),

            nameof(DateTime.Today) => _sqlExpressionFactory.Function(
                "date_trunc",
                [_sqlExpressionFactory.Constant("day"), LocalNow()],
                nullable: false,
                argumentsPropagateNullability: FalseArrays[2],
                typeof(DateTime),
                _timestampMapping),

            nameof(DateTime.Year) => DatePart(instance!, "year"),
            nameof(DateTime.Month) => DatePart(instance!, "month"),
            nameof(DateTime.DayOfYear) => DatePart(instance!, "doy"),
            nameof(DateTime.Day) => DatePart(instance!, "day"),
            nameof(DateTime.Hour) => DatePart(instance!, "hour"),
            nameof(DateTime.Minute) => DatePart(instance!, "minute"),
            nameof(DateTime.Second) => DatePart(instance!, "second"),

            nameof(DateTime.Millisecond) => null, // Too annoying

            // .NET's DayOfWeek is an enum, but its int values happen to correspond to PostgreSQL
            nameof(DateTime.DayOfWeek) => DatePart(instance!, "dow", floor: true),

            // Casting a timestamptz to time (to get the time component) converts it to a local timestamp based on TimeZone.
            // Convert to a timestamp without time zone at UTC to get the right values.
            nameof(DateTime.TimeOfDay) when TryConvertAwayFromTimestampTz(instance!, out var convertedInstance)
                => _sqlExpressionFactory.Convert(
                    convertedInstance,
                    typeof(TimeSpan),
                    _typeMappingSource.FindMapping(typeof(TimeSpan), storeTypeName: "time")),

            // TODO: Should be possible
            nameof(DateTime.Ticks) => null,

            _ => null
        };

        SqlExpression UtcNow()
            => _sqlExpressionFactory.Function(
                "now",
                [],
                nullable: false,
                argumentsPropagateNullability: TrueArrays[0],
                returnType,
                _timestampTzMapping);

        SqlExpression LocalNow()
            => _sqlExpressionFactory.Convert(UtcNow(), returnType, _timestampMapping);
    }

    private SqlExpression? DatePart(
        SqlExpression instance,
        string partName,
        bool floor = false)
    {
        // date_part exists only for timestamp without time zone, so if we pass in a timestamptz it gets converted to a local
        // timestamp based on TimeZone. Convert to a timestamp without time zone at UTC to get the right values.
        if (!TryConvertAwayFromTimestampTz(instance, out instance!))
        {
            return null;
        }

        var result = _sqlExpressionFactory.Function(
            "date_part",
            [_sqlExpressionFactory.Constant(partName), instance],
            nullable: true,
            argumentsPropagateNullability: TrueArrays[2],
            typeof(double));

        if (floor)
        {
            result = _sqlExpressionFactory.Function(
                "floor",
                [result],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(double));
        }

        return _sqlExpressionFactory.Convert(result, typeof(int));
    }

    private SqlExpression? TranslateDateTimeOffset(SqlExpression instance, MemberInfo member)
        => member.Name switch
        {
            // We only support UTC DateTimeOffset, so DateTimeOffset.DateTime is just a matter of converting to timestamp without time zone
            nameof(DateTimeOffset.DateTime) => _sqlExpressionFactory.AtUtc(instance),

            // We only support UTC DateTimeOffset, so DateTimeOffset.UtcDateTime does nothing (type change on CLR change, no change on the
            // PG side.
            nameof(DateTimeOffset.UtcDateTime) => instance,

            // Convert to timestamp without time zone, applying a time zone conversion based on the TimeZone connection parameter.
            nameof(DateTimeOffset.LocalDateTime) => _sqlExpressionFactory.Convert(instance, typeof(DateTime), _timestampMapping),

            // In PG, date_trunc over timestamptz looks at TimeZone, and returns timestamptz. .NET DateTimeOffset.Date just returns the
            // date part (no conversion), and returns an Unspecified DateTime. So we first convert the timestamptz argument to timestamp
            // via AT TIME ZONE 'UTC".
            // Note that we don't use the overload of date_trunc that accepts a timezone as its 3rd argument (like we do for DateTime.Date),
            // since that returns a timestamptz, but DateTimeOffset.Date should return DateTime with Kind=Unspecified
            nameof(DateTimeOffset.Date) =>
                _sqlExpressionFactory.Function(
                    "date_trunc",
                    [_sqlExpressionFactory.Constant("day"), _sqlExpressionFactory.AtUtc(instance)],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(DateTime),
                    _timestampMapping),

            _ => null
        };

    private SqlExpression? TranslateDateOnly(SqlExpression? instance, MemberInfo member)
        => member.Name switch
        {
            // We use fragment rather than a DateOnly constant, since 0001-01-01 gets rendered as -infinity by default.
            // TODO: Set the right type/type mapping after https://github.com/dotnet/efcore/pull/34995 is merged
            nameof(DateOnly.DayNumber) when instance is not null
                => _sqlExpressionFactory.Subtract(instance, _sqlExpressionFactory.Fragment("DATE '0001-01-01'")),

            _ => null
        };

    // Various conversion functions translated here (date_part, ::time) exist only for timestamp without time zone, so if we pass in a
    // timestamptz it gets implicitly converted to a local timestamp based on TimeZone; that's the wrong behavior (these conversions are not
    // supposed to be sensitive to TimeZone).
    // To avoid this, if we get a timestamptz, convert it to a timestamp without time zone (at UTC), which doesn't undergo any timezone
    // conversions.
    private bool TryConvertAwayFromTimestampTz(SqlExpression timestamp, [NotNullWhen(true)] out SqlExpression? result)
    {
        switch (timestamp)
        {
            // We're already dealing with a non-timestamptz mapping, no conversion needed.
            case { TypeMapping: NpgsqlTimestampTypeMapping or NpgsqlDateTimeDateTypeMapping or NpgsqlTimeTypeMapping }:
            case { Type: var type } when type == typeof(DateOnly) || type == typeof(TimeOnly):
                result = timestamp;
                return true;

            // In these cases we know that the expression represents a timestamptz; it's safe to convert to a timestamp without time zone.
            // Note that timestamptz AT TIME ZONE 'UTC' returns the same timestamp but as a timestamp (without time zone).
            case { TypeMapping: NpgsqlTimestampTzTypeMapping }:
            case { Type: var type } when type == typeof(DateTimeOffset):
                result = _sqlExpressionFactory.AtUtc(timestamp);
                return true;

            // If it's a DateTime who's type mapping isn't known (parameter), we cannot ensure that a timestamp without time zone
            // is returned (note that applying AT TIME ZONE 'UTC' on a timestamp without time zone would yield a timestamptz, which would
            // again undergo timestamp conversion)
            case { Type: var type } when type == typeof(DateTime):
                result = null;
                return false;

            default:
                throw new UnreachableException();
        }
    }
}
