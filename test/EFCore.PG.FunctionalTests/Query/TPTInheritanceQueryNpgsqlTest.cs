using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPTInheritanceQueryNpgsqlTest : TPTInheritanceQueryTestBase<TPTInheritanceQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public TPTInheritanceQueryNpgsqlTest(TPTInheritanceQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}