using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ComplexNavigationsQueryNpgsqlTest
        : ComplexNavigationsQueryTestBase<ComplexNavigationsQueryNpgsqlFixture>
    {
        public ComplexNavigationsQueryNpgsqlTest(ComplexNavigationsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/12970")]
        public override void Null_check_in_anonymous_type_projection_should_not_be_removed() {}

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/12882")]
        public override void Null_check_in_Dto_projection_should_not_be_removed() {}
    }
}
