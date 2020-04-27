using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlTransaction : RelationalTransaction, IDbContextTransaction
    {
        readonly DbTransaction _dbTransaction;

        public NpgsqlTransaction(
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
            : base(connection, transaction, transactionId, logger, transactionOwned)
            => _dbTransaction = transaction;

        /// <inheritdoc />
        public virtual void Save(string savepointName)
        {
            using var command = Connection.DbConnection.CreateCommand();
            command.Transaction = _dbTransaction;
            command.CommandText = "SAVEPOINT " + savepointName;
            command.ExecuteNonQuery();
        }

        /// <inheritdoc />
        public virtual async Task SaveAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            using var command = Connection.DbConnection.CreateCommand();
            command.Transaction = _dbTransaction;
            command.CommandText = "SAVEPOINT " + savepointName;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual void Rollback(string savepointName)
        {
            using var command = Connection.DbConnection.CreateCommand();
            command.Transaction = _dbTransaction;
            command.CommandText = "ROLLBACK TO " + savepointName;
            command.ExecuteNonQuery();
        }

        /// <inheritdoc />
        public virtual async Task RollbackAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            using var command = Connection.DbConnection.CreateCommand();
            command.Transaction = _dbTransaction;
            command.CommandText = "ROLLBACK TO " + savepointName;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual void Release(string savepointName)
        {
            using var command = Connection.DbConnection.CreateCommand();
            command.Transaction = _dbTransaction;
            command.CommandText = "RELEASE " + savepointName;
            command.ExecuteNonQuery();
        }

        /// <inheritdoc />
        public virtual async Task ReleaseAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            using var command = Connection.DbConnection.CreateCommand();
            command.Transaction = _dbTransaction;
            command.CommandText = "RELEASE " + savepointName;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual bool AreSavepointsSupported => true;
    }
}
