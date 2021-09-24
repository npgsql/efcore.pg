using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using NpgsqlTypes;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Utilties.Util;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
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
}
