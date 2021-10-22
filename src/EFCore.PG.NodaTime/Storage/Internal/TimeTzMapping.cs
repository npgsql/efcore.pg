using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using NpgsqlTypes;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Utilties.Util;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

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

    public TimeTzMapping() : base("time with time zone", typeof(OffsetTime), NpgsqlDbType.TimeTz) {}

    protected TimeTzMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.TimeTz) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new TimeTzMapping(parameters);

    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new TimeTzMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new TimeTzMapping(Parameters.WithComposedConverter(converter));

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIMETZ '{GenerateLiteralCore(value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $@"""{GenerateLiteralCore(value)}""";

    private string GenerateLiteralCore(object value)
        => Pattern.Format((OffsetTime)value);

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