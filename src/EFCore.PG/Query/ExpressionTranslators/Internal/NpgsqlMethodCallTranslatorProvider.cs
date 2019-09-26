using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public NpgsqlMethodCallTranslatorProvider(
            [NotNull] RelationalMethodCallTranslatorProviderDependencies dependencies,
            IRelationalTypeMappingSource typeMappingSource)
            : base(dependencies)
        {
            var npgsqlSqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
            var npgsqlTypeMappingSource = (NpgsqlTypeMappingSource)typeMappingSource;
            var jsonTranslator = new NpgsqlJsonPocoTranslator(npgsqlSqlExpressionFactory);

            AddTranslators(new IMethodCallTranslator[]
            {
                new NpgsqlArrayMethodTranslator(npgsqlSqlExpressionFactory, jsonTranslator),
                new NpgsqlConvertTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlDateTimeMethodTranslator(npgsqlSqlExpressionFactory, npgsqlTypeMappingSource),
                new NpgsqlNewGuidTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlLikeTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlObjectToStringTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlMathTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlStringMethodTranslator(npgsqlSqlExpressionFactory, npgsqlTypeMappingSource),
                new NpgsqlRegexIsMatchTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlFullTextSearchMethodTranslator(npgsqlSqlExpressionFactory, npgsqlTypeMappingSource),
                new NpgsqlRangeTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlNetworkTranslator(npgsqlSqlExpressionFactory, typeMappingSource),
                new NpgsqlJsonDomTranslator(npgsqlSqlExpressionFactory, typeMappingSource),
                new NpgsqlJsonDbFunctionsTranslator(npgsqlSqlExpressionFactory, typeMappingSource)
            });
        }
    }
}
