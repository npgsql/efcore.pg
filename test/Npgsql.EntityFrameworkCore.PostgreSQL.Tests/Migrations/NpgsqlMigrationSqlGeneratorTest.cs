// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Tests.Migrations
{
    public class MigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        protected override IMigrationsSqlGenerator SqlGenerator
        {
            get
            {
                var typeMapper = new NpgsqlTypeMapper();

                return new NpgsqlMigrationsSqlGenerator(
                    new RelationalCommandBuilderFactory(
                        new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                        new DiagnosticListener("Fake"),
                        typeMapper),
                    new NpgsqlSqlGenerationHelper(),
                    typeMapper,
                    new NpgsqlAnnotationProvider());
            }
        }

        public override void AddColumnOperation_with_defaultValue()
        {
            base.AddColumnOperation_with_defaultValue();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" ADD \"Name\" varchar(30) NOT NULL DEFAULT 'John Doe';" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_defaultValueSql()
        {
            base.AddColumnOperation_with_defaultValueSql();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Birthday\" date DEFAULT (CURRENT_TIMESTAMP);" + EOL,
                Sql);
        }

        [Fact]
        public override void AddColumnOperation_with_computed_column_SQL()
        {
            base.AddColumnOperation_with_computed_column_SQL();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Birthday\" date;" + EOL,
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
                @"ALTER TABLE ""Person"" ADD ""Name"" varchar(30);" + EOL,
                Sql);
        }

        public override void AddForeignKeyOperation_with_name()
        {
            base.AddForeignKeyOperation_with_name();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" ADD CONSTRAINT \"FK_People_Companies\" FOREIGN KEY (\"EmployerId1\", \"EmployerId2\") REFERENCES \"hr\".\"Companies\" (\"Id1\", \"Id2\") ON DELETE CASCADE;" + EOL,
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
                "ALTER TABLE \"dbo\".\"People\" ADD CONSTRAINT \"PK_People\" PRIMARY KEY (\"Id1\", \"Id2\");" + EOL,
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
                "ALTER TABLE \"dbo\".\"People\" ADD CONSTRAINT \"AK_People_DriverLicense\" UNIQUE (\"DriverLicense_State\", \"DriverLicense_Number\");" + EOL,
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
                "ALTER SEQUENCE \"dbo\".\"DefaultSequence\" INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;" + EOL,
                Sql);
        }

        public override void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            base.AlterSequenceOperation_without_minValue_and_maxValue();

            Assert.Equal(
                "ALTER SEQUENCE \"DefaultSequence\" INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;" + EOL,
                Sql);
        }

        public override void CreateIndexOperation_unique()
        {
            base.CreateIndexOperation_unique();

            Assert.Equal(
                "CREATE UNIQUE INDEX \"IX_People_Name\" ON \"dbo\".\"People\" (\"FirstName\", \"LastName\");" + EOL,
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

        public override void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            base.CreateSequenceOperation_with_minValue_and_maxValue();

            Assert.Equal(
                "CREATE SEQUENCE \"dbo\".\"DefaultSequence\" START WITH 3 INCREMENT BY 1 MINVALUE 2 MAXVALUE 816 CYCLE;" + EOL,
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
                "CREATE SEQUENCE \"DefaultSequence\" START WITH 3 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;" + EOL,
                Sql);
        }

        public override void CreateTableOperation()
        {
            base.CreateTableOperation();

            Assert.Equal(
                "CREATE TABLE \"dbo\".\"People\" (" + EOL +
                "    \"Id\" int4 NOT NULL," + EOL +
                "    \"EmployerId\" int4," + EOL +
                "    \"SSN\" char(11)," + EOL +
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
                "ALTER TABLE \"dbo\".\"People\" DROP COLUMN \"LuckyNumber\";" + EOL,
                Sql);
        }

        public override void DropForeignKeyOperation()
        {
            base.DropForeignKeyOperation();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" DROP CONSTRAINT \"FK_People_Companies\";" + EOL,
                Sql);
        }

        public override void DropPrimaryKeyOperation()
        {
            base.DropPrimaryKeyOperation();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" DROP CONSTRAINT \"PK_People\";" + EOL,
                Sql);
        }

        public override void DropSequenceOperation()
        {
            base.DropSequenceOperation();

            Assert.Equal(
                "DROP SEQUENCE \"dbo\".\"DefaultSequence\";" + EOL,
                Sql);
        }

        public override void DropTableOperation()
        {
            base.DropTableOperation();

            Assert.Equal(
                "DROP TABLE \"dbo\".\"People\";" + EOL,
                Sql);
        }

        public override void DropUniqueConstraintOperation()
        {
            base.DropUniqueConstraintOperation();

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" DROP CONSTRAINT \"AK_People_SSN\";" + EOL,
                Sql);
        }

        public override void SqlOperation()
        {
            base.SqlOperation();

            Assert.Equal(
                "-- I <3 DDL;" + EOL,
                Sql);
        }

        #region AlterColumn

        public override void AlterColumnOperation()
        {
            base.AlterColumnOperation();

            Assert.Equal(
                @"ALTER TABLE ""dbo"".""People"" ALTER COLUMN ""LuckyNumber"" TYPE int;" + EOL +
                @"ALTER TABLE ""dbo"".""People"" ALTER COLUMN ""LuckyNumber"" SET NOT NULL;" + EOL +
                @"ALTER TABLE ""dbo"".""People"" ALTER COLUMN ""LuckyNumber"" SET DEFAULT 7;" + EOL,
            Sql);
        }

        public override void AlterColumnOperation_without_column_type()
        {
            base.AlterColumnOperation_without_column_type();

            Assert.Equal(
                @"ALTER TABLE ""People"" ALTER COLUMN ""LuckyNumber"" TYPE int4;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""LuckyNumber"" SET NOT NULL;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""LuckyNumber"" DROP DEFAULT;" + EOL,
            Sql);
        }

        [Fact]
        public void AlterColumnOperation_to_serial()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "IntKey",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false,
                    [NpgsqlFullAnnotationNames.Instance.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
                });

            Assert.Equal(
                @"CREATE SEQUENCE ""People_IntKey_seq"" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""IntKey"" TYPE int;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""IntKey"" SET NOT NULL;" + EOL +
                @"ALTER TABLE ""People"" ALTER COLUMN ""IntKey"" SET DEFAULT (nextval('""People_IntKey_seq""'));" + EOL +
                @"ALTER SEQUENCE ""People_IntKey_seq"" OWNED BY ""People"".""IntKey""",
            Sql);
        }

        #endregion

        #region Npgsql-specific

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
                "CREATE INDEX \"IX_People_Name\" ON \"dbo\".\"People\" USING gin (\"FirstName\");" + EOL,
                Sql);
        }

        [Fact]
        public void EnsurePostgresExtension()
        {
            var op = new AlterDatabaseOperation();
            PostgresExtension.GetOrAddPostgresExtension(op, "hstore");
            Generate(op);

            Assert.Equal(
                @"CREATE EXTENSION IF NOT EXISTS ""hstore"";" + EOL,
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
                @"CREATE EXTENSION IF NOT EXISTS ""hstore"" SCHEMA ""myschema"";" + EOL,
                Sql);
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
                [NpgsqlFullAnnotationNames.Instance.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
            });

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"foo\" serial NOT NULL;" + EOL,
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
                    [NpgsqlFullAnnotationNames.Instance.ValueGeneratedOnAdd] = true
                }));
        }
#pragma warning restore 618

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
                    [NpgsqlFullAnnotationNames.Instance.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.SerialColumn
                });

            Assert.Equal(
                @"ALTER TABLE ""People"" ADD ""foo"" serial NOT NULL DEFAULT 0;" + EOL,
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
                "ALTER INDEX \"myschema\".\"x\" RENAME TO \"y\";" + EOL,
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
                    [NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix + "fillfactor"] = 70,
                    [NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix + "user_catalog_table"] = true,
                    ["some_bogus_name"] = 0
                });

            Assert.Equal(
                "CREATE TABLE \"dbo\".\"People\" (" + EOL +
                "    \"Id\" int4 NOT NULL," + EOL +
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
                    Name="People",
                    Schema="dbo",
                    OldTable = new Annotatable
                    {
                        [NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix + "fillfactor"] = 70,
                        [NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix + "user_catalog_table"] = true,
                        [NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix + "parallel_workers"] = 8
                    },
                    // Add parameter
                    [NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix + "autovacuum_enabled"] = true,
                    // Change parameter
                    [NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix + "fillfactor"] = 80,
                    // Drop parameter user_catalog
                    // Leave parameter unchanged
                    [NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix + "parallel_workers"] = 8
                });

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"People\" SET (autovacuum_enabled=true, fillfactor=80);" + EOL +
                "ALTER TABLE \"dbo\".\"People\" RESET (user_catalog_table);" + EOL,
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
                "CREATE TABLE \"public\".\"foo\" (" + EOL +
                "    \"id\" int4 NOT NULL," + EOL +
                "    PRIMARY KEY (\"id\")" + EOL +
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
    }
}
