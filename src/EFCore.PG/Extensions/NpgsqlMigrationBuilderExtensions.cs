// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class NpgsqlMigrationBuilderExtensions
{
    /// <summary>
    /// Returns true if the active provider in a migration is the Npgsql provider.
    /// </summary>
    /// The migrationBuilder from the parameters on <see cref="Migration.Up(MigrationBuilder)" /> or
    /// <see cref="Migration.Down(MigrationBuilder)" />.
    /// <returns>True if Npgsql is being used; false otherwise.</returns>
    public static bool IsNpgsql(this MigrationBuilder builder)
        => builder.ActiveProvider == typeof(NpgsqlMigrationBuilderExtensions).GetTypeInfo().Assembly.GetName().Name;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static MigrationBuilder EnsurePostgresExtension(
        this MigrationBuilder builder,
        string name,
        string? schema = null,
        string? version = null)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NullButNotEmpty(version, nameof(schema));

        var op = new AlterDatabaseOperation();
        op.GetOrAddPostgresExtension(schema, name, version);
        builder.Operations.Add(op);

        return builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Obsolete("Use EnsurePostgresExtension instead")]
    public static MigrationBuilder CreatePostgresExtension(
        this MigrationBuilder builder,
        string name,
        string? schema = null,
        string? version = null)
        => EnsurePostgresExtension(builder, name, schema, version);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Obsolete("This no longer does anything and should be removed.")]
    public static MigrationBuilder DropPostgresExtension(
        this MigrationBuilder builder,
        string name)
        => builder;
}
