using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlBoolTypeMapping : BoolTypeMapping
    {
        public NpgsqlBoolTypeMapping() : base("bool", System.Data.DbType.Boolean) {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => (bool)value ? "TRUE" : "FALSE";
    }
}
