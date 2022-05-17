using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

/// <summary>
/// The validator that enforces rules for Npgsql.
/// </summary>
public class NpgsqlModelValidator : RelationalModelValidator
{
    /// <summary>
    /// The backend version to target.
    /// </summary>
    private readonly Version _postgresVersion;

    /// <inheritdoc />
    public NpgsqlModelValidator(
        ModelValidatorDependencies dependencies,
        RelationalModelValidatorDependencies relationalDependencies,
        INpgsqlSingletonOptions npgsqlSingletonOptions)
        : base(dependencies, relationalDependencies)
        => _postgresVersion = npgsqlSingletonOptions.PostgresVersion;

    public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.Validate(model, logger);

        ValidateIdentityVersionCompatibility(model);
        ValidateIndexIncludeProperties(model);
    }

    /// <summary>
    /// Validates that identity columns are used only with PostgreSQL 10.0 or later.
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
                $"'{strategy}' requires PostgreSQL 10.0 or later. " +
                "If you're using an older version, set PostgreSQL compatibility mode by calling " +
                $"'optionsBuilder.{nameof(NpgsqlDbContextOptionsBuilder.SetPostgresVersion)}()' in your model's OnConfiguring. " +
                "See the docs for more info.");
        }

        foreach (var property in model.GetEntityTypes().SelectMany(e => e.GetProperties()))
        {
            var propertyStrategy = property.GetValueGenerationStrategy();

            if (propertyStrategy is NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                or NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
            {
                throw new InvalidOperationException(
                    $"{property.DeclaringEntityType}.{property.Name}: '{propertyStrategy}' requires PostgreSQL 10.0 or later.");
            }
        }
    }

    protected virtual void ValidateIndexIncludeProperties(IModel model)
    {
        foreach (var index in model.GetEntityTypes().SelectMany(t => t.GetDeclaredIndexes()))
        {
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
                    duplicateProperty.DeclaringEntityType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringEntityType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName()));
        }
    }
}