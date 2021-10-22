using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlIntervalTypeMapping : NpgsqlTypeMapping
{
    public NpgsqlIntervalTypeMapping() : base("interval", typeof(TimeSpan), NpgsqlDbType.Interval) {}

    protected NpgsqlIntervalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Interval) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlIntervalTypeMapping(parameters);

    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"interval({parameters.Precision})";

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"INTERVAL '{FormatTimeSpanAsInterval((TimeSpan)value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $@"""{FormatTimeSpanAsInterval((TimeSpan)value)}""";

    public static string FormatTimeSpanAsInterval(TimeSpan ts)
        => ts.ToString(
            $@"{(ts < TimeSpan.Zero ? "\\-" : "")}{(ts.Days == 0 ? "" : "d\\ ")}hh\:mm\:ss{(ts.Ticks % 10000000 == 0 ? "" : "\\.FFFFFF")}",
            CultureInfo.InvariantCulture);
}