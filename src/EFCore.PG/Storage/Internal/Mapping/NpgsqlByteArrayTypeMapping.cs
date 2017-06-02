using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlByteArrayTypeMapping : ByteArrayTypeMapping
    {
        public NpgsqlByteArrayTypeMapping() : base("bytea", System.Data.DbType.Binary) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            Check.NotNull(value, nameof(value));
            var bytea = (byte[])value;

            var builder = new StringBuilder(bytea.Length * 2 + 6);

            builder.Append("E'\\\\x");
            foreach (var b in bytea)
                builder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
            builder.Append('\'');

            return builder.ToString();
        }
    }
}
