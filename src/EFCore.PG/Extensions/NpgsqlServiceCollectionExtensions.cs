using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to configure Entity Framework Core for Npgsql.
/// </summary>
// ReSharper disable once UnusedMember.Global
public static class NpgsqlServiceCollectionExtensions
{
    /// <summary>
    ///     <para>
    ///         Registers the given Entity Framework context as a service in the <see cref="IServiceCollection" />
    ///         and configures it to connect to a PostgreSQL database.
    ///     </para>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure the SQL Server provider and connection string.
    ///     </para>
    ///     <para>
    ///         To configure the <see cref="DbContextOptions{TContext}" /> for the context, either override the
    ///         <see cref="DbContext.OnConfiguring" /> method in your derived context, or supply
    ///         an optional action to configure the <see cref="DbContextOptions" /> for the context.
    ///     </para>
    ///     <para>
    ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
    ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
    ///     </para>
    /// </summary>
    /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
    /// <param name="connectionString"> The connection string of the database to connect to. </param>
    /// <param name="npgsqlOptionsAction"> An optional action to allow additional SQL Server specific configuration. </param>
    /// <param name="optionsAction"> An optional action to configure the <see cref="DbContextOptions" /> for the context. </param>
    /// <returns> The same service collection so that multiple calls can be chained. </returns>
    public static IServiceCollection AddNpgsql<TContext>(
        this IServiceCollection serviceCollection,
        string? connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        return serviceCollection.AddDbContext<TContext>((_, options) =>
        {
            optionsAction?.Invoke(options);
            options.UseNpgsql(connectionString, npgsqlOptionsAction);
        });
    }

    /// <summary>
    ///     <para>
    ///         Adds the services required by the Npgsql database provider for Entity Framework
    ///         to an <see cref="IServiceCollection" />.
    ///     </para>
    ///     <para>
    ///         Calling this method is no longer necessary when building most applications, including those that
    ///         use dependency injection in ASP.NET or elsewhere.
    ///         It is only needed when building the internal service provider for use with
    ///         the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
    ///         This is not recommend other than for some advanced scenarios.
    ///     </para>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
    /// <returns>
    ///     The same service collection so that multiple calls can be chained.
    /// </returns>
    public static IServiceCollection AddEntityFrameworkNpgsql(this IServiceCollection serviceCollection)
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, NpgsqlLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<NpgsqlOptionsExtension>>()
            .TryAdd<IValueGeneratorCache>(p => p.GetRequiredService<INpgsqlValueGeneratorCache>())
            .TryAdd<IRelationalTypeMappingSource, NpgsqlTypeMappingSource>()
            .TryAdd<ISqlGenerationHelper, NpgsqlSqlGenerationHelper>()
            .TryAdd<IRelationalAnnotationProvider, NpgsqlAnnotationProvider>()
            .TryAdd<IModelValidator, NpgsqlModelValidator>()
            .TryAdd<IProviderConventionSetBuilder, NpgsqlConventionSetBuilder>()
            .TryAdd<IUpdateSqlGenerator, NpgsqlUpdateSqlGenerator>()
            .TryAdd<IModificationCommandFactory, NpgsqlModificationCommandFactory>()
            .TryAdd<IModificationCommandBatchFactory, NpgsqlModificationCommandBatchFactory>()
            .TryAdd<IValueGeneratorSelector, NpgsqlValueGeneratorSelector>()
            .TryAdd<IRelationalConnection>(p => p.GetRequiredService<INpgsqlRelationalConnection>())
            .TryAdd<IMigrationsSqlGenerator, NpgsqlMigrationsSqlGenerator>()
            .TryAdd<IRelationalDatabaseCreator, NpgsqlDatabaseCreator>()
            .TryAdd<IHistoryRepository, NpgsqlHistoryRepository>()
            .TryAdd<ICompiledQueryCacheKeyGenerator, NpgsqlCompiledQueryCacheKeyGenerator>()
            .TryAdd<IExecutionStrategyFactory, NpgsqlExecutionStrategyFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, NpgsqlQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IMethodCallTranslatorProvider, NpgsqlMethodCallTranslatorProvider>()
            .TryAdd<IAggregateMethodCallTranslatorProvider, NpgsqlAggregateMethodCallTranslatorProvider>()
            .TryAdd<IMemberTranslatorProvider, NpgsqlMemberTranslatorProvider>()
            .TryAdd<IEvaluatableExpressionFilter, NpgsqlEvaluatableExpressionFilter>()
            .TryAdd<IQuerySqlGeneratorFactory, NpgsqlQuerySqlGeneratorFactory>()
            .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, NpgsqlSqlTranslatingExpressionVisitorFactory>()
            .TryAdd<IQueryTranslationPreprocessorFactory, NpgsqlQueryTranslationPreprocessorFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, NpgsqlQueryTranslationPostprocessorFactory>()
            .TryAdd<IRelationalParameterBasedSqlProcessorFactory, NpgsqlParameterBasedSqlProcessorFactory>()
            .TryAdd<ISqlExpressionFactory, NpgsqlSqlExpressionFactory>()
            .TryAdd<ISingletonOptions, INpgsqlSingletonOptions>(p => p.GetRequiredService<INpgsqlSingletonOptions>())
            .TryAdd<IValueConverterSelector, NpgsqlValueConverterSelector>()
            .TryAdd<IQueryCompilationContextFactory, NpgsqlQueryCompilationContextFactory>()
            .TryAddProviderSpecificServices(
                b => b
                    .TryAddSingleton<INpgsqlValueGeneratorCache, NpgsqlValueGeneratorCache>()
                    .TryAddSingleton<INpgsqlSingletonOptions, NpgsqlSingletonOptions>()
                    .TryAddSingleton<INpgsqlSequenceValueGeneratorFactory, NpgsqlSequenceValueGeneratorFactory>()
                    .TryAddScoped<INpgsqlRelationalConnection, NpgsqlRelationalConnection>())
            .TryAddCoreServices();

        return serviceCollection;
    }
}
