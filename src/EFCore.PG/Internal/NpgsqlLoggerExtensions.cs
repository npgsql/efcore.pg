using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal
{
    public static class NpgsqlLoggerExtensions
    {
        #region Scaffolding events

        public static void MissingSchemaWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string schemaName)
        {
            var definition = NpgsqlResources.LogMissingSchema(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    schemaName);
            }
            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void MissingTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = NpgsqlResources.LogMissingTable(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    tableName);
            }
            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void ForeignKeyReferencesMissingPrincipalTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName,
            [CanBeNull] string principalTableName)
        {
            var definition = NpgsqlResources.LogPrincipalTableNotInSelectionSet(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    foreignKeyName, tableName, principalTableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string tableName,
            [NotNull] string columnName,
            [NotNull] string dataTypeName,
            bool nullable,
            [CanBeNull] string defaultValue)
        {
            var definition = NpgsqlResources.LogFoundColumn(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    tableName, columnName, dataTypeName, nullable, defaultValue);
            }
            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void UniqueConstraintFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string uniqueConstraintName,
            [NotNull] string tableName)
        {
            var definition = NpgsqlResources.LogFoundUniqueConstraint(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    uniqueConstraintName, tableName);
            }
            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void EnumColumnSkippedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string columnName)
        {
            var definition = NpgsqlResources.LogEnumColumnSkipped(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    columnName);
            }
            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void ExpressionIndexSkippedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string indexName,
            [NotNull] string tableName)
        {
            var definition = NpgsqlResources.LogExpressionIndexSkipped(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    indexName,
                    tableName);
            }
            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void UnsupportedColumnIndexSkippedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string indexName,
            [NotNull] string tableName)
        {
            var definition = NpgsqlResources.LogUnsupportedColumnIndexSkipped(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    indexName,
                    tableName);
            }
            // No DiagnosticsSource events because these are purely design-time messages
        }

        public static void UnsupportedColumnConstraintSkippedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string indexName,
            [NotNull] string tableName)
        {
            var definition = NpgsqlResources.LogUnsupportedColumnConstraintSkipped(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    indexName,
                    tableName);
            }
            // No DiagnosticsSource events because these are purely design-time messages
        }

        #endregion

        #region Connection events

        public static void AutoPrepareDisabledWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] DbConnection connection,
            Guid connectionId)
        {
            var definition = NpgsqlStrings.LogAutoPrepareDisabled;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    connection.Database,
                    connection.DataSource);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ConnectionEventData(
                        definition,
                        AutoPrepareDisabled,
                        connection,
                        connectionId,
                        false,
                        DateTimeOffset.UtcNow));
            }

            static string AutoPrepareDisabled(EventDefinitionBase def, EventData payload)
            {
                var d = (EventDefinition<string, string>)def;
                var p = (ConnectionEventData)payload;
                return d.GenerateMessage(p.Connection.Database, p.Connection.DataSource);
            }
        }

        #endregion
    }
}
