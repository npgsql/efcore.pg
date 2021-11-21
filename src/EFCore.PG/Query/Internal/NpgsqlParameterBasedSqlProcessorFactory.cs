namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

public class NpgsqlParameterBasedSqlProcessorFactory : IRelationalParameterBasedSqlProcessorFactory
{
    private readonly RelationalParameterBasedSqlProcessorDependencies _dependencies;

    public NpgsqlParameterBasedSqlProcessorFactory(
        RelationalParameterBasedSqlProcessorDependencies dependencies)
        => _dependencies = dependencies;

    public virtual RelationalParameterBasedSqlProcessor Create(bool useRelationalNulls)
        => new NpgsqlParameterBasedSqlProcessor(_dependencies, useRelationalNulls);
}