using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class PGroongaTest : IClassFixture<PGroongaTest.PGroongaFixture>
    {
        public PGroongaTest(PGroongaFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        PGroongaFixture Fixture { get; }

        #region Operators pgroonga_text_full_text_search_ops_v2

        [Fact]
        public void Operator_Match_V2()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Single(r => EF.Functions.Match(r.Content, "engine"));
                Assert.Equal(2, row.Id);
                Assert.Contains(@"WHERE (r.""Content"" &@ 'engine') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_Query_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => EF.Functions.Query(r.Content, "PGroonga OR PostgreSQL"));
                Assert.Equal(2, rows);
                Assert.Contains(@"WHERE (r.""Content"" &@~ 'PGroonga OR PostgreSQL') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_SimilarSearch_V2()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Single(r => EF.Functions.SimilarSearch(r.Content, "Mroonga is a MySQL extension taht uses Groonga"));
                Assert.Equal(3, row.Id);
                Assert.Contains(@"WHERE (r.""Content"" &@* 'Mroonga is a MySQL extension taht uses Groonga') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_ScriptQuery_V2()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Single(r => EF.Functions.ScriptQuery(r.Content, "Id >= 2 && (Content @ 'engine' || Content @ 'rdbms')"));
                Assert.Equal(2, row.Id);
                Assert.Contains(@"WHERE (r.""Content"" &` 'Id >= 2 && (Content @ ''engine'' || Content @ ''rdbms'')') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_MatchIn_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => EF.Functions.MatchIn(r.Content, new[] { "engine", "database" }));
                Assert.Equal(2, rows);
                Assert.Contains(@"WHERE (r.""Content"" &@| ARRAY['engine','database']::text[]) = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_QueryIn_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => EF.Functions.QueryIn(r.Content, new[] { "Groonga engine", "PostgreSQL -PGroonga" }));
                Assert.Equal(2, rows);
                Assert.Contains(@"WHERE (r.""Content"" &@~| ARRAY['Groonga engine','PostgreSQL -PGroonga']::text[]) = TRUE", Sql);
            }
        }

        #endregion Operators pgroonga_text_full_text_search_ops_v2

        #region Operators pgroonga_text_term_search_ops_v2

        [Fact]
        public void Operator_PrefixSearch_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => EF.Functions.PrefixSearch(r.Tag, "pg"));
                Assert.Equal(2, rows);
                Assert.Contains(@"WHERE (r.""Tag"" &^ 'pg') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_PrefixRkSearch_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => EF.Functions.PrefixRkSearch(r.Tag, "pi-ji-"));
                Assert.Equal(2, rows);
                Assert.Contains(@"WHERE (r.""Tag"" &^~ 'pi-ji-') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_PrefixSearchIn_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => EF.Functions.PrefixSearchIn(r.Tag, new[] { "pg", "gro" }));
                Assert.Equal(3, rows);
                Assert.Contains(@"WHERE (r.""Tag"" &^| ARRAY['pg','gro']::text[]) = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_PrefixRkSearchIn_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => EF.Functions.PrefixRkSearchIn(r.Tag, new[] { "pi-ji-", "posu" }));
                Assert.Equal(4, rows);
                Assert.Contains(@"WHERE (r.""Tag"" &^~| ARRAY['pi-ji-','posu']::text[]) = TRUE", Sql);
            }
        }

        #endregion Operators pgroonga_text_term_search_ops_v2

        #region Operators pgroonga_text_regexp_ops_v2

        [Fact]
        public void Operator_RegexpMatch_V2()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Single(r => EF.Functions.RegexpMatch(r.Content, "\\Apostgresql"));
                Assert.Equal(1, row.Id);
                Assert.Contains(@"WHERE (r.""Content"" &~ '\Apostgresql') = TRUE", Sql);
            }
        }

        #endregion Operators pgroonga_text_term_search_ops_v2

        #region Support

        PGroongaContext CreateContext() => Fixture.CreateContext();

        string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class PGroongaFixture : SharedStoreFixtureBase<PGroongaContext>
        {
            protected override string StoreName { get; } = "PGroongaTest";

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection).AddEntityFrameworkNpgsqlPGroonga();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var optionsBuilder = base.AddOptions(builder);
                new NpgsqlDbContextOptionsBuilder(optionsBuilder);

                return optionsBuilder;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.HasPostgresExtension("pgroonga");

                modelBuilder.Entity<PGroongaType>()
                    .HasIndex(t => new { t.Id, t.Content })
                    .ForNpgsqlHasMethod("pgroonga");

                modelBuilder.Entity<PGroongaType>()
                    .HasIndex(t => t.Tag)
                    .ForNpgsqlHasMethod("pgroonga")
                    .ForNpgsqlHasOperators("pgroonga_text_term_search_ops_v2");
            }

            protected override void Seed(PGroongaContext context)
                => PGroongaContext.Seed(context);

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }

        public class PGroongaContext : PoolableDbContext
        {
            public PGroongaContext(DbContextOptions<PGroongaContext> options) : base(options) {}

            public DbSet<PGroongaType> PGroongaTypes { get; set; }

            public static void Seed(PGroongaContext context)
            {
                context.PGroongaTypes.Add(new PGroongaType
                {
                    Id = 1,
                    Tag = "PostgreSQL",
                    Content = "PostgreSQL is a relational database management system."
                });
                context.PGroongaTypes.Add(new PGroongaType
                {
                    Id = 2,
                    Tag = "Groonga",
                    Content = "Groonga is a fast full text search engine that supports all languages."
                });
                context.PGroongaTypes.Add(new PGroongaType
                {
                    Id = 3,
                    Tag = "PGroonga",
                    Content = "PGroonga is a PostgreSQL extension that uses Groonga as index."
                });
                context.PGroongaTypes.Add(new PGroongaType
                {
                    Id = 4,
                    Tag = "pglogical",
                    Content = "There is groonga command."
                });
                context.PGroongaTypes.Add(new PGroongaType
                {
                    Id = 5,
                    Tag = "ポストグレスキューエル",
                    Content = "There is katakana."
                });
                context.PGroongaTypes.Add(new PGroongaType
                {
                    Id = 6,
                    Tag = "ポスグレ",
                    Content = "There is katakana."
                });
                context.PGroongaTypes.Add(new PGroongaType
                {
                    Id = 7,
                    Tag = "グルンガ",
                    Content = "There is katakana."
                });
                context.PGroongaTypes.Add(new PGroongaType
                {
                    Id = 8,
                    Tag = "ピージールンガ",
                    Content = "There is katakana."
                });
                context.PGroongaTypes.Add(new PGroongaType
                {
                    Id = 9,
                    Tag = "ピージーロジカル",
                    Content = "There is katakana."
                });
                context.SaveChanges();
            }
        }

        public class PGroongaType
        {
            public int Id { get; set; }
            public string Tag { get; set; }
            public string Content { get; set; }
        }

        #endregion Support
    }
}
