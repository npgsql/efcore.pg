using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlTsQueryTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTsQueryTypeMapping() : base("tsquery", typeof(NpgsqlTsQuery), NpgsqlDbType.TsQuery) { }

        protected NpgsqlTsQueryTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTsQueryTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlTsQueryTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            Check.NotNull(value, nameof(value));
            var query = (NpgsqlTsQuery)value;
            var builder = new StringBuilder();
            builder.Append("TSQUERY  ");
            var indexOfFirstQuote = builder.Length - 1;
            query.Write(builder, true);
            builder.Replace("'", "''");
            builder[indexOfFirstQuote] = '\'';
            builder.Append("'");
            return builder.ToString();
        }
    }
}
