using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

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
            IRelationalTypeMappingSource typeMappingSource)
            : base(dependencies)
        {
            var sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
            JsonPocoTranslator = new NpgsqlJsonPocoTranslator(typeMappingSource, sqlExpressionFactory);

            AddTranslators(
                new IMemberTranslator[] {
                    new NpgsqlArrayTranslator(typeMappingSource, sqlExpressionFactory, JsonPocoTranslator),
                    new NpgsqlDateTimeMemberTranslator(sqlExpressionFactory),
                    new NpgsqlJsonDomTranslator(typeMappingSource, sqlExpressionFactory),
                    new NpgsqlLTreeTranslator(typeMappingSource, sqlExpressionFactory),
                    JsonPocoTranslator,
                    new NpgsqlRangeTranslator(typeMappingSource, sqlExpressionFactory),
                    new NpgsqlStringMemberTranslator(sqlExpressionFactory),
                    new NpgsqlTimeSpanMemberTranslator(sqlExpressionFactory)
                });
        }
    }
}
