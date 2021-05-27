using System;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlTimestampTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimestampTypeMapping() : base("timestamp without time zone", typeof(DateTime), NpgsqlDbType.Timestamp) {}

        protected NpgsqlTimestampTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Timestamp) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlTimestampTypeMapping(parameters);

        protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
            => parameters.Precision is null ? storeType : $"timestamp({parameters.Precision}) without time zone";

        protected override string GenerateNonNullSqlLiteral(object value)
            => FormattableString.Invariant($"TIMESTAMP '{(DateTime)value:yyyy-MM-dd HH:mm:ss.FFFFFF}'");
    }

    public class NpgsqlTimestampTzTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimestampTzTypeMapping(Type clrType)
            : base("timestamp with time zone", clrType, NpgsqlDbType.TimestampTz) {}

        protected NpgsqlTimestampTzTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimestampTz) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlTimestampTzTypeMapping(parameters);

        protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
            => parameters.Precision is null ? storeType : $"timestamp({parameters.Precision}) with time zone";

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            switch (value)
            {
            case DateTime dt:
                var tz = dt.Kind == DateTimeKind.Local
                    ? $"{dt:zzz}"
                    : " UTC";

                return FormattableString.Invariant($"TIMESTAMPTZ '{dt:yyyy-MM-dd HH:mm:ss.FFFFFF}{tz}'");

            case DateTimeOffset dto:
                return FormattableString.Invariant($"TIMESTAMPTZ '{dto:yyyy-MM-dd HH:mm:ss.FFFFFFzzz}'");

            default:
                throw new InvalidCastException(
                    $"Attempted to generate timestamptz literal for type {value.GetType()}, " +
                    "only DateTime and DateTimeOffset are supported");
            }
        }
    }

    public class NpgsqlDateTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlDateTypeMapping(Type clrType) : base("date", clrType, NpgsqlDbType.Date) {}

        protected NpgsqlDateTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Date) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlDateTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => value switch
            {
                DateTime d => FormattableString.Invariant($"DATE '{d:yyyy-MM-dd}'"),
#if NET6_0_OR_GREATER
                DateOnly d => FormattableString.Invariant($"DATE '{d:yyyy-MM-dd}'"),
#endif
                _ => throw new InvalidCastException($"Can't generate a date SQL literal for CLR type {value.GetType()}")
            };
    }

    public class NpgsqlTimeTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimeTypeMapping(Type clrType) : base("time without time zone", clrType, NpgsqlDbType.Time) {}

        protected NpgsqlTimeTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Time) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlTimeTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => value switch
            {
                TimeSpan ts => ts.Ticks % 10000000 == 0
                    ? FormattableString.Invariant($@"TIME '{value:hh\:mm\:ss}'")
                    : FormattableString.Invariant($@"TIME '{value:hh\:mm\:ss\.FFFFFF}'"),
#if NET6_0_OR_GREATER
                TimeOnly t => t.Ticks % 10000000 == 0
                    ? FormattableString.Invariant($@"TIME '{value:HH\:mm\:ss}'")
                    : FormattableString.Invariant($@"TIME '{value:HH\:mm\:ss\.FFFFFF}'"),
#endif
                _ => throw new InvalidCastException($"Can't generate a time SQL literal for CLR type {value.GetType()}")
            };
    }

    public class NpgsqlTimeTzTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimeTzTypeMapping() : base("time with time zone", typeof(DateTimeOffset), NpgsqlDbType.TimeTz) {}

        protected NpgsqlTimeTzTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimeTz) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlTimeTzTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => FormattableString.Invariant($"TIMETZ '{(DateTimeOffset)value:HH:mm:ss.FFFFFFz}'");
    }

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
            => FormatTimeSpanAsInterval((TimeSpan)value);

        public static string FormatTimeSpanAsInterval(TimeSpan ts)
            => FormattableString.Invariant($"INTERVAL '{ts.ToString($@"{(ts < TimeSpan.Zero ? "\\-" : "")}{(ts.Days == 0 ? "" : "d\\ ")}hh\:mm\:ss{(ts.Ticks % 10000000 == 0 ? "" : "\\.FFFFFF")}")}'");
    }
}
