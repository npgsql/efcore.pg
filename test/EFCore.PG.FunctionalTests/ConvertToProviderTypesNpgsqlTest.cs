using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        public class ConvertToProviderTypesNpgsqlFixture : ConvertToProviderTypesFixtureBase
        {
            public override bool StrictEquality => true;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => false;

            public override bool SupportsLargeStringComparisons => true;

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(
                    c => c.Log(RelationalEventId.QueryClientEvaluationWarning));

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            // TODO: Remove the following after https://github.com/aspnet/EntityFrameworkCore/pull/11587 is merged
            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<MaxLengthDataTypes>(
                    b =>
                    {
                        b.Property(e => e.ByteArray9000).HasConversion<string>().HasMaxLength(LongStringLength * 2);
                    });
            }
        }
    }
}
