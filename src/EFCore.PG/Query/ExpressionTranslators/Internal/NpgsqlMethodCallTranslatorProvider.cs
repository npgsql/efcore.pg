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
                new NpgsqlNewGuidTranslator(npgsqlSqlExpressionFactory, npgsqlOptions.PostgresVersion),
                new NpgsqlLikeTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlObjectToStringTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlMathTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlStringMethodTranslator(npgsqlTypeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlRegexIsMatchTranslator(npgsqlSqlExpressionFactory),
                new NpgsqlFullTextSearchMethodTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlRangeTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlNetworkTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlJsonDomTranslator(typeMappingSource, npgsqlSqlExpressionFactory),
                new NpgsqlJsonDbFunctionsTranslator(typeMappingSource, npgsqlSqlExpressionFactory)
            });
        }
    }
}
