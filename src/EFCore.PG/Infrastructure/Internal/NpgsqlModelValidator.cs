using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlModelValidator(
    ModelValidatorDependencies dependencies,
    RelationalModelValidatorDependencies relationalDependencies,
    INpgsqlSingletonOptions npgsqlSingletonOptions) : RelationalModelValidator(dependencies, relationalDependencies)
{
    /// <summary>
    ///     The backend version to target.
    /// </summary>
    private readonly Version _postgresVersion = npgsqlSingletonOptions.PostgresVersion;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.Validate(model, logger);

        ValidateIdentityVersionCompatibility(model);
    }

    /// <summary>
    ///     Validates that identity columns are used only with PostgreSQL 10.0 or later (model-level check).
    /// </summary>
    /// <param name="model">The model to validate.</param>
    protected virtual void ValidateIdentityVersionCompatibility(IModel model)
    {
        if (_postgresVersion.AtLeast(10))
        {
            return;
        }

        var strategy = model.GetValueGenerationStrategy();

        if (strategy is NpgsqlValueGenerationStrategy.IdentityAlwaysColumn or NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
        {
            throw new InvalidOperationException(
                $"'{strategy}' requires PostgreSQL 10.0 or later. "
                + "If you're using an older version, set PostgreSQL compatibility mode by calling "
                + $"'optionsBuilder.{nameof(NpgsqlDbContextOptionsBuilder.SetPostgresVersion)}()' in your model's OnConfiguring. "
                + "See the docs for more info.");
        }
    }

    /// <inheritdoc />
    protected override void ValidateProperty(
        IProperty property,
        ITypeBase structuralType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateProperty(property, structuralType, logger);

        var strategy = property.GetValueGenerationStrategy();

        // Identity version compatibility (per-property check)
        if (!_postgresVersion.AtLeast(10)
            && strategy is NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                or NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
        {
            throw new InvalidOperationException(
                $"{property.DeclaringType}.{property.Name}: '{strategy}' requires PostgreSQL 10.0 or later.");
        }

        // Value generation strategy compatibility
        var propertyType = property.ClrType;

        switch (strategy)
        {
            case NpgsqlValueGenerationStrategy.None:
                break;

            case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
            case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                if (!NpgsqlPropertyExtensions.IsCompatibleWithValueGeneration(property))
                {
                    throw new InvalidOperationException(
                        NpgsqlStrings.IdentityBadType(
                            property.Name, property.DeclaringType.DisplayName(), propertyType.ShortDisplayName()));
                }

                break;

            case NpgsqlValueGenerationStrategy.SequenceHiLo:
            case NpgsqlValueGenerationStrategy.Sequence:
            case NpgsqlValueGenerationStrategy.SerialColumn:
                if (!NpgsqlPropertyExtensions.IsCompatibleWithValueGeneration(property))
                {
                    throw new InvalidOperationException(
                        NpgsqlStrings.SequenceBadType(
                            property.Name, property.DeclaringType.DisplayName(), propertyType.ShortDisplayName()));
                }

                break;

            default:
                throw new UnreachableException();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ValidateValueGeneration(
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var entityType = key.DeclaringEntityType;
        if (entityType.GetTableName() != null
            && (string?)entityType[RelationalAnnotationNames.MappingStrategy] == RelationalAnnotationNames.TpcMappingStrategy)
        {
            foreach (var storeGeneratedProperty in key.Properties.Where(
                         p => (p.ValueGenerated & ValueGenerated.OnAdd) != 0
                             && p.GetValueGenerationStrategy() != NpgsqlValueGenerationStrategy.Sequence))
            {
                logger.TpcStoreGeneratedIdentityWarning(storeGeneratedProperty);
            }
        }
    }

    /// <inheritdoc />
    protected override void ValidateIndex(
        IIndex index,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateIndex(index, logger);

        var includeProperties = index.GetIncludeProperties();
        if (includeProperties?.Count > 0)
        {
            var notFound = includeProperties
                .FirstOrDefault(i => index.DeclaringEntityType.FindProperty(i) is null);

            if (notFound is not null)
            {
                throw new InvalidOperationException(
                    NpgsqlStrings.IncludePropertyNotFound(index.DeclaringEntityType.DisplayName(), notFound));
            }

            var duplicate = includeProperties
                .GroupBy(i => i)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .FirstOrDefault();

            if (duplicate is not null)
            {
                throw new InvalidOperationException(
                    NpgsqlStrings.IncludePropertyDuplicated(index.DeclaringEntityType.DisplayName(), duplicate));
            }

            var inIndex = includeProperties
                .FirstOrDefault(i => index.Properties.Any(p => i == p.Name));

            if (inIndex is not null)
            {
                throw new InvalidOperationException(
                    NpgsqlStrings.IncludePropertyInIndex(index.DeclaringEntityType.DisplayName(), inIndex));
            }
        }
    }

    /// <inheritdoc />
    protected override void ValidateKey(
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateKey(key, logger);

        if (key.GetWithoutOverlaps() == true)
        {
            ValidateWithoutOverlapsKey(key);
        }
    }

    /// <inheritdoc />
    protected override void ValidateForeignKey(
        IForeignKey foreignKey,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateForeignKey(foreignKey, logger);

        if (foreignKey.GetPeriod() == true)
        {
            ValidatePeriodForeignKey(foreignKey);
        }
    }

    /// <inheritdoc />
    protected override void ValidateStoredProcedures(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateStoredProcedures(entityType, logger);

        if (entityType.GetDeleteStoredProcedure() is { } deleteStoredProcedure)
        {
            ValidateSproc(deleteStoredProcedure, logger);
        }

        if (entityType.GetInsertStoredProcedure() is { } insertStoredProcedure)
        {
            ValidateSproc(insertStoredProcedure, logger);
        }

        if (entityType.GetUpdateStoredProcedure() is { } updateStoredProcedure)
        {
            ValidateSproc(updateStoredProcedure, logger);
        }

        static void ValidateSproc(IStoredProcedure sproc, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var entityType = sproc.EntityType;
            var storeObjectIdentifier = sproc.GetStoreIdentifier();

            if (sproc.ResultColumns.Any())
            {
                throw new InvalidOperationException(
                    NpgsqlStrings.StoredProcedureResultColumnsNotSupported(
                        entityType.DisplayName(),
                        storeObjectIdentifier.DisplayName()));
            }

            if (sproc.IsRowsAffectedReturned)
            {
                throw new InvalidOperationException(
                    NpgsqlStrings.StoredProcedureReturnValueNotSupported(
                        entityType.DisplayName(),
                        storeObjectIdentifier.DisplayName()));
            }
        }
    }

    /// <inheritdoc />
    protected override void ValidateCompatible(
        IProperty property,
        IProperty duplicateProperty,
        string columnName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateCompatible(property, duplicateProperty, columnName, storeObject, logger);

        if (property.GetCompressionMethod(storeObject) != duplicateProperty.GetCompressionMethod(storeObject))
        {
            throw new InvalidOperationException(
                NpgsqlStrings.DuplicateColumnCompressionMethodMismatch(
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName()));
        }
    }

    private void ValidateWithoutOverlapsKey(IKey key)
    {
        var keyName = key.IsPrimaryKey() ? "primary key" : $"alternate key {key.Properties.Format()}";
        var entityType = key.DeclaringEntityType;

        // Check PostgreSQL version requirement
        if (!_postgresVersion.AtLeast(18))
        {
            throw new InvalidOperationException(
                NpgsqlStrings.WithoutOverlapsRequiresPostgres18(keyName, entityType.DisplayName()));
        }

        // Check that the last property is a range type
        var lastProperty = key.Properties[^1];
        var typeMapping = lastProperty.FindTypeMapping();

        if (typeMapping is not NpgsqlRangeTypeMapping)
        {
            throw new InvalidOperationException(
                NpgsqlStrings.WithoutOverlapsRequiresRangeType(
                    keyName,
                    entityType.DisplayName(),
                    lastProperty.Name,
                    lastProperty.ClrType.ShortDisplayName()));
        }
    }

    private void ValidatePeriodForeignKey(IForeignKey foreignKey)
    {
        var entityType = foreignKey.DeclaringEntityType;
        var fkName = foreignKey.Properties.Format();
        var principalKey = foreignKey.PrincipalKey;
        var principalEntityType = principalKey.DeclaringEntityType;

        if (!_postgresVersion.AtLeast(18))
        {
            throw new InvalidOperationException(
                NpgsqlStrings.PeriodRequiresPostgres18(fkName, entityType.DisplayName()));
        }

        // Check that the principal key has WITHOUT OVERLAPS (check this before range type)
        if (principalKey.GetWithoutOverlaps() != true)
        {
            throw new InvalidOperationException(
                NpgsqlStrings.PeriodRequiresWithoutOverlapsOnPrincipal(
                    fkName,
                    entityType.DisplayName(),
                    principalKey.IsPrimaryKey()
                        ? "primary key"
                        : $"alternate key {principalKey.Properties.Format()}",
                    principalEntityType.DisplayName()));
        }

        // Check that the last property is a range type
        var lastProperty = foreignKey.Properties[^1];
        var typeMapping = lastProperty.FindTypeMapping();

        if (typeMapping is not NpgsqlRangeTypeMapping)
        {
            throw new InvalidOperationException(
                NpgsqlStrings.PeriodRequiresRangeType(
                    fkName,
                    entityType.DisplayName(),
                    lastProperty.Name,
                    lastProperty.ClrType.ShortDisplayName()));
        }
    }
}
