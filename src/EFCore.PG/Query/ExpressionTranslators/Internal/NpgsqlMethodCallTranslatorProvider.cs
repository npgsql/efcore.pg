using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public NpgsqlMethodCallTranslatorProvider(
            [NotNull] RelationalMethodCallTranslatorProviderDependencies dependencies,
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] INpgsqlOptions npgsqlOptions)
            : base(dependencies)
        {
            var npgsqlSqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
            var npgsqlTypeMappingSource = (NpgsqlTypeMappingSource)typeMappingSource;
            var jsonTranslator = new NpgsqlJsonPocoTranslator(typeMappingSource, npgsqlSqlExpressionFactory);

            AddTranslators(new IMethodCallTranslator[]
            {
                new NpgsqlArrayTranslator(typeMappingSource, npgsqlSqlExpressionFactory, jsonTranslator),
                new NpgsqlByteArrayMethodTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlConvertTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlDateTimeMethodTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlFullTextSearchMethodTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlJsonDomTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlJsonDbFunctionsTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlLikeTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlMathTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlNetworkTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlNewGuidTranslator(npgsqlSqlExpressionFactory, npgsqlOptions.PostgresVersion),
                new NpgsqlObjectToStringTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlRandomTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlRangeTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlRegexIsMatchTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlStringMethodTranslator(npgsqlTypeMappingSource, npgsqlSqlExpressionFactory),
            });
        }
    }
}
