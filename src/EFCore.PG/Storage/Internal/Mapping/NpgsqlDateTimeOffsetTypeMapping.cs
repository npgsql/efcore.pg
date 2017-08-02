using System.Data;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
    {
        private const string DateTimeOffsetFormatConst = "{0:yyyy-MM-ddTHH:mm:ss.fffzzz}";

        public NpgsqlDateTimeOffsetTypeMapping(
            [NotNull] string storeType,
            [NotNull] DbType? dbType = System.Data.DbType.DateTimeOffset)
            : base(storeType, dbType: dbType)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlDateTimeOffsetTypeMapping(storeType, DbType);

        protected override string SqlLiteralFormatString => "'" + DateTimeOffsetFormatConst + "'";
    }
}
