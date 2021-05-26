using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// A composite member translator that dispatches to multiple specialized member translators specific to Npgsql.
    /// </summary>
    public class NpgsqlMemberTranslatorProvider : RelationalMemberTranslatorProvider
    {
        public virtual NpgsqlJsonPocoTranslator JsonPocoTranslator { get; }

        public NpgsqlMemberTranslatorProvider(
            RelationalMemberTranslatorProviderDependencies dependencies,
            IRelationalTypeMappingSource typeMappingSource,
            INpgsqlOptions npgsqlOptions)
            : base(dependencies)
        {
            var sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
            JsonPocoTranslator = new NpgsqlJsonPocoTranslator(typeMappingSource, sqlExpressionFactory);

            AddTranslators(
                new IMemberTranslator[] {
                    new NpgsqlArrayTranslator(sqlExpressionFactory, JsonPocoTranslator, npgsqlOptions.UseRedshift),
                    new NpgsqlDateTimeMemberTranslator(sqlExpressionFactory),
                    new NpgsqlJsonDomTranslator(typeMappingSource, sqlExpressionFactory),
                    new NpgsqlLTreeTranslator(typeMappingSource, sqlExpressionFactory),
                    JsonPocoTranslator,
                    new NpgsqlRangeTranslator(typeMappingSource, sqlExpressionFactory),
                    new NpgsqlStringMemberTranslator(sqlExpressionFactory),
                    new NpgsqlTimeSpanMemberTranslator(sqlExpressionFactory),
                });
        }
    }
}
