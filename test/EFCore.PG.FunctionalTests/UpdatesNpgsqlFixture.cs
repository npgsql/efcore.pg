using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class UpdatesNpgsqlFixture : UpdatesRelationalFixture
    {
        protected override string StoreName { get; } = "PartialUpdateNpgsqlTest";
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
    }
}
