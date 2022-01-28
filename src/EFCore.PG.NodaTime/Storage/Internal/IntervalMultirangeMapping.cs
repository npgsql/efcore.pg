using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlDbType = NpgsqlTypes.NpgsqlDbType;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class IntervalMultirangeMapping : NpgsqlTypeMapping
{
    private readonly IntervalRangeMapping _intervalRangeMapping;

    public IntervalMultirangeMapping(Type clrType, IntervalRangeMapping intervalRangeMapping)
        : base("tstzmultirange", clrType, NpgsqlDbType.TimestampTzMultirange)
        => _intervalRangeMapping = intervalRangeMapping;

    protected IntervalMultirangeMapping(RelationalTypeMappingParameters parameters, IntervalRangeMapping intervalRangeMapping)
        : base(parameters, NpgsqlDbType.DateMultirange)
        => _intervalRangeMapping = intervalRangeMapping;

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new IntervalMultirangeMapping(parameters, _intervalRangeMapping);

    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new IntervalMultirangeMapping(Parameters.WithStoreTypeAndSize(storeType, size), _intervalRangeMapping);

    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new IntervalMultirangeMapping(Parameters.WithComposedConverter(converter), _intervalRangeMapping);

    protected override string GenerateNonNullSqlLiteral(object value)
        => NpgsqlMultirangeTypeMapping.GenerateNonNullSqlLiteral(value, _intervalRangeMapping, "tstzmultirange");
}