using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class StringQueryTest : IClassFixture<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        #region Indexer

        [Fact]
        public void String_Index_text_with_constant_char_as_int()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.Customers.Where(e => e.Address[0] == 'T').ToList();
                AssertContainsInSql("WHERE ascii(substr(e.\"Address\", 1, 1)) = 84");
            }
        }

        [Fact]
        public void String_Index_text_with_constant_string()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.Customers.Where(e => e.Address[0].ToString() == "T").ToList();
                AssertContainsInSql("WHERE CAST(substr(e.\"Address\", 1, 1) AS text) = 'T'");
            }
        }

        [Fact]
        public void String_Index_text_with_non_constant_char_as_int()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var _ = ctx.Customers.Where(e => e.Address[x] == 'T').ToList();
                AssertContainsInSql("WHERE ascii(substr(e.\"Address\", @__x_0 + 1, 1)) = 84");
            }
        }

        [Fact]
        public void String_Index_text_with_non_constant_string()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var _ = ctx.Customers.Where(e => e.Address[x].ToString() == "T").ToList();
                AssertContainsInSql("WHERE CAST(substr(e.\"Address\", @__x_0 + 1, 1) AS text) = 'T'");
            }
        }

        #endregion

        #region Support

        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> Fixture { get; }

        public StringQueryTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        #endregion
    }
}
