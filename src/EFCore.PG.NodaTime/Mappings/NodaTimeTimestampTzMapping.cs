using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Mappings
{
    public class NodaTimeTimestampTzMapping : NpgsqlTypeMapping
    {
        public NodaTimeTimestampTzMapping() : base("timestamptz", typeof(ZonedDateTime), NpgsqlDbType.TimestampTz) {}

        protected NodaTimeTimestampTzMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NodaTimeTimestampTzMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NodaTimeTimestampTzMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        //protected override string GenerateNonNullSqlLiteral(object value)
        //    => $"JSON '{EscapeSqlLiteral((string)value)}'";
    }
}
