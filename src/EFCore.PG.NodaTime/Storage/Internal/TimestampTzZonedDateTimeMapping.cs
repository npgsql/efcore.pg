using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
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
}
