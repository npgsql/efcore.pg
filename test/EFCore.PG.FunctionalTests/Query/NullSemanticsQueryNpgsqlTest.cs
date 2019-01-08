using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    public class NullSemanticsQueryNpgsqlTest : NullSemanticsQueryTestBase<NullSemanticsQueryNpgsqlFixture>
    {
        public NullSemanticsQueryNpgsqlTest(NullSemanticsQueryNpgsqlFixture fixture)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

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
