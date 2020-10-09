using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions
{
    [EntityFrameworkInternal]
    public class NpgsqlConventionSetBuilder : RelationalConventionSetBuilder
    {
        readonly Version _postgresVersion;

        [EntityFrameworkInternal]
        public NpgsqlConventionSetBuilder(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies,
            [NotNull] INpgsqlOptions npgsqlOptions)
            : base(dependencies, relationalDependencies)
            => _postgresVersion = npgsqlOptions.PostgresVersion;

        [EntityFrameworkInternal]
        public override ConventionSet CreateConventionSet()
        {
            var conventionSet = base.CreateConventionSet();

            var valueGenerationStrategyConvention = new NpgsqlValueGenerationStrategyConvention(Dependencies, RelationalDependencies, _postgresVersion);
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
            conventionSet.ModelInitializedConventions.Add(new RelationalMaxIdentifierLengthConvention(63, Dependencies, RelationalDependencies));

            var valueGenerationConvention = new NpgsqlValueGenerationConvention(Dependencies, RelationalDependencies);
            ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, valueGenerationConvention);

            ReplaceConvention(conventionSet.EntityTypePrimaryKeyChangedConventions, valueGenerationConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGenerationConvention);

            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGenerationConvention);

            var storeGenerationConvention =
                new NpgsqlStoreGenerationConvention(Dependencies, RelationalDependencies);
            ReplaceConvention(conventionSet.PropertyAnnotationChangedConventions, storeGenerationConvention);
            ReplaceConvention(
                conventionSet.PropertyAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);

            conventionSet.ModelFinalizingConventions.Add(valueGenerationStrategyConvention);
            ReplaceConvention(conventionSet.ModelFinalizingConventions, storeGenerationConvention);

            return conventionSet;
        }

        [EntityFrameworkInternal]
        public static ConventionSet Build()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddDbContext<DbContext>(o => o.UseNpgsql("Server=."))
                .BuildServiceProvider();

            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DbContext>())
                {
                    return ConventionSet.CreateConventionSet(context);
                }
            }
        }
    }
}
