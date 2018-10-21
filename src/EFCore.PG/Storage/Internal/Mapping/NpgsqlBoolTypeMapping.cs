using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlBoolTypeMapping : RelationalTypeMapping
    {
        public NpgsqlBoolTypeMapping() : base("boolean", typeof(bool), System.Data.DbType.Boolean) {}

        protected NpgsqlBoolTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlBoolTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => (bool)value ? "TRUE" : "FALSE";
    }
}
