using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    // ReSharper disable once UnusedMember.Global
    public class LazyLoadProxyNpgsqlTest : LazyLoadProxyTestBase<LazyLoadProxyNpgsqlTest.LoadNpgsqlFixture>
    {
        public LazyLoadProxyNpgsqlTest(LoadNpgsqlFixture fixture)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        [ConditionalFact]  // Requires MARS
        public override void Top_level_projection_track_entities_before_passing_to_client_method() {}

        [ConditionalTheory(Skip = "Possibly requires MARS, investigate")]
        public override void Lazy_load_one_to_one_reference_with_recursive_property(EntityState state)
            => base.Lazy_load_one_to_one_reference_with_recursive_property(state);

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        protected override void RecordLog() => Sql = Fixture.TestSqlLoggerFactory.Sql;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private string Sql { get; set; }

        public class LoadNpgsqlFixture : LoadFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
                // supported.
                modelBuilder.Entity<Quest>().Property(q => q.Birthday).HasColumnType("timestamp without time zone");
                modelBuilder.Entity<Parson>().Property(q => q.Birthday).HasColumnType("timestamp without time zone");
            }
        }
    }
}
