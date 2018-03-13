using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlCitextTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCitextTypeMapping() : base("citext", typeof(string), NpgsqlDbType.Citext) {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"CITEXT '{EscapeSqlLiteral((string)value)}'";

        string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
