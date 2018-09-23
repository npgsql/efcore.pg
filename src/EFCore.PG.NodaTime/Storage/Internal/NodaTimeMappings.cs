#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    #region timestamp

    public class TimestampInstantMapping : NpgsqlTypeMapping
    {
        public TimestampInstantMapping() : base("timestamp", typeof(Instant), NpgsqlDbType.Timestamp) {}

        protected TimestampInstantMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Timestamp) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampInstantMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TimestampInstantMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMP '{InstantPattern.ExtendedIso.Format((Instant)value)}'";
    }

    public class TimestampLocalDateTimeMapping : NpgsqlTypeMapping
    {
        public TimestampLocalDateTimeMapping() : base("timestamp", typeof(LocalDateTime), NpgsqlDbType.Timestamp) {}

        protected TimestampLocalDateTimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Timestamp) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampLocalDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TimestampLocalDateTimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMP '{LocalDateTimePattern.ExtendedIso.Format((LocalDateTime)value)}'";
    }

    #endregion timestamp

    #region timestamptz

    public class TimestampTzInstantMapping : NpgsqlTypeMapping
    {
        public TimestampTzInstantMapping() : base("timestamp with time zone", typeof(Instant), NpgsqlDbType.TimestampTz) {}

        protected TimestampTzInstantMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimestampTz) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampTzInstantMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TimestampTzInstantMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMPTZ '{InstantPattern.ExtendedIso.Format((Instant)value)}'";
    }

    public class TimestampTzOffsetDateTimeMapping : NpgsqlTypeMapping
    {
        public TimestampTzOffsetDateTimeMapping() : base("timestamp with time zone", typeof(OffsetDateTime), NpgsqlDbType.TimestampTz) {}

        protected TimestampTzOffsetDateTimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimestampTz) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampTzOffsetDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TimestampTzOffsetDateTimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMPTZ '{OffsetDateTimePattern.ExtendedIso.Format((OffsetDateTime)value)}'";
    }

    public class TimestampTzZonedDateTimeMapping : NpgsqlTypeMapping
    {
        static readonly ZonedDateTimePattern Pattern =
            ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFo<G>",
                DateTimeZoneProviders.Tzdb);

        public TimestampTzZonedDateTimeMapping() : base("timestamp with time zone", typeof(ZonedDateTime), NpgsqlDbType.TimestampTz) {}

        protected TimestampTzZonedDateTimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimestampTz) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampTzZonedDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TimestampTzZonedDateTimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMPTZ '{Pattern.Format((ZonedDateTime)value)}'";
    }

    #endregion timestamptz

    #region date

    public class DateMapping : NpgsqlTypeMapping
    {
        public DateMapping() : base("date", typeof(LocalDate), NpgsqlDbType.Date) {}

        protected DateMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Date) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new DateMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new DateMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"DATE '{LocalDatePattern.Iso.Format((LocalDate)value)}'";
    }

    #endregion date

    #region time

    public class TimeMapping : NpgsqlTypeMapping
    {
        public TimeMapping() : base("time", typeof(LocalTime), NpgsqlDbType.Time) {}

        protected TimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Time) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIME '{LocalTimePattern.ExtendedIso.Format((LocalTime)value)}'";
    }

    #endregion time

    #region timetz

    public class TimeTzMapping : NpgsqlTypeMapping
    {
        static readonly OffsetTimePattern Pattern =
            OffsetTimePattern.CreateWithInvariantCulture("HH':'mm':'ss;FFFFFFo<G>");

        public TimeTzMapping() : base("time with time zone", typeof(OffsetTime), NpgsqlDbType.TimeTz) {}

        protected TimeTzMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimeTz) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimeTzMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TimeTzMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMETZ '{Pattern.Format((OffsetTime)value)}'";
    }

    #endregion timetz

    #region interval

    public class IntervalMapping : NpgsqlTypeMapping
    {
        public IntervalMapping() : base("interval", typeof(Period), NpgsqlDbType.Interval) {}

        protected IntervalMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Interval) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new IntervalMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new IntervalMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"INTERVAL '{PeriodPattern.NormalizingIso.Format((Period)value)}'";
    }

    #endregion interval
}
