using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

[EntityFrameworkInternal]
public class NpgsqlConventionSetBuilder : RelationalConventionSetBuilder
{
    private readonly Version _postgresVersion;

    [EntityFrameworkInternal]
    public NpgsqlConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies,
        INpgsqlOptions npgsqlOptions)
        : base(dependencies, relationalDependencies)
        => _postgresVersion = npgsqlOptions.PostgresVersion;

    [EntityFrameworkInternal]
    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        var valueGenerationStrategyConvention = new NpgsqlValueGenerationStrategyConvention(Dependencies, _postgresVersion);
        conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
        conventionSet.ModelInitializedConventions.Add(new RelationalMaxIdentifierLengthConvention(63, Dependencies, RelationalDependencies));

        ValueGenerationConvention valueGenerationConvention = new NpgsqlValueGenerationConvention(Dependencies, RelationalDependencies);
        ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, valueGenerationConvention);

        ReplaceConvention(
            conventionSet.EntityTypeAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);

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
        ReplaceConvention(
            conventionSet.ModelFinalizingConventions,
            (SharedTableConvention)new NpgsqlSharedTableConvention(Dependencies, RelationalDependencies));

        ReplaceConvention(
            conventionSet.ModelFinalizedConventions,
            (RuntimeModelConvention)new NpgsqlRuntimeModelConvention(Dependencies, RelationalDependencies));

        return conventionSet;
    }

    /// <summary>
    ///     <para>
    ///         Call this method to build a <see cref="ConventionSet" /> for Npgsql when using
    ///         the <see cref="ModelBuilder" /> outside of <see cref="DbContext.OnModelCreating" />.
    ///     </para>
    ///     <para>
    ///         Note that it is unusual to use this method.
    ///         Consider using <see cref="DbContext" /> in the normal way instead.
    ///     </para>
    /// </summary>
    /// <returns> The convention set. </returns>
    public static ConventionSet Build()
    {
        using var serviceScope = CreateServiceScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
        return ConventionSet.CreateConventionSet(context);
    }

    /// <summary>
    ///     <para>
    ///         Call this method to build a <see cref="ModelBuilder" /> for Npgsql outside of <see cref="DbContext.OnModelCreating" />.
    ///     </para>
    ///     <para>
    ///         Note that it is unusual to use this method.
    ///         Consider using <see cref="DbContext" /> in the normal way instead.
    ///     </para>
    /// </summary>
    /// <returns> The convention set. </returns>
    public static ModelBuilder CreateModelBuilder()
    {
        using var serviceScope = CreateServiceScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
        return new ModelBuilder(ConventionSet.CreateConventionSet(context), context.GetService<ModelDependencies>());
    }

    private static IServiceScope CreateServiceScope()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkNpgsql()
            .AddDbContext<DbContext>(
                (p, o) =>
                    o.UseNpgsql("Server=.")
                        .UseInternalServiceProvider(p))
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
    }
}