using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlParameterBasedSqlProcessor : RelationalParameterBasedSqlProcessor
    {
        readonly SqlNullabilityProcessor _sqlNullabilityProcessor;

        public NpgsqlParameterBasedSqlProcessor(
            [NotNull] RelationalParameterBasedSqlProcessorDependencies dependencies,
            bool useRelationalNulls)
            : base(dependencies, useRelationalNulls)
            => _sqlNullabilityProcessor = new NpgsqlSqlNullabilityProcessor(dependencies, useRelationalNulls);

        /// <inheritdoc />
        protected override SelectExpression ProcessSqlNullability(
            SelectExpression selectExpression, IReadOnlyDictionary<string, object> parametersValues, out bool canCache)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(parametersValues, nameof(parametersValues));

            return _sqlNullabilityProcessor.Process(selectExpression, parametersValues, out canCache);
        }
    }
}
