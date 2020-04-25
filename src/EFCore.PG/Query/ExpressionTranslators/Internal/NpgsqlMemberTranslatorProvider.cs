using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

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
            var npgsqlSqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
            JsonPocoTranslator = new NpgsqlJsonPocoTranslator(npgsqlSqlExpressionFactory);

            AddTranslators(
                new IMemberTranslator[] {
                    new NpgsqlArrayTranslator(npgsqlSqlExpressionFactory, JsonPocoTranslator),
                    new NpgsqlStringMemberTranslator(npgsqlSqlExpressionFactory),
                    new NpgsqlDateTimeMemberTranslator(npgsqlSqlExpressionFactory),
                    new NpgsqlRangeTranslator(npgsqlSqlExpressionFactory),
                    new NpgsqlJsonDomTranslator(npgsqlSqlExpressionFactory, typeMappingSource),
                    JsonPocoTranslator,
                    new NpgsqlTimeSpanMemberTranslator(npgsqlSqlExpressionFactory)
                });
        }
    }
}
