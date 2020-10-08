using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ComplexNavigationsWeakQueryNpgsqlTest : ComplexNavigationsWeakQueryRelationalTestBase<ComplexNavigationsWeakQueryNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public ComplexNavigationsWeakQueryNpgsqlTest(ComplexNavigationsWeakQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task SelectMany_with_navigation_and_Distinct(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_navigation_and_Distinct(async))).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin, message);
        }

        [ConditionalTheory(Skip = "#1142")]
        public override Task Accessing_optional_property_inside_result_operator_subquery(bool async)
            => base.Accessing_optional_property_inside_result_operator_subquery(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/22532")]
        public override Task Distinct_skip_without_orderby(bool async)
            => base.Distinct_skip_without_orderby(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/22532")]
        public override Task Distinct_take_without_orderby(bool async)
            => base.Distinct_take_without_orderby(async);
    }
}
