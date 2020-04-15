using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

            public IRelationalCommandBuilder Append(string value)
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

            public int ExecuteNonQuery(RelationalCommandParameterObject parameterObject)
            {
                var connection = parameterObject.Connection;
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteNonQuery(parameterObject);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException("", "", "", errorNumber);
                }

                return result;
            }

            public Task<int> ExecuteNonQueryAsync(
                RelationalCommandParameterObject parameterObject,
                CancellationToken cancellationToken = new CancellationToken())
            {
                var connection = parameterObject.Connection;
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteNonQueryAsync(parameterObject, cancellationToken);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException("", "", "", errorNumber);
                }

                return result;
            }

            public object ExecuteScalar(RelationalCommandParameterObject parameterObject)
            {
                var connection = parameterObject.Connection;
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteScalar(parameterObject);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException("", "", "", errorNumber);
                }

                return result;
            }

            public async Task<object> ExecuteScalarAsync(
                RelationalCommandParameterObject parameterObject,
                CancellationToken cancellationToken = new CancellationToken())
            {
                var connection = parameterObject.Connection;
                var errorNumber = PreExecution(connection);

                var result = await _realRelationalCommand.ExecuteScalarAsync(parameterObject, cancellationToken);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException("", "", "", errorNumber);
                }

                return result;
            }

            public RelationalDataReader ExecuteReader(RelationalCommandParameterObject parameterObject)
            {
                var connection = parameterObject.Connection;
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteReader(parameterObject);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    result.Dispose(); // Normally, in non-test case, reader is disposed by using in caller code
                    throw new PostgresException("", "", "", errorNumber);
                }

                return result;
            }

            public async Task<RelationalDataReader> ExecuteReaderAsync(
                RelationalCommandParameterObject parameterObject,
                CancellationToken cancellationToken = new CancellationToken())
            {
                var connection = parameterObject.Connection;
                var errorNumber = PreExecution(connection);

                var result = await _realRelationalCommand.ExecuteReaderAsync(parameterObject, cancellationToken);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    result.Dispose(); // Normally, in non-test case, reader is disposed by using in caller code
                    throw new PostgresException("", "", "", errorNumber);
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
                            throw new PostgresException("", "", "", testConnection.ErrorCode);
                        }
                        errorNumber = testConnection.ErrorCode;
                    }
                }
                return errorNumber;
            }
        }
    }
}
