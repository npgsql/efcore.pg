using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Mappings
{
    public class NodaTimeTimestampInstantMapping : NpgsqlTypeMapping
    {
        public NodaTimeTimestampInstantMapping() : base("timestamp", typeof(Instant), NpgsqlDbType.Timestamp) {}

        protected NodaTimeTimestampInstantMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NodaTimeTimestampInstantMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NodaTimeTimestampInstantMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMP '{InstantPattern.ExtendedIso.Format((Instant)value)}'";
    }
}
