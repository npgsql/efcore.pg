using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryBugsTest : IClassFixture<NpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public QueryBugsTest(NpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            //Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected NpgsqlFixture Fixture { get; }

        [Fact]
        public async Task Bug278()
        {
            using (var testStore = NpgsqlTestStore.CreateScratch())
            using (var context = new Bug278Context(new DbContextOptionsBuilder()
                .UseNpgsql(testStore.Connection)
                .Options))
            {
                context.Database.EnsureCreated();
                context.Entities.Add(new Bug278Entity { ChannelCodes = new[] { 1, 1 } });
                context.SaveChanges();

                var actual = await context.Entities.Select(x => new
                {
                    Codes = x.ChannelCodes.Select(c => (ChannelCode)c)
                }).FirstOrDefaultAsync();

                Assert.Equal(new[] { ChannelCode.Code, ChannelCode.Code }, actual.Codes);
            }
        }

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
    }
}
