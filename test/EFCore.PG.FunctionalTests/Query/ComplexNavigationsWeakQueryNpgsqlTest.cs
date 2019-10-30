using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ComplexNavigationsWeakQueryNpgsqlTest : ComplexNavigationsWeakQueryTestBase<ComplexNavigationsWeakQueryNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public ComplexNavigationsWeakQueryNpgsqlTest(ComplexNavigationsWeakQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/18679")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Include13(bool isAsync) => base.Include13(isAsync);

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/18679")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Include14(bool isAsync) => base.Include13(isAsync);
    }
}
