using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Utilties.Util;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TimestampTzOffsetDateTimeMapping : NpgsqlTypeMapping
{
    private static readonly ConstructorInfo Constructor =
        typeof(OffsetDateTime).GetConstructor([typeof(LocalDateTime), typeof(Offset)])!;

    private static readonly MethodInfo OffsetFromHoursMethod =
        typeof(Offset).GetMethod(nameof(Offset.FromHours), [typeof(int)])!;

    private static readonly MethodInfo OffsetFromSecondsMethod =
        typeof(Offset).GetMethod(nameof(Offset.FromSeconds), [typeof(int)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TimestampTzOffsetDateTimeMapping Default { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TimestampTzOffsetDateTimeMapping()
        : base("timestamp with time zone", typeof(OffsetDateTime), NpgsqlDbType.TimestampTz, JsonOffsetDateTimeReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected TimestampTzOffsetDateTimeMapping(RelationalTypeMappingParameters parameters)
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
        => new TimestampTzOffsetDateTimeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override RelationalTypeMapping WithStoreTypeAndSize(string storeType, int? size)
        => new TimestampTzOffsetDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

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
        => $"TIMESTAMPTZ '{Format((OffsetDateTime)value)}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $"""
            "{Format((OffsetDateTime)value)}"
            """;

    private static string Format(OffsetDateTime offsetDateTime)
        => OffsetDateTimePattern.ExtendedIso.Format(offsetDateTime);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        var offsetDateTime = (OffsetDateTime)value;
        var offsetSeconds = offsetDateTime.Offset.Seconds;

        return Expression.New(
            Constructor,
            TimestampLocalDateTimeMapping.GenerateCodeLiteral(offsetDateTime.LocalDateTime),
            offsetSeconds % 3600 == 0
                ? ConstantCall(OffsetFromHoursMethod, offsetSeconds / 3600)
                : ConstantCall(OffsetFromSecondsMethod, offsetSeconds));
    }

    private sealed class JsonOffsetDateTimeReaderWriter : JsonValueReaderWriter<OffsetDateTime>
    {
        private static readonly PropertyInfo InstanceProperty = typeof(JsonOffsetDateTimeReaderWriter).GetProperty(nameof(Instance))!;

        public static JsonOffsetDateTimeReaderWriter Instance { get; } = new();

        public override OffsetDateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => OffsetDateTimePattern.ExtendedIso.Parse(manager.CurrentReader.GetString()!).GetValueOrThrow();

        public override void ToJsonTyped(Utf8JsonWriter writer, OffsetDateTime value)
            => writer.WriteStringValue(Format(value));

        /// <inheritdoc />
        public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
    }
}
