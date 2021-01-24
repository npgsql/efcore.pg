using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    /// <summary>
    /// The default factory for Npgsql-specific query SQL generators.
    /// </summary>
    public class NpgsqlQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        [NotNull] readonly QuerySqlGeneratorDependencies _dependencies;
        [NotNull] readonly INpgsqlOptions _npgsqlOptions;

        public NpgsqlQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] INpgsqlOptions npgsqlOptions)
        {
            _dependencies = dependencies;
            _npgsqlOptions = npgsqlOptions;
        }

        public virtual QuerySqlGenerator Create()
            => new NpgsqlQuerySqlGenerator(
                _dependencies,
                _npgsqlOptions.ReverseNullOrderingEnabled,
                _npgsqlOptions.PostgresVersion);
    }
}
