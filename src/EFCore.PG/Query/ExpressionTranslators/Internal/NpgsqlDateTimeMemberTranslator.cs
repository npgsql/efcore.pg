using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
/// Provides translation services for <see cref="DateTime"/> members.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/functions-datetime.html
/// </remarks>
public class NpgsqlDateTimeMemberTranslator : IMemberTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _timestampMapping;
    private readonly RelationalTypeMapping _timestampTzMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlDateTimeMemberTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory sqlExpressionFactory)
    {
        _timestampMapping = typeMappingSource.FindMapping("timestamp without time zone")!;
        _timestampTzMapping = typeMappingSource.FindMapping("timestamp with time zone")!;
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var type = member.DeclaringType;

        if (type != typeof(DateTime)
            && type != typeof(DateTimeOffset)
            && type != typeof(DateOnly)
            && type != typeof(TimeOnly))
        {
            return null;
        }

        if (type == typeof(DateTimeOffset)
            && instance is not null
            && TranslateDateTimeOffset(instance, member, returnType) is { } translated)
        {
            return translated;
        }

        if (member.Name == nameof(DateTime.Date))
        {
            // Note that DateTime.Date returns a DateTime, not a DateOnly (introduced later); so we convert using date_trunc (which returns
            // a PG timestamp/timestamptz) rather than a conversion to PG date (compare with NodaTime where we want a LocalDate).

            // When given a timestamptz, date_trunc performs the truncation with respect to TimeZone; to avoid that, we use the overload
            // accepting a time zone, and pass UTC. For regular timestamp (or in legacy timestamp mode), we use the simpler overload without
            // a time zone.
            switch (instance?.TypeMapping)
            {
                case NpgsqlTimestampTypeMapping:
                case NpgsqlTimestampTzTypeMapping when NpgsqlTypeMappingSource.LegacyTimestampBehavior:
                    return _sqlExpressionFactory.Function(
                        "date_trunc",
                        new[] { _sqlExpressionFactory.Constant("day"), instance! },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        returnType,
                        instance.TypeMapping);

                case NpgsqlTimestampTzTypeMapping:
                    return _sqlExpressionFactory.Function(
                        "date_trunc",
                        new[] { _sqlExpressionFactory.Constant("day"), instance, _sqlExpressionFactory.Constant("UTC") },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[3],
                        returnType,
                        instance.TypeMapping);

                // If DateTime.Date is invoked on a PostgreSQL date, simply no-op.
                case NpgsqlDateTypeMapping:
                    return instance;

                default:
                    return null;
            }
        }

        return member.Name switch
        {
            // Legacy behavior
            nameof(DateTime.Now)    when NpgsqlTypeMappingSource.LegacyTimestampBehavior
                => UtcNow(),
            nameof(DateTime.UtcNow) when NpgsqlTypeMappingSource.LegacyTimestampBehavior
                => _sqlExpressionFactory.AtUtc(UtcNow()), // Return a UTC timestamp, but as timestamp without time zone

            // We support getting a local DateTime via DateTime.Now (based on PG TimeZone), but there's no way to get a non-UTC
            // DateTimeOffset.
            nameof(DateTime.Now) => type == typeof(DateTimeOffset)
                ? throw new InvalidOperationException("Cannot translate DateTimeOffset.Now - use UtcNow.")
                : LocalNow(),
            nameof(DateTime.UtcNow) => UtcNow(),

            nameof(DateTime.Today) => _sqlExpressionFactory.Function(
                "date_trunc",
                new[] { _sqlExpressionFactory.Constant("day"), LocalNow() },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                returnType),

            nameof(DateTime.Year)      => GetDatePartExpression(instance!, "year"),
            nameof(DateTime.Month)     => GetDatePartExpression(instance!, "month"),
            nameof(DateTime.DayOfYear) => GetDatePartExpression(instance!, "doy"),
            nameof(DateTime.Day)       => GetDatePartExpression(instance!, "day"),
            nameof(DateTime.Hour)      => GetDatePartExpression(instance!, "hour"),
            nameof(DateTime.Minute)    => GetDatePartExpression(instance!, "minute"),
            nameof(DateTime.Second)    => GetDatePartExpression(instance!, "second"),

            nameof(DateTime.Millisecond) => null, // Too annoying

            // .NET's DayOfWeek is an enum, but its int values happen to correspond to PostgreSQL
            nameof(DateTime.DayOfWeek) => GetDatePartExpression(instance!, "dow", floor: true),

            // TODO: Technically possible simply via casting to PG time, should be better in EF Core 3.0
            // but ExplicitCastExpression only allows casting to PG types that
            // are default-mapped from CLR types (timespan maps to interval,
            // which timestamp cannot be cast into)
            nameof(DateTime.TimeOfDay) => null,

            // TODO: Should be possible
            nameof(DateTime.Ticks) => null,

            _ => null
        };

        SqlExpression UtcNow()
            => _sqlExpressionFactory.Function(
                "now",
                Array.Empty<SqlExpression>(),
                nullable: false,
                argumentsPropagateNullability: TrueArrays[0],
                returnType,
                _timestampTzMapping);

        SqlExpression LocalNow()
            => _sqlExpressionFactory.Convert(UtcNow(), returnType, _timestampMapping);
    }

    private SqlExpression GetDatePartExpression(
        SqlExpression instance,
        string partName,
        bool floor = false)
    {
        if (instance.Type == typeof(DateTimeOffset))
        {
            // date_part exists only for timestamp without time zone, so if we pass in a timestamptz it gets converted to a local
            // timestamp based on TimeZone. Convert to a timestamp without time zone at UTC to get the right values.
            instance = _sqlExpressionFactory.AtUtc(instance);
        }

        var result = _sqlExpressionFactory.Function(
            "date_part",
            new[]
            {
                _sqlExpressionFactory.Constant(partName),
                instance
            },
            nullable: true,
            argumentsPropagateNullability: TrueArrays[2],
            typeof(double));

        if (floor)
        {
            result = _sqlExpressionFactory.Function(
                "floor",
                new[] { result },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(double));
        }

        return _sqlExpressionFactory.Convert(result, typeof(int));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? TranslateDateTimeOffset(
        SqlExpression instance,
        MemberInfo member,
        Type returnType)
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
            // via AT TIME ZONE 'UTC"
            nameof(DateTimeOffset.Date) =>
                _sqlExpressionFactory.Function(
                    "date_trunc",
                    new SqlExpression[] { _sqlExpressionFactory.Constant("day"), _sqlExpressionFactory.AtUtc(instance) },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(DateTime),
                    _timestampTzMapping),

            _ => null
        };
}
