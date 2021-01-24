using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ComplexNavigationsQueryNpgsqlTest
        : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQueryNpgsqlTest.ComplexNavigationsQueryNpgsqlFixture>
    {
        public ComplexNavigationsQueryNpgsqlTest(ComplexNavigationsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [ConditionalTheory(Skip = "#1142")]
        public override Task Accessing_optional_property_inside_result_operator_subquery(bool async)
            => base.Accessing_optional_property_inside_result_operator_subquery(async);

        public class ComplexNavigationsQueryNpgsqlFixture
            : ComplexNavigationsQueryRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var optionsBuilder = base.AddOptions(builder);
                new NpgsqlDbContextOptionsBuilder(optionsBuilder).ReverseNullOrdering();
                return optionsBuilder;
            }
        }
    }
}
