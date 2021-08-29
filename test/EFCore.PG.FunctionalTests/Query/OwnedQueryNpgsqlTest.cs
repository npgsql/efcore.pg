using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class OwnedQueryNpgsqlTest : OwnedQueryRelationalTestBase<OwnedQueryNpgsqlTest.OwnedQueryNpgsqlFixture>
    {
        public OwnedQueryNpgsqlTest(OwnedQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class OwnedQueryNpgsqlFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
                // supported.
                modelBuilder.Entity<OwnedPerson>(
                    eb => eb.OwnsMany(
                        p => p.Orders, ob => ob.IndexerProperty<DateTime>("OrderDate").HasColumnType("timestamp without time zone")));
            }
        }
    }
}
