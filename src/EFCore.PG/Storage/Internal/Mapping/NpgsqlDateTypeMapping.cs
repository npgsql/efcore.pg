using System;
using Microsoft.EntityFrameworkCore.Storage.Converters;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlDateTypeMapping : RelationalTypeMapping
    {
        public NpgsqlDateTypeMapping()
            : this(null) {}

        public NpgsqlDateTypeMapping(ValueConverter converter)
            : base("date", typeof(DateTime), converter, System.Data.DbType.Date) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlDateTypeMapping(Converter);

        protected override string GenerateNonNullSqlLiteral(object value)
            => ((DateTime)value).ToString("yyyy-MM-dd");
    }
}
