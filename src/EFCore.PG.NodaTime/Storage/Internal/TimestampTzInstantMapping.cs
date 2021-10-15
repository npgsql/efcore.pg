using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using NpgsqlTypes;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
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
            => $"TIMESTAMPTZ '{GenerateLiteralCore(value)}'";

        protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
            => $@"""{GenerateLiteralCore(value)}""";

        private string GenerateLiteralCore(object value)
        {
            var instant = (Instant)value;

            if (!NpgsqlNodaTimeTypeMappingSourcePlugin.DisableDateTimeInfinityConversions)
            {
                if (instant == Instant.MinValue)
                {
                    return "-infinity";
                }

                if (instant == Instant.MaxValue)
                {
                    return "infinity";
                }
            }

            return InstantPattern.ExtendedIso.Format(instant);
        }

        internal static Expression GenerateCodeLiteral(Instant instant)
            => Expression.Call(_fromUnixTimeTicks, Expression.Constant(instant.ToUnixTimeTicks()));

        public override Expression GenerateCodeLiteral(object value)
            => GenerateCodeLiteral((Instant)value);

        private static readonly MethodInfo _fromUnixTimeTicks
            = typeof(Instant).GetRuntimeMethod(nameof(Instant.FromUnixTimeTicks), new[] { typeof(long) })!;
    }
}
