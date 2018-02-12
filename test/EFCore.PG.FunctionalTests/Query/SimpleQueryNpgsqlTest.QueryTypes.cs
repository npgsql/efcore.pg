using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryNpgsqlTest
    {
        [Fact(Skip="https://github.com/aspnet/EntityFrameworkCore/pull/10797, merged for 2.1-preview2")]
        public override void QueryType_select_where_navigation() {}

        [Fact(Skip="https://github.com/aspnet/EntityFrameworkCore/pull/10797, merged for 2.1-preview2")]
        public override void QueryType_select_where_navigation_multi_level() {}

        [Fact(Skip="https://github.com/aspnet/EntityFrameworkCore/pull/10797, merged for 2.1-preview2")]
        public override void QueryType_with_defining_query() {}

        [Fact(Skip="https://github.com/aspnet/EntityFrameworkCore/pull/10797, merged for 2.1-preview2")]
        public override void QueryType_with_included_nav() {}

        [Fact(Skip="https://github.com/aspnet/EntityFrameworkCore/pull/10797, merged for 2.1-preview2")]
        public override void QueryType_with_included_navs_multi_level() {}

        [Fact(Skip="https://github.com/aspnet/EntityFrameworkCore/pull/10797, merged for 2.1-preview2")]
        public override void QueryType_with_mixed_tracking() {}
    }
}
