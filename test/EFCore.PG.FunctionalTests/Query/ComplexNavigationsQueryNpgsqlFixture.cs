using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQueryNpgsqlFixture
       : ComplexNavigationsQueryRelationalFixtureBase
    {
        protected override string StoreName { get; } = "ComplexNavigations";
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
    }
}
