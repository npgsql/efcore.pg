using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class TestRelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        public TestRelationalCommandBuilderFactory(
            RelationalCommandBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        public RelationalCommandBuilderDependencies Dependencies { get; }

        public virtual IRelationalCommandBuilder Create()
            => new TestRelationalCommandBuilder(Dependencies);

#pragma warning disable EF1001
        class TestRelationalCommandBuilder : IRelationalCommandBuilder
        {
            readonly List<IRelationalParameter> _parameters = new List<IRelationalParameter>();

            public TestRelationalCommandBuilder(
                RelationalCommandBuilderDependencies dependencies)
            {
                Dependencies = dependencies;
            }

            public IndentedStringBuilder Instance { get; } = new IndentedStringBuilder();

            public RelationalCommandBuilderDependencies Dependencies { get; }

            public IReadOnlyList<IRelationalParameter> Parameters => _parameters;

            public IRelationalCommandBuilder AddParameter(IRelationalParameter parameter)
            {
                _parameters.Add(parameter);

                return this;
            }

            public IRelationalTypeMappingSource TypeMappingSource => Dependencies.TypeMappingSource;

            public IRelationalCommand Build()
                => new TestRelationalCommand(
                    Dependencies,
                    Instance.ToString(),
                    Parameters);

            public IRelationalCommandBuilder Append(object value)
            {
                Instance.Append(value);

                return this;
            }

            public IRelationalCommandBuilder AppendLine()
            {
                Instance.AppendLine();

                return this;
            }

            public IRelationalCommandBuilder IncrementIndent()
            {
                Instance.IncrementIndent();

                return this;
            }

            public IRelationalCommandBuilder DecrementIndent()
            {
                Instance.DecrementIndent();

                return this;
            }

            public int CommandTextLength => Instance.Length;
        }
#pragma warning restore EF1001

        class TestRelationalCommand : IRelationalCommand
        {
            readonly RelationalCommand _realRelationalCommand;

            public TestRelationalCommand(
                RelationalCommandBuilderDependencies dependencies,
                string commandText,
                IReadOnlyList<IRelationalParameter> parameters)
            {
                _realRelationalCommand = new RelationalCommand(dependencies, commandText, parameters);
            }

            public string CommandText => _realRelationalCommand.CommandText;

            public IReadOnlyList<IRelationalParameter> Parameters => _realRelationalCommand.Parameters;

            public int ExecuteNonQuery(
                IRelationalConnection connection,
                IReadOnlyDictionary<string, object> parameterValues,
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
            {
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteNonQuery(connection, parameterValues, logger);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            public Task<int> ExecuteNonQueryAsync(
                IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues,
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                CancellationToken cancellationToken = new CancellationToken())
            {
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteNonQueryAsync(connection, parameterValues, logger, cancellationToken);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            public object ExecuteScalar(
                IRelationalConnection connection,
                IReadOnlyDictionary<string, object> parameterValues,
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
            {
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteScalar(connection, parameterValues, logger);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            public async Task<object> ExecuteScalarAsync(
                IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues,
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                CancellationToken cancellationToken = new CancellationToken())
            {
                var errorNumber = PreExecution(connection);

                var result = await _realRelationalCommand.ExecuteScalarAsync(connection, parameterValues, logger, cancellationToken);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            public RelationalDataReader ExecuteReader(
                IRelationalConnection connection,
                IReadOnlyDictionary<string, object> parameterValues,
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
            {
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteReader(connection, parameterValues, logger);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            public async Task<RelationalDataReader> ExecuteReaderAsync(
                IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues,
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                CancellationToken cancellationToken = new CancellationToken())
            {
                var errorNumber = PreExecution(connection);

                var result = await _realRelationalCommand.ExecuteReaderAsync(connection, parameterValues, logger, cancellationToken);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            string PreExecution(IRelationalConnection connection)
            {
                string errorNumber = null;
                var testConnection = (TestNpgsqlConnection)connection;

                testConnection.ExecutionCount++;
                if (testConnection.ExecutionFailures.Count > 0)
                {
                    var fail = testConnection.ExecutionFailures.Dequeue();
                    if (fail.HasValue)
                    {
                        if (fail.Value)
                        {
                            testConnection.DbConnection.Close();
                            throw new PostgresException { SqlState = testConnection.ErrorCode };
                        }
                        errorNumber = testConnection.ErrorCode;
                    }
                }
                return errorNumber;
            }
        }
    }
}
