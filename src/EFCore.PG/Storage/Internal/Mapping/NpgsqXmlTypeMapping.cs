using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlXmlTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlXmlTypeMapping() : base("xml", typeof(string), NpgsqlDbType.Xml) {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"XML '{EscapeSqlLiteral((string)value)}'";

        string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
