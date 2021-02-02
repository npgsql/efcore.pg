using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class LTreeTypeMapping : NpgsqlStringTypeMapping
    {
        public LTreeTypeMapping()
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        typeof(LTree),
                        new ValueConverter<LTree, string>(l => (string)l, s => new(s))),
                    "ltree"),
                NpgsqlDbType.LTree)
        {
        }

        protected LTreeTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.LTree)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new LTreeTypeMapping(parameters);
    }
}
