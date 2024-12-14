using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlTimestampTzTypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static NpgsqlTimestampTzTypeMapping Default { get; } = new(typeof(DateTime));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlTimestampTzTypeMapping(Type clrType)
        : base(
            "timestamp with time zone",
            clrType,
            NpgsqlDbType.TimestampTz,
            clrType == typeof(DateTime)
                ? NpgsqlJsonTimestampTzDateTimeReaderWriter.Instance
                : clrType == typeof(DateTimeOffset)
                    ? NpgsqlJsonTimestampTzDateTimeOffsetReaderWriter.Instance
                    : throw new ArgumentException("clrType must be DateTime or DateTimeOffset", nameof(clrType)))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlTimestampTzTypeMapping(RelationalTypeMappingParameters parameters)
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
        => new NpgsqlTimestampTzTypeMapping(parameters);

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
        => $"TIMESTAMPTZ '{Format(value)}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $"""
            "{Format(value)}"
            """;

    private static string Format(object value)
        => value switch
        {
            DateTime dateTime => Format(dateTime),
            DateTimeOffset dateTimeOffset => Format(dateTimeOffset),
            _ => throw new InvalidCastException(
                $"Attempted to generate timestamptz literal for type {value.GetType()}, only DateTime and DateTimeOffset are supported")
        };

    private static string Format(DateTime dateTime)
    {
        if (!NpgsqlTypeMappingSource.DisableDateTimeInfinityConversions)
        {
            if (dateTime == DateTime.MinValue)
            {
                return "-infinity";
            }

            if (dateTime == DateTime.MaxValue)
            {
                return "infinity";
            }
        }

        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFF", CultureInfo.InvariantCulture) + 'Z',

            DateTimeKind.Unspecified => NpgsqlTypeMappingSource.LegacyTimestampBehavior || dateTime == default
                ? dateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFF", CultureInfo.InvariantCulture) + 'Z'
                : throw new ArgumentException(
                    $"'timestamp with time zone' literal cannot be generated for {dateTime.Kind} DateTime: a UTC DateTime is required"),

            DateTimeKind.Local => NpgsqlTypeMappingSource.LegacyTimestampBehavior
                ? dateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFzzz", CultureInfo.InvariantCulture)
                : throw new ArgumentException(
                    $"'timestamp with time zone' literal cannot be generated for {dateTime.Kind} DateTime: a UTC DateTime is required"),

            _ => throw new UnreachableException()
        };
    }

    private static string Format(DateTimeOffset dateTimeOffset)
    {
        if (!NpgsqlTypeMappingSource.DisableDateTimeInfinityConversions)
        {
            if (dateTimeOffset == DateTimeOffset.MinValue)
            {
                return "-infinity";
            }

            if (dateTimeOffset == DateTimeOffset.MaxValue)
            {
                return "infinity";
            }
        }

        return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFzzz", CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class NpgsqlJsonTimestampTzDateTimeReaderWriter : JsonValueReaderWriter<DateTime>
    {
        private static readonly PropertyInfo InstanceProperty
            = typeof(NpgsqlJsonTimestampTzDateTimeReaderWriter).GetProperty(nameof(Instance))!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static NpgsqlJsonTimestampTzDateTimeReaderWriter Instance { get; } = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override DateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        {
            var s = manager.CurrentReader.GetString()!;

            if (!NpgsqlTypeMappingSource.DisableDateTimeInfinityConversions)
            {
                switch (s)
                {
                    case "-infinity":
                        return DateTime.MinValue;
                    case "infinity":
                        return DateTime.MaxValue;
                }
            }

            // Our JSON string representation ends with Z (UTC), but DateTime.Parse returns a Local timestamp even in that case. Convert
            // it in order to return a DateTime with Kind UTC.
            return DateTime.Parse(s, CultureInfo.InvariantCulture).ToUniversalTime();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void ToJsonTyped(Utf8JsonWriter writer, DateTime value)
            => writer.WriteStringValue(Format(value));

        /// <inheritdoc />
        public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class NpgsqlJsonTimestampTzDateTimeOffsetReaderWriter : JsonValueReaderWriter<DateTimeOffset>
    {
        private static readonly PropertyInfo InstanceProperty
            = typeof(NpgsqlJsonTimestampTzDateTimeOffsetReaderWriter).GetProperty(nameof(Instance))!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static NpgsqlJsonTimestampTzDateTimeOffsetReaderWriter Instance { get; } = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override DateTimeOffset FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        {
            var s = manager.CurrentReader.GetString()!;

            if (!NpgsqlTypeMappingSource.DisableDateTimeInfinityConversions)
            {
                switch (s)
                {
                    case "-infinity":
                        return DateTimeOffset.MinValue;
                    case "infinity":
                        return DateTimeOffset.MaxValue;
                }
            }

            return DateTimeOffset.Parse(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void ToJsonTyped(Utf8JsonWriter writer, DateTimeOffset value)
            => writer.WriteStringValue(Format(value));

        /// <inheritdoc />
        public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
    }
}
