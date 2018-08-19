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

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlUintTypeMapping(parameters, NpgsqlDbType);
    }
}
