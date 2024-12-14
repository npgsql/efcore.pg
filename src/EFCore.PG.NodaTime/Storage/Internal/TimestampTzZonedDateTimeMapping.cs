using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NodaTime.Text;
using NodaTime.TimeZones;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TimestampTzZonedDateTimeMapping : NpgsqlTypeMapping
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
        => $"TIMESTAMPTZ '{Pattern.Format((ZonedDateTime)value)}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $"""
            "{Pattern.Format((ZonedDateTime)value)}"
            """;

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
        private static readonly PropertyInfo InstanceProperty = typeof(JsonZonedDateTimeReaderWriter).GetProperty(nameof(Instance))!;

        public static JsonZonedDateTimeReaderWriter Instance { get; } = new();

        public override ZonedDateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => Pattern.Parse(manager.CurrentReader.GetString()!).GetValueOrThrow();

        public override void ToJsonTyped(Utf8JsonWriter writer, ZonedDateTime value)
            => writer.WriteStringValue(Pattern.Format(value));

        /// <inheritdoc />
        public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
    }
}
