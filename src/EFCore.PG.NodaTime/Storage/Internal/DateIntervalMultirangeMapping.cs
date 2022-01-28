using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlDbType = NpgsqlTypes.NpgsqlDbType;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class DateIntervalMultirangeMapping : NpgsqlTypeMapping
{
    private readonly DateIntervalRangeMapping _dateIntervalRangeMapping;

    public DateIntervalMultirangeMapping(Type clrType, DateIntervalRangeMapping dateIntervalRangeMapping)
        : base("datemultirange", clrType, NpgsqlDbType.DateMultirange)
        => _dateIntervalRangeMapping = dateIntervalRangeMapping;

    protected DateIntervalMultirangeMapping(RelationalTypeMappingParameters parameters, DateIntervalRangeMapping dateIntervalRangeMapping)
        : base(parameters, NpgsqlDbType.DateMultirange)
        => _dateIntervalRangeMapping = dateIntervalRangeMapping;

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new DateIntervalMultirangeMapping(parameters, _dateIntervalRangeMapping);

    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new DateIntervalMultirangeMapping(Parameters.WithStoreTypeAndSize(storeType, size), _dateIntervalRangeMapping);

    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new DateIntervalMultirangeMapping(Parameters.WithComposedConverter(converter), _dateIntervalRangeMapping);

    protected override string GenerateNonNullSqlLiteral(object value)
        => NpgsqlMultirangeTypeMapping.GenerateNonNullSqlLiteral(value, _dateIntervalRangeMapping, "datemultirange");
}