using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlTrigramsMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        public NpgsqlTrigramsMethodCallTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
            => Translators = new IMethodCallTranslator[]
            {
                new NpgsqlTrigramsMethodTranslator((NpgsqlSqlExpressionFactory) sqlExpressionFactory, typeMappingSource),
            };

        public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
    }
}
