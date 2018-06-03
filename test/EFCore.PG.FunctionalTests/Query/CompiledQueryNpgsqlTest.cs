using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class CompiledQueryNpgsqlTest : CompiledQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public CompiledQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        [ConditionalFact(Skip = "Throws: Can't write CLR type System.String[] with handler type TextHandler")]
        public override void Query_with_array_parameter()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context, string[] args)
                    => context.Customers.Where(c => c.CustomerID == args[0]));

            using (var context = CreateContext())
            {
                var args = new[] { "ALFKI" };

                // BUG: this passes
                var _ = context.Customers.Where(c => c.CustomerID == args[0]).ToList();

                // BUG: this throws
                // System.InvalidCastException : Can't write CLR type System.String[] with handler type TextHandler
                var result = query(context, args).First().CustomerID;

                Assert.Equal("ALFKI", result);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("ANATR", query(context, new[] { "ANATR" }).First().CustomerID);
            }
        }

        [ConditionalFact(Skip = "Throws: Can't write CLR type System.String[] with handler type TextHandler")]
        public override async Task Query_with_array_parameter_async()
        {
            await base.Query_with_array_parameter_async();
        }
    }
}
