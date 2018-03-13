using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlJsonbTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlJsonbTypeMapping() : base("jsonb", typeof(string), NpgsqlDbType.Jsonb) {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"JSONB '{EscapeSqlLiteral((string)value)}'";

        string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
