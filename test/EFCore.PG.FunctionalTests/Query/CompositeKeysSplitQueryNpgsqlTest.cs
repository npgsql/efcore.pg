using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class CompositeKeysSplitQueryNpgsqlTest : CompositeKeysSplitQueryRelationalTestBase<CompositeKeysQueryNpgsqlFixture>
    {
        public CompositeKeysSplitQueryNpgsqlTest(
            CompositeKeysQueryNpgsqlFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
