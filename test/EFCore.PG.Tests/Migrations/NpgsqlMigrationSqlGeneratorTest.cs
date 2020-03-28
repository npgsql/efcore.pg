using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations
{
    public class NpgsqlMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        #region Database

        [Fact]
        public virtual void CreateDatabaseOperation()
        {
            Generate(new NpgsqlCreateDatabaseOperation { Name = "Northwind" });

            AssertSql(
                @"CREATE DATABASE ""Northwind"";
");
        }

        [Fact]
        public virtual void CreateDatabaseOperation_with_template()
        {
            Generate(new NpgsqlCreateDatabaseOperation
            {
                Name = "Northwind",
                Template = "MyTemplate"
            });

            AssertSql(
                @"CREATE DATABASE ""Northwind"" TEMPLATE ""MyTemplate"";
");
        }

        [Fact]
        public virtual void CreateDatabaseOperation_with_tablespace()
        {
            Generate(new NpgsqlCreateDatabaseOperation
            {
                Name = "some_db",
                Tablespace = "MyTablespace"
            });

            AssertSql(
                @"CREATE DATABASE some_db TABLESPACE ""MyTablespace"";
");
        }

        #endregion

        // Which index collations are available on a given PostgreSQL varies (e.g. Linux vs. Windows)
        // so we test support for this on the generated SQL only, and not against the database in MigrationsNpsqlTest.
        [Fact]
        public void CreateIndexOperation_collation()
        {
            Generate(new CreateIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                Columns = new[] { "FirstName", "LastName" },
                [NpgsqlAnnotationNames.IndexCollation] = new[] { null, "de_DE" }
            });

            AssertSql(
                @"CREATE INDEX ""IX_People_Name"" ON dbo.""People"" (""FirstName"", ""LastName"" COLLATE ""de_DE"");
");
        }

        #region CockroachDB interleave-in-parent

        // Note that we don't run tests against actual CockroachDB instances, so these are unit tests asserting on SQL
        // only

        [Fact]
        public void CreateTableOperation_with_cockroach_interleave_in_parent()
        {
            var op =
                new CreateTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            Table = "People",
                            Schema = "dbo",
                            ClrType = typeof(int),
                            IsNullable = false
                        },
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Id" }
                    }
                };

            var interleaveInParent = new CockroachDbInterleaveInParent(op);
            interleaveInParent.ParentTableSchema = "my_schema";
            interleaveInParent.ParentTableName = "my_parent";
            interleaveInParent.InterleavePrefix = new List<string> { "col_a", "col_b" };

            Generate(op);

            AssertSql(
                @"CREATE TABLE dbo.""People"" (
    ""Id"" integer NOT NULL,
    PRIMARY KEY (""Id"")
)
INTERLEAVE IN PARENT my_schema.my_parent (col_a, col_b);
");
        }

        #endregion CockroachDB interleave-in-parent

#pragma warning disable 618
        [Fact]
        public virtual void AddColumnOperation_serial_old_annotation_throws()
        {
            Assert.Throws<NotSupportedException>(() =>
                Generate(new AddColumnOperation
                {
                    Table = "People",
                    Name = "foo",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false,
                    [NpgsqlAnnotationNames.ValueGeneratedOnAdd] = true
                }));
        }
#pragma warning restore 618

        public NpgsqlMigrationSqlGeneratorTest() : base(NpgsqlTestHelpers.Instance)
        {
        }
    }
}
