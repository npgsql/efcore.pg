using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlRegconfigTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlRegconfigTypeMapping() : base("regconfig", typeof(string), NpgsqlDbType.Unknown) { }

        protected NpgsqlRegconfigTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Unknown) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlRegconfigTypeMapping(parameters);
    }
}
