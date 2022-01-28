using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

public class NpgsqlQueryCompilationContextFactory : IQueryCompilationContextFactory
{
    private readonly QueryCompilationContextDependencies _dependencies;
    private readonly RelationalQueryCompilationContextDependencies _relationalDependencies;

    public NpgsqlQueryCompilationContextFactory(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies)
    {
        Check.NotNull(dependencies, nameof(dependencies));
        Check.NotNull(relationalDependencies, nameof(relationalDependencies));

        _dependencies = dependencies;
        _relationalDependencies = relationalDependencies;
    }

    public virtual QueryCompilationContext Create(bool async)
        => new NpgsqlQueryCompilationContext(_dependencies, _relationalDependencies, async);
}