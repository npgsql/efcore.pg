using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;
using static Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Utilties.Util;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class TimestampLocalDateTimeMapping : NpgsqlTypeMapping
{
    private static readonly ConstructorInfo ConstructorWithMinutes =
        typeof(LocalDateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) })!;

    private static readonly ConstructorInfo ConstructorWithSeconds =
        typeof(LocalDateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) })!;

    private static readonly MethodInfo PlusNanosecondsMethod =
        typeof(LocalDateTime).GetMethod(nameof(LocalDateTime.PlusNanoseconds), new[] { typeof(long) })!;

    public TimestampLocalDateTimeMapping() : base("timestamp without time zone", typeof(LocalDateTime), NpgsqlDbType.Timestamp) {}

    protected TimestampLocalDateTimeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Timestamp) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new TimestampLocalDateTimeMapping(Parameters);

    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new TimestampLocalDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new TimestampLocalDateTimeMapping(Parameters.WithComposedConverter(converter));

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIMESTAMP '{GenerateLiteralCore(value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $@"""{GenerateLiteralCore(value)}""";

    private string GenerateLiteralCore(object value)
        => LocalDateTimePattern.ExtendedIso.Format((LocalDateTime)value);

    public override Expression GenerateCodeLiteral(object value) => GenerateCodeLiteral((LocalDateTime)value);

    internal static Expression GenerateCodeLiteral(LocalDateTime dateTime)
    {
        if (dateTime.Second == 0 && dateTime.NanosecondOfSecond == 0)
        {
            return ConstantNew(ConstructorWithMinutes, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute);
        }

        var newExpr = ConstantNew(ConstructorWithSeconds, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

        return dateTime.NanosecondOfSecond == 0
            ? (Expression)newExpr
            : Expression.Call(newExpr, PlusNanosecondsMethod, Expression.Constant((long)dateTime.NanosecondOfSecond));
    }
}