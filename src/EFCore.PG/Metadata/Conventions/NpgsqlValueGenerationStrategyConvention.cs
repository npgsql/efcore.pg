using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions
{
    /// <summary>
    /// A convention that configures the default model <see cref="NpgsqlValueGenerationStrategy"/> as
    /// <see cref="NpgsqlValueGenerationStrategy.IdentityByDefaultColumn"/> for newer PostgreSQL versions,
    /// and <see cref="NpgsqlValueGenerationStrategy.SerialColumn"/> for pre-10.0 versions.
    /// </summary>
    public class NpgsqlValueGenerationStrategyConvention : IModelInitializedConvention, IModelFinalizingConvention
    {
        readonly Version _postgresVersion;

        /// <summary>
        /// Creates a new instance of <see cref="NpgsqlValueGenerationStrategyConvention" />.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
        /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
        /// <param name="postgresVersion">The PostgreSQL version being targeted. This affects the default value generation strategy.</param>
        public NpgsqlValueGenerationStrategyConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies,
            [CanBeNull] Version postgresVersion)
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
                    var table = entityType.GetTableName();
                    if (table != null)
                    {
                        var storeObject = StoreObjectIdentifier.Table(table, entityType.GetSchema());
                        strategy = property.GetValueGenerationStrategy(storeObject);
                        if (strategy == NpgsqlValueGenerationStrategy.None
                            && !IsStrategyNoneNeeded(property, storeObject))
                        {
                            strategy = null;
                        }
                    }
                    else
                    {
                        var view = entityType.GetViewName();
                        if (view != null)
                        {
                            var storeObject = StoreObjectIdentifier.View(view, entityType.GetViewSchema());
                            strategy = property.GetValueGenerationStrategy(storeObject);
                            if (strategy == NpgsqlValueGenerationStrategy.None
                                && !IsStrategyNoneNeeded(property, storeObject))
                            {
                                strategy = null;
                            }
                        }
                    }

                    // Needed for the annotation to show up in the model snapshot
                    if (strategy != null)
                    {
                        property.Builder.HasValueGenerationStrategy(strategy);
                    }
                }
            }

            static bool IsStrategyNoneNeeded(IProperty property, StoreObjectIdentifier storeObject)
            {
                if (property.ValueGenerated == ValueGenerated.OnAdd
                    && property.GetDefaultValue(storeObject) == null
                    && property.GetDefaultValueSql(storeObject) == null
                    && property.GetComputedColumnSql(storeObject) == null
                    && property.DeclaringEntityType.Model.GetValueGenerationStrategy() != NpgsqlValueGenerationStrategy.None)
                {
                    var providerClrType = (property.GetValueConverter() ?? property.FindRelationalTypeMapping(storeObject)?.Converter)
                        ?.ProviderClrType.UnwrapNullableType();

                    return providerClrType != null && (providerClrType.IsInteger());
                }

                return false;
            }
        }
    }
}
