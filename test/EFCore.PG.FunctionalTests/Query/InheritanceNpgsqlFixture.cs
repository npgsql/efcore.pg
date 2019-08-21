using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class InheritanceNpgsqlFixture : InheritanceRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory =>  NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<AnimalQuery>().HasNoKey().ToQuery(
                () => context.Set<AnimalQuery>().FromSqlRaw(@"SELECT * FROM ""Animal"""));
        }
    }
}
