using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullKeysNpgsqlTest : NullKeysTestBase<NullKeysNpgsqlTest.NullKeysNpgsqlFixture>
    {
        public NullKeysNpgsqlTest(NullKeysNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysNpgsqlFixture : NullKeysFixtureBase
        {
            protected override string StoreName { get; } = "StringsContext";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
