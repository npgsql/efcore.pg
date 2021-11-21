using Microsoft.EntityFrameworkCore.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

public class NpgsqlQueryCompilationContext : RelationalQueryCompilationContext
{
    public NpgsqlQueryCompilationContext(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies, bool async)
        : base(dependencies, relationalDependencies, async)
    {
    }

    public override bool IsBuffering
        => base.IsBuffering ||
            QuerySplittingBehavior == Microsoft.EntityFrameworkCore.QuerySplittingBehavior.SplitQuery;
}