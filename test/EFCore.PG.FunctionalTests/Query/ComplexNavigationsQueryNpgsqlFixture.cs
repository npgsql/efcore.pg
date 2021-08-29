using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ComplexNavigationsQueryNpgsqlFixture : ComplexNavigationsQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
            // supported.
            modelBuilder.Entity<Level1>().Property(l => l.Date).HasColumnType("timestamp without time zone");
            modelBuilder.Entity<Level2>().Property(l => l.Date).HasColumnType("timestamp without time zone");
        }
        // public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        // {
        //     var optionsBuilder = base.AddOptions(builder);
        //     new NpgsqlDbContextOptionsBuilder(optionsBuilder).ReverseNullOrdering();
        //     return optionsBuilder;
        // }
    }
}
