using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
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
    public class NpgsqlValueGenerationStrategyConvention : IModelInitializedConvention, IModelFinalizedConvention
    {
        [CanBeNull] readonly Version _postgresVersion;

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

        /// <summary>
        /// Called after a model is initialized.
        /// </summary>
        /// <param name="modelBuilder">The builder for the model.</param>
        /// <param name="context">Additional information associated with convention execution.</param>
        public virtual void ProcessModelInitialized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
            => modelBuilder.HasValueGenerationStrategy(
                _postgresVersion != null && _postgresVersion < new Version(10, 0)
                    ? NpgsqlValueGenerationStrategy.SerialColumn
                    : NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        /// <summary>
        /// Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder">The builder for the model.</param>
        /// <param name="context">Additional information associated with convention execution.</param>
        public virtual void ProcessModelFinalized(
            IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    // Needed for the annotation to show up in the model snapshot
                    var strategy = property.GetValueGenerationStrategy();
                    if (strategy != NpgsqlValueGenerationStrategy.None)
                    {
                        property.Builder.HasValueGenerationStrategy(strategy);
                    }
                }
            }
        }
    }
}
