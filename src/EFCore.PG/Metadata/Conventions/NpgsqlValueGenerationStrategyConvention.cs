namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

/// <summary>
/// A convention that configures the default model <see cref="NpgsqlValueGenerationStrategy"/> as
/// <see cref="NpgsqlValueGenerationStrategy.IdentityByDefaultColumn"/> for newer PostgreSQL versions,
/// and <see cref="NpgsqlValueGenerationStrategy.SerialColumn"/> for pre-10.0 versions.
/// </summary>
public class NpgsqlValueGenerationStrategyConvention : IModelInitializedConvention, IModelFinalizingConvention
{
    private readonly Version? _postgresVersion;

    /// <summary>
    /// Creates a new instance of <see cref="NpgsqlValueGenerationStrategyConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="postgresVersion">The PostgreSQL version being targeted. This affects the default value generation strategy.</param>
    public NpgsqlValueGenerationStrategyConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        Version? postgresVersion)
    {
        Dependencies = dependencies;
        _postgresVersion = postgresVersion;
    }

    /// <summary>
    /// Parameter object containing service dependencies.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelInitialized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.HasValueGenerationStrategy(
            _postgresVersion < new Version(10, 0)
                ? NpgsqlValueGenerationStrategy.SerialColumn
                : NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                NpgsqlValueGenerationStrategy? strategy = null;
                var declaringTable = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
                if (declaringTable.Name != null!)
                {
                    strategy = property.GetValueGenerationStrategy(declaringTable, Dependencies.TypeMappingSource);
                    if (strategy == NpgsqlValueGenerationStrategy.None
                        && !IsStrategyNoneNeeded(property, declaringTable))
                    {
                        strategy = null;
                    }
                }
                else
                {
                    var declaringView = property.GetMappedStoreObjects(StoreObjectType.View).FirstOrDefault();
                    if (declaringView.Name != null!)
                    {
                        strategy = property.GetValueGenerationStrategy(declaringView, Dependencies.TypeMappingSource);
                        if (strategy == NpgsqlValueGenerationStrategy.None
                            && !IsStrategyNoneNeeded(property, declaringView))
                        {
                            strategy = null;
                        }
                    }
                }

                // Needed for the annotation to show up in the model snapshot
                if (strategy is not null)
                {
                    property.Builder.HasValueGenerationStrategy(strategy);
                }
            }
        }

        bool IsStrategyNoneNeeded(IReadOnlyProperty property, StoreObjectIdentifier storeObject)
        {
            if (property.ValueGenerated == ValueGenerated.OnAdd
                && !property.TryGetDefaultValue(storeObject, out _)
                && property.GetDefaultValueSql(storeObject) is null
                && property.GetComputedColumnSql(storeObject) is null
                && property.DeclaringEntityType.Model.GetValueGenerationStrategy() != NpgsqlValueGenerationStrategy.None)
            {
                var providerClrType = (property.GetValueConverter()
                        ?? (property.FindRelationalTypeMapping(storeObject)
                            ?? Dependencies.TypeMappingSource.FindMapping((IProperty)property))?.Converter)
                    ?.ProviderClrType.UnwrapNullableType();

                return providerClrType is not null && (providerClrType.IsInteger());
            }

            return false;
        }
    }
}