using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public NpgsqlMethodCallTranslatorProvider(
            ISqlExpressionFactory sqlExpressionFactory,
            IRelationalTypeMappingSource typeMappingSource,
            IEnumerable<IMethodCallTranslatorPlugin> plugins)
            : base(sqlExpressionFactory, plugins)
        {
            var npgsqlSqlExpressionFactory = (NpgsqlSqlExpressionFactory)sqlExpressionFactory;
            var npgsqlTypeMappingSource = (NpgsqlTypeMappingSource)typeMappingSource;

            AddTranslators(new IMethodCallTranslator[]
            {
                new NpgsqlArrayMethodTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlConvertTranslator(sqlExpressionFactory),
                new NpgsqlDateTimeMethodTranslator(sqlExpressionFactory, npgsqlTypeMappingSource),
                new NpgsqlNewGuidTranslator(sqlExpressionFactory),
                new NpgsqlLikeTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlObjectToStringTranslator(sqlExpressionFactory),
                new NpgsqlMathTranslator(sqlExpressionFactory),
                new NpgsqlStringMethodTranslator(sqlExpressionFactory, npgsqlTypeMappingSource),
                new NpgsqlRegexIsMatchTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlFullTextSearchMethodTranslator(npgsqlSqlExpressionFactory, npgsqlTypeMappingSource),
                new NpgsqlRangeTranslator(sqlExpressionFactory),
                new NpgsqlNetworkTranslator(sqlExpressionFactory, typeMappingSource)
            });
        }
    }
}
