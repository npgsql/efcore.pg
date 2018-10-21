using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlByteArrayTypeMapping : RelationalTypeMapping
    {
        public NpgsqlByteArrayTypeMapping() : base("bytea", typeof(byte[]), System.Data.DbType.Binary) {}

        protected NpgsqlByteArrayTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlByteArrayTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            Check.NotNull(value, nameof(value));
            var bytea = (byte[])value;

            var builder = new StringBuilder(bytea.Length * 2 + 6);

            builder.Append("BYTEA E'\\\\x");
            foreach (var b in bytea)
                builder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
            builder.Append('\'');

            return builder.ToString();
        }
    }
}
