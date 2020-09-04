using System.Data.Common;
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

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/{6563,20364}")]
        public override void Bad_data_error_handling_invalid_cast() {}
        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/{6563,20364}")]
        public override void Bad_data_error_handling_invalid_cast_projection() {}

        [Fact(Skip="https://github.com/aspnet/EntityFrameworkCore/pull/15423")]
        public override void FromSqlRaw_does_not_parameterize_interpolated_string() {}

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override void Bad_data_error_handling_invalid_cast_key()
            => base.Bad_data_error_handling_invalid_cast_key();

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override void Bad_data_error_handling_invalid_cast_no_tracking()
            => base.Bad_data_error_handling_invalid_cast_no_tracking();

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override void Bad_data_error_handling_null()
            => base.Bad_data_error_handling_null();

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override void Bad_data_error_handling_null_no_tracking()
            => base.Bad_data_error_handling_null_no_tracking();

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override void Bad_data_error_handling_null_projection()
            => base.Bad_data_error_handling_null_projection();

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/20364")]
        public override void FromSqlRaw_queryable_simple_columns_out_of_order_and_not_enough_columns_throws()
            => base.FromSqlRaw_queryable_simple_columns_out_of_order_and_not_enough_columns_throws();

        protected override DbParameter CreateDbParameter(string name, object value)
            => new NpgsqlParameter
            {
                ParameterName = name,
                Value = value
            };
    }
}
