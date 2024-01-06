// ReSharper disable once CheckNamespace

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DateTimeZoneMapping : RelationalTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DateTimeZoneMapping(string storeType)
        : base(
            new RelationalTypeMappingParameters(
                new(typeof(DateTimeZone), new DateTimeZoneConverter(), new DateTimeZoneComparer()), storeType))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected DateTimeZoneMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new DateTimeZoneMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
        => Expression.Call(
            Expression.Property(null, typeof(DateTimeZoneProviders).GetProperty(nameof(DateTimeZoneProviders.Tzdb))!),
            typeof(IDateTimeZoneProvider).GetMethod(nameof(IDateTimeZoneProvider.GetZoneOrNull), [typeof(string)])!,
            Expression.Constant(((DateTimeZone)value).Id));

    private sealed class DateTimeZoneConverter() : ValueConverter<DateTimeZone, string>(
        tz => tz.Id,
        id => DateTimeZoneProviders.Tzdb[id]);

    private sealed class DateTimeZoneComparer() : ValueComparer<DateTimeZone>(
        (tz1, tz2) => tz1 == null ? tz2 == null : tz2 != null && tz1.Id == tz2.Id,
        tz => tz.GetHashCode());
}
