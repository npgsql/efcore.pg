using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class ConcurrencyDetectorNpgsqlTest : ConcurrencyDetectorRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public ConcurrencyDetectorNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        protected override async Task ConcurrencyDetectorTest(Func<NorthwindContext, Task> test)
        {
            await base.ConcurrencyDetectorTest(test);

            Assert.Empty(Fixture.TestSqlLoggerFactory.SqlStatements);
        }
    }
}
