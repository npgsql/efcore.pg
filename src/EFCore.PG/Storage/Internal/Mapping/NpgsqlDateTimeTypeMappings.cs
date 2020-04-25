using System;
using JetBrains.Annotations;
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

        protected override string GenerateNonNullSqlLiteral(object value)
            => FormattableString.Invariant($"TIMESTAMP '{(DateTime)value:yyyy-MM-dd HH:mm:ss.FFFFFF}'");
    }

    public class NpgsqlTimestampTzTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimestampTzTypeMapping([NotNull] Type clrType)
            : base("timestamp with time zone", clrType, NpgsqlDbType.TimestampTz) {}

        protected NpgsqlTimestampTzTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimestampTz) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlTimestampTzTypeMapping(parameters);

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
        public NpgsqlDateTypeMapping() : base("date", typeof(DateTime), NpgsqlDbType.Date) {}

        protected NpgsqlDateTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Date) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlDateTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => FormattableString.Invariant($"DATE '{(DateTime)value:yyyy-MM-dd}'");
    }

    public class NpgsqlTimeTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimeTypeMapping() : base("time without time zone", typeof(TimeSpan), NpgsqlDbType.Time) {}

        protected NpgsqlTimeTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Time) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlTimeTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var ts = (TimeSpan)value;
            return ts.Ticks % 10000000 == 0
                ? FormattableString.Invariant($@"TIME '{(TimeSpan)value:hh\:mm\:ss}'")
                : FormattableString.Invariant($@"TIME '{(TimeSpan)value:hh\:mm\:ss\.FFFFFF}'");
        }
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

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var ts = (TimeSpan)value;
            return FormattableString.Invariant($"INTERVAL '{ts.ToString($@"{(ts < TimeSpan.Zero ? "\\-" : "")}{(ts.Days == 0 ? "" : "d\\ ")}hh\:mm\:ss{(ts.Ticks % 10000000 == 0 ? "" : "\\.FFFFFF")}")}'");
        }
    }
}
