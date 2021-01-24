using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class FromSqlQueryNpgsqlTest : FromSqlQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public FromSqlQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFramework/issues/{6563,20364}")]
        public override Task Bad_data_error_handling_invalid_cast(bool async)
            => base.Bad_data_error_handling_invalid_cast(async);

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFramework/issues/{6563,20364}")]
        public override Task Bad_data_error_handling_invalid_cast_projection(bool async)
            => base.Bad_data_error_handling_invalid_cast_projection(async);

        protected override DbParameter CreateDbParameter(string name, object value)
            => new NpgsqlParameter
            {
                ParameterName = name,
                Value = value
            };
    }
}
