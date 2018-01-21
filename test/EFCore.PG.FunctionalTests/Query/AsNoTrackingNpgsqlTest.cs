using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsNoTrackingNpgsqlTest : AsNoTrackingTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public AsNoTrackingNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
