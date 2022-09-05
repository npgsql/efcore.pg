// ReSharper disable once CheckNamespace

using System.Text;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class IntervalRangeMapping : NpgsqlTypeMapping
{
    private static readonly ConstructorInfo _constructor =
        typeof(Interval).GetConstructor(new[] { typeof(Instant), typeof(Instant) })!;

    private static readonly ConstructorInfo _constructorWithNulls =
        typeof(Interval).GetConstructor(new[] { typeof(Instant?), typeof(Instant?) })!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IntervalRangeMapping()
        : base("tstzrange", typeof(Interval), NpgsqlDbType.TimestampTzRange)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected IntervalRangeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.TimestampTzRange)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new IntervalRangeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new IntervalRangeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new IntervalRangeMapping(Parameters.WithComposedConverter(converter));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{GenerateEmbeddedNonNullSqlLiteral(value)}'::tstzrange";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
    {
        var interval = (Interval)value;

        var stringBuilder = new StringBuilder("[");

        if (interval.HasStart)
        {
            stringBuilder.Append(InstantPattern.ExtendedIso.Format(interval.Start));
        }

        stringBuilder.Append(',');

        if (interval.HasEnd)
        {
            stringBuilder.Append(InstantPattern.ExtendedIso.Format(interval.End));
        }

        stringBuilder.Append(')');

        return stringBuilder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        var interval = (Interval)value;

        return interval.HasStart && interval.HasEnd
            ? Expression.New(
                _constructor,
                TimestampTzInstantMapping.GenerateCodeLiteral(interval.Start),
                TimestampTzInstantMapping.GenerateCodeLiteral(interval.End))
            : Expression.New(
                _constructorWithNulls,
                interval.HasStart
                    ? Expression.Convert(
                        TimestampTzInstantMapping.GenerateCodeLiteral(interval.Start),
                        typeof(Instant?))
                    : Expression.Constant(null, typeof(Instant?)),
                interval.HasEnd
                    ? Expression.Convert(
                        TimestampTzInstantMapping.GenerateCodeLiteral(interval.End),
                        typeof(Instant?))
                    : Expression.Constant(null, typeof(Instant?)));
    }
}
