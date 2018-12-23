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

        #region Functions

        [Fact]
        public void Function_PgroongaCommand()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Where(t => t.Id == 1).Select(x => EF.Functions.PgroongaCommand("status"))
                    .Single();
                Assert.Contains("uptime", rows);
                Assert.Contains(@"SELECT pgroonga_command('status')", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaCommandEscapeValue()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1)
                    .Select(x => EF.Functions.PgroongaCommandEscapeValue("(PostgreSQL")).Single();
                Assert.Equal("\"(PostgreSQL\"", row);
                Assert.Contains(@"SELECT pgroonga_command_escape_value('(PostgreSQL')", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaEscape()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1).Select(x => EF.Functions.PgroongaEscape(100)).Single();
                Assert.Equal("100", row);
                Assert.Contains(@"SELECT pgroonga_escape(100)", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaFlush()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1)
                    .Select(x => EF.Functions.PgroongaFlush("ix_pgroongatypes_id_content")).Single();
                Assert.Equal(true, row);
                Assert.Contains(@"SELECT pgroonga_flush('ix_pgroongatypes_id_content')", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaHighlightHtml()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1).Select(x =>
                        EF.Functions.PgroongaHighlightHtml("PGroonga is a PostgreSQL extension.",
                            new[] { "PostgreSQL" }))
                    .Single();
                Assert.Contains("<span class=\"keyword\">PostgreSQL</span>", row);
                Assert.Contains(@"SELECT pgroonga_highlight_html('PGroonga is a PostgreSQL extension.', ARRAY['PostgreSQL']", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaIsWritable()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1).Select(x => EF.Functions.PgroongaIsWritable()).Single();
                Assert.Equal(true, row);
                Assert.Contains(@"SELECT pgroonga_is_writable()", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaMatchPositionsByte()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1).Select(x =>
                        EF.Functions.PgroongaMatchPositionsByte("PGroonga is a PostgreSQL extension.",
                            new[] { "PostgreSQL" }))
                    .Single();
                Assert.Equal(new[,] { { 14, 10 } }, row);
                Assert.Contains(@"SELECT pgroonga_match_positions_byte('PGroonga is a PostgreSQL extension.', ARRAY['PostgreSQL']", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaMatchPositionsCharacter()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1).Select(x =>
                        EF.Functions.PgroongaMatchPositionsCharacter("PGroonga is a PostgreSQL extension.",
                            new[] { "PostgreSQL" }))
                    .Single();
                Assert.Equal(new[,] { { 14, 10 } }, row);
                Assert.Contains(@"SELECT pgroonga_match_positions_character('PGroonga is a PostgreSQL extension.', ARRAY['PostgreSQL']", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaNormalize()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1)
                    .Select(x => EF.Functions.PgroongaNormalize("aBcDe 123")).Single();
                Assert.Equal("abcde 123", row);
                Assert.Contains(@"SELECT pgroonga_normalize('aBcDe 123')", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaQueryEscape()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1)
                    .Select(x => EF.Functions.PgroongaQueryEscape("(PostgreSQL")).Single();
                Assert.Equal("\\(PostgreSQL", row);
                Assert.Contains(@"SELECT pgroonga_query_escape('(PostgreSQL')", Sql);
            }
        }

        /*
        [Fact]
        public void Function_PgroongaQueryExpand()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1).Select(x =>
                    EF.Functions.PgroongaQueryExpand("synonyms", "term", "synonyms", "PGroonga OR Mroonga")).Single();
                Assert.Equal("((PGroonga) OR (Groonga PostgreSQL)) OR Mroonga", row);
                Assert.Contains(@"SELECT pgroonga_query_expand('synonyms', 'term', 'synonyms',", Sql);
            }
        }
        */

        [Fact]
        public void Function_PgroongaQueryExtractKeywords()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1).Select(x =>
                    EF.Functions.PgroongaQueryExtractKeywords("Groonga PostgreSQL")).Single();
                Assert.Equal(new[] { "PostgreSQL", "Groonga" }, row);
                Assert.Contains(@"SELECT pgroonga_query_extract_keywords('Groonga PostgreSQL')", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaSetWritable()
        {
            using (var ctx = CreateContext())
            {
                var row1 = ctx.PGroongaTypes.Where(t => t.Id == 1).Select(x => EF.Functions.PgroongaSetWritable(false)).Single();
                Assert.Equal(true, row1);
                Assert.Contains(@"SELECT pgroonga_set_writable(FALSE)", Sql);
                var row2 = ctx.PGroongaTypes.Where(t => t.Id == 1).Select(x => EF.Functions.PgroongaSetWritable(true)).Single();
                Assert.Equal(false, row2);
            }
        }

        [Fact]
        public void Function_PgroongaScore()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes
                    .Where(r => r.Content.Match("engine"))
                    .Select(r => EF.Functions.PgroongaScore(r.Content))
                    .Single();
                Assert.Equal(1, row);
                Assert.Contains(@"SELECT pgroonga_score(r.tableoid, r.ctid)", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaSnippetHtml()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Where(t => t.Id == 1)
                    .Select(x =>
                        EF.Functions.PgroongaSnippetHtml(
                            @"Groonga is a fast and accurate full text search engine based on
inverted index. One of the characteristics of Groonga is that a
newly registered document instantly appears in search results.
Also, Groonga allows updates without read locks. These characteristics
result in superior performance on real-time applications.
\n
\n
Groonga is also a column-oriented database management system (DBMS).
Compared with well-known row-oriented systems, such as MySQL and
PostgreSQL, column-oriented systems are more suited for aggregate
queries. Due to this advantage, Groonga can cover weakness of
row-oriented systems.", new[] { "fast", "PostgreSQL" }))
                    .First();
                Assert.Equal(2, row.Length);
                Assert.Contains(@"SELECT pgroonga_snippet_html(", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaTableName()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes
                    .Where(r => r.Content.Match("engine"))
                    .Select(r => EF.Functions.PgroongaTableName("ix_pgroongatypes_id_content"))
                    .Single();
                Assert.Contains("Sources", row);
                Assert.Contains(@"SELECT pgroonga_table_name('ix_pgroongatypes_id_content')", Sql);
            }
        }

        /*
        [Fact]
        public void Function_PgroongaWalApply()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes
                    .Where(r => r.Content.Match("engine"))
                    .Select(r => EF.Functions.PgroongaWalApply("ix_pgroongatypes_id_content"))
                    .Single();
                Assert.Contains(@"SELECT pgroonga_wal_apply('ix_pgroongatypes_id_content')", Sql);
            }
        }

        [Fact]
        public void Function_PgroongaWalTruncate()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes
                    .Where(r => r.Content.Match("engine"))
                    .Select(r => EF.Functions.PgroongaWalTruncate("ix_pgroongatypes_id_content"))
                    .Single();
                Assert.Contains(@"SELECT pgroonga_wal_truncate('ix_pgroongatypes_id_content')", Sql);
            }
        }
        */

        #endregion Functions

        #region Operators pgroonga_text_full_text_search_ops_v2

        [Fact]
        public void Operator_Match_V2()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Single(r => r.Content.Match("engine"));
                Assert.Equal(2, row.Id);
                Assert.Contains(@"WHERE (r.""Content"" &@ 'engine') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_Query_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => r.Content.Query("PGroonga OR PostgreSQL"));
                Assert.Equal(2, rows);
                Assert.Contains(@"WHERE (r.""Content"" &@~ 'PGroonga OR PostgreSQL') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_SimilarSearch_V2()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Single(r => r.Content.SimilarSearch("Mroonga is a MySQL extension taht uses Groonga"));
                Assert.Equal(3, row.Id);
                Assert.Contains(@"WHERE (r.""Content"" &@* 'Mroonga is a MySQL extension taht uses Groonga') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_ScriptQuery_V2()
        {
            using (var ctx = CreateContext())
            {
                var row = ctx.PGroongaTypes.Single(r => r.Content.ScriptQuery("Id >= 2 && (Content @ 'engine' || Content @ 'rdbms')"));
                Assert.Equal(2, row.Id);
                Assert.Contains(@"WHERE (r.""Content"" &` 'Id >= 2 && (Content @ ''engine'' || Content @ ''rdbms'')') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_MatchIn_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => r.Content.MatchIn(new[] { "engine", "database" }));
                Assert.Equal(2, rows);
                Assert.Contains(@"WHERE (r.""Content"" &@| ARRAY['engine','database']::text[]) = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_QueryIn_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => r.Content.QueryIn(new[] { "Groonga engine", "PostgreSQL -PGroonga" }));
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
                var rows = ctx.PGroongaTypes.Count(r => r.Tag.PrefixSearch("pg"));
                Assert.Equal(2, rows);
                Assert.Contains(@"WHERE (r.""Tag"" &^ 'pg') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_PrefixRkSearch_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => r.Tag.PrefixRkSearch("pi-ji-"));
                Assert.Equal(2, rows);
                Assert.Contains(@"WHERE (r.""Tag"" &^~ 'pi-ji-') = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_PrefixSearchIn_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => r.Tag.PrefixSearchIn(new[] { "pg", "gro" }));
                Assert.Equal(3, rows);
                Assert.Contains(@"WHERE (r.""Tag"" &^| ARRAY['pg','gro']::text[]) = TRUE", Sql);
            }
        }

        [Fact]
        public void Operator_PrefixRkSearchIn_V2()
        {
            using (var ctx = CreateContext())
            {
                var rows = ctx.PGroongaTypes.Count(r => r.Tag.PrefixRkSearchIn(new[] { "pi-ji-", "posu" }));
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
                var row = ctx.PGroongaTypes.Single(r => r.Content.RegexpMatch("\\Apostgresql"));
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
                    .ForNpgsqlHasMethod("pgroonga")
                    .HasName("ix_pgroongatypes_id_content");

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
