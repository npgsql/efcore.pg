namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class ManyToManyTrackingNpgsqlTest
        : ManyToManyTrackingNpgsqlTestBase<ManyToManyTrackingNpgsqlTest.ManyToManyTrackingNpgsqlFixture>
    {
        public ManyToManyTrackingNpgsqlTest(ManyToManyTrackingNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class ManyToManyTrackingNpgsqlFixture : ManyToManyTrackingNpgsqlFixtureBase
        {
        }
    }
}
