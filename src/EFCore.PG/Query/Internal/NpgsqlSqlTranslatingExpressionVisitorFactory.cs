using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlSqlTranslatingExpressionVisitorFactory : IRelationalSqlTranslatingExpressionVisitorFactory
    {
        [NotNull] readonly RelationalSqlTranslatingExpressionVisitorDependencies _dependencies;
        [NotNull] readonly ISqlGenerationHelper _sqlGenerationHelper;

        public NpgsqlSqlTranslatingExpressionVisitorFactory(
            [NotNull] RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
        {
            _dependencies = dependencies;
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        public virtual RelationalSqlTranslatingExpressionVisitor Create(
            QueryCompilationContext queryCompilationContext,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            => new NpgsqlSqlTranslatingExpressionVisitor(
                _dependencies,
                queryCompilationContext,
                queryableMethodTranslatingExpressionVisitor,
                _sqlGenerationHelper);
    }
}
