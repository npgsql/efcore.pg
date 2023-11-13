using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Northwind;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindQueryNpgsqlFixture<TModelCustomizer> : NorthwindQueryRelationalFixture<TModelCustomizer>
    where TModelCustomizer : IModelCustomizer, new()
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlNorthwindTestStoreFactory.Instance;

    protected override Type ContextType
        => typeof(NorthwindNpgsqlContext);

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var optionsBuilder = base.AddOptions(builder);
        new NpgsqlDbContextOptionsBuilder(optionsBuilder).SetPostgresVersion(TestEnvironment.PostgresVersion);
        return optionsBuilder;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // Note that we map price properties to numeric(12,2) columns, not to money as in SqlServer, since in
        // PG, money is discouraged/obsolete and various tests fail with it.

        modelBuilder.Entity<Order>(
            b =>
            {
                b.Property(o => o.EmployeeID).HasColumnType("int");
                b.Property(o => o.OrderDate).HasColumnType("timestamp without time zone");
            });

        modelBuilder.Entity<Employee>(
            b =>
            {
                b.Property(c => c.EmployeeID).HasColumnType("int");
                b.Property(c => c.ReportsTo).HasColumnType("int");
            });

        modelBuilder.Entity<Order>()
            .Property(o => o.EmployeeID)
            .HasColumnType("int");

        modelBuilder.Entity<Product>()
            .Property(p => p.UnitsInStock)
            .HasColumnType("smallint");
    }
}
