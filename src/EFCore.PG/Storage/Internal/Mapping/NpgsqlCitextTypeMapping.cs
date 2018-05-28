using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlCitextTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCitextTypeMapping() : base("citext", typeof(string), NpgsqlDbType.Citext) {}

        protected NpgsqlCitextTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlCitextTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlCitextTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"CITEXT '{EscapeSqlLiteral((string)value)}'";

        string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
