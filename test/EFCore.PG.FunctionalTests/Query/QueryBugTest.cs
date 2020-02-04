using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
// ReSharper disable UnusedAutoPropertyAccessor.Local

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

        protected NpgsqlFixture Fixture { get; }

        #region Bug278

        [Fact(Skip = "Skipped for preview7, edge case")]
        public void Bug278()
        {
            using var _ = CreateDatabase278();
            using var context = new Bug278Context(_options);
            var actual = context.Entities.Select(x => new
            {
                Codes = x.ChannelCodes.Select(c => (ChannelCode)c)
            }).ToList()[0];

            Assert.Equal(new[] { ChannelCode.Code, ChannelCode.Code }, actual.Codes);
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

        public enum ChannelCode { Code = 1 }

        public class Bug278Entity
        {
            public int Id { get; set; }
            public int[] ChannelCodes { get; set; }
        }

        class Bug278Context : DbContext
        {
            public Bug278Context(DbContextOptions options) : base(options) {}
            public DbSet<Bug278Entity> Entities { get; set; }
        }

        #endregion Bug278

        #region Bug920

        [Fact]
        public void Bug920()
        {
            using var _ = CreateDatabase920();
            using var context = new Bug920Context(_options);
            context.Entities.Add(new Bug920Entity { Enum = Bug920Enum.Two });
            context.SaveChanges();
        }

        NpgsqlTestStore CreateDatabase920()
            => CreateTestStore(() => new Bug920Context(_options), context => ClearLog());

        public enum Bug920Enum { One, Two }

        public class Bug920Entity
        {
            public int Id { get; set; }
            [Column(TypeName="char(3)")]
            public Bug920Enum Enum { get; set; }
        }

        class Bug920Context : DbContext
        {
            public Bug920Context(DbContextOptions options) : base(options) {}
            public DbSet<Bug920Entity> Entities { get; set; }
        }

        #endregion Bug920

        DbContextOptions _options;

        NpgsqlTestStore CreateTestStore<TContext>(
            Func<TContext> contextCreator,
            Action<TContext> contextInitializer)
            where TContext : DbContext, IDisposable
        {
            var testStore = NpgsqlTestStore.CreateInitialized("QueryBugsTest");

            _options = Fixture.CreateOptions(testStore);

            using var context = contextCreator();
            context.Database.EnsureCreatedResiliently();
            contextInitializer?.Invoke(context);
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
