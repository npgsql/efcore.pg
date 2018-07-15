using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlTsVectorTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTsVectorTypeMapping() : base("tsvector", typeof(NpgsqlTsVector), NpgsqlDbType.TsVector) { }

        protected NpgsqlTsVectorTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTsVectorTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlTsVectorTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            Check.NotNull(value, nameof(value));
            var vector = (NpgsqlTsVector)value;
            var builder = new StringBuilder();
            builder.Append("TSVECTOR  ");
            var indexOfFirstQuote = builder.Length - 1;
            builder.Append(vector);
            builder.Replace("'", "''");
            builder[indexOfFirstQuote] = '\'';
            builder.Append("'");
            return builder.ToString();
        }
    }
}
