using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlTimestampTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimestampTypeMapping() : base("timestamp without time zone", typeof(DateTime), NpgsqlDbType.Timestamp) {}

        protected NpgsqlTimestampTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTimestampTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlTimestampTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMP '{(DateTime)value:yyyy-MM-dd HH:mm:ss.FFF}'";
    }

    public class NpgsqlTimestampTzTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimestampTzTypeMapping(Type clrType) : base("timestamp with time zone", clrType, NpgsqlDbType.TimestampTZ) {}

        protected NpgsqlTimestampTzTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTimestampTzTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlTimestampTzTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            switch (value)
            {
            case DateTime dt:
                var tz = dt.Kind == DateTimeKind.Local
                    ? $"{dt:zzz}"
                    : " UTC";

                return $"TIMESTAMPTZ '{dt:yyyy-MM-dd HH:mm:ss.FFF}{tz}'";

            case DateTimeOffset dto:
                return $"TIMESTAMPTZ '{dto:yyyy-MM-dd HH:mm:ss.FFFzzz}'";

            default:
                throw new InvalidCastException(
                    $"Attempted to generate timestamptz literal for type {value.GetType()}, " +
                    "only DateTime and DateTimeOffset are supported");
            }
        }
    }

    public class NpgsqlDateTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlDateTypeMapping() : base("date", typeof(DateTime), NpgsqlDbType.Date) {}

        protected NpgsqlDateTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlDateTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlDateTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"DATE '{(DateTime)value:yyyy-MM-dd}'";
    }

    public class NpgsqlTimeTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimeTypeMapping() : base("time without time zone", typeof(TimeSpan), NpgsqlDbType.Time) {}

        protected NpgsqlTimeTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTimeTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlTimeTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var ts = (TimeSpan)value;
            return ts.Milliseconds == 0
                ? $@"TIME '{(TimeSpan)value:hh\:mm\:ss}'"
                : $@"TIME '{(TimeSpan)value:hh\:mm\:ss\.FFF}'";
        }
    }

    public class NpgsqlTimeTzTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimeTzTypeMapping() : base("time with time zone", typeof(DateTimeOffset), NpgsqlDbType.TimeTZ) {}

        protected NpgsqlTimeTzTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTimeTzTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlTimeTzTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMETZ '{(DateTimeOffset)value:HH:mm:ss.FFFz}'";
    }

    public class NpgsqlIntervalTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlIntervalTypeMapping() : base("interval", typeof(TimeSpan), NpgsqlDbType.Interval) {}

        protected NpgsqlIntervalTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlIntervalTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlIntervalTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var ts = (TimeSpan)value;
            return $"INTERVAL '{ts.ToString($@"{(ts < TimeSpan.Zero ? "\\-" : "")}{(ts.Days == 0 ? "" : "d\\ ")}hh\:mm\:ss{(ts.Milliseconds == 0 ? "" : $"\\.FFF")}")}'";
        }
    }
}
