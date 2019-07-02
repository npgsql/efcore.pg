#if PREVIEW7
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlQueryCompilationContext : RelationalQueryCompilationContext
    {
        public NpgsqlQueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            bool trackQueryResults)
            : base(
                dependencies,
                linqOperatorProvider,
                queryMethodProvider,
                trackQueryResults)
        {
        }

        public override bool IsLateralJoinSupported => true;

        public override int MaxTableAliasLength => 63;
    }
}
#endif
