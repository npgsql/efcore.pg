using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NodaTime.Text;
using NodaTime.TimeZones;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TimestampTzZonedDateTimeMapping : NpgsqlTypeMapping, INpgsqlJsonScalarTypeMapping
{
    private static readonly ZonedDateTimePattern Pattern =
        ZonedDateTimePattern.CreateWithInvariantCulture(
            "uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFo<G>",
            DateTimeZoneProviders.Tzdb);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TimestampTzZonedDateTimeMapping Default { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TimestampTzZonedDateTimeMapping()
        : base("timestamp with time zone", typeof(ZonedDateTime), NpgsqlDbType.TimestampTz, JsonZonedDateTimeReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected TimestampTzZonedDateTimeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.TimestampTz)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new TimestampTzZonedDateTimeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override RelationalTypeMapping WithStoreTypeAndSize(string storeType, int? size)
        => new TimestampTzZonedDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"timestamp({parameters.Precision}) with time zone";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIMESTAMPTZ '{Format((ZonedDateTime)value)}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $"""
            "{Format((ZonedDateTime)value)}"
            """;

    private static string Format(ZonedDateTime zonedDateTime)
        => zonedDateTime.Year > 0
            ? Pattern.Format(zonedDateTime)
            : zonedDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFo<G>", CultureInfo.InvariantCulture) + " BC";

    /// <inheritdoc />
    /// <remarks>
    ///     Keep the serialized offset rather than the zone ID to disambiguate DST overlaps.
    /// </remarks>
    public virtual SqlExpression ConvertFromJsonText(SqlExpression expression)
    {
        // PostgreSQL rounds fractional seconds on input. Truncate the text first, then round up pre-2000 instants with
        // discarded nanoseconds to match Npgsql's truncation toward the PostgreSQL epoch.
        var timestamp = new SqlUnaryExpression(
            ExpressionType.Convert,
            new SqlFunctionExpression(
                "regexp_replace",
                [
                    expression,
                    new SqlConstantExpression(@"(\.\d{6})\d+| [^ ]+ \(([^)]+)\)$", NpgsqlStringTypeMapping.Default),
                    new SqlConstantExpression(@"\1\2", NpgsqlStringTypeMapping.Default),
                    new SqlConstantExpression("g", NpgsqlStringTypeMapping.Default)
                ],
                nullable: true,
                argumentsPropagateNullability: [true, false, false, false],
                typeof(string),
                NpgsqlStringTypeMapping.Default),
            ClrType,
            this);

        return new CaseExpression(
            [
                new CaseWhenClause(
                    new SqlBinaryExpression(
                        ExpressionType.AndAlso,
                        new SqlBinaryExpression(
                            ExpressionType.LessThan,
                            timestamp,
                            new SqlConstantExpression(Instant.FromUtc(2000, 1, 1, 0, 0).InUtc(), Default),
                            typeof(bool),
                            NpgsqlBoolTypeMapping.Default),
#pragma warning disable EF1001
                        new PgRegexMatchExpression(
                            expression,
                            new SqlConstantExpression(@"\.\d{6}\d*[1-9]\d* ", NpgsqlStringTypeMapping.Default),
                            RegexOptions.None,
                            NpgsqlBoolTypeMapping.Default),
#pragma warning restore EF1001
                        typeof(bool),
                        NpgsqlBoolTypeMapping.Default),
                    new SqlBinaryExpression(
                        ExpressionType.Add,
                        timestamp,
                        new SqlConstantExpression(Duration.FromNanoseconds(1000), DurationIntervalMapping.Default),
                        ClrType,
                        this))
            ],
            timestamp);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        var zonedDateTime = (ZonedDateTime)value;

        return Expression.New(
            Constructor,
            TimestampTzInstantMapping.GenerateCodeLiteral(zonedDateTime.ToInstant()),
            Expression.Call(
                Expression.MakeMemberAccess(
                    null,
                    TzdbDateTimeZoneSourceDefaultMember),
                ForIdMethod,
                Expression.Constant(zonedDateTime.Zone.Id)));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(ZonedDateTime).GetConstructor([typeof(Instant), typeof(DateTimeZone)])!;

    private static readonly MemberInfo TzdbDateTimeZoneSourceDefaultMember =
        typeof(TzdbDateTimeZoneSource).GetMember(nameof(TzdbDateTimeZoneSource.Default))[0];

    private static readonly MethodInfo ForIdMethod =
        typeof(TzdbDateTimeZoneSource).GetRuntimeMethod(
            nameof(TzdbDateTimeZoneSource.ForId),
            [typeof(string)])!;

    private sealed class JsonZonedDateTimeReaderWriter : JsonValueReaderWriter<ZonedDateTime>
    {
        private static readonly ZonedDateTimePattern JsonPattern =
            ZonedDateTimePattern.ExtendedFormatOnlyIso.WithZoneProvider(DateTimeZoneProviders.Tzdb);

        private static readonly PropertyInfo InstanceProperty = typeof(JsonZonedDateTimeReaderWriter).GetProperty(nameof(Instance))!;

        public static JsonZonedDateTimeReaderWriter Instance { get; } = new();

        public override ZonedDateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        {
            var text = manager.CurrentReader.GetString()!;
            var result = JsonPattern.Parse(text);
            return result.Success ? result.Value : Pattern.Parse(text).GetValueOrThrow();
        }

        public override void ToJsonTyped(Utf8JsonWriter writer, ZonedDateTime value)
            => writer.WriteStringValue(JsonPattern.Format(value));

        /// <inheritdoc />
        public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
    }
}
