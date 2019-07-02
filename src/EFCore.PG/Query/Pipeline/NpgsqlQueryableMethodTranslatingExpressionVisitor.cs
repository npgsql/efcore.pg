using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline
{
    public class NpgsqlQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
    {
        public NpgsqlQueryableMethodTranslatingExpressionVisitor(
            IModel model,
            IRelationalSqlTranslatingExpressionVisitorFactory relationalSqlTranslatingExpressionVisitorFactory,
            ISqlExpressionFactory sqlExpressionFactory)
            : base(model, relationalSqlTranslatingExpressionVisitorFactory, sqlExpressionFactory)
        {
        }
    }
}
