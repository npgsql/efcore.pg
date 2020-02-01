using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class NullSemanticsQueryNpgsqlTest : NullSemanticsQueryTestBase<NullSemanticsQueryNpgsqlFixture>
    {
        public NullSemanticsQueryNpgsqlTest(NullSemanticsQueryNpgsqlFixture fixture)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        [ConditionalTheory(Skip = "Null semantics for array ANY not yet implemented, #1142")]
        public override Task Contains_with_local_array_closure_false_with_null(bool async)
            => base.Contains_with_local_array_closure_false_with_null(async);

        [ConditionalTheory(Skip = "Null semantics for array ANY not yet implemented, #1142")]
        public override Task Contains_with_local_nullable_array_closure_negated(bool async)
            => base.Contains_with_local_nullable_array_closure_negated(async);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

        protected override NullSemanticsContext CreateContext(bool useRelationalNulls = false)
        {
            var options = new DbContextOptionsBuilder(Fixture.CreateOptions());
            if (useRelationalNulls)
            {
                new NpgsqlDbContextOptionsBuilder(options).UseRelationalNulls();
            }

            var context = new NullSemanticsContext(options.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }
    }
}
