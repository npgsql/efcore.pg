using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using NpgsqlTypes;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Utilties.Util;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class TimestampTzOffsetDateTimeMapping : NpgsqlTypeMapping
{
    private static readonly ConstructorInfo Constructor =
        typeof(OffsetDateTime).GetConstructor(new[] { typeof(LocalDateTime), typeof(Offset) })!;

    private static readonly MethodInfo OffsetFromHoursMethod =
        typeof(Offset).GetMethod(nameof(Offset.FromHours), new[] { typeof(int) })!;

    private static readonly MethodInfo OffsetFromSecondsMethod =
        typeof(Offset).GetMethod(nameof(Offset.FromSeconds), new[] { typeof(int) })!;

    public TimestampTzOffsetDateTimeMapping() : base("timestamp with time zone", typeof(OffsetDateTime), NpgsqlDbType.TimestampTz) {}

    protected TimestampTzOffsetDateTimeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.TimestampTz) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new TimestampTzOffsetDateTimeMapping(parameters);

    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new TimestampTzOffsetDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new TimestampTzOffsetDateTimeMapping(Parameters.WithComposedConverter(converter));

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
        => $"TIMESTAMPTZ '{GenerateLiteralCore(value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $@"""{GenerateLiteralCore(value)}""";

    private string GenerateLiteralCore(object value)
        => OffsetDateTimePattern.ExtendedIso.Format((OffsetDateTime)value);

    public override Expression GenerateCodeLiteral(object value)
    {
        var offsetDateTime = (OffsetDateTime)value;
        var offsetSeconds = offsetDateTime.Offset.Seconds;

        return Expression.New(Constructor,
            TimestampLocalDateTimeMapping.GenerateCodeLiteral(offsetDateTime.LocalDateTime),
            offsetSeconds % 3600 == 0
                ? ConstantCall(OffsetFromHoursMethod, offsetSeconds / 3600)
                : ConstantCall(OffsetFromSecondsMethod, offsetSeconds));
    }
}