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
public class TimeMapping : NpgsqlTypeMapping
{
    private static readonly ConstructorInfo ConstructorWithMinutes =
        typeof(LocalTime).GetConstructor([typeof(int), typeof(int)])!;

    private static readonly ConstructorInfo ConstructorWithSeconds =
        typeof(LocalTime).GetConstructor([typeof(int), typeof(int), typeof(int)])!;

    private static readonly MethodInfo FromHourMinuteSecondNanosecondMethod =
        typeof(LocalTime).GetMethod(
            nameof(LocalTime.FromHourMinuteSecondNanosecond),
            [typeof(int), typeof(int), typeof(int), typeof(long)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TimeMapping Default { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TimeMapping()
        : base("time", typeof(LocalTime), NpgsqlDbType.Time, JsonLocalTimeReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected TimeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Time)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new TimeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override RelationalTypeMapping WithStoreTypeAndSize(string storeType, int? size)
        => new TimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"time({parameters.Precision})";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIME '{GenerateEmbeddedNonNullSqlLiteral(value)}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => LocalTimePattern.ExtendedIso.Format((LocalTime)value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        var time = (LocalTime)value;
        return time.NanosecondOfSecond != 0
            ? ConstantCall(FromHourMinuteSecondNanosecondMethod, time.Hour, time.Minute, time.Second, (long)time.NanosecondOfSecond)
            : time.Second != 0
                ? ConstantNew(ConstructorWithSeconds, time.Hour, time.Minute, time.Second)
                : ConstantNew(ConstructorWithMinutes, time.Hour, time.Minute);
    }

    private sealed class JsonLocalTimeReaderWriter : JsonValueReaderWriter<LocalTime>
    {
        private static readonly PropertyInfo InstanceProperty = typeof(JsonLocalTimeReaderWriter).GetProperty(nameof(Instance))!;

        public static JsonLocalTimeReaderWriter Instance { get; } = new();

        public override LocalTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => LocalTimePattern.ExtendedIso.Parse(manager.CurrentReader.GetString()!).GetValueOrThrow();

        public override void ToJsonTyped(Utf8JsonWriter writer, LocalTime value)
            => writer.WriteStringValue(LocalTimePattern.ExtendedIso.Format(value));

        /// <inheritdoc />
        public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
    }
}
