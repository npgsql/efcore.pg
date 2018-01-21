using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsTrackingNpgsqlTest : AsTrackingTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public AsTrackingNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
