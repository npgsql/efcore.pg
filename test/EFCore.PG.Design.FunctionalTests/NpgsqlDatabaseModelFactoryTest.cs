using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
//using Npgsql.EntityFrameworkCore.PostgreSQL.Design.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

//namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.FunctionalTests
//{
//    public class NpgsqlDatabaseModelFactoryTest : IClassFixture<NpgsqlDatabaseModelFixture>
//    {
//        [Fact]
//        public void It_reads_tables()
//        {
//            var sql = @"
//CREATE TABLE public.everest (id int);
//CREATE TABLE public.denali (id int);";
//            var dbModel = CreateModel(sql, new List<string> { "everest", "denali" });

//            Assert.Collection(dbModel.Tables.OrderBy(t => t.Name),
//                d =>
//                {
//                    Assert.Equal("public", d.Schema);
//                    Assert.Equal("denali", d.Name);
//                },
//                e =>
//                {
//                    Assert.Equal("public", e.Schema);
//                    Assert.Equal("everest", e.Name);
//                });
//        }

//        [Fact]
//        public void It_reads_foreign_keys()
//        {
//            _fixture.ExecuteNonQuery("CREATE SCHEMA db2");
//            var sql = "CREATE TABLE public.ranges (id INT PRIMARY KEY);" +
//                      "CREATE TABLE db2.mountains (range_id INT NOT NULL, FOREIGN KEY (range_id) REFERENCES ranges(id) ON DELETE CASCADE)";
//            var dbModel = CreateModel(sql, new List<string> { "ranges", "mountains" });

//            var fk = Assert.Single(dbModel.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

//            Assert.Equal("db2", fk.Table.Schema);
//            Assert.Equal("mountains", fk.Table.Name);
//            Assert.Equal("public", fk.PrincipalTable.Schema);
//            Assert.Equal("ranges", fk.PrincipalTable.Name);
//            Assert.Equal("range_id", fk.Columns.Single().Name);
//            Assert.Equal("id", fk.PrincipalColumns.Single().Name);
//            Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
//        }

//        [Fact]
//        public void It_reads_composite_foreign_keys()
//        {
//            _fixture.ExecuteNonQuery("CREATE SCHEMA db3");
//            var sql = "CREATE TABLE public.ranges1 (id INT, alt_id INT, PRIMARY KEY(id, alt_id));" +
//                      "CREATE TABLE db3.mountains1 (range_id INT NOT NULL, range_alt_id INT NOT NULL, FOREIGN KEY (range_id, range_alt_id) REFERENCES ranges1(id, alt_id) ON DELETE NO ACTION)";
//            var dbModel = CreateModel(sql, new List<string> { "ranges1", "mountains1" });

//            var fk = Assert.Single(dbModel.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

//            Assert.Equal("db3", fk.Table.Schema);
//            Assert.Equal("mountains1", fk.Table.Name);
//            Assert.Equal("public", fk.PrincipalTable.Schema);
//            Assert.Equal("ranges1", fk.PrincipalTable.Name);
//            Assert.Equal(new[] { "range_id", "range_alt_id" }, fk.Columns.Select(c => c.Name).ToArray());
//            Assert.Equal(new[] { "id", "alt_id" }, fk.PrincipalColumns.Select(c => c.Name).ToArray());
//            Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
//        }

//        [Fact]
//        public void It_reads_indexes()
//        {
//            var sql = @"CREATE TABLE place (id int PRIMARY KEY, name int UNIQUE, location int);" +
//                      @"CREATE INDEX ""IX_name_location"" ON place (name, location)";
//            var dbModel = CreateModel(sql, new List<string> { "place" });

//            var indexes = dbModel.Tables.Single().Indexes;

//            Assert.All(indexes, c =>
//            {
//                Assert.Equal("public", c.Table.Schema);
//                Assert.Equal("place", c.Table.Name);
//            });

//            Assert.Collection(indexes,
//                unique =>
//                {
//                    Assert.True(unique.IsUnique);
//                    Assert.Equal("name", unique.Columns.Single().Name);
//                },
//                composite =>
//                {
//                    Assert.Equal("IX_name_location", composite.Name);
//                    Assert.False(composite.IsUnique);
//                    Assert.Equal(new List<string> { "name", "location" }, composite.Columns.Select(c => c.Name).ToList());
//                });
//        }

//        [Fact]
//        public void It_reads_columns()
//        {
//            var sql = @"
//CREATE TABLE public.mountains_columns (
//    id int,
//    name varchar(100) NOT NULL,
//    latitude decimal(5,2) DEFAULT 0.0,
//    created timestamp DEFAULT now(),
//    discovered_date numeric,
//    PRIMARY KEY (name, id)
//);";
//            var dbModel = CreateModel(sql, new List<string> { "mountains_columns" });

//            var columns = dbModel.Tables.Single().Columns;

//            Assert.All(columns, c =>
//            {
//                Assert.Equal("public", c.Table.Schema);
//                Assert.Equal("mountains_columns", c.Table.Name);
//            });

//            Assert.Collection(
//                columns,
//                id =>
//                    {
//                        Assert.Equal("id", id.Name);
//                        Assert.Equal("int4", id.StoreType);
//                        Assert.False(id.IsNullable);
//                        Assert.Null(id.DefaultValueSql);
//                    },
//                name =>
//                    {
//                        Assert.Equal("name", name.Name);
//                        Assert.Equal("varchar(100)", name.StoreType);
//                        Assert.False(name.IsNullable);
//                        Assert.Null(name.DefaultValueSql);
//                    },
//                lat =>
//                    {
//                        Assert.Equal("latitude", lat.Name);
//                        Assert.Equal("numeric(5, 2)", lat.StoreType);
//                        Assert.True(lat.IsNullable);
//                        Assert.Equal("0.0", lat.DefaultValueSql);
//                    },
//                created =>
//                    {
//                        Assert.Equal("created", created.Name);
//                        Assert.Equal("timestamp", created.StoreType);
//                        Assert.True(created.IsNullable);
//                        Assert.Equal("now()", created.DefaultValueSql);
//                    },
//                discovered =>
//                    {
//                        Assert.Equal("discovered_date", discovered.Name);
//                        Assert.Equal("numeric", discovered.StoreType);
//                        Assert.True(discovered.IsNullable);
//                        Assert.Null(discovered.DefaultValueSql);
//                    });
//        }

//        [Theory]
//        [InlineData("varchar(341)", 341)]
//        [InlineData("char(89)", 89)]
//        public void It_reads_max_length(string type, int? length)
//        {
//            var sql = "DROP TABLE IF EXISTS strings;" +
//                     $"CREATE TABLE public.strings (char_column {type});";
//            var db = CreateModel(sql, new List<string> { "strings" });

//            Assert.Equal(type, db.Tables.Single().Columns.Single().StoreType);
//        }

//        [Fact]
//        public void It_reads_primary_key()
//        {
//            var sql = "CREATE TABLE place (id int PRIMARY KEY, name int UNIQUE, location int);" +
//                      "CREATE INDEX ix_location_name ON place (location, name);";

//            var dbModel = CreateModel(sql, new List<string> { "place" });

//            var pkIndex = dbModel.Tables.Single().PrimaryKey;

//            Assert.Equal("place", pkIndex.Table.Name);
//            Assert.Equal(new List<string> { "id" }, pkIndex.Columns.Select(ic => ic.Name).ToList());
//        }

//        [Fact]
//        public void It_filters_tables()
//        {
//            var sql = @"CREATE TABLE public.k2 (id int, a varchar, UNIQUE (a));" +
//                      @"CREATE TABLE public.kilimanjaro (id int, b varchar, UNIQUE (b), FOREIGN KEY (b) REFERENCES k2 (a));";

//            var selectionSet = new List<string> { "k2" };

//            var dbModel = CreateModel(sql, selectionSet);
//            var table = Assert.Single(dbModel.Tables);
//            Assert.Equal("k2", table.Name);
//            Assert.Equal(2, table.Columns.Count);
//            Assert.Equal(1, table.Indexes.Count);
//            Assert.Empty(table.ForeignKeys);
//        }

//        [Fact]
//        public void It_reads_sequences()
//        {
//            var sql = @"CREATE SEQUENCE ""DefaultValues_ascending_read"";
 
//CREATE SEQUENCE ""DefaultValues_descending_read"" INCREMENT BY -1;

//CREATE SEQUENCE ""CustomSequence_read""
//    START WITH 1 
//    INCREMENT BY 2 
//    MAXVALUE 8 
//    MINVALUE -3 
//    CYCLE;";

//            var dbModel = CreateModel(sql);
//            Assert.Collection(dbModel.Sequences.Where(s => s.Name.EndsWith("_read")).OrderBy(s => s.Name),
//                c =>
//                    {
//                        Assert.Equal("CustomSequence_read", c.Name);
//                        Assert.Equal("public", c.Schema);
//                        Assert.Equal("bigint", c.StoreType);
//                        Assert.Equal(1, c.StartValue);
//                        Assert.Equal(2, c.IncrementBy);
//                        Assert.Equal(8, c.MaxValue);
//                        Assert.Equal(-3, c.MinValue);
//                        Assert.True(c.IsCyclic);
//                    },
//                da =>
//                    {
//                        Assert.Equal("DefaultValues_ascending_read", da.Name);
//                        Assert.Equal("public", da.Schema);
//                        Assert.Equal("bigint", da.StoreType);
//                        Assert.Equal(1, da.IncrementBy);
//                        Assert.False(da.IsCyclic);
//                        Assert.Null(da.MaxValue);
//                        Assert.Null(da.MinValue);
//                        Assert.Null(da.StartValue);
//                    },
//                dd =>
//                {
//                    Assert.Equal("DefaultValues_descending_read", dd.Name);
//                    Assert.Equal("public", dd.Schema);
//                    Assert.Equal("bigint", dd.StoreType);
//                    Assert.Equal(-1, dd.IncrementBy);
//                    Assert.False(dd.IsCyclic);
//                    Assert.Null(dd.MaxValue);
//                    Assert.Null(dd.MinValue);
//                    Assert.Null(dd.StartValue);
//                });
//        }

//        [Fact]
//        public void SequenceSerial()
//        {
//            var dbModel = CreateModel(@"CREATE TABLE ""SerialSequence"" (""SerialSequenceId"" serial primary key)");

//            // Sequences which belong to a serial column should not get reverse engineered as separate sequences
//            Assert.Empty(dbModel.Sequences.Where(s => s.Name == "SerialSequence_SerialSequenceId_seq"));

//            // Now make sure the field itself is properly reverse-engineered.
//            var column = dbModel.Tables.Single(t => t.Name == "SerialSequence").Columns.Single();
//            Assert.Equal(ValueGenerated.OnAdd, column.ValueGenerated);
//            Assert.Null(column.DefaultValueSql);
//            //Assert.True(column.Npgsql().IsSerial);
//        }

//        [Fact]
//        public void SequenceNonSerial()
//        {
//            var dbModel = CreateModel(@"
//CREATE SEQUENCE some_sequence;
//CREATE TABLE non_serial_sequence (id integer PRIMARY KEY DEFAULT nextval('some_sequence'))");

//            var column = dbModel.Tables.Single(t => t.Name == "non_serial_sequence").Columns.Single();
//            Assert.Equal("nextval('some_sequence'::regclass)", column.DefaultValueSql);
//            // Npgsql has special identification for serial columns (scaffolding them with ValueGenerated.OnAdd
//            // and removing the default), but not for non-serial sequence-driven columns, which are scaffolded
//            // with a DefaultValue. This is consistent with the SqlServer scaffolding behavior.
//            Assert.Null(column.ValueGenerated);
//        }

//        /*
//        [Fact, IssueLink("https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/77")]
//        public void SequencesOnlyFromRequestedSchema()
//        {
//            var dbModel = CreateModel(@"
//CREATE SEQUENCE public.some_sequence2;
//CREATE SCHEMA not_interested;
//CREATE SEQUENCE not_interested.some_other_sequence;
//", new TableSelectionSet(Enumerable.Empty<string>(), new[] { "public" }));

//            var sequence = dbModel.Sequences.Single();
//            Assert.Equal("public", sequence.Schema);
//            Assert.Equal("some_sequence2", sequence.Name);
//        }*/

//        [Fact]
//        public void DefaultSchemaIsPublic()
//            => Assert.Equal("public", _fixture.CreateModel("SELECT 1").DefaultSchema);

//        readonly NpgsqlDatabaseModelFixture _fixture;

//        public DatabaseModel CreateModel(string createSql, IEnumerable<string> tables = null)
//            => _fixture.CreateModel(createSql, tables);

//        public NpgsqlDatabaseModelFactoryTest(NpgsqlDatabaseModelFixture fixture)
//        {
//            _fixture = fixture;
//        }

//        [Fact]
//        public void Test_Scaffolding_2_1()
//        {
//            var appServiceProivder = new ServiceCollection()
//                .AddDbContext<PostgreSQL.FunctionalTests.NpgsqlValueGenerationScenariosTest.ContextBase>()
//                .BuildServiceProvider();

//            var serviceScope = appServiceProivder
//                  .GetRequiredService<IServiceScopeFactory>()
//                  .CreateScope();

//            var context = serviceScope.ServiceProvider.GetService<PostgreSQL.FunctionalTests.NpgsqlValueGenerationScenariosTest.ContextBase>();
//            var logger = context.GetService<Microsoft.EntityFrameworkCore.Diagnostics.IDiagnosticsLogger<Microsoft.EntityFrameworkCore.DbLoggerCategory.Scaffolding>>();
//            var test = new NpgsqlDatabaseModelFactory(logger);
//            var ret = test.Create(context.Database.GetDbConnection(), new List<string> { "Orders" }, new List<string> { "public" });
//        }
//    }
//}
