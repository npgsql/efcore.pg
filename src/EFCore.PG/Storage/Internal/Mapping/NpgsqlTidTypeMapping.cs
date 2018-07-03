using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlTidTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTidTypeMapping()
            : base("tid", typeof(NpgsqlTid), NpgsqlDbType.Tid) {}

        protected NpgsqlTidTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTidTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlTidTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var tid = (NpgsqlTid)value;
            var builder = new StringBuilder("TID '(");
            builder.Append(tid.BlockNumber);
            builder.Append(',');
            builder.Append(tid.OffsetNumber);
            builder.Append(")'");
            return builder.ToString();
        }
    }
}
