using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal
{
    public static class NpgsqlLoggerExtensions
    {
        public static void MissingSchemaWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string schemaName)
        {
            var definition = NpgsqlResources.LogMissingSchema(diagnostics);

            if (diagnostics.ShouldLog(definition))
                definition.Log(diagnostics, schemaName);

            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void MissingTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = NpgsqlResources.LogMissingTable(diagnostics);

            if (diagnostics.ShouldLog(definition))
                definition.Log(diagnostics, tableName);

            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void ForeignKeyReferencesMissingPrincipalTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName,
            [CanBeNull] string principalTableName)
        {
            var definition = NpgsqlResources.LogPrincipalTableNotInSelectionSet(diagnostics);

            if (diagnostics.ShouldLog(definition))
                definition.Log(diagnostics, foreignKeyName, tableName, principalTableName);

            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void ColumnFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string tableName,
            [NotNull] string columnName,
            [NotNull] string dataTypeName,
            bool nullable,
            bool identity,
            [CanBeNull] string defaultValue,
            [CanBeNull] string computedValue)
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

        public static void CollationFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string schema,
            [NotNull] string collationName,
            [NotNull] string lcCollate,
            [NotNull] string lcCtype,
            [NotNull] string provider,
            bool deterministic)
        {
            var definition = NpgsqlResources.LogFoundCollation(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        collationName,
                        schema,
                        lcCollate,
                        lcCtype,
                        provider,
                        deterministic));
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void UniqueConstraintFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string uniqueConstraintName,
            [NotNull] string tableName)
        {
            var definition = NpgsqlResources.LogFoundUniqueConstraint(diagnostics);

            if (diagnostics.ShouldLog(definition))
                definition.Log(diagnostics, uniqueConstraintName, tableName);

            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void EnumColumnSkippedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string columnName)
        {
            var definition = NpgsqlResources.LogEnumColumnSkipped(diagnostics);

            if (diagnostics.ShouldLog(definition))
                definition.Log(diagnostics, columnName);

            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void ExpressionIndexSkippedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string indexName,
            [NotNull] string tableName)
        {
            var definition = NpgsqlResources.LogExpressionIndexSkipped(diagnostics);

            if (diagnostics.ShouldLog(definition))
                definition.Log(diagnostics, indexName, tableName);

            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void UnsupportedColumnIndexSkippedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string indexName,
            [NotNull] string tableName)
        {
            var definition = NpgsqlResources.LogUnsupportedColumnIndexSkipped(diagnostics);

            if (diagnostics.ShouldLog(definition))
                definition.Log(diagnostics, indexName, tableName);

            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void UnsupportedColumnConstraintSkippedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string indexName,
            [NotNull] string tableName)
        {
            var definition = NpgsqlResources.LogUnsupportedColumnConstraintSkipped(diagnostics);

            if (diagnostics.ShouldLog(definition))
                definition.Log(diagnostics, indexName, tableName);

            // No DiagnosticsSource events because these are purely design-time messages
        }
    }
}
