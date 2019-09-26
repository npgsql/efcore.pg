using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public partial class SimpleQueryNpgsqlTest
    {
        [Theory(Skip = "SQL translation not implemented, too annoying")]
        public override Task Where_datetime_millisecond_component(bool isAsync) => null;

        [Theory(Skip = "Translation not implemented yet, #873")]
        public override Task Where_datetimeoffset_now_component(bool isAsync) => null;

        [Theory(Skip = "Translation not implemented yet, #873")]
        public override Task Where_datetimeoffset_utcnow_component(bool isAsync) => null;

        [Theory(Skip = "PostgreSQL only has log(x, base) over numeric, may be possible to cast back and forth though")]
        public override Task Where_math_log_new_base(bool isAsync) => null;

        [Theory(Skip = "Convert on DateTime not yet supported")]
        public override Task Convert_ToString(bool isAsync) => null;
    }
}
