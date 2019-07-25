using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    /// <summary>
    /// The default factory for Npgsql-specific query SQL generators.
    /// </summary>
    public class NpgsqlQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        [NotNull] readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        [NotNull] readonly ISqlGenerationHelper _sqlGenerationHelper;
        [NotNull] readonly INpgsqlOptions _npgsqlOptions;

        public NpgsqlQuerySqlGeneratorFactory(
            IRelationalCommandBuilderFactory commandBuilderFactory,
            ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] INpgsqlOptions npgsqlOptions)
        {
            _commandBuilderFactory = commandBuilderFactory;
            _sqlGenerationHelper = sqlGenerationHelper;
            _npgsqlOptions = npgsqlOptions;
        }

        public virtual QuerySqlGenerator Create()
            => new NpgsqlQuerySqlGenerator(
                _commandBuilderFactory,
                _sqlGenerationHelper,
                _npgsqlOptions.ReverseNullOrderingEnabled,
                _npgsqlOptions.PostgresVersion);
    }
}
