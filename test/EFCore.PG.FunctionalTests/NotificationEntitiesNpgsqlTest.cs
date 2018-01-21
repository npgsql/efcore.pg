using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class NotificationEntitiesNpgsqlTest
        : NotificationEntitiesTestBase<NotificationEntitiesNpgsqlTest.NotificationEntitiesNpgsqlFixture>
    {
        public NotificationEntitiesNpgsqlTest(NotificationEntitiesNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesNpgsqlFixture : NotificationEntitiesFixtureBase
        {
            protected override string StoreName { get; } = "NotificationEntities";

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
