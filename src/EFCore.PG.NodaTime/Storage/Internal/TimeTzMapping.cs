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
public class TimeTzMapping : NpgsqlTypeMapping
{
    private static readonly ConstructorInfo OffsetTimeConstructor =
        typeof(OffsetTime).GetConstructor(new[] { typeof(LocalTime), typeof(Offset) })!;

    private static readonly ConstructorInfo LocalTimeConstructorWithMinutes =
        typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int) })!;

    private static readonly ConstructorInfo LocalTimeConstructorWithSeconds =
        typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) })!;

    private static readonly MethodInfo LocalTimeFromHourMinuteSecondNanosecondMethod =
        typeof(LocalTime).GetMethod(nameof(LocalTime.FromHourMinuteSecondNanosecond),
            new[] { typeof(int), typeof(int), typeof(int), typeof(long) })!;

    private static readonly MethodInfo OffsetFromHoursMethod =
        typeof(Offset).GetMethod(nameof(Offset.FromHours), new[] { typeof(int) })!;

    private static readonly MethodInfo OffsetFromSeconds =
        typeof(Offset).GetMethod(nameof(Offset.FromSeconds), new[] { typeof(int) })!;

    private static readonly OffsetTimePattern Pattern =
        OffsetTimePattern.CreateWithInvariantCulture("HH':'mm':'ss;FFFFFFo<G>");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TimeTzMapping() : base("time with time zone", typeof(OffsetTime), NpgsqlDbType.TimeTz) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected TimeTzMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.TimeTz) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new TimeTzMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new TimeTzMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new TimeTzMapping(Parameters.WithComposedConverter(converter));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIMETZ '{GenerateLiteralCore(value)}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $@"""{GenerateLiteralCore(value)}""";

    private string GenerateLiteralCore(object value)
        => Pattern.Format((OffsetTime)value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        var offsetTime = (OffsetTime)value;
        var offsetSeconds = offsetTime.Offset.Seconds;

        Expression newLocalTimeExpr;
        if (offsetTime.NanosecondOfSecond != 0)
        {
            newLocalTimeExpr = ConstantCall(LocalTimeFromHourMinuteSecondNanosecondMethod, offsetTime.Hour, offsetTime.Minute, offsetTime.Second, (long)offsetTime.NanosecondOfSecond);
        }
        else if (offsetTime.Second != 0)
        {
            newLocalTimeExpr = ConstantNew(LocalTimeConstructorWithSeconds, offsetTime.Hour, offsetTime.Minute, offsetTime.Second);
        }
        else
        {
            newLocalTimeExpr = ConstantNew(LocalTimeConstructorWithMinutes, offsetTime.Hour, offsetTime.Minute);
        }

        return Expression.New(OffsetTimeConstructor,
            newLocalTimeExpr,
            offsetSeconds % 3600 == 0
                ? ConstantCall(OffsetFromHoursMethod, offsetSeconds / 3600)
                : ConstantCall(OffsetFromSeconds, offsetSeconds));
    }
}
