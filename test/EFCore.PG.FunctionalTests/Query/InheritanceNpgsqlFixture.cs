using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class InheritanceNpgsqlFixture : InheritanceRelationalFixture
    {
        protected virtual string DatabaseName => "InheritanceNpgsqlTest";
        protected override string StoreName { get; } = "InheritanceNpgsqlTest";
        protected override ITestStoreFactory TestStoreFactory =>  NpgsqlTestStoreFactory.Instance;
    }
}
