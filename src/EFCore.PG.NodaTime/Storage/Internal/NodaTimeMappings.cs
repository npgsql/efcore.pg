using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;
using System.Linq.Expressions;
using System.Reflection;
using static Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Utilties.Util;

// ReSharper disable once CheckNamespace
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

        // GenerateCodeLiteral isn't implemented because round-tripping Instant would require rendering an expression such as
        // NodaConstants.UnixEpoch + Duration.FromNanoseconds(nanoseconds), which isn't currently supported by EF Core's code
        // generator
    }

    public class TimestampLocalDateTimeMapping : NpgsqlTypeMapping
    {
        static readonly ConstructorInfo _ctorInfo1 =
            typeof(LocalDateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        static readonly ConstructorInfo _ctorInfo2 =
            typeof(LocalDateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        static readonly MethodInfo _plusNanosecondsMethodInfo =
            typeof(LocalDateTime).GetMethod(nameof(LocalDateTime.PlusNanoseconds), new[] { typeof(long) });

        public TimestampLocalDateTimeMapping() : base("timestamp", typeof(LocalDateTime), NpgsqlDbType.Timestamp) {}

        protected TimestampLocalDateTimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Timestamp) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampLocalDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TimestampLocalDateTimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMP '{LocalDateTimePattern.ExtendedIso.Format((LocalDateTime)value)}'";

        public override Expression GenerateCodeLiteral(object value) => GenerateCodeLiteral((LocalDateTime)value);

        internal static Expression GenerateCodeLiteral(LocalDateTime dateTime)
        {
            if (dateTime.Second == 0 && dateTime.NanosecondOfSecond == 0)
                return ConstantNew(_ctorInfo1, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute);

            var newExpr = ConstantNew(_ctorInfo2, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

            return dateTime.NanosecondOfSecond == 0
                ? (Expression)newExpr
                : Expression.Call(newExpr, _plusNanosecondsMethodInfo, Expression.Constant((long)dateTime.NanosecondOfSecond));
        }
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

        // GenerateCodeLiteral isn't implemented because round-tripping Instant would require rendering an expression such as
        // NodaConstants.UnixEpoch + Duration.FromNanoseconds(nanoseconds), which isn't currently supported by EF Core's code
        // generator
    }

    public class TimestampTzOffsetDateTimeMapping : NpgsqlTypeMapping
    {
        static readonly ConstructorInfo _ctorInfo =
            typeof(OffsetDateTime).GetConstructor(new[] { typeof(LocalDateTime), typeof(Offset) });

        static readonly MethodInfo _offsetFactoryMethodInfo1 =
            typeof(Offset).GetMethod(nameof(Offset.FromHours), new[] { typeof(int) });

        static readonly MethodInfo _offsetFactoryMethodInfo2 =
            typeof(Offset).GetMethod(nameof(Offset.FromSeconds), new[] { typeof(int) });

        public TimestampTzOffsetDateTimeMapping() : base("timestamp with time zone", typeof(OffsetDateTime), NpgsqlDbType.TimestampTz) {}

        protected TimestampTzOffsetDateTimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimestampTz) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampTzOffsetDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TimestampTzOffsetDateTimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMPTZ '{OffsetDateTimePattern.ExtendedIso.Format((OffsetDateTime)value)}'";

        public override Expression GenerateCodeLiteral(object value)
        {
            var offsetDateTime = (OffsetDateTime)value;
            var offsetSeconds = offsetDateTime.Offset.Seconds;

            return Expression.New(_ctorInfo,
                TimestampLocalDateTimeMapping.GenerateCodeLiteral(offsetDateTime.LocalDateTime),
                offsetSeconds % 3600 == 0
                    ? ConstantCall(_offsetFactoryMethodInfo1, offsetSeconds / 3600)
                    : ConstantCall(_offsetFactoryMethodInfo2, offsetSeconds));
        }
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

        // GenerateCodeLiteral isn't implemented because round-tripping DateTimeZone would require a property access into
        // DateTimeZoneProviders, which isn't currently supported by EF Core's code generator
    }

    #endregion timestamptz

    #region date

    public class DateMapping : NpgsqlTypeMapping
    {
        static readonly ConstructorInfo _ctorInfo =
            typeof(LocalDate).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) });

        public DateMapping() : base("date", typeof(LocalDate), NpgsqlDbType.Date) {}

        protected DateMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Date) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new DateMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new DateMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"DATE '{LocalDatePattern.Iso.Format((LocalDate)value)}'";

        public override Expression GenerateCodeLiteral(object value)
        {
            var date = (LocalDate)value;
            return ConstantNew(_ctorInfo, date.Year, date.Month, date.Day);
        }
    }

    #endregion date

    #region time

    public class TimeMapping : NpgsqlTypeMapping
    {
        static readonly ConstructorInfo _ctorInfo1 =
            typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int) });

        static readonly ConstructorInfo _ctorInfo2 =
            typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) });

        static readonly MethodInfo _factoryMethodInfo =
            typeof(LocalTime).GetMethod(nameof(LocalTime.FromHourMinuteSecondNanosecond),
                new[] { typeof(int), typeof(int), typeof(int), typeof(long) });

        public TimeMapping() : base("time", typeof(LocalTime), NpgsqlDbType.Time) {}

        protected TimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Time) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new TimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIME '{LocalTimePattern.ExtendedIso.Format((LocalTime)value)}'";

        public override Expression GenerateCodeLiteral(object value)
        {
            var time = (LocalTime)value;

            if (time.NanosecondOfSecond != 0)
                return ConstantCall(_factoryMethodInfo, time.Hour, time.Minute, time.Second, (long)time.NanosecondOfSecond);

            if (time.Second != 0)
                return ConstantNew(_ctorInfo2, time.Hour, time.Minute, time.Second);

            return ConstantNew(_ctorInfo1, time.Hour, time.Minute);
        }
    }

    #endregion time

    #region timetz

    public class TimeTzMapping : NpgsqlTypeMapping
    {
        static readonly ConstructorInfo _ctorInfo =
            typeof(OffsetTime).GetConstructor(new[] { typeof(LocalTime), typeof(Offset) });

        static readonly ConstructorInfo _localTimeCtorInfo1 =
            typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int) });

        static readonly ConstructorInfo _localTimeCtorInfo2 =
            typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) });

        static readonly MethodInfo _localTimeFactoryMethodInfo =
            typeof(LocalTime).GetMethod(nameof(LocalTime.FromHourMinuteSecondNanosecond),
                new[] { typeof(int), typeof(int), typeof(int), typeof(long) });

        static readonly MethodInfo _offsetFactoryMethodInfo1 =
            typeof(Offset).GetMethod(nameof(Offset.FromHours), new[] { typeof(int) });

        static readonly MethodInfo _offsetFactoryMethodInfo2 =
            typeof(Offset).GetMethod(nameof(Offset.FromSeconds), new[] { typeof(int) });

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

        public override Expression GenerateCodeLiteral(object value)
        {
            var offsetTime = (OffsetTime)value;
            var offsetSeconds = offsetTime.Offset.Seconds;

            Expression newLocalTimeExpr = null;
            if (offsetTime.NanosecondOfSecond != 0)
                newLocalTimeExpr = ConstantCall(_localTimeFactoryMethodInfo, offsetTime.Hour, offsetTime.Minute, offsetTime.Second, (long)offsetTime.NanosecondOfSecond);
            else if (offsetTime.Second != 0)
                newLocalTimeExpr = ConstantNew(_localTimeCtorInfo2, offsetTime.Hour, offsetTime.Minute, offsetTime.Second);
            else
                newLocalTimeExpr = ConstantNew(_localTimeCtorInfo1, offsetTime.Hour, offsetTime.Minute);

            return Expression.New(_ctorInfo,
                newLocalTimeExpr,
                offsetSeconds % 3600 == 0
                    ? ConstantCall(_offsetFactoryMethodInfo1, offsetSeconds / 3600)
                    : ConstantCall(_offsetFactoryMethodInfo2, offsetSeconds));
        }
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

        // GenerateCodeLiteral isn't implemented because round-tripping Period would require either using the plus operator
        // to compose the components (years + months...), or setting properties on PeriodBuilder, neither of which is
        // currently supported by EF Core's code generator
    }

    #endregion interval
}
