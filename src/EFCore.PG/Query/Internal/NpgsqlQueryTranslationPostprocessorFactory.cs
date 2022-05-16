namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

public class NpgsqlQueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
{
    public NpgsqlQueryTranslationPostprocessorFactory(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual QueryTranslationPostprocessorDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryTranslationPostprocessorDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
        => new NpgsqlQueryTranslationPostprocessor(Dependencies, RelationalDependencies, queryCompilationContext);
}
