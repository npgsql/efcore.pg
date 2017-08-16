using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryNavigationsNpgsqlTest : QueryNavigationsTestBase<NorthwindQueryNpgsqlFixture>
    {
        public QueryNavigationsNpgsqlTest(
            NorthwindQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact(Skip="https://github.com/aspnet/EntityFramework/issues/9039")]
        public override void Select_collection_navigation_simple() {}

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/9039")]
        public override void Select_collection_navigation_multi_part() {}

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
