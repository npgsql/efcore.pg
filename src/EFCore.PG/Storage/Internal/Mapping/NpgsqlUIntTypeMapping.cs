using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlUintTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlUintTypeMapping([NotNull] string storeType, NpgsqlDbType npgsqlDbType)
            : base(storeType, typeof(uint), npgsqlDbType) {}

        protected NpgsqlUintTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlUintTypeMapping(parameters, NpgsqlDbType);
    }
}
