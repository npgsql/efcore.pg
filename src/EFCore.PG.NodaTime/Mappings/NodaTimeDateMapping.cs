using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Mappings
{
    public class NodaTimeDateMapping : NpgsqlTypeMapping
    {
        public NodaTimeDateMapping() : base("date", typeof(LocalDate), NpgsqlDbType.Date) {}

        protected NodaTimeDateMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NodaTimeDateMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NodaTimeDateMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"DATE '{LocalDatePattern.Iso.Format((LocalDate)value)}'";
    }
}
