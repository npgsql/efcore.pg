using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Utilties.Util;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class TimeMapping : NpgsqlTypeMapping
{
    private static readonly ConstructorInfo ConstructorWithMinutes =
        typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int) })!;

    private static readonly ConstructorInfo ConstructorWithSeconds =
        typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) })!;

    private static readonly MethodInfo FromHourMinuteSecondNanosecondMethod =
        typeof(LocalTime).GetMethod(nameof(LocalTime.FromHourMinuteSecondNanosecond),
            new[] { typeof(int), typeof(int), typeof(int), typeof(long) })!;

    public TimeMapping() : base("time", typeof(LocalTime), NpgsqlDbType.Time) {}

    protected TimeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Time) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new TimeMapping(parameters);

    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new TimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new TimeMapping(Parameters.WithComposedConverter(converter));

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIME '{GenerateEmbeddedNonNullSqlLiteral(value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => LocalTimePattern.ExtendedIso.Format((LocalTime)value);

    public override Expression GenerateCodeLiteral(object value)
    {
        var time = (LocalTime)value;
        return time.NanosecondOfSecond != 0
            ? ConstantCall(FromHourMinuteSecondNanosecondMethod, time.Hour, time.Minute, time.Second, (long)time.NanosecondOfSecond)
            : time.Second != 0
                ? ConstantNew(ConstructorWithSeconds, time.Hour, time.Minute, time.Second)
                : ConstantNew(ConstructorWithMinutes, time.Hour, time.Minute);
    }
}