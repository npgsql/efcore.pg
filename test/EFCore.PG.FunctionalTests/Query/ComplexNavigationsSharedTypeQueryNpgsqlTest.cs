using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ComplexNavigationsSharedTypeQueryNpgsqlTest
        : ComplexNavigationsSharedTypeQueryRelationalTestBase<ComplexNavigationsSharedTypeQueryNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public ComplexNavigationsSharedTypeQueryNpgsqlTest(
            ComplexNavigationsSharedTypeQueryNpgsqlFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/26353")]
        public override Task Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(bool async)
            => base.Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/22532")]
        public override Task Distinct_skip_without_orderby(bool async)
            => base.Distinct_skip_without_orderby(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/22532")]
        public override Task Distinct_take_without_orderby(bool async)
            => base.Distinct_take_without_orderby(async);
    }
}
