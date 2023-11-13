using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Northwind;

public class NorthwindNpgsqlContext : NorthwindRelationalContext
{
    public NorthwindNpgsqlContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Note that we map price properties to numeric(12,2) columns, not to money as in SqlServer, since in
        // PG, money is discouraged/obsolete and various tests fail with it.

        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasPostgresExtension("btree_gist"); // For the <-> (distance) operator

        modelBuilder.Entity<Employee>(
            b =>
            {
                b.Property(c => c.EmployeeID).HasColumnType("int");
                b.Property(c => c.ReportsTo).HasColumnType("int");
            });

        modelBuilder.Entity<Product>(
            b =>
            {
                b.Property(p => p.UnitsInStock).HasColumnType("smallint");
            });

        modelBuilder.Entity<Order>(
            b =>
            {
                b.Property(o => o.EmployeeID).HasColumnType("int");
                b.Property(o => o.OrderDate).HasColumnType("timestamp without time zone");
            });

        modelBuilder.Entity<OrderDetail>(
            b =>
            {
                b.Property(p => p.Quantity).HasColumnType("smallint");
                b.Property(p => p.Discount).HasColumnType("real");
            });

        modelBuilder.Entity<CustomerQuery>().ToSqlQuery(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region" FROM "Customers" AS "c"
""");
    }
}
