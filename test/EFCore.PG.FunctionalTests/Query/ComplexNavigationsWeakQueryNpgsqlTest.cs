using System.Threading.Tasks;
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

        [ConditionalTheory(Skip = "#1142")]
        public override Task Accessing_optional_property_inside_result_operator_subquery(bool async)
            => base.Accessing_optional_property_inside_result_operator_subquery(async);
    }
}
