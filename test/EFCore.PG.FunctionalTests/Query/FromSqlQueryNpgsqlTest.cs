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

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/15423")]
        public override Task FromSqlRaw_does_not_parameterize_interpolated_string(bool async)
            => base.FromSqlRaw_does_not_parameterize_interpolated_string(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override Task Bad_data_error_handling_invalid_cast_key(bool async)
            => base.Bad_data_error_handling_invalid_cast_key(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override Task Bad_data_error_handling_invalid_cast_no_tracking(bool async)
            => base.Bad_data_error_handling_invalid_cast_no_tracking(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override Task Bad_data_error_handling_null(bool async)
            => base.Bad_data_error_handling_null(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override Task Bad_data_error_handling_null_no_tracking(bool async)
            => base.Bad_data_error_handling_null_no_tracking(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override Task Bad_data_error_handling_null_projection(bool async)
            => base.Bad_data_error_handling_null_projection(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override Task FromSqlRaw_queryable_simple_columns_out_of_order_and_not_enough_columns_throws(bool async)
            => base.FromSqlRaw_queryable_simple_columns_out_of_order_and_not_enough_columns_throws(async);

        protected override DbParameter CreateDbParameter(string name, object value)
            => new NpgsqlParameter
            {
                ParameterName = name,
                Value = value
            };
    }
}
