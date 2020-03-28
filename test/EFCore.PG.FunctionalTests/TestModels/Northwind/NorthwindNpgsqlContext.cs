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

#pragma warning disable CS0618 // Type or member is obsolete
            modelBuilder.Entity<CustomerQuery>().HasNoKey().ToQuery(
                () => CustomerQueries.FromSqlInterpolated($@"SELECT ""c"".""CustomerID"" || {_empty} as ""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"" FROM ""Customers"" AS ""c"""
                ));
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
