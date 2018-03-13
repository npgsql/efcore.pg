using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        public override void AddColumnOperation_with_defaultValue()
        {
            base.AddColumnOperation_with_defaultValue();

            Assert.Equal(
                "ALTER TABLE dbo.\"People\" ADD \"Name\" varchar(30) NOT NULL DEFAULT 'John Doe';" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_defaultValueSql()
        {
            base.AddColumnOperation_with_defaultValueSql();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Birthday\" date NULL DEFAULT (CURRENT_TIMESTAMP);" + EOL,
                Sql);
        }

        [Fact]
        public override void AddColumnOperation_with_computed_column_SQL()
        {
            base.AddColumnOperation_with_computed_column_SQL();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Birthday\" date NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Alias\" text NOT NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength()
        {
            base.AddColumnOperation_with_maxLength();

            Assert.Equal(
                @"ALTER TABLE ""Person"" ADD ""Name"" varchar(30) NULL;" + EOL,
                Sql);
        }

        public override void AddForeignKeyOperation_with_name()
        {
            base.AddForeignKeyOperation_with_name();

            Assert.Equal(
                "ALTER TABLE dbo.\"People\" ADD CONSTRAINT \"FK_People_Companies\" FOREIGN KEY (\"EmployerId1\", \"EmployerId2\") REFERENCES hr.\"Companies\" (\"Id1\", \"Id2\") ON DELETE CASCADE;" + EOL,
                Sql);
        }

        public override void AddForeignKeyOperation_without_name()
        {
            base.AddForeignKeyOperation_without_name();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD FOREIGN KEY (\"SpouseId\") REFERENCES \"People\" (\"Id\");" + EOL,
                Sql);
        }

        public override void AddPrimaryKeyOperation_with_name()
        {
            base.AddPrimaryKeyOperation_with_name();

            Assert.Equal(
                "ALTER TABLE dbo.\"People\" ADD CONSTRAINT \"PK_People\" PRIMARY KEY (\"Id1\", \"Id2\");" + EOL,
                Sql);
        }

        public override void AddPrimaryKeyOperation_without_name()
        {
            base.AddPrimaryKeyOperation_without_name();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD PRIMARY KEY (\"Id\");" + EOL,
                Sql);
        }

        public override void AddUniqueConstraintOperation_with_name()
        {
            base.AddUniqueConstraintOperation_with_name();

            Assert.Equal(
                "ALTER TABLE dbo.\"People\" ADD CONSTRAINT \"AK_People_DriverLicense\" UNIQUE (\"DriverLicense_State\", \"DriverLicense_Number\");" + EOL,
                Sql);
        }

        public override void AddUniqueConstraintOperation_without_name()
        {
            base.AddUniqueConstraintOperation_without_name();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD UNIQUE (\"SSN\");" + EOL,
                Sql);
        }

        public override void AlterSequenceOperation_with_minValue_and_maxValue()
        {
            base.AlterSequenceOperation_with_minValue_and_maxValue();

            Assert.Equal(
                "ALTER SEQUENCE dbo.\"EntityFrameworkHiLoSequence\" INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;" + EOL,
                Sql);
        }

        public override void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            base.AlterSequenceOperation_without_minValue_and_maxValue();

            Assert.Equal(
                "ALTER SEQUENCE \"EntityFrameworkHiLoSequence\" INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;" + EOL,
                Sql);
        }

        public override void CreateIndexOperation_unique()
        {
            base.CreateIndexOperation_unique();

            Assert.Equal(
                "CREATE UNIQUE INDEX \"IX_People_Name\" ON dbo.\"People\" (\"FirstName\", \"LastName\");" + EOL,
                Sql);
        }

        public override void CreateIndexOperation_nonunique()
        {
            base.CreateIndexOperation_nonunique();

            Assert.Equal(
                "CREATE INDEX \"IX_People_Name\" ON \"People\" (\"Name\");" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateDatabaseOperation()
        {
            Generate(new NpgsqlCreateDatabaseOperation { Name = "Northwind" });

            Assert.Equal(
                @"CREATE DATABASE ""Northwind"";" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateDatabaseOperation_with_template()
        {
            Generate(new NpgsqlCreateDatabaseOperation
            {
                Name = "Northwind",
                Template = "MyTemplate"
            });

            Assert.Equal(
                @"CREATE DATABASE ""Northwind"" TEMPLATE ""MyTemplate"";" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateDatabaseOperation_with_tablespace()
        {
            Generate(new NpgsqlCreateDatabaseOperation
            {
                Name = "some_db",
                Tablespace = "MyTablespace"
            });

            Assert.Equal(
                @"CREATE DATABASE some_db TABLESPACE ""MyTablespace"";" + EOL,
                Sql);
        }

        public override void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_with_minValue_and_maxValue();

            Assert.Equal(
                "CREATE SEQUENCE dbo.\"EntityFrameworkHiLoSequence\" START WITH 3 INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;" + EOL,
                Sql);
        }

        public override void CreateSequenceOperation_with_minValue_and_maxValue_not_long()
        {
            // In PostgreSQL, sequence data types are always bigint.
            // http://www.postgresql.org/docs/9.4/static/infoschema-sequences.html
        }

        public override void CreateSequenceOperation_without_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_without_minValue_and_maxValue();

            Assert.Equal(
                "CREATE SEQUENCE \"EntityFrameworkHiLoSequence\" START WITH 3 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;" + EOL,
                Sql);
        }

        public override void CreateTableOperation()
        {
            base.CreateTableOperation();

            Assert.Equal(
                "CREATE TABLE dbo.\"People\" (" + EOL +
                "    \"Id\" integer NOT NULL," + EOL +
                "    \"EmployerId\" integer NULL," + EOL +
                "    \"SSN\" char(11) NULL," + EOL +
                "    PRIMARY KEY (\"Id\")," + EOL +
                "    UNIQUE (\"SSN\")," + EOL +
                "    FOREIGN KEY (\"EmployerId\") REFERENCES \"Companies\" (\"Id\")" + EOL +
                ");" + EOL,
                Sql);
        }

        public override void DropColumnOperation()
        {
            base.DropColumnOperation();

            Assert.Equal(
                "ALTER TABLE dbo.\"People\" DROP COLUMN \"LuckyNumber\";" + EOL,
                Sql);
        }

        public override void DropForeignKeyOperation()
        {
            base.DropForeignKeyOperation();

            Assert.Equal(
                "ALTER TABLE dbo.\"People\" DROP CONSTRAINT \"FK_People_Companies\";" + EOL,
                Sql);
        }

        public override void DropPrimaryKeyOperation()
        {
            base.DropPrimaryKeyOperation();

            Assert.Equal(
                "ALTER TABLE dbo.\"People\" DROP CONSTRAINT \"PK_People\";" + EOL,
                Sql);
        }

        public override void DropSequenceOperation()
        {
            base.DropSequenceOperation();

            Assert.Equal(
                "DROP SEQUENCE dbo.\"EntityFrameworkHiLoSequence\";" + EOL,
                Sql);
        }

        public override void DropTableOperation()
        {
            base.DropTableOperation();

            Assert.Equal(
                "DROP TABLE dbo.\"People\";" + EOL,
                Sql);
        }

        public override void DropUniqueConstraintOperation()
        {
            base.DropUniqueConstraintOperation();

            Assert.Equal(
                "ALTER TABLE dbo.\"People\" DROP CONSTRAINT \"AK_People_SSN\";" + EOL,
                Sql);
        }

        #region AlterColumn

        public override void AlterColumnOperation()
        {
            base.AlterColumnOperation();

            Assert.Equal(
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" TYPE int;" + EOL +
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET NOT NULL;" + EOL +
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET DEFAULT 7;" + EOL,
            Sql);
        }

        public override void AlterColumnOperation_without_column_type()
        {
            base.AlterColumnOperation_without_column_type();

            Assert.Equal(
                @"ALTER TABLE ""People"" ALTER COLUMN ""LuckyNumber"" TYPE integer;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""LuckyNumber"" SET NOT NULL;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""LuckyNumber"" DROP DEFAULT;" + EOL,
            Sql);
        }

        [Fact]
        public void AlterColumnOperation_with_defaultValue()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30
                });

            Assert.Equal(
                "ALTER TABLE \"People\" ALTER COLUMN \"Name\" TYPE varchar(30);" + EOL +
                "ALTER TABLE \"People\" ALTER COLUMN \"Name\" SET NOT NULL;" + EOL +
                "ALTER TABLE \"People\" ALTER COLUMN \"Name\" DROP DEFAULT;" + EOL,
                Sql);
        }

        #endregion

        #region Value generation add

        [Fact]
        public virtual void AddColumnOperation_serial()
        {
            Generate(new AddColumnOperation
            {
                Table = "People",
                Name = "foo",
                ClrType = typeof(int),
                ColumnType = "int",
                IsNullable = false,
                [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
            });

            Assert.Equal(
                "ALTER TABLE \"People\" ADD foo serial NOT NULL;" + EOL,
                Sql);
        }

        // EFCore will add a default in some cases, e.g. adding a non-nullable column
        // to an existing table. This shouldn't affect serial column creation.
        // See #68
        [Fact]
        public void AddColumnOperation_serial_with_default()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "foo",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    DefaultValue = 0,
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
                });

            Assert.Equal(
                @"ALTER TABLE ""People"" ADD foo serial NOT NULL DEFAULT 0;" + EOL,
                Sql);
        }

        [Fact]
        public void AddColumnOperation_with_identity_always()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                });

            Assert.Equal(
                @"ALTER TABLE ""People"" ADD ""Id"" integer NOT NULL GENERATED ALWAYS AS IDENTITY;" + EOL,
                Sql);
        }

        [Fact]
        public void AddColumnOperation_with_identity_by_default()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                });

            Assert.Equal(
                @"ALTER TABLE ""People"" ADD ""Id"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY;" + EOL,
                Sql);
        }

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

        #endregion Value generation add

        #region Value generation alter

        [Fact]
        public void AlterColumnOperation_to_identity()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                });

            Assert.Equal(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" ADD GENERATED ALWAYS AS IDENTITY;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE integer;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public void AlterColumnOperation_to_serial()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "LongKey",
                    ClrType = typeof(long),
                    IsNullable = false,
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
                });

            Assert.Equal(
                @"CREATE SEQUENCE ""People_LongKey_seq"" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;" + EOL +
                @"GO" + EOL + EOL +  // Note that GO here is just a delimiter introduced in the tests to indicate a batch boundary
                @"ALTER TABLE ""People"" ALTER COLUMN ""LongKey"" TYPE bigint;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""LongKey"" SET NOT NULL;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""LongKey"" SET DEFAULT (nextval('""People_LongKey_seq""'));" + EOL +
                @"ALTER SEQUENCE ""People_LongKey_seq"" OWNED BY ""People"".""LongKey""",
                Sql);
        }

        [Fact]
        public void AlterColumnOperation_identity_to_identity()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    OldColumn = new ColumnOperation
                    {
                        [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    },
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                });

            Assert.Equal(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET GENERATED ALWAYS;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE integer;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public void AlterColumnOperation_serial_to_identity()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    OldColumn = new ColumnOperation
                    {
                        [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
                    },
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                });

            Assert.Equal(@"ALTER SEQUENCE ""People_Id_seq"" RENAME TO ""People_Id_old_seq"";" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" DROP DEFAULT;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" ADD GENERATED ALWAYS AS IDENTITY;" + EOL +
                @"SELECT * FROM setval('""People_Id_seq""', nextval('""People_Id_old_seq""'), false);" + EOL +
                @"DROP SEQUENCE ""People_Id_old_seq"";" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE integer;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;" + EOL,
                Sql);
        }

        #endregion Value generation alter

        #region Indexes

        [Fact]
        public void CreateIndexOperation_method()
        {
            Generate(new CreateIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                Columns = new[] { "FirstName" },
                [NpgsqlAnnotationNames.Prefix + NpgsqlAnnotationNames.IndexMethod] = "gin"
            });

            Assert.Equal(
                "CREATE INDEX \"IX_People_Name\" ON dbo.\"People\" USING gin (\"FirstName\");" + EOL,
                Sql);
        }

        [Fact]
        public void RenameIndexOperation()
        {
            Generate(
                new RenameIndexOperation
                {
                    Table = "People",
                    Name = "x",
                    NewName = "y",
                    Schema = "myschema"
                });

            Assert.Equal(
                "ALTER INDEX myschema.x RENAME TO y;" + EOL,
                Sql);
        }

        #endregion Indexes

        #region PostgreSQL extensions

        [Fact]
        public void EnsurePostgresExtension()
        {
            var op = new AlterDatabaseOperation();
            PostgresExtension.GetOrAddPostgresExtension(op, "hstore");
            Generate(op);

            Assert.Equal(
                @"CREATE EXTENSION IF NOT EXISTS hstore;" + EOL,
                Sql);
        }

        [Fact]
        public void EnsurePostgresExtension_with_schema()
        {
            var op = new AlterDatabaseOperation();
            var extension = PostgresExtension.GetOrAddPostgresExtension(op, "hstore");
            extension.Schema = "myschema";
            Generate(op);

            Assert.Equal(
                @"CREATE EXTENSION IF NOT EXISTS hstore SCHEMA myschema;" + EOL,
                Sql);
        }

        #endregion PostgreSQL extensions

        #region PostgreSQL Storage Parameters

        [Fact]
        public void CreateTableOperation_with_storage_parameter()
        {
            Generate(
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
                            ClrType = typeof(int),
                            IsNullable = false
                        },
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Id" }
                    },
                    [NpgsqlAnnotationNames.StorageParameterPrefix + "fillfactor"] = 70,
                    [NpgsqlAnnotationNames.StorageParameterPrefix + "user_catalog_table"] = true,
                    ["some_bogus_name"] = 0
                });

            Assert.Equal(
                "CREATE TABLE dbo.\"People\" (" + EOL +
                "    \"Id\" integer NOT NULL," + EOL +
                "    PRIMARY KEY (\"Id\")" + EOL +
                ")" + EOL +
                "WITH (fillfactor=70, user_catalog_table=true);" + EOL,
                Sql);
        }

        [Fact]
        public void AlterTable_change_storage_parameters()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    OldTable = new Annotatable
                    {
                        [NpgsqlAnnotationNames.StorageParameterPrefix + "fillfactor"] = 70,
                        [NpgsqlAnnotationNames.StorageParameterPrefix + "user_catalog_table"] = true,
                        [NpgsqlAnnotationNames.StorageParameterPrefix + "parallel_workers"] = 8
                    },
                    // Add parameter
                    [NpgsqlAnnotationNames.StorageParameterPrefix + "autovacuum_enabled"] = true,
                    // Change parameter
                    [NpgsqlAnnotationNames.StorageParameterPrefix + "fillfactor"] = 80,
                    // Drop parameter user_catalog
                    // Leave parameter unchanged
                    [NpgsqlAnnotationNames.StorageParameterPrefix + "parallel_workers"] = 8
                });

            Assert.Equal(
                "ALTER TABLE dbo.\"People\" SET (autovacuum_enabled=true, fillfactor=80);" + EOL +
                "ALTER TABLE dbo.\"People\" RESET (user_catalog_table);" + EOL,
                Sql);
        }

        #endregion

        #region System columns

        [Fact]
        public void CreateTableOperation_with_system_column()
        {
            Generate(new CreateTableOperation
            {
                Name = "foo",
                Schema = "public",
                Columns = {
                    new AddColumnOperation {
                        Name = "id",
                        Table = "foo",
                        ClrType = typeof(int),
                        IsNullable = false
                    },
                    new AddColumnOperation {
                        Name = "xmin",
                        Table = "foo",
                        ClrType = typeof(uint),
                        IsNullable = false
                    }
                },
                PrimaryKey = new AddPrimaryKeyOperation
                {
                    Columns = new[] { "id" }
                }
            });

            Assert.Equal(
                "CREATE TABLE public.foo (" + EOL +
                "    id integer NOT NULL," + EOL +
                "    PRIMARY KEY (id)" + EOL +
                ");" + EOL,
                Sql);
        }

        [Fact]
        public void AddColumnOperation_with_system_column()
        {
            Generate(new AddColumnOperation
            {
                Table = "foo",
                Schema = "public",
                Name = "xmin"
            });

            Assert.Empty(Sql);
        }

        [Fact]
        public void DropColumnOperation_with_system_column()
        {
            Generate(new DropColumnOperation
            {
                Table = "foo",
                Schema = "public",
                Name = "xmin"
            });

            Assert.Empty(Sql);
        }

        [Fact]
        public void AlterColumnOperation_with_system_column()
        {
            Generate(new AlterColumnOperation
            {
                Table = "foo",
                Schema = "public",
                Name = "xmin",
                ClrType = typeof(int),
                ColumnType = "int",
                IsNullable = false,
                DefaultValue = 7
            });

            Assert.Empty(Sql);
        }

        #endregion

        #region PostgreSQL comments

        [Fact]
        public void CreateTableOperation_with_comment()
        {
            Generate(
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
                            ClrType = typeof(int),
                            IsNullable = false
                        },
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Id" }
                    },
                    [NpgsqlAnnotationNames.Comment] = "Some comment",
                });

            Assert.Equal(
                "CREATE TABLE dbo.\"People\" (" + EOL +
                "    \"Id\" integer NOT NULL," + EOL +
                "    PRIMARY KEY (\"Id\")" + EOL +
                ");" + EOL +
                "COMMENT ON TABLE dbo.\"People\" IS 'Some comment';" + EOL,
                Sql);
        }

        [Fact]
        public void CreateTableOperation_with_comment_on_column()
        {
            Generate(
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
                            ClrType = typeof(int),
                            IsNullable = false,
                            [NpgsqlAnnotationNames.Comment] = "Some comment",
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Id" }
                    }
                });

            Assert.Equal(
                "CREATE TABLE dbo.\"People\" (" + EOL +
                "    \"Id\" integer NOT NULL," + EOL +
                "    PRIMARY KEY (\"Id\")" + EOL +
                ");" + EOL +
                "COMMENT ON COLUMN dbo.\"People\".\"Id\" IS 'Some comment';" + EOL,
                Sql);
        }

        [Fact]
        public void AlterTable_change_comment()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    OldTable = new Annotatable { [NpgsqlAnnotationNames.Comment] = "Old comment" },
                    [NpgsqlAnnotationNames.Comment] = "New comment"
                });

            Assert.Equal(
                "COMMENT ON TABLE dbo.\"People\" IS 'New comment';" + EOL,
                Sql);
        }

        [Fact]
        public void AlterTable_remove_comment()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    OldTable = new Annotatable { [NpgsqlAnnotationNames.Comment] = "New comment" }
                });
            Assert.Equal(
                "COMMENT ON TABLE dbo.\"People\" IS NULL;" + EOL,
                Sql);
        }

        [Fact]
        public void AddColumnOperation_with_comment()
        {
            Generate(new AddColumnOperation
            {
                Schema = "dbo",
                Table = "People",
                Name = "foo",
                ClrType = typeof(int),
                ColumnType = "int",
                IsNullable = false,
                [NpgsqlAnnotationNames.Comment] = "Some comment",
            });

            Assert.Equal(
                "ALTER TABLE dbo.\"People\" ADD foo int NOT NULL;" + EOL +
                "COMMENT ON COLUMN dbo.\"People\".foo IS 'Some comment';" + EOL,
                Sql);
        }

        [Fact]
        public void AlterColumn_change_comment()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "LuckyNumber",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false,
                    DefaultValue = 7,
                    OldColumn = new ColumnOperation { [NpgsqlAnnotationNames.Comment] = "Old comment" },
                    [NpgsqlAnnotationNames.Comment] = "New comment"
                });

            Assert.Equal(
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" TYPE int;" + EOL +
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET NOT NULL;" + EOL +
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET DEFAULT 7;" + EOL +
                "COMMENT ON COLUMN dbo.\"People\".\"LuckyNumber\" IS 'New comment'",
                Sql);
        }

        [Fact]
        public void AlterColumn_remove_comment()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Schema = "dbo",
                    Name = "LuckyNumber",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false,
                    DefaultValue = 7,
                    OldColumn = new ColumnOperation { [NpgsqlAnnotationNames.Comment] = "Old comment" }
                });

            Assert.Equal(
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" TYPE int;" + EOL +
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET NOT NULL;" + EOL +
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET DEFAULT 7;" + EOL +
                "COMMENT ON COLUMN dbo.\"People\".\"LuckyNumber\" IS NULL",
                Sql);
        }

        #endregion

        #region CockroachDB interleave-in-parent

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

            Assert.Equal(
                "CREATE TABLE dbo.\"People\" (" + EOL +
                "    \"Id\" integer NOT NULL," + EOL +
                "    PRIMARY KEY (\"Id\")" + EOL +
                ")" + EOL +
                "INTERLEAVE IN PARENT my_schema.my_parent (col_a, col_b);" + EOL,
                Sql);
        }

        #endregion CockroachDB interleave-in-parent

        #region Sequence data types

        [Fact]
        public void CreateSequenceOperation_with_data_type_smallint()
        {
            Generate(
                new CreateSequenceOperation
                {
                    Name = "short_sequence",
                    Schema = "public",
                    ClrType = typeof(short)
                });

            Assert.StartsWith(
                "CREATE SEQUENCE public.short_sequence AS smallint",
                Sql);
        }

        #endregion Sequence data types

        public NpgsqlMigrationSqlGeneratorTest()
            : base(NpgsqlTestHelpers.Instance)
        {
        }
    }
}
