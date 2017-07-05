namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsNoTrackingNpgsqlTest : AsNoTrackingTestBase<NorthwindQueryNpgsqlFixture>
    {
        public AsNoTrackingNpgsqlTest(NorthwindQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
