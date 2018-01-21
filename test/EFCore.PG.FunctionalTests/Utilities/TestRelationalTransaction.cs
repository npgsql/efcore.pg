using System;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class TestRelationalTransactionFactory : IRelationalTransactionFactory
    {
        public RelationalTransaction Create(
            IRelationalConnection connection,
            DbTransaction transaction,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
            => new TestRelationalTransaction(connection, transaction, logger, transactionOwned);
    }

    public class TestRelationalTransaction : RelationalTransaction
    {
        private readonly TestNpgsqlConnection _testConnection;

        public TestRelationalTransaction(
            IRelationalConnection connection,
            DbTransaction transaction,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
            : base(connection, transaction, logger, transactionOwned)
        {
            _testConnection = (TestNpgsqlConnection)connection;
        }

        public override void Commit()
        {
            if (_testConnection.CommitFailures.Count > 0)
            {
                var fail = _testConnection.CommitFailures.Dequeue();
                if (fail.HasValue)
                {
                    if (fail.Value)
                    {
                        this.GetDbTransaction().Rollback();
                    }
                    else
                    {
                        this.GetDbTransaction().Commit();
                    }
                    _testConnection.DbConnection.Close();
                    throw new PostgresException { SqlState = _testConnection.ErrorCode };
                }
            }

            base.Commit();
        }
    }
}
