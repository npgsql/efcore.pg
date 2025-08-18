﻿namespace Microsoft.EntityFrameworkCore;

public class PropertyValuesNpgsqlTest(PropertyValuesNpgsqlTest.PropertyValuesNpgsqlFixture fixture)
    : PropertyValuesRelationalTestBase<PropertyValuesNpgsqlTest.PropertyValuesNpgsqlFixture>(fixture)
{
    public class PropertyValuesNpgsqlFixture : PropertyValuesRelationalFixture
    {
        protected override string StoreName { get; } = "PropertyValues";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
            // supported.
            modelBuilder.Entity<PastEmployee>().Property(e => e.TerminationDate).HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Building>()
                .Property(b => b.Value).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CurrentEmployee>()
                .Property(ce => ce.LeaveBalance).HasColumnType("decimal(18,2)");
        }
    }
}
