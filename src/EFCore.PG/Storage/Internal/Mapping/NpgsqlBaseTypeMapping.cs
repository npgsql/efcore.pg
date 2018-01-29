#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Utilities;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public sealed class NpgsqlBaseTypeMapping : NpgsqlTypeMapping
    {
        readonly Func<object, string> _literalValueGenerator;

        internal NpgsqlBaseTypeMapping([NotNull] string storeType, [NotNull] Type clrType, NpgsqlDbType npgsqlDbType)
            : this(storeType, clrType, null, npgsqlDbType)
        {}

        internal NpgsqlBaseTypeMapping([NotNull] string storeType, [NotNull] Type clrType, ValueConverter converter, NpgsqlDbType npgsqlDbType)
            : base(storeType, clrType, converter, npgsqlDbType)
        {
            _literalValueGenerator = ResolveLiteralValueGenerator(storeType);
        }

        protected override string GenerateNonNullSqlLiteral([NotNull] object value)
            => _literalValueGenerator(value);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlBaseTypeMapping(storeType, ClrType, Converter, NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlBaseTypeMapping(StoreType, ClrType, ComposeConverter(converter), NpgsqlDbType);

        static Func<object, string> ResolveLiteralValueGenerator(string storeType)
        {
            if (LiteralValueGenerators.TryGetValue(storeType, out var generator))
                return generator;

            var openParen = storeType.IndexOf("(", StringComparison.Ordinal);
            if (openParen > 0)
            {
                var baseStoreType = storeType.Substring(0, openParen).ToLower();
                if (LiteralValueGenerators.TryGetValue(baseStoreType, out generator))
                    return generator;
            }

            return NullGenerator;
        }

        static readonly Func<object, string> NullGenerator = v => v.ToString();

        static readonly Dictionary<string, Func<object, string>> LiteralValueGenerators =
            new Dictionary<string, Func<object, string>>
            {
                { "bool", v => (bool)v ? "TRUE" : "FALSE"},
                { "bytea", v => {
                    Check.NotNull(v, nameof(v));
                    var bytea = (byte[])v;

                    var builder = new StringBuilder(bytea.Length * 2 + 6);
                    builder.Append("E'\\\\x");
                    foreach (var b in bytea)
                        builder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
                    builder.Append('\'');
                    return builder.ToString();
                }},

                { "char", v => $"'{EscapeSqlLiteral((string)v)}'"},
                { "text", v => $"'{EscapeSqlLiteral((string)v)}'"},
                { "varchar", v => $"'{EscapeSqlLiteral((string)v)}'"},

                { "timestamp", v => string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-ddTHH:mm:ss.fffzzz}", Check.NotNull(v, nameof(v)))},
                { "timestamp without time zone", v => string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-ddTHH:mm:ss.fffzzz}", Check.NotNull(v, nameof(v)))},
                { "timestamptz", v => string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-ddTHH:mm:ss.fffzzz}", Check.NotNull(v, nameof(v)))},
                { "timestamp with time zone", v => string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-ddTHH:mm:ss.fffzzz}", Check.NotNull(v, nameof(v)))},
            };

        static string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
