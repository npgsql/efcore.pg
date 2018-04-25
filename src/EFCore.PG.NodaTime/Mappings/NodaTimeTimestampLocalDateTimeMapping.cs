using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Mappings
{
    public class NodaTimeTimestampLocalDateTimeMapping : NpgsqlTypeMapping
    {
        public NodaTimeTimestampLocalDateTimeMapping() : base("timestamp", typeof(LocalDateTime), NpgsqlDbType.Timestamp) {}

        protected NodaTimeTimestampLocalDateTimeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NodaTimeTimestampLocalDateTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NodaTimeTimestampLocalDateTimeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMP '{LocalDateTimePattern.ExtendedIso.Format((LocalDateTime)value)}'";
    }
}
