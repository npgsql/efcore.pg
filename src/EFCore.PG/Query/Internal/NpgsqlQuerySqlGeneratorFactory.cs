using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
/// The default factory for Npgsql-specific query SQL generators.
/// </summary>
public class NpgsqlQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
{
    private readonly QuerySqlGeneratorDependencies _dependencies;
    private readonly INpgsqlSingletonOptions _npgsqlSingletonOptions;

    public NpgsqlQuerySqlGeneratorFactory(
        QuerySqlGeneratorDependencies dependencies,
        INpgsqlSingletonOptions npgsqlSingletonOptions)
    {
        _dependencies = dependencies;
        _npgsqlSingletonOptions = npgsqlSingletonOptions;
    }

    public virtual QuerySqlGenerator Create()
        => new NpgsqlQuerySqlGenerator(
            _dependencies,
            _npgsqlSingletonOptions.ReverseNullOrderingEnabled,
            _npgsqlSingletonOptions.PostgresVersion);
}