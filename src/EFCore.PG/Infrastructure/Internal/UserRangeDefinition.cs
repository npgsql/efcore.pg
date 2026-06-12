namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

/// <summary>
///     A definition for a user-defined PostgreSQL range to be mapped.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public sealed record UserRangeDefinition
{
    /// <summary>
    ///     The name of the PostgreSQL range type to be mapped.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public string StoreTypeName { get; }

    /// <summary>
    ///     The PostgreSQL schema in which the range is defined. If null, the default schema is used
    ///     (which is public unless changed on the model).
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public string? StoreTypeSchema { get; }

    /// <summary>
    ///     The CLR type of the range's subtype (or element).
    ///     The actual mapped type will be an <see cref="NpgsqlRange{T}" /> over this type.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public Type SubtypeClrType { get; }

    /// <summary>
    ///     Optionally, the name of the range's PostgreSQL subtype (or element).
    ///     This is usually not needed - the subtype will be inferred based on <see cref="SubtypeClrType" />.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public string? SubtypeName { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public UserRangeDefinition(string name, string? schema, Type subtypeClrType, string? subtypeName)
    {
        StoreTypeName = name;
        StoreTypeSchema = schema;
        SubtypeClrType = subtypeClrType;
        SubtypeName = subtypeName;
    }
}
