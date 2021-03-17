using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public virtual NpgsqlLTreeTranslator LTreeTranslator { get; }

        public NpgsqlMethodCallTranslatorProvider(
            RelationalMethodCallTranslatorProviderDependencies dependencies,
            IRelationalTypeMappingSource typeMappingSource,
            INpgsqlOptions npgsqlOptions)
            : base(dependencies)
        {
            var npgsqlSqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
            var npgsqlTypeMappingSource = (NpgsqlTypeMappingSource)typeMappingSource;
            var jsonTranslator = new NpgsqlJsonPocoTranslator(typeMappingSource, npgsqlSqlExpressionFactory);
            LTreeTranslator = new NpgsqlLTreeTranslator(typeMappingSource, npgsqlSqlExpressionFactory);

            AddTranslators(new IMethodCallTranslator[]
            {
                new NpgsqlArrayTranslator(typeMappingSource, npgsqlSqlExpressionFactory, jsonTranslator),
                new NpgsqlByteArrayMethodTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlConvertTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlDateTimeMethodTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlFullTextSearchMethodTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlFuzzyStringMatchMethodTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlJsonDomTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlJsonDbFunctionsTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlLikeTranslator(npgsqlSqlExpressionFactory),
                LTreeTranslator,
                new NpgsqlMathTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlNetworkTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlNewGuidTranslator(npgsqlSqlExpressionFactory, npgsqlOptions.PostgresVersion),
                new NpgsqlObjectToStringTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlRandomTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlRangeTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlRegexIsMatchTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlStringMethodTranslator(npgsqlTypeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlTrigramsMethodTranslator(npgsqlTypeMappingSource, npgsqlSqlExpressionFactory),
            });
        }
    }
}
