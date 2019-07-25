using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline
{
    public class NpgsqlSqlTranslatingExpressionVisitorFactory : RelationalSqlTranslatingExpressionVisitorFactory
    {
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        readonly IMemberTranslatorProvider _memberTranslatorProvider;
        readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;

        public NpgsqlSqlTranslatingExpressionVisitorFactory(
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
            : base(sqlExpressionFactory, memberTranslatorProvider, methodCallTranslatorProvider)
        {
            _sqlExpressionFactory = (NpgsqlSqlExpressionFactory)sqlExpressionFactory;
            _memberTranslatorProvider = memberTranslatorProvider;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
        }

        public override RelationalSqlTranslatingExpressionVisitor Create(
            IModel model,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            => new NpgsqlSqlTranslatingExpressionVisitor(
                model,
                queryableMethodTranslatingExpressionVisitor,
                _sqlExpressionFactory,
                _memberTranslatorProvider,
                _methodCallTranslatorProvider);
    }
}
