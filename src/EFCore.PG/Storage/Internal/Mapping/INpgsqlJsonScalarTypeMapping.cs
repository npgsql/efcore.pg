namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     A type mapping whose JSON representation requires more than a cast to extract as a database value.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release.
/// </remarks>
public interface INpgsqlJsonScalarTypeMapping
{
    /// <summary>
    ///     Converts text extracted from JSON into the mapped database representation.
    /// </summary>
    SqlExpression ConvertFromJsonText(SqlExpression expression);
}
