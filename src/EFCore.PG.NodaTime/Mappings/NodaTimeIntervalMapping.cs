using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Mappings
{
    public class NodaTimeIntervalMapping : NpgsqlTypeMapping
    {
        public NodaTimeIntervalMapping() : base("interval", typeof(Period), NpgsqlDbType.Interval) {}

        protected NodaTimeIntervalMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NodaTimeIntervalMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NodaTimeIntervalMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"INTERVAL '{PeriodPattern.NormalizingIso.Format((Period)value)}'";
    }
}
