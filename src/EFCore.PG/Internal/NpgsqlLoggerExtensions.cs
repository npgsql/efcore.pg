namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class NpgsqlLoggerExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void MissingSchemaWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? schemaName)
    {
        var definition = NpgsqlResources.LogMissingSchema(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, schemaName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void MissingTableWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? tableName)
    {
        var definition = NpgsqlResources.LogMissingTable(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ForeignKeyReferencesMissingPrincipalTableWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? foreignKeyName,
        string? tableName,
        string? principalTableName)
    {
        var definition = NpgsqlResources.LogPrincipalTableNotInSelectionSet(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, foreignKeyName, tableName, principalTableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ColumnFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string tableName,
        string columnName,
        string dataTypeName,
        bool nullable,
        bool identity,
        string? defaultValue,
        string? computedValue)
    {
        var definition = NpgsqlResources.LogFoundColumn(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                l => l.LogDebug(
                    definition.EventId,
                    null,
                    definition.MessageFormat,
                    tableName,
                    columnName,
                    dataTypeName,
                    nullable,
                    identity,
                    defaultValue,
                    computedValue));
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void CollationFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string schema,
        string collationName,
        string lcCollate,
        string lcCtype,
        string? provider,
        bool deterministic)
    {
        var definition = NpgsqlResources.LogFoundCollation(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, collationName, schema, lcCollate, lcCtype, provider, deterministic);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void UniqueConstraintFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? uniqueConstraintName,
        string tableName)
    {
        var definition = NpgsqlResources.LogFoundUniqueConstraint(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, uniqueConstraintName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void EnumColumnSkippedWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string columnName)
    {
        var definition = NpgsqlResources.LogEnumColumnSkipped(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, columnName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ExpressionIndexSkippedWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string indexName,
        string tableName)
    {
        var definition = NpgsqlResources.LogExpressionIndexSkipped(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, indexName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void UnsupportedColumnIndexSkippedWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string indexName,
        string tableName)
    {
        var definition = NpgsqlResources.LogUnsupportedColumnIndexSkipped(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, indexName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void UnsupportedColumnConstraintSkippedWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? indexName,
        string tableName)
    {
        var definition = NpgsqlResources.LogUnsupportedColumnConstraintSkipped(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, indexName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
}
