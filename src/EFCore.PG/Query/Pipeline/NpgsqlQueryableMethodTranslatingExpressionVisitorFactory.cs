using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline
{
    public class NpgsqlQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
    {
        readonly ISqlExpressionFactory _sqlExpressionFactory;
        readonly IRelationalSqlTranslatingExpressionVisitorFactory _relationalSqlTranslatingExpressionVisitorFactory;

        public NpgsqlQueryableMethodTranslatingExpressionVisitorFactory(
            IRelationalSqlTranslatingExpressionVisitorFactory relationalSqlTranslatingExpressionVisitorFactory,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _relationalSqlTranslatingExpressionVisitorFactory = relationalSqlTranslatingExpressionVisitorFactory;
        }

        public QueryableMethodTranslatingExpressionVisitor Create(IModel model)
            => new NpgsqlQueryableMethodTranslatingExpressionVisitor(
                model,
                _relationalSqlTranslatingExpressionVisitorFactory,
                _sqlExpressionFactory);
    }
}
