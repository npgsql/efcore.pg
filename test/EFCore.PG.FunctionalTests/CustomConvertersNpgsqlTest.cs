namespace Microsoft.EntityFrameworkCore;

public class CustomConvertersNpgsqlTest(CustomConvertersNpgsqlTest.CustomConvertersNpgsqlFixture fixture)
    : CustomConvertersTestBase<CustomConvertersNpgsqlTest.CustomConvertersNpgsqlFixture>(fixture)
{
    // Disabled: PostgreSQL is case-sensitive
    public override Task Can_insert_and_read_back_with_case_insensitive_string_key()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "DateTimeOffset with non-zero offset, https://github.com/dotnet/efcore/issues/26068")]
    public override Task Can_insert_and_read_back_object_backed_data_types()
        => Task.CompletedTask;

    public override void Value_conversion_on_enum_collection_contains()
        => Assert.Contains(
            CoreStrings.TranslationFailed("").Substring(47),
            Assert.Throws<InvalidOperationException>(() => base.Value_conversion_on_enum_collection_contains()).Message);

    public class CustomConvertersNpgsqlFixture : CustomConvertersFixtureBase
    {
        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => false;

        public override bool SupportsUnicodeToAnsiConversion
            => true;

        public override bool SupportsLargeStringComparisons
            => true;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public override bool SupportsBinaryKeys
            => true;

        public override bool SupportsDecimalComparisons
            => true;

        public override DateTime DefaultDateTime
            => new();

        public override bool PreservesDateTimeKind
            => false;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
            // supported.
            modelBuilder.Entity<ObjectBackedDataTypes>().Property(b => b.DateTime)
                .HasColumnType("timestamp without time zone");

            // We don't support DateTimeOffset with non-zero offset, so we need to override the seeding data
            var objectBackedDataTypes = modelBuilder.Entity<ObjectBackedDataTypes>().Metadata.GetSeedData().Single();
            objectBackedDataTypes[nameof(ObjectBackedDataTypes.DateTimeOffset)]
                = new DateTimeOffset(new DateTime(), TimeSpan.Zero);
        }
    }
}
