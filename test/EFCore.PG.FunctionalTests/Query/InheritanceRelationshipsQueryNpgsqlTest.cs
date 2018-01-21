namespace Microsoft.EntityFrameworkCore.Query
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
