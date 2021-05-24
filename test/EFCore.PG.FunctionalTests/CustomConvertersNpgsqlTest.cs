using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class CustomConvertersNpgsqlTest : CustomConvertersTestBase<CustomConvertersNpgsqlTest.CustomConvertersNpgsqlFixture>
    {
        public CustomConvertersNpgsqlTest(CustomConvertersNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        // Disabled: PostgreSQL is case-sensitive
        public override void Can_insert_and_read_back_with_case_insensitive_string_key() {}

        public override void Value_conversion_on_enum_collection_contains()
        {
            Assert.Contains(
                CoreStrings.TranslationFailed("").Substring(47),
                Assert.Throws<InvalidOperationException>(() => base.Value_conversion_on_enum_collection_contains()).Message);
        }

        public class CustomConvertersNpgsqlFixture : CustomConvertersFixtureBase
        {
            public override bool StrictEquality => true;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => true;

            public override bool SupportsLargeStringComparisons => true;

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            public override bool SupportsBinaryKeys => true;

            public override bool SupportsDecimalComparisons => true;

            public override DateTime DefaultDateTime => new();
        }
    }
}
