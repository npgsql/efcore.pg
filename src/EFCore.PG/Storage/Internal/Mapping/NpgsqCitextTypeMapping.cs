using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlCitextTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCitextTypeMapping() : base("citext", typeof(string), NpgsqlDbType.Citext) {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"CITEXT '{EscapeSqlLiteral((string)value)}'";

        string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
