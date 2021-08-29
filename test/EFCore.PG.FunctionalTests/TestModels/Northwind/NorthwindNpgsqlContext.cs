using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Northwind
{
    public class NorthwindNpgsqlContext : NorthwindRelationalContext
    {
        public NorthwindNpgsqlContext(DbContextOptions options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("uuid-ossp");
            modelBuilder.HasPostgresExtension("unaccent");

            modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnType("timestamp without time zone");

            modelBuilder.Entity<CustomerQuery>().ToSqlQuery(@"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"" FROM ""Customers"" AS ""c""");
        }
    }
}
