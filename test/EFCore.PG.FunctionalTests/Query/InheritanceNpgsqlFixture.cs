using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceNpgsqlFixture : InheritanceRelationalFixture
    {
        protected virtual string DatabaseName => "InheritanceNpgsqlTest";
        protected override string StoreName { get; } = "InheritanceNpgsqlTest";
        protected override ITestStoreFactory TestStoreFactory =>  NpgsqlTestStoreFactory.Instance;
    }
}
