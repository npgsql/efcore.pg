using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Mappings
{
    public class NodaTimeTimeMapping : NpgsqlTypeMapping
    {
        public NodaTimeTimeMapping() : base("time", typeof(LocalTime), NpgsqlDbType.Time) {}

        protected NodaTimeTimeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NodaTimeTimeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NodaTimeTimeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIME '{LocalTimePattern.ExtendedIso.Format((LocalTime)value)}'";
    }
}
