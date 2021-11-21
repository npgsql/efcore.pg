using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class ComplexNavigationsQueryNpgsqlTest : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQueryNpgsqlFixture>
{
    public ComplexNavigationsQueryNpgsqlTest(ComplexNavigationsQueryNpgsqlFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/26353")]
    public override Task Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(bool async)
        => base.Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(async);
}