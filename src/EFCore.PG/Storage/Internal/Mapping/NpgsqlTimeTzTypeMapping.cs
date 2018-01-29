using System;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlTimeTzTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimeTzTypeMapping() : base("timetz", typeof(DateTimeOffset), NpgsqlDbType.TimeTZ) {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => throw new NotImplementedException();
    }
}
