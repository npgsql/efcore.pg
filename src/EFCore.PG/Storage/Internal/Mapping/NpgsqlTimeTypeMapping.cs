using System;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlTimeTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimeTypeMapping() : base("time without time zone", typeof(DateTime), NpgsqlDbType.Time) {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => throw new NotImplementedException();
    }
}
