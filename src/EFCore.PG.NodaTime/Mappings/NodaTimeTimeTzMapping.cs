using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Mappings
{
    public class NodaTimeTimeTzMapping : NpgsqlTypeMapping
    {
        public NodaTimeTimeTzMapping() : base("timetz", typeof(OffsetDateTime), NpgsqlDbType.TimeTz) {}

        protected NodaTimeTimeTzMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NodaTimeTimeTzMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NodaTimeTimeTzMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        //protected override string GenerateNonNullSqlLiteral(object value)
        //    => $"JSON '{EscapeSqlLiteral((string)value)}'";
    }
}
