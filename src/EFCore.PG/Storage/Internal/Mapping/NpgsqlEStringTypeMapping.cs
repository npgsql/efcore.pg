using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// Represents a so-called PostgreSQL E-string literal string, which allows C-style escape sequences.
    /// This is a "virtual" type mapping which is never returned by <see cref="NpgsqlTypeMappingSource"/>.
    /// It is only used internally by some method translators to produce literal strings.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/sql-syntax-lexical.html#SQL-SYNTAX-CONSTANTS
    /// </remarks>
    public class NpgsqlEStringTypeMapping : StringTypeMapping
    {
        public NpgsqlEStringTypeMapping() : base("does_not_exist") {}

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"E'{EscapeSqlLiteral((string)value)}'";
    }
}
