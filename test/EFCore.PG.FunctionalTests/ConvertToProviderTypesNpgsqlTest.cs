using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class ConvertToProviderTypesNpgsqlTest : ConvertToProviderTypesTestBase<ConvertToProviderTypesNpgsqlTest.ConvertToProviderTypesNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public ConvertToProviderTypesNpgsqlTest(ConvertToProviderTypesNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        [Fact]
        public override void Can_insert_and_read_with_max_length_set()
        {
            const string shortString = "Sky";
            var shortBinary = new byte[] { 8, 8, 7, 8, 7 };

            var longString = new string('X', Fixture.LongStringLength);
            var longBinary = new byte[Fixture.LongStringLength];
            for (var i = 0; i < longBinary.Length; i++)
            {
                longBinary[i] = (byte)i;
            }

            using (var context = CreateContext())
            {
                context.Set<MaxLengthDataTypes>().Add(
                    new MaxLengthDataTypes
                    {
                        Id = 79,
                        String3 = shortString,
                        ByteArray5 = shortBinary,
                        String9000 = longString,
                        ByteArray9000 = longBinary
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<MaxLengthDataTypes>().Where(e => e.Id == 79).ToList().Single();

                Assert.Equal(shortString, dt.String3);
                Assert.Equal(shortBinary, dt.ByteArray5);
                Assert.Equal(longString, dt.String9000);
                Assert.Equal(longBinary, dt.ByteArray9000);
            }
        }

        [ConditionalFact(Skip = "DateTimeOffset with non-zero offset, https://github.com/dotnet/efcore/issues/26068")]
        public override void Can_insert_and_read_back_non_nullable_backed_data_types() {}

        [ConditionalFact(Skip = "DateTimeOffset with non-zero offset, https://github.com/dotnet/efcore/issues/26068")]
        public override void Can_insert_and_read_back_nullable_backed_data_types() {}

        [ConditionalFact(Skip = "DateTimeOffset with non-zero offset, https://github.com/dotnet/efcore/issues/26068")]
        public override void Can_insert_and_read_back_object_backed_data_types() {}

        public class ConvertToProviderTypesNpgsqlFixture : ConvertToProviderTypesFixtureBase
        {
            public override bool StrictEquality => true;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => false;

            public override bool SupportsLargeStringComparisons => true;

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            public override bool SupportsBinaryKeys => true;

            public override bool SupportsDecimalComparisons => true;

            public override DateTime DefaultDateTime => new();

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
                // supported.
                modelBuilder.Entity<ObjectBackedDataTypes>().Property(b => b.DateTime)
                    .HasColumnType("timestamp without time zone");
                modelBuilder.Entity<NullableBackedDataTypes>().Property(b => b.DateTime)
                    .HasColumnType("timestamp without time zone");
                modelBuilder.Entity<NonNullableBackedDataTypes>().Property(b => b.DateTime)
                    .HasColumnType("timestamp without time zone");

                // We don't support DateTimeOffset with non-zero offset, so we need to override the seeding data
                var objectBackedDataTypes = modelBuilder.Entity<ObjectBackedDataTypes>().Metadata.GetSeedData().Single();
                objectBackedDataTypes[nameof(ObjectBackedDataTypes.DateTimeOffset)]
                    = new DateTimeOffset(new DateTime(), TimeSpan.Zero);

                var nullableBackedDataTypes = modelBuilder.Entity<NullableBackedDataTypes>().Metadata.GetSeedData().Single();
                nullableBackedDataTypes[nameof(NullableBackedDataTypes.DateTimeOffset)]
                    = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.Zero);

                var nonNullableBackedDataTypes = modelBuilder.Entity<NonNullableBackedDataTypes>().Metadata.GetSeedData().Single();
                nonNullableBackedDataTypes[nameof(NonNullableBackedDataTypes.DateTimeOffset)]
                    = new DateTimeOffset(new DateTime(), TimeSpan.Zero);
            }
        }
    }
}
