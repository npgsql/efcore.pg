using System.Data.Common;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

public class TestRelationalTransactionFactory : IRelationalTransactionFactory
{
    public TestRelationalTransactionFactory(RelationalTransactionFactoryDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    protected virtual RelationalTransactionFactoryDependencies Dependencies { get; }

    public RelationalTransaction Create(
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
        bool transactionOwned)
        => new TestRelationalTransaction(connection, transaction, logger, transactionOwned, Dependencies.SqlGenerationHelper);
}

public class TestRelationalTransaction : RelationalTransaction
{
    private readonly TestNpgsqlConnection _testConnection;

    public TestRelationalTransaction(
        IRelationalConnection connection,
        DbTransaction transaction,
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
        bool transactionOwned,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(connection, transaction, new Guid(), logger, transactionOwned, sqlGenerationHelper)
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
                throw new PostgresException("", "", "", _testConnection.ErrorCode);
            }
        }

        base.Commit();
    }

    public override async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_testConnection.CommitFailures.Count > 0)
        {
            var fail = _testConnection.CommitFailures.Dequeue();
            if (fail.HasValue)
            {
                if (fail.Value)
                {
                    await this.GetDbTransaction().RollbackAsync(cancellationToken);
                }
                else
                {
                    await this.GetDbTransaction().CommitAsync(cancellationToken);
                }

                await _testConnection.DbConnection.CloseAsync();
                throw new PostgresException("", "", "", _testConnection.ErrorCode);
            }
        }

        await base.CommitAsync(cancellationToken);
    }

    public override bool SupportsSavepoints
        => true;
}
