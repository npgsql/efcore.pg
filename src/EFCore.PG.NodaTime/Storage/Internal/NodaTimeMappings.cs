using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;
using System.Linq.Expressions;
using System.Reflection;
using NodaTime.TimeZones;
using static Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Utilties.Util;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    #region timestamp

    public class TimestampLocalDateTimeMapping : NpgsqlTypeMapping
    {
        private static readonly ConstructorInfo ConstructorWithMinutes =
            typeof(LocalDateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) })!;

        private static readonly ConstructorInfo ConstructorWithSeconds =
            typeof(LocalDateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) })!;

        private static readonly MethodInfo PlusNanosecondsMethod =
            typeof(LocalDateTime).GetMethod(nameof(LocalDateTime.PlusNanoseconds), new[] { typeof(long) })!;

        public TimestampLocalDateTimeMapping() : base("timestamp", typeof(LocalDateTime), NpgsqlDbType.Timestamp) {}

        protected TimestampLocalDateTimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Timestamp) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new TimestampLocalDateTimeMapping(Parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampLocalDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new TimestampLocalDateTimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMP '{LocalDateTimePattern.ExtendedIso.Format((LocalDateTime)value)}'";

        public override Expression GenerateCodeLiteral(object value) => GenerateCodeLiteral((LocalDateTime)value);

        internal static Expression GenerateCodeLiteral(LocalDateTime dateTime)
        {
            if (dateTime.Second == 0 && dateTime.NanosecondOfSecond == 0)
                return ConstantNew(ConstructorWithMinutes, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute);

            var newExpr = ConstantNew(ConstructorWithSeconds, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

            return dateTime.NanosecondOfSecond == 0
                ? (Expression)newExpr
                : Expression.Call(newExpr, PlusNanosecondsMethod, Expression.Constant((long)dateTime.NanosecondOfSecond));
        }
    }

    // Should only be used only with EnableLegacyTimestampBehavior.
    // However, when upgrading to 6.0 with existing migrations, model snapshots still contain old mappings (Instant mapped to timestamp),
    // and EF Core's model differ expects type mappings to be found for these. See https://github.com/dotnet/efcore/issues/26168.
    public class LegacyTimestampInstantMapping : NpgsqlTypeMapping
    {
        public LegacyTimestampInstantMapping()
            : base("timestamp", typeof(Instant), NpgsqlDbType.Timestamp)
        {
        }

        protected LegacyTimestampInstantMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Timestamp) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new LegacyTimestampInstantMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new LegacyTimestampInstantMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new LegacyTimestampInstantMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMP '{InstantPattern.ExtendedIso.Format((Instant)value)}'";

        public override Expression GenerateCodeLiteral(object value)
            => TimestampTzInstantMapping.GenerateCodeLiteral((Instant)value);
    }

    #endregion timestamp

    #region timestamptz

    public class TimestampTzInstantMapping : NpgsqlTypeMapping
    {
        public TimestampTzInstantMapping() : base("timestamp with time zone", typeof(Instant), NpgsqlDbType.TimestampTz) {}

        protected TimestampTzInstantMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimestampTz) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new TimestampTzInstantMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampTzInstantMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new TimestampTzInstantMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMPTZ '{InstantPattern.ExtendedIso.Format((Instant)value)}'";

        internal static Expression GenerateCodeLiteral(Instant instant)
            => Expression.Call(_fromUnixTimeTicks, Expression.Constant(instant.ToUnixTimeTicks()));

        public override Expression GenerateCodeLiteral(object value)
            => GenerateCodeLiteral((Instant)value);

        private static readonly MethodInfo _fromUnixTimeTicks
            = typeof(Instant).GetRuntimeMethod(nameof(Instant.FromUnixTimeTicks), new[] { typeof(long) })!;
    }

    public class TimestampTzOffsetDateTimeMapping : NpgsqlTypeMapping
    {
        private static readonly ConstructorInfo Constructor =
            typeof(OffsetDateTime).GetConstructor(new[] { typeof(LocalDateTime), typeof(Offset) })!;

        private static readonly MethodInfo OffsetFromHoursMethod =
            typeof(Offset).GetMethod(nameof(Offset.FromHours), new[] { typeof(int) })!;

        private static readonly MethodInfo OffsetFromSecondsMethod =
            typeof(Offset).GetMethod(nameof(Offset.FromSeconds), new[] { typeof(int) })!;

        public TimestampTzOffsetDateTimeMapping() : base("timestamp with time zone", typeof(OffsetDateTime), NpgsqlDbType.TimestampTz) {}

        protected TimestampTzOffsetDateTimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimestampTz) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new TimestampTzOffsetDateTimeMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampTzOffsetDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new TimestampTzOffsetDateTimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMPTZ '{OffsetDateTimePattern.ExtendedIso.Format((OffsetDateTime)value)}'";

        public override Expression GenerateCodeLiteral(object value)
        {
            var offsetDateTime = (OffsetDateTime)value;
            var offsetSeconds = offsetDateTime.Offset.Seconds;

            return Expression.New(Constructor,
                TimestampLocalDateTimeMapping.GenerateCodeLiteral(offsetDateTime.LocalDateTime),
                offsetSeconds % 3600 == 0
                    ? ConstantCall(OffsetFromHoursMethod, offsetSeconds / 3600)
                    : ConstantCall(OffsetFromSecondsMethod, offsetSeconds));
        }
    }

    public class TimestampTzZonedDateTimeMapping : NpgsqlTypeMapping
    {
        private static readonly ZonedDateTimePattern Pattern =
            ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFo<G>",
                DateTimeZoneProviders.Tzdb);

        public TimestampTzZonedDateTimeMapping() : base("timestamp with time zone", typeof(ZonedDateTime), NpgsqlDbType.TimestampTz) {}

        protected TimestampTzZonedDateTimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimestampTz) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new TimestampTzZonedDateTimeMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimestampTzZonedDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new TimestampTzZonedDateTimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMPTZ '{Pattern.Format((ZonedDateTime)value)}'";

        public override Expression GenerateCodeLiteral(object value)
        {
            var zonedDateTime = (ZonedDateTime)value;

            return Expression.New(
                Constructor,
                TimestampTzInstantMapping.GenerateCodeLiteral(zonedDateTime.ToInstant()),
                Expression.Call(
                    Expression.MakeMemberAccess(
                        null,
                        TzdbDateTimeZoneSourceDefaultMember),
                    ForIdMethod,
                    Expression.Constant(zonedDateTime.Zone.Id)));
        }

        private static readonly ConstructorInfo Constructor =
            typeof(ZonedDateTime).GetConstructor(new[] { typeof(Instant), typeof(DateTimeZone) })!;

        private static readonly MemberInfo TzdbDateTimeZoneSourceDefaultMember =
            typeof(TzdbDateTimeZoneSource).GetMember(nameof(TzdbDateTimeZoneSource.Default))[0];

        private static readonly MethodInfo ForIdMethod =
            typeof(TzdbDateTimeZoneSource).GetRuntimeMethod(
                nameof(TzdbDateTimeZoneSource.ForId),
                new[] { typeof(string) })!;
    }

    #endregion timestamptz

    #region date

    public class DateMapping : NpgsqlTypeMapping
    {
        private static readonly ConstructorInfo Constructor =
            typeof(LocalDate).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) })!;

        public DateMapping() : base("date", typeof(LocalDate), NpgsqlDbType.Date) {}

        protected DateMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Date) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new DateMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new DateMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new DateMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"DATE '{LocalDatePattern.Iso.Format((LocalDate)value)}'";

        public override Expression GenerateCodeLiteral(object value)
        {
            var date = (LocalDate)value;
            return ConstantNew(Constructor, date.Year, date.Month, date.Day);
        }
    }

    #endregion date

    #region time

    public class TimeMapping : NpgsqlTypeMapping
    {
        private static readonly ConstructorInfo ConstructorWithMinutes =
            typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int) })!;

        private static readonly ConstructorInfo ConstructorWithSeconds =
            typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) })!;

        private static readonly MethodInfo FromHourMinuteSecondNanosecondMethod =
            typeof(LocalTime).GetMethod(nameof(LocalTime.FromHourMinuteSecondNanosecond),
                new[] { typeof(int), typeof(int), typeof(int), typeof(long) })!;

        public TimeMapping() : base("time", typeof(LocalTime), NpgsqlDbType.Time) {}

        protected TimeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Time) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new TimeMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new TimeMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIME '{LocalTimePattern.ExtendedIso.Format((LocalTime)value)}'";

        public override Expression GenerateCodeLiteral(object value)
        {
            var time = (LocalTime)value;
            return time.NanosecondOfSecond != 0
                ? ConstantCall(FromHourMinuteSecondNanosecondMethod, time.Hour, time.Minute, time.Second, (long)time.NanosecondOfSecond)
                : (Expression)(time.Second != 0
                    ? ConstantNew(ConstructorWithSeconds, time.Hour, time.Minute, time.Second)
                    : ConstantNew(ConstructorWithMinutes, time.Hour, time.Minute));
        }
    }

    #endregion time

    #region timetz

    public class TimeTzMapping : NpgsqlTypeMapping
    {
        private static readonly ConstructorInfo OffsetTimeConstructor =
            typeof(OffsetTime).GetConstructor(new[] { typeof(LocalTime), typeof(Offset) })!;

        private static readonly ConstructorInfo LocalTimeConstructorWithMinutes =
            typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int) })!;

        private static readonly ConstructorInfo LocalTimeConstructorWithSeconds =
            typeof(LocalTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) })!;

        private static readonly MethodInfo LocalTimeFromHourMinuteSecondNanosecondMethod =
            typeof(LocalTime).GetMethod(nameof(LocalTime.FromHourMinuteSecondNanosecond),
                new[] { typeof(int), typeof(int), typeof(int), typeof(long) })!;

        private static readonly MethodInfo OffsetFromHoursMethod =
            typeof(Offset).GetMethod(nameof(Offset.FromHours), new[] { typeof(int) })!;

        private static readonly MethodInfo OffsetFromSeconds =
            typeof(Offset).GetMethod(nameof(Offset.FromSeconds), new[] { typeof(int) })!;

        private static readonly OffsetTimePattern Pattern =
            OffsetTimePattern.CreateWithInvariantCulture("HH':'mm':'ss;FFFFFFo<G>");

        public TimeTzMapping() : base("time with time zone", typeof(OffsetTime), NpgsqlDbType.TimeTz) {}

        protected TimeTzMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimeTz) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new TimeTzMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new TimeTzMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new TimeTzMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMETZ '{Pattern.Format((OffsetTime)value)}'";

        public override Expression GenerateCodeLiteral(object value)
        {
            var offsetTime = (OffsetTime)value;
            var offsetSeconds = offsetTime.Offset.Seconds;

            Expression newLocalTimeExpr;
            if (offsetTime.NanosecondOfSecond != 0)
                newLocalTimeExpr = ConstantCall(LocalTimeFromHourMinuteSecondNanosecondMethod, offsetTime.Hour, offsetTime.Minute, offsetTime.Second, (long)offsetTime.NanosecondOfSecond);
            else if (offsetTime.Second != 0)
                newLocalTimeExpr = ConstantNew(LocalTimeConstructorWithSeconds, offsetTime.Hour, offsetTime.Minute, offsetTime.Second);
            else
                newLocalTimeExpr = ConstantNew(LocalTimeConstructorWithMinutes, offsetTime.Hour, offsetTime.Minute);

            return Expression.New(OffsetTimeConstructor,
                newLocalTimeExpr,
                offsetSeconds % 3600 == 0
                    ? ConstantCall(OffsetFromHoursMethod, offsetSeconds / 3600)
                    : ConstantCall(OffsetFromSeconds, offsetSeconds));
        }
    }

    #endregion timetz

    #region interval

    public class PeriodIntervalMapping : NpgsqlTypeMapping
    {
        private static readonly MethodInfo FromYears = typeof(Period).GetRuntimeMethod(nameof(Period.FromYears), new[] { typeof(int) })!;
        private static readonly MethodInfo FromMonths = typeof(Period).GetRuntimeMethod(nameof(Period.FromMonths), new[] { typeof(int) })!;
        private static readonly MethodInfo FromWeeks = typeof(Period).GetRuntimeMethod(nameof(Period.FromWeeks), new[] { typeof(int) })!;
        private static readonly MethodInfo FromDays = typeof(Period).GetRuntimeMethod(nameof(Period.FromDays), new[] { typeof(int) })!;
        private static readonly MethodInfo FromHours = typeof(Period).GetRuntimeMethod(nameof(Period.FromHours), new[] { typeof(long) })!;
        private static readonly MethodInfo FromMinutes = typeof(Period).GetRuntimeMethod(nameof(Period.FromMinutes), new[] { typeof(long) })!;
        private static readonly MethodInfo FromSeconds = typeof(Period).GetRuntimeMethod(nameof(Period.FromSeconds), new[] { typeof(long) })!;
        private static readonly MethodInfo FromMilliseconds = typeof(Period).GetRuntimeMethod(nameof(Period.FromMilliseconds), new[] { typeof(long) })!;
        private static readonly MethodInfo FromNanoseconds = typeof(Period).GetRuntimeMethod(nameof(Period.FromNanoseconds), new[] { typeof(long) })!;

        private static readonly PropertyInfo Zero = typeof(Period).GetProperty(nameof(Period.Zero))!;

        public PeriodIntervalMapping() : base("interval", typeof(Period), NpgsqlDbType.Interval) {}

        protected PeriodIntervalMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Interval) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new PeriodIntervalMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new PeriodIntervalMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new PeriodIntervalMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"INTERVAL '{PeriodPattern.NormalizingIso.Format((Period)value)}'";

        public override Expression GenerateCodeLiteral(object value)
        {
            var period = (Period)value;
            Expression? e = null;

            if (period.Years != 0)
                Compose(Expression.Call(FromYears, Expression.Constant(period.Years)));
            if (period.Months != 0)
                Compose(Expression.Call(FromMonths, Expression.Constant(period.Months)));
            if (period.Weeks != 0)
                Compose(Expression.Call(FromWeeks, Expression.Constant(period.Weeks)));
            if (period.Days != 0)
                Compose(Expression.Call(FromDays, Expression.Constant(period.Days)));
            if (period.Hours != 0)
                Compose(Expression.Call(FromHours, Expression.Constant(period.Hours)));
            if (period.Minutes != 0)
                Compose(Expression.Call(FromMinutes, Expression.Constant(period.Minutes)));
            if (period.Seconds != 0)
                Compose(Expression.Call(FromSeconds, Expression.Constant(period.Seconds)));
            if (period.Milliseconds != 0)
                Compose(Expression.Call(FromMilliseconds, Expression.Constant(period.Milliseconds)));
            if (period.Nanoseconds != 0)
                Compose(Expression.Call(FromNanoseconds, Expression.Constant(period.Nanoseconds)));

            return e ?? Expression.MakeMemberAccess(null, Zero);

            void Compose(Expression toAdd) => e = e is null ? toAdd : Expression.Add(e, toAdd);
        }
    }

    public class DurationIntervalMapping : NpgsqlTypeMapping
    {
        private static readonly MethodInfo FromDays = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromDays), new[] { typeof(int) })!;
        private static readonly MethodInfo FromHours = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromHours), new[] { typeof(int) })!;
        private static readonly MethodInfo FromMinutes = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromMinutes), new[] { typeof(long) })!;
        private static readonly MethodInfo FromSeconds = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromSeconds), new[] { typeof(long) })!;
        private static readonly MethodInfo FromMilliseconds = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromMilliseconds), new[] { typeof(long) })!;

        private static readonly PropertyInfo Zero = typeof(Duration).GetProperty(nameof(Duration.Zero))!;

        public DurationIntervalMapping() : base("interval", typeof(Duration), NpgsqlDbType.Interval) {}

        protected DurationIntervalMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Interval) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new DurationIntervalMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new DurationIntervalMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new DurationIntervalMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => NpgsqlIntervalTypeMapping.FormatTimeSpanAsInterval(((Duration)value).ToTimeSpan());

        public override Expression GenerateCodeLiteral(object value)
        {
            var duration = (Duration)value;
            Expression? e = null;

            if (duration.Days != 0)
                Compose(Expression.Call(FromDays, Expression.Constant(duration.Days)));
            if (duration.Hours != 0)
                Compose(Expression.Call(FromHours, Expression.Constant(duration.Hours)));
            if (duration.Minutes != 0)
                Compose(Expression.Call(FromMinutes, Expression.Constant((long)duration.Minutes)));
            if (duration.Seconds != 0)
                Compose(Expression.Call(FromSeconds, Expression.Constant((long)duration.Seconds)));
            if (duration.Milliseconds != 0)
                Compose(Expression.Call(FromMilliseconds, Expression.Constant((long)duration.Milliseconds)));

            return e ?? Expression.MakeMemberAccess(null, Zero);

            void Compose(Expression toAdd) => e = e is null ? toAdd : Expression.Add(e, toAdd);
        }
    }

    #endregion interval
}
