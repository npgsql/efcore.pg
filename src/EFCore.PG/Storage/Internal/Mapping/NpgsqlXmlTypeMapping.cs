using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlXmlTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlXmlTypeMapping() : base("xml", typeof(string), NpgsqlDbType.Xml) {}

        protected NpgsqlXmlTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Xml) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlXmlTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"XML '{EscapeSqlLiteral((string)value)}'";

        string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
