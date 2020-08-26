using JetBrains.Annotations;
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
            [NotNull] RelationalMemberTranslatorProviderDependencies dependencies,
            [NotNull] IRelationalTypeMappingSource typeMappingSource)
            : base(dependencies)
        {
            var sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
            JsonPocoTranslator = new NpgsqlJsonPocoTranslator(typeMappingSource, sqlExpressionFactory);

            AddTranslators(
                new IMemberTranslator[] {
                    new NpgsqlArrayTranslator(typeMappingSource, sqlExpressionFactory, JsonPocoTranslator),
                    new NpgsqlStringMemberTranslator(sqlExpressionFactory),
                    new NpgsqlDateTimeMemberTranslator(sqlExpressionFactory),
                    new NpgsqlRangeTranslator(typeMappingSource, sqlExpressionFactory),
                    new NpgsqlJsonDomTranslator(typeMappingSource, sqlExpressionFactory),
                    JsonPocoTranslator,
                    new NpgsqlTimeSpanMemberTranslator(sqlExpressionFactory)
                });
        }
    }
}
