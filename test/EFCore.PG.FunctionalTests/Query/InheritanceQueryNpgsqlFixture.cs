using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class InheritanceQueryNpgsqlFixture : InheritanceQueryRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory =>  NpgsqlTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<AnimalQuery>().HasNoKey().ToSqlQuery(@"SELECT * FROM ""Animals""");
    }
}