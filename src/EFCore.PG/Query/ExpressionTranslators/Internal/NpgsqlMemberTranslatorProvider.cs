using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// A composite member translator that dispatches to multiple specialized member translators specific to Npgsql.
    /// </summary>
    public class NpgsqlMemberTranslatorProvider : RelationalMemberTranslatorProvider
    {
        public NpgsqlMemberTranslatorProvider(
            ISqlExpressionFactory sqlExpressionFactory,
            IEnumerable<IMemberTranslatorPlugin> plugins)
            : base(sqlExpressionFactory, plugins)
        {
            var npgsqlSqlExpressionFactory = (NpgsqlSqlExpressionFactory)sqlExpressionFactory;

            AddTranslators(
                new IMemberTranslator[] {
                    new NpgsqlStringMemberTranslator(sqlExpressionFactory),
                    new NpgsqlDateTimeMemberTranslator(npgsqlSqlExpressionFactory),
                    new NpgsqlRangeTranslator(sqlExpressionFactory)
                });
        }
    }
}
