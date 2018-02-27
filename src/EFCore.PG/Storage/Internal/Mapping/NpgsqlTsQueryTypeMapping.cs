using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlTsQueryTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTsQueryTypeMapping() : base("tsquery", typeof(NpgsqlTsQuery), NpgsqlDbType.TsQuery) { }

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
