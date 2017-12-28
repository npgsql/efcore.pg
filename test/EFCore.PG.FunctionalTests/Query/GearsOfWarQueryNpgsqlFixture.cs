using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQueryNpgsqlFixture : GearsOfWarQueryRelationalFixture
    {
        protected override string StoreName { get; } = "GearsOfWarQueryTest";
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.HasPostgresExtension("uuid-ossp");
        }
    }
}
