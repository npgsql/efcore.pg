using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class TPTRelationshipsQueryNpgsqlTest
        : TPTRelationshipsQueryTestBase<TPTRelationshipsQueryNpgsqlTest.TPTRelationshipsQueryNpgsqlFixture>
    {
        public TPTRelationshipsQueryNpgsqlTest(
            TPTRelationshipsQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
            => fixture.TestSqlLoggerFactory.Clear();

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_with_inheritance_on_derived1_split(bool async)
            => base.Include_collection_with_inheritance_on_derived1_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_with_inheritance_on_derived2_split(bool async)
            => base.Include_collection_with_inheritance_on_derived2_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_with_inheritance_on_derived3_split(bool async)
            => base.Include_collection_with_inheritance_on_derived3_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_with_inheritance_on_derived_reverse_split(bool async)
            => base.Include_collection_with_inheritance_on_derived_reverse_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_with_inheritance_reverse_split(bool async)
            => base.Include_collection_with_inheritance_reverse_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_with_inheritance_split(bool async)
            => base.Include_collection_with_inheritance_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_with_inheritance_with_filter_reverse_split(bool async)
            => base.Include_collection_with_inheritance_with_filter_reverse_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_with_inheritance_with_filter_split(bool async)
            => base.Include_collection_with_inheritance_with_filter_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_without_inheritance_reverse_split(bool async)
            => base.Include_collection_without_inheritance_reverse_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_without_inheritance_split(bool async)
            => base.Include_collection_without_inheritance_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_without_inheritance_with_filter_reverse_split(bool async)
            => base.Include_collection_without_inheritance_with_filter_reverse_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Include_collection_without_inheritance_with_filter_split(bool async)
            => base.Include_collection_without_inheritance_with_filter_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Nested_include_with_inheritance_collection_collection_split(bool async)
            => base.Nested_include_with_inheritance_collection_collection_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Nested_include_with_inheritance_collection_reference_split(bool async)
            => base.Nested_include_with_inheritance_collection_reference_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Nested_include_with_inheritance_reference_collection_on_base_split(bool async)
            => base.Nested_include_with_inheritance_reference_collection_on_base_split(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/22293")]
        public override Task Nested_include_with_inheritance_reference_collection_split(bool async)
            => base.Nested_include_with_inheritance_reference_collection_split(async);

        public class TPTRelationshipsQueryNpgsqlFixture : TPTRelationshipsQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
