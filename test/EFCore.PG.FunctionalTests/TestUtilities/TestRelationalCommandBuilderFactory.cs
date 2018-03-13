using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class TestRelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _logger;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public TestRelationalCommandBuilderFactory(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            IRelationalTypeMappingSource typeMappingSource)
        {
            _logger = logger;
            _typeMappingSource = typeMappingSource;
        }

        public virtual IRelationalCommandBuilder Create()
            => new TestRelationalCommandBuilder(_logger, _typeMappingSource);

        private class TestRelationalCommandBuilder : IRelationalCommandBuilder
        {
            private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _logger;

            public TestRelationalCommandBuilder(
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                IRelationalTypeMappingSource typeMappingSource)
            {
                _logger = logger;
                ParameterBuilder = new RelationalParameterBuilder(typeMappingSource);
            }

            IndentedStringBuilder IInfrastructure<IndentedStringBuilder>.Instance { get; } = new IndentedStringBuilder();

            public IRelationalParameterBuilder ParameterBuilder { get; }

            public IRelationalCommand Build()
                => new TestRelationalCommand(
                    _logger,
                    ((IInfrastructure<IndentedStringBuilder>)this).Instance.ToString(),
                    ParameterBuilder.Parameters);
        }

        private class TestRelationalCommand : IRelationalCommand
        {
            private readonly RelationalCommand _realRelationalCommand;

            public TestRelationalCommand(
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                string commandText,
                IReadOnlyList<IRelationalParameter> parameters)
            {
                _realRelationalCommand = new RelationalCommand(logger, commandText, parameters);
            }

            public string CommandText => _realRelationalCommand.CommandText;

            public IReadOnlyList<IRelationalParameter> Parameters => _realRelationalCommand.Parameters;

            public int ExecuteNonQuery(
                IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues)
            {
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteNonQuery(connection, parameterValues);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            public Task<int> ExecuteNonQueryAsync(
                IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, CancellationToken cancellationToken = new CancellationToken())
            {
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteNonQueryAsync(connection, parameterValues, cancellationToken);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            public object ExecuteScalar(
                IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues)
            {
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteScalar(connection, parameterValues);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            public async Task<object> ExecuteScalarAsync(
                IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, CancellationToken cancellationToken = new CancellationToken())
            {
                var errorNumber = PreExecution(connection);

                var result = await _realRelationalCommand.ExecuteScalarAsync(connection, parameterValues, cancellationToken);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            public RelationalDataReader ExecuteReader(
                IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues)
            {
                var errorNumber = PreExecution(connection);

                var result = _realRelationalCommand.ExecuteReader(connection, parameterValues);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            public async Task<RelationalDataReader> ExecuteReaderAsync(
                IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, CancellationToken cancellationToken = new CancellationToken())
            {
                var errorNumber = PreExecution(connection);

                var result = await _realRelationalCommand.ExecuteReaderAsync(connection, parameterValues, cancellationToken);
                if (errorNumber != null)
                {
                    connection.DbConnection.Close();
                    throw new PostgresException { SqlState = errorNumber };
                }
                return result;
            }

            private string PreExecution(IRelationalConnection connection)
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
