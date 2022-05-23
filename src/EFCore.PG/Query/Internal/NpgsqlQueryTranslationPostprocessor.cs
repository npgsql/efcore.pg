namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

public class NpgsqlQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
{
    public NpgsqlQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
    }

    public override Expression Process(Expression query)
    {
        var result = base.Process(query);

        result = new NpgsqlSetOperationTypeResolutionCompensatingExpressionVisitor().Visit(result);

        return result;
    }
}
