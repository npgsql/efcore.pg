using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ComplexNavigationsSharedTypeQueryNpgsqlTest : ComplexNavigationsSharedQueryTypeRelationalTestBase<
        ComplexNavigationsSharedTypeQueryNpgsqlTest.ComplexNavigationsSharedTypeQueryNpgsqlFixture>
    {
        public ComplexNavigationsSharedTypeQueryNpgsqlTest(
            ComplexNavigationsSharedTypeQueryNpgsqlFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task SelectMany_with_navigation_and_Distinct(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_navigation_and_Distinct(async))).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin, message);
        }

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/22532")]
        public override Task Distinct_skip_without_orderby(bool async)
            => base.Distinct_skip_without_orderby(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/22532")]
        public override Task Distinct_take_without_orderby(bool async)
            => base.Distinct_take_without_orderby(async);

        public class ComplexNavigationsSharedTypeQueryNpgsqlFixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => NpgsqlTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var optionsBuilder = base.AddOptions(builder);
                new NpgsqlDbContextOptionsBuilder(optionsBuilder).ReverseNullOrdering();
                return optionsBuilder;
            }
        }
    }
}
