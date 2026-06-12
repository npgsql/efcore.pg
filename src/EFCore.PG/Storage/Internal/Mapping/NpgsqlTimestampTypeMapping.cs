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
public class NpgsqlTimestampTypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static NpgsqlTimestampTypeMapping Default { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlTimestampTypeMapping()
        : base("timestamp without time zone", typeof(DateTime), NpgsqlDbType.Timestamp, NpgsqlJsonTimestampReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlTimestampTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Timestamp)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlTimestampTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"timestamp({parameters.Precision}) without time zone";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIMESTAMP '{GenerateLiteralCore(value)}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $"""
            "{GenerateLiteralCore(value)}"
            """;

    private string GenerateLiteralCore(object value)
        => FormatDateTime((DateTime)value);

    private static string FormatDateTime(DateTime dateTime)
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

        return NpgsqlTypeMappingSource.LegacyTimestampBehavior || dateTime.Kind != DateTimeKind.Utc
            ? dateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFF", CultureInfo.InvariantCulture)
            : throw new ArgumentException("'timestamp without time zone' literal cannot be generated for a UTC DateTime");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class NpgsqlJsonTimestampReaderWriter : JsonValueReaderWriter<DateTime>
    {
        private static readonly PropertyInfo InstanceProperty = typeof(NpgsqlJsonTimestampReaderWriter).GetProperty(nameof(Instance))!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static NpgsqlJsonTimestampReaderWriter Instance { get; } = new();

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

            return DateTime.Parse(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void ToJsonTyped(Utf8JsonWriter writer, DateTime value)
            => writer.WriteStringValue(FormatDateTime(value));

        /// <inheritdoc />
        public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
    }
}
