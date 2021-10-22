using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
/// The default factory for Npgsql-specific query SQL generators.
/// </summary>
public class NpgsqlQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
{
    private readonly QuerySqlGeneratorDependencies _dependencies;
    private readonly INpgsqlOptions _npgsqlOptions;

    public NpgsqlQuerySqlGeneratorFactory(
        QuerySqlGeneratorDependencies dependencies,
        INpgsqlOptions npgsqlOptions)
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