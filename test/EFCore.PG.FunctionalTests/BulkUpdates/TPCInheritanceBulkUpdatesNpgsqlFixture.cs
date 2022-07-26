using Microsoft.EntityFrameworkCore.BulkUpdates;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPCInheritanceBulkUpdatesNpgsqlFixture : TPCInheritanceBulkUpdatesFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override bool UseGeneratedKeys
        => false;
}
