using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlRegconfigTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlRegconfigTypeMapping() : base("regconfig", typeof(string), NpgsqlDbType.Unknown) { }

        protected NpgsqlRegconfigTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlRegconfigTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlRegconfigTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);
    }
}
