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
public class TimestampLocalDateTimeMapping : NpgsqlTypeMapping
{
    private static readonly ConstructorInfo ConstructorWithMinutes =
        typeof(LocalDateTime).GetConstructor([typeof(int), typeof(int), typeof(int), typeof(int), typeof(int)])!;

    private static readonly ConstructorInfo ConstructorWithSeconds =
        typeof(LocalDateTime).GetConstructor([typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int)])!;

    private static readonly MethodInfo PlusNanosecondsMethod =
        typeof(LocalDateTime).GetMethod(nameof(LocalDateTime.PlusNanoseconds), [typeof(long)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TimestampLocalDateTimeMapping Default { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TimestampLocalDateTimeMapping()
        : base("timestamp without time zone", typeof(LocalDateTime), NpgsqlDbType.Timestamp, JsonLocalDateTimeReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected TimestampLocalDateTimeMapping(RelationalTypeMappingParameters parameters)
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
        => new TimestampLocalDateTimeMapping(Parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override RelationalTypeMapping WithStoreTypeAndSize(string storeType, int? size)
        => new TimestampLocalDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

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
        => $"TIMESTAMP '{Format((LocalDateTime)value)}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $"""
            "{Format((LocalDateTime)value)}"
            """;

    private static string Format(LocalDateTime localDateTime)
    {
        if (!NpgsqlNodaTimeTypeMappingSourcePlugin.DisableDateTimeInfinityConversions)
        {
            if (localDateTime == LocalDateTime.MinIsoValue)
            {
                return "-infinity";
            }

            if (localDateTime == LocalDateTime.MaxIsoValue)
            {
                return "infinity";
            }
        }

        return LocalDateTimePattern.ExtendedIso.Format(localDateTime);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
        => GenerateCodeLiteral((LocalDateTime)value);

    internal static Expression GenerateCodeLiteral(LocalDateTime dateTime)
    {
        if (dateTime is { Second: 0, NanosecondOfSecond: 0 })
        {
            return ConstantNew(ConstructorWithMinutes, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute);
        }

        var newExpr = ConstantNew(
            ConstructorWithSeconds, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

        return dateTime.NanosecondOfSecond == 0
            ? newExpr
            : Expression.Call(newExpr, PlusNanosecondsMethod, Expression.Constant((long)dateTime.NanosecondOfSecond));
    }

    private sealed class JsonLocalDateTimeReaderWriter : JsonValueReaderWriter<LocalDateTime>
    {
        private static readonly PropertyInfo InstanceProperty = typeof(JsonLocalDateTimeReaderWriter).GetProperty(nameof(Instance))!;

        public static JsonLocalDateTimeReaderWriter Instance { get; } = new();

        public override LocalDateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        {
            var s = manager.CurrentReader.GetString()!;

            if (!NpgsqlNodaTimeTypeMappingSourcePlugin.DisableDateTimeInfinityConversions)
            {
                switch (s)
                {
                    case "-infinity":
                        return LocalDateTime.MinIsoValue;
                    case "infinity":
                        return LocalDateTime.MaxIsoValue;
                }
            }

            return LocalDateTimePattern.ExtendedIso.Parse(s).GetValueOrThrow();
        }

        public override void ToJsonTyped(Utf8JsonWriter writer, LocalDateTime value)
            => writer.WriteStringValue(Format(value));

        /// <inheritdoc />
        public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
    }
}
