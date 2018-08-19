using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlJsonTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlJsonTypeMapping() : base("json", typeof(string), NpgsqlDbType.Json) {}

        protected NpgsqlJsonTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Json) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlJsonTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"JSON '{EscapeSqlLiteral((string)value)}'";

        string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
