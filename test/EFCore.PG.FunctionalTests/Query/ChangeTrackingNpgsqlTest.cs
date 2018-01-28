using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ChangeTrackingNpgsqlTest : ChangeTrackingTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public ChangeTrackingNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        protected override NorthwindContext CreateNoTrackingContext()
            => new NorthwindRelationalContext(
                new DbContextOptionsBuilder(Fixture.CreateOptions())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);
    }
}
