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
    public class NpgsqlJsonbTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlJsonbTypeMapping() : base("jsonb", typeof(string), NpgsqlDbType.Jsonb) {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"JSONB '{EscapeSqlLiteral((string)value)}'";

        string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
