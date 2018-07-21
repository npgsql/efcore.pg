using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class CompositeKeyEndToEndNpgsqlTest : CompositeKeyEndToEndTestBase<CompositeKeyEndToEndNpgsqlTest.CompositeKeyEndToEndNpgsqlFixture>
    {
        public CompositeKeyEndToEndNpgsqlTest(CompositeKeyEndToEndNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class CompositeKeyEndToEndNpgsqlFixture : CompositeKeyEndToEndFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
