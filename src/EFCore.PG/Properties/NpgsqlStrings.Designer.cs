// <auto-generated />

using System.Resources;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;

#nullable enable

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class NpgsqlStrings
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Npgsql.EntityFrameworkCore.PostgreSQL.Properties.NpgsqlStrings", typeof(NpgsqlStrings).Assembly);

        /// <summary>
        ///     Using two distinct data sources within a service provider is not supported, and Entity Framework is not building its own internal service provider. Either allow Entity Framework to build the service provider by removing the call to '{useInternalServiceProvider}', or ensure that the same data source is used for all uses of a given service provider passed to '{useInternalServiceProvider}'.
        /// </summary>
        public static string TwoDataSourcesInSameServiceProvider(object? useInternalServiceProvider)
            => string.Format(
                GetString("TwoDataSourcesInSameServiceProvider", nameof(useInternalServiceProvider)),
                useInternalServiceProvider);

        /// <summary>
        ///     '{entityType1}.{property1}' and '{entityType2}.{property2}' are both mapped to column '{columnName}' in '{table}', but are configured with different compression methods.
        /// </summary>
        public static string DuplicateColumnCompressionMethodMismatch(object? entityType1, object? property1, object? entityType2, object? property2, object? columnName, object? table)
            => string.Format(
                GetString("DuplicateColumnCompressionMethodMismatch", nameof(entityType1), nameof(property1), nameof(entityType2), nameof(property2), nameof(columnName), nameof(table)),
                entityType1, property1, entityType2, property2, columnName, table);

        /// <summary>
        ///     '{entityType1}.{property1}' and '{entityType2}.{property2}' are both mapped to column '{columnName}' in '{table}' but are configured with different value generation strategies.
        /// </summary>
        public static string DuplicateColumnNameValueGenerationStrategyMismatch(object? entityType1, object? property1, object? entityType2, object? property2, object? columnName, object? table)
            => string.Format(
                GetString("DuplicateColumnNameValueGenerationStrategyMismatch", nameof(entityType1), nameof(property1), nameof(entityType2), nameof(property2), nameof(columnName), nameof(table)),
                entityType1, property1, entityType2, property2, columnName, table);

        /// <summary>
        ///     The indexes {index1} on '{entityType1}' and {index2} on '{entityType2}' are both mapped to '{table}.{indexName}', but have different collation configurations.
        /// </summary>
        public static string DuplicateIndexCollationMismatch(object? index1, object? entityType1, object? index2, object? entityType2, object? table, object? indexName)
            => string.Format(
                GetString("DuplicateIndexCollationMismatch", nameof(index1), nameof(entityType1), nameof(index2), nameof(entityType2), nameof(table), nameof(indexName)),
                index1, entityType1, index2, entityType2, table, indexName);

        /// <summary>
        ///     The indexes {index1} on '{entityType1}' and {index2} on '{entityType2}' are both mapped to '{table}.{indexName}', but have different concurrent creation configurations.
        /// </summary>
        public static string DuplicateIndexConcurrentCreationMismatch(object? index1, object? entityType1, object? index2, object? entityType2, object? table, object? indexName)
            => string.Format(
                GetString("DuplicateIndexConcurrentCreationMismatch", nameof(index1), nameof(entityType1), nameof(index2), nameof(entityType2), nameof(table), nameof(indexName)),
                index1, entityType1, index2, entityType2, table, indexName);

        /// <summary>
        ///     The indexes {index1} on '{entityType1}' and {index2} on '{entityType2}' are both mapped to '{table}.{indexName}', but have different included columns: {includedColumns1} and {includedColumns2}.
        /// </summary>
        public static string DuplicateIndexIncludedMismatch(object? index1, object? entityType1, object? index2, object? entityType2, object? table, object? indexName, object? includedColumns1, object? includedColumns2)
            => string.Format(
                GetString("DuplicateIndexIncludedMismatch", nameof(index1), nameof(entityType1), nameof(index2), nameof(entityType2), nameof(table), nameof(indexName), nameof(includedColumns1), nameof(includedColumns2)),
                index1, entityType1, index2, entityType2, table, indexName, includedColumns1, includedColumns2);

        /// <summary>
        ///     The EF Core 7.0 JSON support isn't currently supported by the Npgsql provider. To map to JSON, see https://www.npgsql.org/efcore/mapping/json.html.
        /// </summary>
        public static string Ef7JsonMappingNotSupported
            => GetString("Ef7JsonMappingNotSupported");

        /// <summary>
        ///     The 'FreeText' method is not supported because the query has switched to client-evaluation. Inspect the log to determine which query expressions are triggering client-evaluation.
        /// </summary>
        public static string FreeTextFunctionOnClient
            => GetString("FreeTextFunctionOnClient");

        /// <summary>
        ///     Heterogeneous store types detected when making new array ({type1}, {type2}).
        /// </summary>
        public static string HeterogeneousTypesInNewArray(object? type1, object? type2)
            => string.Format(
                GetString("HeterogeneousTypesInNewArray", nameof(type1), nameof(type2)),
                type1, type2);

        /// <summary>
        ///     Identity value generation cannot be used for the property '{property}' on entity type '{entityType}' because the property type is '{propertyType}'. Identity value generation can only be used with signed integer properties.
        /// </summary>
        public static string IdentityBadType(object? property, object? entityType, object? propertyType)
            => string.Format(
                GetString("IdentityBadType", nameof(property), nameof(entityType), nameof(propertyType)),
                property, entityType, propertyType);

        /// <summary>
        ///     Include property '{entityType}.{property}' cannot be defined multiple times
        /// </summary>
        public static string IncludePropertyDuplicated(object? entityType, object? property)
            => string.Format(
                GetString("IncludePropertyDuplicated", nameof(entityType), nameof(property)),
                entityType, property);

        /// <summary>
        ///     Include property '{entityType}.{property}' is already included in the index
        /// </summary>
        public static string IncludePropertyInIndex(object? entityType, object? property)
            => string.Format(
                GetString("IncludePropertyInIndex", nameof(entityType), nameof(property)),
                entityType, property);

        /// <summary>
        ///     Include property '{entityType}.{property}' not found
        /// </summary>
        public static string IncludePropertyNotFound(object? entityType, object? property)
            => string.Format(
                GetString("IncludePropertyNotFound", nameof(entityType), nameof(property)),
                entityType, property);

        /// <summary>
        ///     The specified table '{table}' is not valid. Specify tables using the format '[schema].[table]'.
        /// </summary>
        public static string InvalidTableToIncludeInScaffolding(object? table)
            => string.Format(
                GetString("InvalidTableToIncludeInScaffolding", nameof(table)),
                table);

        /// <summary>
        ///     The property '{property}' on entity type '{entityType}' is configured to use 'SequenceHiLo' value generator, which is only intended for keys. If this was intentional configure an alternate key on the property, otherwise call 'ValueGeneratedNever' or configure store generation for this property.
        /// </summary>
        public static string NonKeyValueGeneration(object? property, object? entityType)
            => string.Format(
                GetString("NonKeyValueGeneration", nameof(property), nameof(entityType)),
                property, entityType);

        /// <summary>
        ///     Row values comparisons require two tuple arguments of the same length.
        /// </summary>
        public static string RowValueComparisonRequiresTuplesOfSameLength
            => GetString("RowValueComparisonRequiresTuplesOfSameLength");

        /// <summary>
        ///     Cannot set ProvideClientCertificatesCallback, RemoteCertificateValidationCallback or ProvidePasswordCallback when a data source is provided.
        /// </summary>
        public static string CannotUseDataSourceWithAuthCallbacks
            => GetString("CannotUseDataSourceWithAuthCallbacks");

        /// <summary>
        ///     PostgreSQL sequences cannot be used to generate values for the property '{property}' on entity type '{entityType}' because the property type is '{propertyType}'. Sequences can only be used with integer properties.
        /// </summary>
        public static string SequenceBadType(object? property, object? entityType, object? propertyType)
            => string.Format(
                GetString("SequenceBadType", nameof(property), nameof(entityType), nameof(propertyType)),
                property, entityType, propertyType);

        /// <summary>
        ///     The entity type '{entityType}' is mapped to the stored procedure '{sproc}', which is configured with result columns. PostgreSQL stored procedures do not support result columns; use output parameters instead.
        /// </summary>
        public static string StoredProcedureResultColumnsNotSupported(object? entityType, object? sproc)
            => string.Format(
                GetString("StoredProcedureResultColumnsNotSupported", nameof(entityType), nameof(sproc)),
                entityType, sproc);

        /// <summary>
        ///     The entity type '{entityType}' is mapped to the stored procedure '{sproc}', which is configured with a return value. PostgreSQL stored procedures do not support return values; use an output parameter instead.
        /// </summary>
        public static string StoredProcedureReturnValueNotSupported(object? entityType, object? sproc)
            => string.Format(
                GetString("StoredProcedureReturnValueNotSupported", nameof(entityType), nameof(sproc)),
                entityType, sproc);

        /// <summary>
        ///     An exception has been raised that is likely due to a transient failure. Consider enabling transient error resiliency by adding 'EnableRetryOnFailure()' to the 'UseSqlServer' call.
        /// </summary>
        public static string TransientExceptionDetected
            => GetString("TransientExceptionDetected");

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name)!;
            for (var i = 0; i < formatterNames.Length; i++)
            {
                value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
            }

            return value;
        }
    }
}

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class NpgsqlResources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Npgsql.EntityFrameworkCore.PostgreSQL.Properties.NpgsqlStrings", typeof(NpgsqlResources).Assembly);

        /// <summary>
        ///     Enum column '{name}' cannot be scaffolded, define a CLR enum type and add the property manually.
        /// </summary>
        public static EventDefinition<string> LogEnumColumnSkipped(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogEnumColumnSkipped;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogEnumColumnSkipped,
                    logger,
                    static logger => new EventDefinition<string>(
                        logger.Options,
                        NpgsqlEfEventId.EnumColumnSkippedWarning,
                        LogLevel.Warning,
                        "NpgsqlEfEventId.EnumColumnSkippedWarning",
                        level => LoggerMessage.Define<string>(
                            level,
                            NpgsqlEfEventId.EnumColumnSkippedWarning,
                            _resourceManager.GetString("LogEnumColumnSkipped")!)));
            }

            return (EventDefinition<string>)definition;
        }

        /// <summary>
        ///     Expression index '{name}' on table {tableName} cannot be scaffolded, expression indices aren't supported and must be added via raw SQL in migrations.
        /// </summary>
        public static EventDefinition<string, string> LogExpressionIndexSkipped(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogExpressionIndexSkipped;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogExpressionIndexSkipped,
                    logger,
                    static logger => new EventDefinition<string, string>(
                        logger.Options,
                        NpgsqlEfEventId.ExpressionIndexSkippedWarning,
                        LogLevel.Warning,
                        "NpgsqlEfEventId.ExpressionIndexSkippedWarning",
                        level => LoggerMessage.Define<string, string>(
                            level,
                            NpgsqlEfEventId.ExpressionIndexSkippedWarning,
                            _resourceManager.GetString("LogExpressionIndexSkipped")!)));
            }

            return (EventDefinition<string, string>)definition;
        }

        /// <summary>
        ///     Found collation with name: {collationName}, schema: {schema}, LC_COLLATE: {lcCollate}, LC_CTYPE: {lcCtype}, provider: {provider}, deterministic: {isDeterministic}
        /// </summary>
        public static EventDefinition<string, string, string, string, string?, bool> LogFoundCollation(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundCollation;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundCollation,
                    logger,
                    static logger => new EventDefinition<string, string, string, string, string?, bool>(
                        logger.Options,
                        NpgsqlEfEventId.CollationFound,
                        LogLevel.Debug,
                        "NpgsqlEfEventId.CollationFound",
                        level => LoggerMessage.Define<string, string, string, string, string?, bool>(
                            level,
                            NpgsqlEfEventId.CollationFound,
                            _resourceManager.GetString("LogFoundCollation")!)));
            }

            return (EventDefinition<string, string, string, string, string?, bool>)definition;
        }

        /// <summary>
        ///     Found column with table: {tableName}, column name: {columnName}, data type: {dataType}, nullable: {isNullable}, identity: {isIdentity}, default value: {defaultValue}, computed value: {computedValue}
        /// </summary>
        public static FallbackEventDefinition LogFoundColumn(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundColumn;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundColumn,
                    logger,
                    static logger => new FallbackEventDefinition(
                        logger.Options,
                        NpgsqlEfEventId.ColumnFound,
                        LogLevel.Debug,
                        "NpgsqlEfEventId.ColumnFound",
                        _resourceManager.GetString("LogFoundColumn")!));
            }

            return (FallbackEventDefinition)definition;
        }

        /// <summary>
        ///     Found foreign key on table: {tableName}, name: {foreignKeyName}, principal table: {principalTableName}, delete action: {deleteAction}.
        /// </summary>
        public static EventDefinition<string, string, string, string> LogFoundForeignKey(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundForeignKey;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundForeignKey,
                    logger,
                    static logger => new EventDefinition<string, string, string, string>(
                        logger.Options,
                        NpgsqlEfEventId.ForeignKeyFound,
                        LogLevel.Debug,
                        "NpgsqlEfEventId.ForeignKeyFound",
                        level => LoggerMessage.Define<string, string, string, string>(
                            level,
                            NpgsqlEfEventId.ForeignKeyFound,
                            _resourceManager.GetString("LogFoundForeignKey")!)));
            }

            return (EventDefinition<string, string, string, string>)definition;
        }

        /// <summary>
        ///     Found index with name: {indexName}, table: {tableName}, is unique: {isUnique}.
        /// </summary>
        public static EventDefinition<string, string, bool> LogFoundIndex(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundIndex;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundIndex,
                    logger,
                    static logger => new EventDefinition<string, string, bool>(
                        logger.Options,
                        NpgsqlEfEventId.IndexFound,
                        LogLevel.Debug,
                        "NpgsqlEfEventId.IndexFound",
                        level => LoggerMessage.Define<string, string, bool>(
                            level,
                            NpgsqlEfEventId.IndexFound,
                            _resourceManager.GetString("LogFoundIndex")!)));
            }

            return (EventDefinition<string, string, bool>)definition;
        }

        /// <summary>
        ///     Found primary key with name: {primaryKeyName}, table: {tableName}.
        /// </summary>
        public static EventDefinition<string, string> LogFoundPrimaryKey(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundPrimaryKey;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundPrimaryKey,
                    logger,
                    static logger => new EventDefinition<string, string>(
                        logger.Options,
                        NpgsqlEfEventId.PrimaryKeyFound,
                        LogLevel.Debug,
                        "NpgsqlEfEventId.PrimaryKeyFound",
                        level => LoggerMessage.Define<string, string>(
                            level,
                            NpgsqlEfEventId.PrimaryKeyFound,
                            _resourceManager.GetString("LogFoundPrimaryKey")!)));
            }

            return (EventDefinition<string, string>)definition;
        }

        /// <summary>
        ///     Found sequence name: {name}, data type: {dataType}, cyclic: {isCyclic}, increment: {increment}, start: {start}, minimum: {min}, maximum: {max}.
        /// </summary>
        public static FallbackEventDefinition LogFoundSequence(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundSequence;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundSequence,
                    logger,
                    static logger => new FallbackEventDefinition(
                        logger.Options,
                        NpgsqlEfEventId.SequenceFound,
                        LogLevel.Debug,
                        "NpgsqlEfEventId.SequenceFound",
                        _resourceManager.GetString("LogFoundSequence")!));
            }

            return (FallbackEventDefinition)definition;
        }

        /// <summary>
        ///     Found table with name: {name}.
        /// </summary>
        public static EventDefinition<string> LogFoundTable(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundTable;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundTable,
                    logger,
                    static logger => new EventDefinition<string>(
                        logger.Options,
                        NpgsqlEfEventId.TableFound,
                        LogLevel.Debug,
                        "NpgsqlEfEventId.TableFound",
                        level => LoggerMessage.Define<string>(
                            level,
                            NpgsqlEfEventId.TableFound,
                            _resourceManager.GetString("LogFoundTable")!)));
            }

            return (EventDefinition<string>)definition;
        }

        /// <summary>
        ///     Found unique constraint with name: {uniqueConstraintName}, table: {tableName}.
        /// </summary>
        public static EventDefinition<string?, string> LogFoundUniqueConstraint(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundUniqueConstraint;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogFoundUniqueConstraint,
                    logger,
                    static logger => new EventDefinition<string?, string>(
                        logger.Options,
                        NpgsqlEfEventId.UniqueConstraintFound,
                        LogLevel.Debug,
                        "NpgsqlEfEventId.UniqueConstraintFound",
                        level => LoggerMessage.Define<string?, string>(
                            level,
                            NpgsqlEfEventId.UniqueConstraintFound,
                            _resourceManager.GetString("LogFoundUniqueConstraint")!)));
            }

            return (EventDefinition<string?, string>)definition;
        }

        /// <summary>
        ///     Unable to find a schema in the database matching the selected schema {schema}.
        /// </summary>
        public static EventDefinition<string?> LogMissingSchema(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogMissingSchema;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogMissingSchema,
                    logger,
                    static logger => new EventDefinition<string?>(
                        logger.Options,
                        NpgsqlEfEventId.MissingSchemaWarning,
                        LogLevel.Warning,
                        "NpgsqlEfEventId.MissingSchemaWarning",
                        level => LoggerMessage.Define<string?>(
                            level,
                            NpgsqlEfEventId.MissingSchemaWarning,
                            _resourceManager.GetString("LogMissingSchema")!)));
            }

            return (EventDefinition<string?>)definition;
        }

        /// <summary>
        ///     Unable to find a table in the database matching the selected table {table}.
        /// </summary>
        public static EventDefinition<string?> LogMissingTable(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogMissingTable;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogMissingTable,
                    logger,
                    static logger => new EventDefinition<string?>(
                        logger.Options,
                        NpgsqlEfEventId.MissingTableWarning,
                        LogLevel.Warning,
                        "NpgsqlEfEventId.MissingTableWarning",
                        level => LoggerMessage.Define<string?>(
                            level,
                            NpgsqlEfEventId.MissingTableWarning,
                            _resourceManager.GetString("LogMissingTable")!)));
            }

            return (EventDefinition<string?>)definition;
        }

        /// <summary>
        ///     For foreign key {foreignKeyName} on table {tableName}, unable to find the column called {principalColumnName} on the foreign key's principal table, {principaltableName}. Skipping foreign key.
        /// </summary>
        public static EventDefinition<string, string, string, string> LogPrincipalColumnNotFound(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogPrincipalColumnNotFound;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogPrincipalColumnNotFound,
                    logger,
                    static logger => new EventDefinition<string, string, string, string>(
                        logger.Options,
                        NpgsqlEfEventId.ForeignKeyPrincipalColumnMissingWarning,
                        LogLevel.Warning,
                        "NpgsqlEfEventId.ForeignKeyPrincipalColumnMissingWarning",
                        level => LoggerMessage.Define<string, string, string, string>(
                            level,
                            NpgsqlEfEventId.ForeignKeyPrincipalColumnMissingWarning,
                            _resourceManager.GetString("LogPrincipalColumnNotFound")!)));
            }

            return (EventDefinition<string, string, string, string>)definition;
        }

        /// <summary>
        ///     For foreign key {fkName} on table {tableName}, unable to model the end of the foreign key on principal table {principaltableName}. This is usually because the principal table was not included in the selection set.
        /// </summary>
        public static EventDefinition<string?, string?, string?> LogPrincipalTableNotInSelectionSet(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogPrincipalTableNotInSelectionSet;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogPrincipalTableNotInSelectionSet,
                    logger,
                    static logger => new EventDefinition<string?, string?, string?>(
                        logger.Options,
                        NpgsqlEfEventId.ForeignKeyReferencesMissingPrincipalTableWarning,
                        LogLevel.Warning,
                        "NpgsqlEfEventId.ForeignKeyReferencesMissingPrincipalTableWarning",
                        level => LoggerMessage.Define<string?, string?, string?>(
                            level,
                            NpgsqlEfEventId.ForeignKeyReferencesMissingPrincipalTableWarning,
                            _resourceManager.GetString("LogPrincipalTableNotInSelectionSet")!)));
            }

            return (EventDefinition<string?, string?, string?>)definition;
        }

        /// <summary>
        ///     Constraint '{name}' on table {tableName} cannot be scaffolded because it includes a column that cannot be scaffolded (e.g. enum).
        /// </summary>
        public static EventDefinition<string?, string> LogUnsupportedColumnConstraintSkipped(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogUnsupportedColumnConstraintSkipped;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogUnsupportedColumnConstraintSkipped,
                    logger,
                    static logger => new EventDefinition<string?, string>(
                        logger.Options,
                        NpgsqlEfEventId.UnsupportedColumnConstraintSkippedWarning,
                        LogLevel.Warning,
                        "NpgsqlEfEventId.UnsupportedColumnConstraintSkippedWarning",
                        level => LoggerMessage.Define<string?, string>(
                            level,
                            NpgsqlEfEventId.UnsupportedColumnConstraintSkippedWarning,
                            _resourceManager.GetString("LogUnsupportedColumnConstraintSkipped")!)));
            }

            return (EventDefinition<string?, string>)definition;
        }

        /// <summary>
        ///     Index '{name}' on table {tableName} cannot be scaffolded because it includes a column that cannot be scaffolded (e.g. enum).
        /// </summary>
        public static EventDefinition<string, string> LogUnsupportedColumnIndexSkipped(IDiagnosticsLogger logger)
        {
            var definition = ((NpgsqlLoggingDefinitions)logger.Definitions).LogUnsupportedColumnIndexSkipped;
            if (definition is null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((NpgsqlLoggingDefinitions)logger.Definitions).LogUnsupportedColumnIndexSkipped,
                    logger,
                    static logger => new EventDefinition<string, string>(
                        logger.Options,
                        NpgsqlEfEventId.UnsupportedColumnIndexSkippedWarning,
                        LogLevel.Warning,
                        "NpgsqlEfEventId.UnsupportedColumnIndexSkippedWarning",
                        level => LoggerMessage.Define<string, string>(
                            level,
                            NpgsqlEfEventId.UnsupportedColumnIndexSkippedWarning,
                            _resourceManager.GetString("LogUnsupportedColumnIndexSkipped")!)));
            }

            return (EventDefinition<string, string>)definition;
        }
    }
}

