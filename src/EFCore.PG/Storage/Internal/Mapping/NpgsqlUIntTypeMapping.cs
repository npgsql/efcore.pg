using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlUintTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlUintTypeMapping(string storeType, NpgsqlDbType npgsqlDbType)
            : base(storeType, typeof(uint), npgsqlDbType) {}

        protected NpgsqlUintTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlUintTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlUintTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);
    }
}
