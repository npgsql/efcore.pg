using Microsoft.EntityFrameworkCore.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class InheritanceRelationshipsQueryNpgsqlTest : InheritanceRelationshipsQueryTestBase<InheritanceRelationshipsQueryNpgsqlFixture>
    {
        public InheritanceRelationshipsQueryNpgsqlTest(InheritanceRelationshipsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override void ClearLog() {}
    }
}
