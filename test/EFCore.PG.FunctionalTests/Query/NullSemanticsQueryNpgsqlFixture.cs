using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullSemanticsQueryNpgsqlFixture : NullSemanticsQueryRelationalFixture
    {
        protected override string StoreName { get; } = "NullSemanticsQueryTest";
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
    }
}
