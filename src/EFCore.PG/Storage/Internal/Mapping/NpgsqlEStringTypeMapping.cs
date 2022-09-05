namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

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
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlEStringTypeMapping() : base("does_not_exist", System.Data.DbType.String) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"E'{EscapeSqlLiteral((string)value)}'";
}
