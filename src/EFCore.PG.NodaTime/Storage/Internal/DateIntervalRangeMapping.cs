using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlDbType = NpgsqlTypes.NpgsqlDbType;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class DateIntervalRangeMapping : NpgsqlTypeMapping
{
    private static readonly ConstructorInfo _constructorWithDates =
        typeof(DateInterval).GetConstructor(new[] { typeof(LocalDate), typeof(LocalDate) })!;

    private static readonly ConstructorInfo _localDateConstructor =
        typeof(LocalDate).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) })!;

    public DateIntervalRangeMapping()
        : base("daterange", typeof(DateInterval), NpgsqlDbType.DateRange)
    {
    }

    protected DateIntervalRangeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.DateRange)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new DateIntervalRangeMapping(parameters);

    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new DateIntervalRangeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new DateIntervalRangeMapping(Parameters.WithComposedConverter(converter));

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{GenerateEmbeddedNonNullSqlLiteral(value)}'::daterange";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
    {
        var dateInterval = (DateInterval)value;
        return $"[{LocalDatePattern.Iso.Format(dateInterval.Start)},{LocalDatePattern.Iso.Format(dateInterval.End)}]";
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var (start, end) = (DateInterval)value;
        return Expression.New(
            _constructorWithDates,
            Expression.New(
                _localDateConstructor, Expression.Constant(start.Year), Expression.Constant(start.Month),
                Expression.Constant(start.Day)),
            Expression.New(
                _localDateConstructor, Expression.Constant(end.Year), Expression.Constant(end.Month), Expression.Constant(end.Day))
        );
    }
}