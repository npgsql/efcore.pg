using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlUnaccentMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        public NpgsqlUnaccentMethodCallTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
            => Translators = new IMethodCallTranslator[]
            {
                new NpgsqlUnaccentMethodTranslator((NpgsqlSqlExpressionFactory) sqlExpressionFactory, typeMappingSource),
            };

        public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
    }
}
