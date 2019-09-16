using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
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

            AssertSql(
                @"ALTER TABLE dbo.""People"" ADD ""Name"" varchar(30) NOT NULL DEFAULT 'John Doe';
");
        }

        public override void AddColumnOperation_with_defaultValueSql()
        {
            base.AddColumnOperation_with_defaultValueSql();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Birthday"" date NULL DEFAULT (CURRENT_TIMESTAMP);
");
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_computedSql()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "FullName",
                    ClrType = typeof(string),
                    ComputedColumnSql = @"""FirstName"" || ' ' || ""LastName"""
                });

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""FullName"" text GENERATED ALWAYS AS (""FirstName"" || ' ' || ""LastName"") STORED;
");
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Alias"" text NOT NULL;
");
        }

        public override void AddColumnOperation_with_maxLength()
        {
            base.AddColumnOperation_with_maxLength();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" character varying(30) NULL;
");
        }

        [Fact]
        public void AddColumnOperation_with_huge_varchar()
        {
            // PostgreSQL doesn't allow varchar(x) with x > 10485760, so we map this to text.
            // See #342 and https://www.postgresql.org/message-id/15790.1291824247%40sss.pgh.pa.us
            Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<string>("Name").HasMaxLength(10485761),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 10485761,
                    IsNullable = true
                });

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" text NULL;
");
        }

        public override void AddForeignKeyOperation_with_name()
        {
            base.AddForeignKeyOperation_with_name();

            AssertSql(
                @"ALTER TABLE dbo.""People"" ADD CONSTRAINT ""FK_People_Companies"" FOREIGN KEY (""EmployerId1"", ""EmployerId2"") REFERENCES hr.""Companies"" (""Id1"", ""Id2"") ON DELETE CASCADE;
");
        }

        public override void AddForeignKeyOperation_without_name()
        {
            base.AddForeignKeyOperation_without_name();

            AssertSql(
                @"ALTER TABLE ""People"" ADD FOREIGN KEY (""SpouseId"") REFERENCES ""People"" (""Id"");
");
        }

        public override void AddPrimaryKeyOperation_with_name()
        {
            base.AddPrimaryKeyOperation_with_name();

            AssertSql(
                @"ALTER TABLE dbo.""People"" ADD CONSTRAINT ""PK_People"" PRIMARY KEY (""Id1"", ""Id2"");
");
        }

        public override void AddPrimaryKeyOperation_without_name()
        {
            base.AddPrimaryKeyOperation_without_name();

            AssertSql(
                @"ALTER TABLE ""People"" ADD PRIMARY KEY (""Id"");
");
        }

        public override void AddUniqueConstraintOperation_with_name()
        {
            base.AddUniqueConstraintOperation_with_name();

            AssertSql(
                @"ALTER TABLE dbo.""People"" ADD CONSTRAINT ""AK_People_DriverLicense"" UNIQUE (""DriverLicense_State"", ""DriverLicense_Number"");
");
        }

        public override void AddUniqueConstraintOperation_without_name()
        {
            base.AddUniqueConstraintOperation_without_name();

            AssertSql(
                @"ALTER TABLE ""People"" ADD UNIQUE (""SSN"");
");
        }

        public override void AlterSequenceOperation_with_minValue_and_maxValue()
        {
            base.AlterSequenceOperation_with_minValue_and_maxValue();

            AssertSql(
                @"ALTER SEQUENCE dbo.""EntityFrameworkHiLoSequence"" INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;
");
        }

        public override void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            base.AlterSequenceOperation_without_minValue_and_maxValue();

            AssertSql(
                @"ALTER SEQUENCE ""EntityFrameworkHiLoSequence"" INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
");
        }

        public override void CreateIndexOperation_unique()
        {
            base.CreateIndexOperation_unique();

            AssertSql(
                @"CREATE UNIQUE INDEX ""IX_People_Name"" ON dbo.""People"" (""FirstName"", ""LastName"");
");
        }

        public override void CreateIndexOperation_nonunique()
        {
            base.CreateIndexOperation_nonunique();

            AssertSql(
                @"CREATE INDEX ""IX_People_Name"" ON ""People"" (""Name"");
");
        }

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

        [Fact]
        public override void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_with_minValue_and_maxValue();

            AssertSql(
                @"CREATE SEQUENCE dbo.""EntityFrameworkHiLoSequence"" START WITH 3 INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;
");
        }

        [Fact]
        public override void CreateSequenceOperation_with_minValue_and_maxValue_not_long()
        {
            base.CreateSequenceOperation_with_minValue_and_maxValue_not_long();

            AssertSql(
                @"CREATE SEQUENCE dbo.""EntityFrameworkHiLoSequence"" AS integer START WITH 3 INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;
");

            using (TestHelpers.WithPostgresVersion(new Version(9, 5)))
            {
                base.CreateSequenceOperation_with_minValue_and_maxValue_not_long();
                AssertSql(
                    @"CREATE SEQUENCE dbo.""EntityFrameworkHiLoSequence"" START WITH 3 INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;
");
            }
        }

        [Fact]
        public override void CreateSequenceOperation_without_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_without_minValue_and_maxValue();

            AssertSql(
                @"CREATE SEQUENCE ""EntityFrameworkHiLoSequence"" START WITH 3 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
");
        }

        public override void CreateTableOperation()
        {
            base.CreateTableOperation();

            AssertSql(
                @"CREATE TABLE dbo.""People"" (
    ""Id"" integer NOT NULL,
    ""EmployerId"" integer NULL,
    ""SSN"" char(11) NULL,
    PRIMARY KEY (""Id""),
    UNIQUE (""SSN""),
    CHECK (SSN > 0),
    FOREIGN KEY (""EmployerId"") REFERENCES ""Companies"" (""Id"")
);
COMMENT ON TABLE dbo.""People"" IS 'Table comment';
COMMENT ON COLUMN dbo.""People"".""EmployerId"" IS 'Employer ID comment';
");
        }

        public override void DropColumnOperation()
        {
            base.DropColumnOperation();

            AssertSql(
                @"ALTER TABLE dbo.""People"" DROP COLUMN ""LuckyNumber"";
");
        }

        public override void DropForeignKeyOperation()
        {
            base.DropForeignKeyOperation();

            AssertSql(
                @"ALTER TABLE dbo.""People"" DROP CONSTRAINT ""FK_People_Companies"";
");
        }

        public override void DropPrimaryKeyOperation()
        {
            base.DropPrimaryKeyOperation();

            AssertSql(
                @"ALTER TABLE dbo.""People"" DROP CONSTRAINT ""PK_People"";
");
        }

        public override void DropSequenceOperation()
        {
            base.DropSequenceOperation();

            AssertSql(
                @"DROP SEQUENCE dbo.""EntityFrameworkHiLoSequence"";
");
        }

        public override void DropTableOperation()
        {
            base.DropTableOperation();

            AssertSql(
                @"DROP TABLE dbo.""People"";
");
        }

        public override void DropUniqueConstraintOperation()
        {
            base.DropUniqueConstraintOperation();

            AssertSql(
                @"ALTER TABLE dbo.""People"" DROP CONSTRAINT ""AK_People_SSN"";
");
        }

        #region AlterColumn

        public override void AlterColumnOperation()
        {
            base.AlterColumnOperation();

            AssertSql(
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" TYPE int;
ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET NOT NULL;
ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET DEFAULT 7;
");
        }

        public override void AlterColumnOperation_without_column_type()
        {
            base.AlterColumnOperation_without_column_type();

            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""LuckyNumber"" TYPE integer;
ALTER TABLE ""People"" ALTER COLUMN ""LuckyNumber"" SET NOT NULL;
ALTER TABLE ""People"" ALTER COLUMN ""LuckyNumber"" DROP DEFAULT;
");
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

            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Name"" TYPE character varying(30);
ALTER TABLE ""People"" ALTER COLUMN ""Name"" SET NOT NULL;
ALTER TABLE ""People"" ALTER COLUMN ""Name"" DROP DEFAULT;
");
        }

        #endregion

        #region Value generation add

        [Fact]
        public void CreateTableOperation_with_value_generation()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "People",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "IdentityByDefault",
                            Table = "People",
                            ClrType = typeof(int),
                            [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        },
                        new AddColumnOperation
                        {
                            Name = "IdentityAlways",
                            Table = "People",
                            ClrType = typeof(int),
                            [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                        },
                        new AddColumnOperation
                        {
                            Name = "IdentityAlwaysWithSequenceOptions",
                            Table = "People",
                            ClrType = typeof(int),
                            [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityAlwaysColumn,
                            [NpgsqlAnnotationNames.IdentityOptions] = new IdentitySequenceOptionsData
                            {
                                StartValue = 10,
                                IncrementBy = 2,
                                MaxValue = 2000
                            }.Serialize()
                        },
                        new AddColumnOperation
                        {
                            Name = "Serial",
                            Table = "People",
                            ClrType = typeof(int),
                            [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "IdentityByDefault" }
                    }
                });

            AssertSql(
                @"CREATE TABLE ""People"" (
    ""IdentityByDefault"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    ""IdentityAlways"" integer NOT NULL GENERATED ALWAYS AS IDENTITY,
    ""IdentityAlwaysWithSequenceOptions"" integer NOT NULL GENERATED ALWAYS AS IDENTITY (START WITH 10 INCREMENT BY 2 MAXVALUE 2000),
    ""Serial"" serial NOT NULL,
    PRIMARY KEY (""IdentityByDefault"")
);
");
        }

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

            AssertSql(
                @"ALTER TABLE ""People"" ADD foo serial NOT NULL;
");
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

            AssertSql(
                @"ALTER TABLE ""People"" ADD foo serial NOT NULL DEFAULT 0;
");
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

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Id"" integer NOT NULL GENERATED ALWAYS AS IDENTITY;
");
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

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Id"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY;
");
        }

        [Fact]
        public void AddColumnOperation_with_identity_sequence_options_all()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn,
                    [NpgsqlAnnotationNames.IdentityOptions] = new IdentitySequenceOptionsData
                    {
                        StartValue = 5,
                        MinValue = 3,
                        MaxValue = 2000,
                        IncrementBy = 2,
                        IsCyclic = true,
                        NumbersToCache = 10
                    }.Serialize()
                });

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Id"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY (START WITH 5 INCREMENT BY 2 MINVALUE 3 MAXVALUE 2000 CYCLE CACHE 10);
");
        }

        [Fact]
        public void AddColumnOperation_with_identity_sequence_options_some()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn,
                    [NpgsqlAnnotationNames.IdentityOptions] = new IdentitySequenceOptionsData
                    {
                        MaxValue = 2000,
                        NumbersToCache = 10
                    }.Serialize()
                });

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Id"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY (MAXVALUE 2000 CACHE 10);
");
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

            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE integer;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" ADD GENERATED ALWAYS AS IDENTITY;
");
        }

        [Fact]
        public void AlterColumnOperation_int_to_serial_public()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "IntKey",
                    ClrType = typeof(int),
                    IsNullable = false,
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
                });

            // Note that GO here is just a delimiter introduced in the tests to indicate a batch boundary
            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""IntKey"" TYPE integer;
ALTER TABLE ""People"" ALTER COLUMN ""IntKey"" SET NOT NULL;
CREATE SEQUENCE ""People_IntKey_seq"" AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
GO

ALTER TABLE ""People"" ALTER COLUMN ""IntKey"" SET DEFAULT (nextval('""People_IntKey_seq""'));
ALTER SEQUENCE ""People_IntKey_seq"" OWNED BY ""People"".""IntKey"";
");
        }

        [Fact]
        public void AlterColumnOperation_int_to_serial_non_public()
        {
            Generate(
                new AlterColumnOperation
                {
                    Schema = "dbo",
                    Table = "People",
                    Name = "IntKey",
                    ClrType = typeof(int),
                    IsNullable = false,
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
                });

            // Note that GO here is just a delimiter introduced in the tests to indicate a batch boundary
            AssertSql(
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""IntKey"" TYPE integer;
ALTER TABLE dbo.""People"" ALTER COLUMN ""IntKey"" SET NOT NULL;
CREATE SEQUENCE dbo.""People_IntKey_seq"" AS integer START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
GO

ALTER TABLE dbo.""People"" ALTER COLUMN ""IntKey"" SET DEFAULT (nextval('dbo.""People_IntKey_seq""'));
ALTER SEQUENCE dbo.""People_IntKey_seq"" OWNED BY dbo.""People"".""IntKey"";
");
        }

        [Fact]
        public void AlterColumnOperation_long_to_bigserial()
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

            // Note that GO here is just a delimiter introduced in the tests to indicate a batch boundary
            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""LongKey"" TYPE bigint;
ALTER TABLE ""People"" ALTER COLUMN ""LongKey"" SET NOT NULL;
CREATE SEQUENCE ""People_LongKey_seq"" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
GO

ALTER TABLE ""People"" ALTER COLUMN ""LongKey"" SET DEFAULT (nextval('""People_LongKey_seq""'));
ALTER SEQUENCE ""People_LongKey_seq"" OWNED BY ""People"".""LongKey"";
");
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

            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE integer;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET GENERATED ALWAYS;
");
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

            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE integer;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;
ALTER SEQUENCE ""People_Id_seq"" RENAME TO ""People_Id_old_seq"";
ALTER TABLE ""People"" ALTER COLUMN ""Id"" DROP DEFAULT;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" ADD GENERATED ALWAYS AS IDENTITY;
SELECT * FROM setval('""People_Id_seq""', nextval('""People_Id_old_seq""'), false);
DROP SEQUENCE ""People_Id_old_seq"";
");
        }

        [Fact]
        public void AlterColumnOperation_serial_change_type()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(long),
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(int),
                        [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
                    },
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
                });

            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE bigint;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;
");
        }

        [ConditionalFact]
        public void AlterColumnOperation_computed()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "FullName",
                    ClrType = typeof(string),
                    ComputedColumnSql = @"""FirstName"" || ' ' || ""LastName"""
                });

            AssertSql(@"ALTER TABLE ""People"" DROP COLUMN ""FullName"";
GO

ALTER TABLE ""People"" ADD ""FullName"" text GENERATED ALWAYS AS (""FirstName"" || ' ' || ""LastName"") STORED;
");
        }

        [Fact]
        public void AlterColumnOperation_identity_restart()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn,
                    [NpgsqlAnnotationNames.IdentityOptions] = new IdentitySequenceOptionsData
                    {
                        StartValue = 10,
                    }.Serialize(),
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(int),
                        [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn,
                        [NpgsqlAnnotationNames.IdentityOptions] = new IdentitySequenceOptionsData
                        {
                            StartValue = 1
                        }.Serialize()
                    }
                });

            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE integer;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" RESTART WITH 10;
");
        }

        [Fact]
        public void AlterColumnOperation_alter_identity_sequence_options()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn,
                    [NpgsqlAnnotationNames.IdentityOptions] = new IdentitySequenceOptionsData
                    {
                        IncrementBy = 2,
                        MaxValue = 1000,
                        IsCyclic = false
                    }.Serialize(),
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(int),
                        [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn,
                        [NpgsqlAnnotationNames.IdentityOptions] = new IdentitySequenceOptionsData
                        {
                            IncrementBy = 1,
                            MaxValue = 1000,
                            IsCyclic = true
                        }.Serialize()
                    }
                });

            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE integer;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET INCREMENT BY 2;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NO CYCLE;
");
        }

        [Fact]
        public void AlterColumnOperation_to_identity_with_sequence_options()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn,
                    [NpgsqlAnnotationNames.IdentityOptions] = new IdentitySequenceOptionsData
                    {
                        StartValue = 5,
                        IsCyclic = true,
                        NumbersToCache = 5
                    }.Serialize(),
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(int)
                    }
                });

            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE integer;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" ADD GENERATED BY DEFAULT AS IDENTITY (START WITH 5 CYCLE CACHE 5);
");
        }

        [Fact]
        public void AlterColumnOperation_identity_remove_sequence_options()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn,
                    OldColumn = new ColumnOperation
                    {
                        ClrType = typeof(int),
                        [NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn,
                        [NpgsqlAnnotationNames.IdentityOptions] = new IdentitySequenceOptionsData
                        {
                            StartValue = 5,
                            IsCyclic = true,
                            NumbersToCache = 5
                        }.Serialize(),
                    }
                });

            AssertSql(
                @"ALTER TABLE ""People"" ALTER COLUMN ""Id"" TYPE integer;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NOT NULL;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" RESTART WITH 1;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET NO CYCLE;
ALTER TABLE ""People"" ALTER COLUMN ""Id"" SET CACHE 1;
");
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
                [NpgsqlAnnotationNames.IndexMethod] = "gin"
            });

            AssertSql(
                @"CREATE INDEX ""IX_People_Name"" ON dbo.""People"" USING gin (""FirstName"");
");
        }

        [Fact]
        public void CreateIndexOperation_operations()
        {
            Generate(new CreateIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                Columns = new[] { "FirstName", "LastName" },
                [NpgsqlAnnotationNames.IndexOperators] = new[] { "text_pattern_ops" }
            });

            AssertSql(
                @"CREATE INDEX ""IX_People_Name"" ON dbo.""People"" (""FirstName"" text_pattern_ops, ""LastName"");
");
        }

        [Fact]
        public void CreateIndexOperation_includes()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    [NpgsqlAnnotationNames.IndexInclude] = new[] { "FirstName", "LastName" }
                });

            AssertSql(
                @"CREATE INDEX ""IX_People_Name"" ON ""People"" (""Name"") INCLUDE (""FirstName"", ""LastName"");
");
        }

        [Fact]
        public void CreateIndexOperation_schema_qualified_operations()
        {
            Generate(new CreateIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                Columns = new[] { "FirstName" },
                [NpgsqlAnnotationNames.IndexOperators] = new[] { "myschema.TextOperation" }
            });

            AssertSql(
                @"CREATE INDEX ""IX_People_Name"" ON dbo.""People"" (""FirstName"" myschema.""TextOperation"");
");
        }

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
                @"CREATE INDEX ""IX_People_Name"" ON dbo.""People"" (""FirstName"", ""LastName"" COLLATE de_DE);
");
        }

        [Fact]
        public void CreateIndexOperation_sort_order()
        {
            Generate(new CreateIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                Columns = new[] { "FirstName", "LastName" },
                [NpgsqlAnnotationNames.IndexSortOrder] = new[] { SortOrder.Descending, SortOrder.Ascending }
            });

            AssertSql(
                @"CREATE INDEX ""IX_People_Name"" ON dbo.""People"" (""FirstName"" DESC, ""LastName"");
");
        }

        [Fact]
        public void CreateIndexOperation_nulls_first()
        {
            Generate(new CreateIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                Columns = new[] { "FirstName", "MiddleName", "LastName" },
                [NpgsqlAnnotationNames.IndexNullSortOrder] = new[] { NullSortOrder.NullsFirst, NullSortOrder.Unspecified, NullSortOrder.NullsLast }
            });

            AssertSql(
                @"CREATE INDEX ""IX_People_Name"" ON dbo.""People"" (""FirstName"" NULLS FIRST, ""MiddleName"", ""LastName"" NULLS LAST);
");
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

            AssertSql(
                @"ALTER INDEX myschema.x RENAME TO y;
");
        }

        [Fact]
        public void CreateIndexOperation_concurrently()
        {
            Generate(new CreateIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                Columns = new[] { "FirstName" },
                [NpgsqlAnnotationNames.CreatedConcurrently] = true
            });

            Assert.Equal(
                "CREATE INDEX CONCURRENTLY \"IX_People_Name\" ON dbo.\"People\" (\"FirstName\");" + EOL,
                Sql);
        }

        #endregion Indexes

        #region PostgreSQL extensions

        [Fact]
        public void EnsurePostgresExtension()
        {
            var op = new AlterDatabaseOperation();
            op.GetOrAddPostgresExtension(null, "hstore", null);
            Generate(op);

            AssertSql(
                @"CREATE EXTENSION IF NOT EXISTS hstore;
");
        }

        [Fact]
        public void EnsurePostgresExtension_with_schema()
        {
            var op = new AlterDatabaseOperation();
            op.GetOrAddPostgresExtension("myschema", "hstore", null);
            Generate(op);

            AssertSql(
                @"CREATE EXTENSION IF NOT EXISTS hstore SCHEMA myschema;
");
        }

        #endregion PostgreSQL extensions

        #region Enums

        [Fact]
        public void CreatePostgresEnum()
        {
            var op = new AlterDatabaseOperation();
            PostgresEnum.GetOrAddPostgresEnum(op, "public", "my_enum", new[] { "value1", "value2" });
            Generate(op);

            AssertSql(@"CREATE TYPE public.my_enum AS ENUM ('value1', 'value2');
");
        }

        [Fact]
        public void CreatePostgresEnumWithSchema()
        {
            var op = new AlterDatabaseOperation();
            PostgresEnum.GetOrAddPostgresEnum(op, "some_schema", "my_enum", new[] { "value1", "value2" });
            Generate(op);

            AssertSql(
                @"CREATE SCHEMA IF NOT EXISTS some_schema;
GO

CREATE TYPE some_schema.my_enum AS ENUM ('value1', 'value2');
");
        }

        [Fact]
        public void DropPostgresEnum()
        {
            var op = new AlterDatabaseOperation();
            PostgresEnum.GetOrAddPostgresEnum(op.OldDatabase, "public", "my_enum", new[] { "value1", "value2" });
            Generate(op);

            AssertSql(@"DROP TYPE public.my_enum;
");
        }

        [Fact] // #979
        public void DoNotAlterPostgresEnum()
        {
            var op = new AlterDatabaseOperation();
            PostgresEnum.GetOrAddPostgresEnum(op,             "public", "my_enum", new[] { "value1", "value2" });
            PostgresEnum.GetOrAddPostgresEnum(op.OldDatabase, "public", "my_enum", new[] { "value1", "value2" });
            Generate(op);

            AssertSql("");
        }

        #endregion Enums

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

            AssertSql(
                @"CREATE TABLE dbo.""People"" (
    ""Id"" integer NOT NULL,
    PRIMARY KEY (""Id"")
)
WITH (fillfactor=70, user_catalog_table=true);
");
        }

        [Fact]
        public void AlterTable_change_storage_parameters()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    OldTable = new TableOperation
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

            AssertSql(
                @"ALTER TABLE dbo.""People"" SET (autovacuum_enabled=true, fillfactor=80);
ALTER TABLE dbo.""People"" RESET (user_catalog_table);
");
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

            AssertSql(
                @"CREATE TABLE public.foo (
    id integer NOT NULL,
    PRIMARY KEY (id)
);
");
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
                    Comment = "Some comment",
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
                });

            AssertSql(
                @"CREATE TABLE dbo.""People"" (
    ""Id"" integer NOT NULL,
    PRIMARY KEY (""Id"")
);
COMMENT ON TABLE dbo.""People"" IS 'Some comment';
");
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
                            Comment = "Some comment"
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Id" }
                    }
                });

            AssertSql(
                @"CREATE TABLE dbo.""People"" (
    ""Id"" integer NOT NULL,
    PRIMARY KEY (""Id"")
);
COMMENT ON COLUMN dbo.""People"".""Id"" IS 'Some comment';
");
        }

        [Fact]
        public void AlterTable_change_comment()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    Comment = "New comment",
                    OldTable = new TableOperation { Comment = "Old comment" }
                });

            AssertSql(
                @"COMMENT ON TABLE dbo.""People"" IS 'New comment';
");
        }

        [Fact]
        public void AlterTable_remove_comment()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    OldTable = new TableOperation { Comment = "New comment" }
                });
            AssertSql(
                @"COMMENT ON TABLE dbo.""People"" IS NULL;
");
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
                Comment = "Some comment"
            });

            AssertSql(
                @"ALTER TABLE dbo.""People"" ADD foo int NOT NULL;
COMMENT ON COLUMN dbo.""People"".foo IS 'Some comment';
");
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
                    Comment = "New comment",
                    OldColumn = new ColumnOperation { Comment = "Old comment" }
                });

            AssertSql(
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" TYPE int;
ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET NOT NULL;
ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET DEFAULT 7;
COMMENT ON COLUMN dbo.""People"".""LuckyNumber"" IS 'New comment';
");
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
                    OldColumn = new ColumnOperation { Comment = "Old comment" }
                });

            AssertSql(
                @"ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" TYPE int;
ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET NOT NULL;
ALTER TABLE dbo.""People"" ALTER COLUMN ""LuckyNumber"" SET DEFAULT 7;
COMMENT ON COLUMN dbo.""People"".""LuckyNumber"" IS NULL;
");
        }

        #endregion

        #region Unlogged Table

        [Fact]
        public void CreateTableOperation_with_unlogged()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    [NpgsqlAnnotationNames.UnloggedTable] = true
                });

            AssertSql(@"CREATE UNLOGGED TABLE dbo.""People"" (

);
");
        }

        [Fact]
        public void AlterTable_set_unlogged()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    OldTable = new TableOperation(),
                    [NpgsqlAnnotationNames.UnloggedTable] = true
                });

            AssertSql(@"ALTER TABLE dbo.""People"" SET UNLOGGED;
");
        }

        [Fact]
        public void AlterTable_set_logged()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    OldTable = new TableOperation { [NpgsqlAnnotationNames.UnloggedTable] = true },
                    [NpgsqlAnnotationNames.UnloggedTable] = false
                });

            AssertSql(@"ALTER TABLE dbo.""People"" SET LOGGED;
");
        }

        [Fact]
        public void AlterTable_remove_unlogged()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    OldTable = new TableOperation { [NpgsqlAnnotationNames.UnloggedTable] = true }
                });

            AssertSql(@"ALTER TABLE dbo.""People"" SET LOGGED;
");
        }

        [Fact]
        public void AlterTable_remove_not_unlogged_noop()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    OldTable = new TableOperation { [NpgsqlAnnotationNames.UnloggedTable] = false }
                });

            AssertSql("");
        }

        [Fact]
        public void AlterTable_set_not_unlogged_noop()
        {
            Generate(
                new AlterTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    [NpgsqlAnnotationNames.UnloggedTable] = false
                });

            AssertSql("");
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

            AssertSql(
                @"CREATE TABLE dbo.""People"" (
    ""Id"" integer NOT NULL,
    PRIMARY KEY (""Id"")
)
INTERLEAVE IN PARENT my_schema.my_parent (col_a, col_b);
");
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

            AssertSql(
                @"CREATE SEQUENCE public.short_sequence AS smallint START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
");

            using (TestHelpers.WithPostgresVersion(new Version(9, 5)))
            {
                Generate(
                    new CreateSequenceOperation {
                        Name = "short_sequence",
                        Schema = "public",
                        ClrType = typeof(short)
                    });

                AssertSql(
                    @"CREATE SEQUENCE public.short_sequence START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
");
            }
        }

        #endregion Sequence data types

        [Fact]
        public void StoreTypeNames()
        {
            Generate(new CreateTableOperation
                {
                    Name = "types",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "text",
                            Table = "types",
                            ClrType = typeof(string),
                            ColumnType = "text"
                        },
                        // #396
                        new AddColumnOperation
                        {
                            Name = "text_upper",
                            Table = "types",
                            ClrType = typeof(string),
                            ColumnType = "TEXT"
                        },
                        new AddColumnOperation
                        {
                            Name = "varchar",
                            Table = "types",
                            ClrType = typeof(string),
                            ColumnType = "varchar(3)"
                        },
                        // At least for now, it's the user's responsibility to quote store type name when needed,
                        // because it seems standard for people to specify either text or TEXT, and both should work.
                        new AddColumnOperation
                        {
                            Name = "SomeCamelCaseEnum",
                            Table = "types",
                            ClrType = typeof(string),
                            ColumnType = @"""SomeCamelCaseEnum"""
                        },
                    },
                });

            AssertSql(@"CREATE TABLE types (
    text text NOT NULL,
    text_upper TEXT NOT NULL,
    varchar varchar(3) NOT NULL,
    ""SomeCamelCaseEnum"" ""SomeCamelCaseEnum"" NOT NULL
);
");
        }  // yuval

        [Fact]
        public void FixedLength()
        {
            Generate(new CreateTableOperation
            {
                Name = "types",
                Columns =
                {
                    new AddColumnOperation
                    {
                        Name = "char",
                        Table = "types",
                        ClrType = typeof(string),
                        MaxLength = 30,
                        IsFixedLength = true
                    },
                    new AddColumnOperation
                    {
                        Name = "varchar",
                        Table = "types",
                        ClrType = typeof(string),
                        MaxLength = 30,
                        IsFixedLength = false
                    },
                    new AddColumnOperation
                    {
                        Name = "bit",
                        Table = "types",
                        ClrType = typeof(BitArray),
                        MaxLength = 30,
                        IsFixedLength = true
                    },
                    new AddColumnOperation
                    {
                        Name = "varbit",
                        Table = "types",
                        ClrType = typeof(BitArray),
                        MaxLength = 30,
                        IsFixedLength = false
                    }
                }
            });
            AssertSql(@"CREATE TABLE types (
    char character(30) NOT NULL,
    varchar character varying(30) NOT NULL,
    bit bit(30) NOT NULL,
    varbit bit varying(30) NOT NULL
);
");
        }

        protected new NpgsqlTestHelpers TestHelpers => (NpgsqlTestHelpers)base.TestHelpers;

        public NpgsqlMigrationSqlGeneratorTest()
            : base(NpgsqlTestHelpers.Instance)
        {
        }
    }
}
