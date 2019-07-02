using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class QueryBugsTest : IClassFixture<NpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public QueryBugsTest(NpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected NpgsqlFixture Fixture { get; }

        #region Bug278

        [Fact(Skip = "Skipped for preview7, edge case")]
        public void Bug278()
        {
            using (CreateDatabase278())
            using (var context = new Bug278Context(_options))
            {
                var actual = context.Entities.Select(x => new
                {
                    Codes = x.ChannelCodes.Select(c => (ChannelCode)c)
                }).ToList()[0];

                Assert.Equal(new[] { ChannelCode.Code, ChannelCode.Code }, actual.Codes);
            }
        }

        NpgsqlTestStore CreateDatabase278()
            => CreateTestStore(
                () => new Bug278Context(_options),
                context =>
                {
                    context.Entities.Add(new Bug278Entity { ChannelCodes = new[] { 1, 1 } });
                    context.SaveChanges();
                    ClearLog();
                });

        // ReSharper disable once MemberCanBePrivate.Global
        public enum ChannelCode { Code = 1 }

        // ReSharper disable once MemberCanBePrivate.Global
        public class Bug278Entity
        {
            // ReSharper disable once UnusedMember.Global
            public int Id { get; set; }
            public int[] ChannelCodes { get; set; }
        }

        class Bug278Context : DbContext
        {
            public Bug278Context(DbContextOptions options) : base(options) {}
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public DbSet<Bug278Entity> Entities { get; set; }
        }

        #endregion Bug278

        DbContextOptions _options;

        NpgsqlTestStore CreateTestStore<TContext>(
            Func<TContext> contextCreator,
            Action<TContext> contextInitializer)
            where TContext : DbContext, IDisposable
        {
            var testStore = NpgsqlTestStore.CreateInitialized("QueryBugsTest");

            _options = Fixture.CreateOptions(testStore);

            using (var context = contextCreator())
            {
                context.Database.EnsureCreatedResiliently();
                contextInitializer?.Invoke(context);
            }

            return testStore;
        }

        protected void ClearLog()
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        void AssertSql(params string[] expected)
        {
            Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
        }
    }
}
