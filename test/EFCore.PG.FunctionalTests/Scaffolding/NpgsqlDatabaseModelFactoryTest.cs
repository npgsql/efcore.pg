using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding
{
    public class NpgsqlDatabaseModelFactoryTest : IClassFixture<NpgsqlDatabaseModelFactoryTest.NpgsqlDatabaseModelFixture>
    {
        protected NpgsqlDatabaseModelFixture Fixture { get; }

        public NpgsqlDatabaseModelFactoryTest(NpgsqlDatabaseModelFixture fixture) => Fixture = fixture;

        #region Sequences

        [Fact]
        public void Create_sequences_with_facets()
        {
            Test(
                @"
CREATE SEQUENCE ""DefaultFacetsSequence"";

CREATE SEQUENCE db2.""CustomFacetsSequence""
    AS int
    START WITH 1
    INCREMENT BY 2
    MAXVALUE 8
    MINVALUE -3
    CYCLE;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var defaultSequence = dbModel.Sequences.First(ds => ds.Name == "DefaultFacetsSequence");
                        Assert.Equal("public", defaultSequence.Schema);
                        Assert.Equal("DefaultFacetsSequence", defaultSequence.Name);
                        Assert.Equal("bigint", defaultSequence.StoreType);
                        Assert.False(defaultSequence.IsCyclic);
                        Assert.Equal(1, defaultSequence.IncrementBy);
                        Assert.Null(defaultSequence.StartValue);
                        Assert.Null(defaultSequence.MinValue);
                        Assert.Null(defaultSequence.MaxValue);

                        var customSequence = dbModel.Sequences.First(ds => ds.Name == "CustomFacetsSequence");
                        Assert.Equal("db2", customSequence.Schema);
                        Assert.Equal("CustomFacetsSequence", customSequence.Name);
                        Assert.Equal("integer", customSequence.StoreType);
                        Assert.True(customSequence.IsCyclic);
                        Assert.Equal(2, customSequence.IncrementBy);
                        Assert.Equal(1, customSequence.StartValue);
                        Assert.Equal(-3, customSequence.MinValue);
                        Assert.Equal(8, customSequence.MaxValue);
                    },
                @"
DROP SEQUENCE ""DefaultFacetsSequence"";
DROP SEQUENCE db2.""CustomFacetsSequence""");
        }

        [Fact(Skip="Travis doesn't have PG10 which is required. Need to set up conditional fact.")]
        public void Sequence_min_max_start_values_are_null_if_default()
        {
            Test(
                @"
CREATE SEQUENCE ""SmallIntSequence"" AS smallint;
CREATE SEQUENCE ""IntSequence"" AS int;
CREATE SEQUENCE ""BigIntSequence"" AS bigint;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        Assert.All(
                            dbModel.Sequences,
                            s =>
                                {
                                    Assert.Null(s.StartValue);
                                    Assert.Null(s.MinValue);
                                    Assert.Null(s.MaxValue);
                                });
                    },
                @"
DROP SEQUENCE ""SmallIntSequence"";
DROP SEQUENCE ""IntSequence"";
DROP SEQUENCE ""BigIntSequence"";");
        }

        [Fact]
        public void Filter_sequences_based_on_schema()
        {
            Test(
                @"
CREATE SEQUENCE ""Sequence"";
CREATE SEQUENCE db2.""Sequence""",
                Enumerable.Empty<string>(),
                new[] { "db2" },
                dbModel =>
                    {
                        var sequence = Assert.Single(dbModel.Sequences);
                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("db2", sequence.Schema);
                        Assert.Equal("Sequence", sequence.Name);
                        Assert.Equal("bigint", sequence.StoreType);
                        Assert.False(sequence.IsCyclic);
                        Assert.Equal(1, sequence.IncrementBy);
                    },
                @"
DROP SEQUENCE ""Sequence"";
DROP SEQUENCE db2.""Sequence"";");
        }

        #endregion

        #region Model

        [Fact]
        public void Set_default_schema()
        {
            Test(
                "SELECT 1",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        Assert.Equal("public", dbModel.DefaultSchema);
                    },
                null);
        }

        [Fact]
        public void Create_tables()
        {
            Test(
                @"
CREATE TABLE ""Everest"" ( id int );
CREATE TABLE ""Denali"" ( id int );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        Assert.Collection(
                            dbModel.Tables.OrderBy(t => t.Name),
                            d =>
                                {
                                    Assert.Equal("public", d.Schema);
                                    Assert.Equal("Denali", d.Name);
                                },
                            e =>
                                {
                                    Assert.Equal("public", e.Schema);
                                    Assert.Equal("Everest", e.Name);
                                });
                    },
                @"
DROP TABLE ""Everest"";
DROP TABLE ""Denali"";");
        }

        #endregion

        #region FilteringSchemaTable

        [Fact]
        public void Filter_schemas()
        {
            Test(
                @"
CREATE TABLE db2.""K2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE ""Kilimanjaro"" ( Id int, B varchar, UNIQUE (B));",
                Enumerable.Empty<string>(),
                new[] { "db2" },
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);
                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("K2", table.Name);
                        Assert.Equal(2, table.Columns.Count);
                        Assert.Equal(1, table.UniqueConstraints.Count);
                        Assert.Empty(table.ForeignKeys);
                    },
                @"
DROP TABLE ""Kilimanjaro"";

DROP TABLE db2.""K2"";");
        }

        [Fact]
        public void Filter_tables()
        {
            Test(
                @"
CREATE TABLE ""K2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE ""Kilimanjaro"" ( Id int, B varchar, UNIQUE (B), FOREIGN KEY (B) REFERENCES ""K2"" (A) );",
                new[] { "K2" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);
                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("K2", table.Name);
                        Assert.Equal(2, table.Columns.Count);
                        Assert.Equal(1, table.UniqueConstraints.Count);
                        Assert.Empty(table.ForeignKeys);
                    },
                @"
DROP TABLE ""Kilimanjaro"";
DROP TABLE ""K2"";");
        }

        [Fact]
        public void Filter_tables_with_qualified_name()
        {
            Test(
                @"
CREATE TABLE ""K.2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE ""Kilimanjaro"" ( Id int, B varchar, UNIQUE (B) );",
                new[] { @"""K.2""" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);
                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("K.2", table.Name);
                        Assert.Equal(2, table.Columns.Count);
                        Assert.Equal(1, table.UniqueConstraints.Count);
                        Assert.Empty(table.ForeignKeys);
                    },
                @"
DROP TABLE ""Kilimanjaro"";
DROP TABLE ""K.2"";");
        }

        [Fact]
        public void Filter_tables_with_schema_qualified_name1()
        {
            Test(
                @"
CREATE TABLE public.""K2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE db2.""K2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE ""Kilimanjaro"" ( Id int, B varchar, UNIQUE (B) );",
                new[] { "public.K2" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);
                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("K2", table.Name);
                        Assert.Equal(2, table.Columns.Count);
                        Assert.Equal(1, table.UniqueConstraints.Count);
                        Assert.Empty(table.ForeignKeys);
                    },
                @"
DROP TABLE ""Kilimanjaro"";
DROP TABLE ""K2"";
DROP TABLE db2.""K2"";");
        }

        [Fact]
        public void Filter_tables_with_schema_qualified_name2()
        {
            Test(
                @"
CREATE TABLE ""K.2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE ""db.2"".""K.2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE ""db.2"".""Kilimanjaro"" ( Id int, B varchar, UNIQUE (B) );",
                new[] { @"""db.2"".""K.2""" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);
                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("K.2", table.Name);
                        Assert.Equal(2, table.Columns.Count);
                        Assert.Equal(1, table.UniqueConstraints.Count);
                        Assert.Empty(table.ForeignKeys);
                    },
                @"
DROP TABLE ""db.2"".""Kilimanjaro"";
DROP TABLE ""K.2"";
DROP TABLE ""db.2"".""K.2"";");
        }

        [Fact]
        public void Filter_tables_with_schema_qualified_name3()
        {
            Test(
                @"
CREATE TABLE ""K.2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE ""db2"".""K.2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE ""Kilimanjaro"" ( Id int, B varchar, UNIQUE (B) );",
                new[] { @"public.""K.2""" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);
                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("K.2", table.Name);
                        Assert.Equal(2, table.Columns.Count);
                        Assert.Equal(1, table.UniqueConstraints.Count);
                        Assert.Empty(table.ForeignKeys);
                    },
                @"
DROP TABLE ""Kilimanjaro"";
DROP TABLE ""K.2"";
DROP TABLE db2.""K.2"";");
        }

        [Fact]
        public void Filter_tables_with_schema_qualified_name4()
        {
            Test(
                @"
CREATE TABLE ""K2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE ""db.2"".""K2"" ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE ""db.2"".""Kilimanjaro"" ( Id int, B varchar, UNIQUE (B) );",
                new[] { @"""db.2"".K2" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);
                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("K2", table.Name);
                        Assert.Equal(2, table.Columns.Count);
                        Assert.Equal(1, table.UniqueConstraints.Count);
                        Assert.Empty(table.ForeignKeys);
                    },
                @"
DROP TABLE ""db.2"".""Kilimanjaro"";
DROP TABLE ""K2"";
DROP TABLE ""db.2"".""K2"";");
        }

        [Fact]
        public void Complex_filtering_validation()
        {
            Test(
                @"
CREATE SEQUENCE public.""Sequence"";
CREATE SEQUENCE ""db2"".""Sequence"";

CREATE TABLE ""db.2"".""QuotedTableName"" ( ""Id"" int PRIMARY KEY );
CREATE TABLE ""db.2"".""Table.With.Dot"" ( ""Id"" int PRIMARY KEY );
CREATE TABLE ""db.2"".""SimpleTableName"" ( ""Id"" int PRIMARY KEY );
CREATE TABLE ""db.2"".""JustTableName"" ( ""Id"" int PRIMARY KEY );

CREATE TABLE ""public"".""QuotedTableName"" ( ""Id"" int PRIMARY KEY );
CREATE TABLE ""public"".""Table.With.Dot"" ( ""Id"" int PRIMARY KEY );
CREATE TABLE ""public"".""SimpleTableName"" ( ""Id"" int PRIMARY KEY );
CREATE TABLE ""public"".""JustTableName"" ( ""Id"" int PRIMARY KEY );

CREATE TABLE ""db2"".""QuotedTableName"" ( ""Id"" int PRIMARY KEY );
CREATE TABLE ""db2"".""Table.With.Dot"" ( ""Id"" int PRIMARY KEY );
CREATE TABLE ""db2"".""SimpleTableName"" ( ""Id"" int PRIMARY KEY );
CREATE TABLE ""db2"".""JustTableName"" ( ""Id"" int PRIMARY KEY );

CREATE TABLE ""db2"".""PrincipalTable"" (
    ""Id"" int PRIMARY KEY,
    ""UC1"" text,
    ""UC2"" int,
    ""Index1"" bit,
    ""Index2"" bigint,
    CONSTRAINT ""UX"" UNIQUE (""UC1"", ""UC2"")
);

CREATE INDEX ""IX_COMPOSITE"" ON ""db2"".""PrincipalTable"" ( ""Index2"", ""Index1"" );

CREATE TABLE ""db2"".""DependentTable"" (
    ""Id"" int PRIMARY KEY,
    ""ForeignKeyId1"" text,
    ""ForeignKeyId2"" int,
    FOREIGN KEY (""ForeignKeyId1"", ""ForeignKeyId2"") REFERENCES ""db2"".""PrincipalTable""(""UC1"", ""UC2"") ON DELETE CASCADE
);",
                new[] { @"""db.2"".""QuotedTableName""", @"""db.2"".SimpleTableName", @"public.""Table.With.Dot""", @"public.""SimpleTableName""", @"""JustTableName""" },
                new[] { "db2" },
                dbModel =>
                {
                    var sequence = Assert.Single(dbModel.Sequences);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Equal("db2", sequence.Schema);

                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db.2" && t.Name == "QuotedTableName"));
                    Assert.Empty(dbModel.Tables.Where(t => t.Schema == "db.2" && t.Name == "Table.With.Dot"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db.2" && t.Name == "SimpleTableName"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db.2" && t.Name == "JustTableName"));

                    Assert.Empty(dbModel.Tables.Where(t => t.Schema == "public" && t.Name == "QuotedTableName"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "public" && t.Name == "Table.With.Dot"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "public" && t.Name == "SimpleTableName"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "public" && t.Name == "JustTableName"));

                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "QuotedTableName"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "Table.With.Dot"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "SimpleTableName"));
                    Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "JustTableName"));

                    var principalTable = Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "PrincipalTable"));
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.NotNull(principalTable.PrimaryKey);
                    Assert.Single(principalTable.UniqueConstraints);
                    Assert.Single(principalTable.Indexes);

                    var dependentTable = Assert.Single(dbModel.Tables.Where(t => t.Schema == "db2" && t.Name == "DependentTable"));
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.Single(dependentTable.ForeignKeys);
                },
                @"
DROP SEQUENCE ""public"".""Sequence"";
DROP SEQUENCE ""db2"".""Sequence"";

DROP TABLE ""db.2"".""QuotedTableName"";
DROP TABLE ""db.2"".""Table.With.Dot"";
DROP TABLE ""db.2"".""SimpleTableName"";
DROP TABLE ""db.2"".""JustTableName"";

DROP TABLE ""public"".""QuotedTableName"";
DROP TABLE ""public"".""Table.With.Dot"";
DROP TABLE ""public"".""SimpleTableName"";
DROP TABLE ""public"".""JustTableName"";

DROP TABLE ""db2"".""QuotedTableName"";
DROP TABLE ""db2"".""Table.With.Dot"";
DROP TABLE ""db2"".""SimpleTableName"";
DROP TABLE ""db2"".""JustTableName"";
DROP TABLE ""db2"".""DependentTable"";
DROP TABLE ""db2"".""PrincipalTable"";");
        }

        #endregion

        #region Table

        [Fact]
        public void Create_columns()
        {
            Test(
                @"
CREATE TABLE ""Blogs"" (
    ""Id"" int,
    ""Name"" text NOT NULL
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = dbModel.Tables.Single();

                        Assert.Equal(2, table.Columns.Count);
                        Assert.All(
                            table.Columns, c =>
                                {
                                    Assert.Equal("public", c.Table.Schema);
                                    Assert.Equal("Blogs", c.Table.Name);
                                });

                        Assert.Single(table.Columns.Where(c => c.Name == "Id"));
                        Assert.Single(table.Columns.Where(c => c.Name == "Name"));
                    },
                @"DROP TABLE ""Blogs""");
        }

        [Fact]
        public void Create_primary_key()
        {
            Test(
                @"
CREATE TABLE ""PrimaryKeyTable"" (
    ""Id"" int PRIMARY KEY
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var pk = dbModel.Tables.Single().PrimaryKey;

                        Assert.Equal("public", pk.Table.Schema);
                        Assert.Equal("PrimaryKeyTable", pk.Table.Name);
                        Assert.StartsWith("PrimaryKeyTable_pkey", pk.Name);
                        Assert.Equal(new List<string> { "Id" }, pk.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE ""PrimaryKeyTable""");
        }

        [Fact]
        public void Create_unique_constraints()
        {
            Test(
                @"
CREATE TABLE ""UniqueConstraint"" (
    ""Id"" int,
    ""Name"" int Unique,
    ""IndexProperty"" int,
    ""Unq1"" int,
    ""Unq2"" int,
    UNIQUE (""Unq1"", ""Unq2"")
);

CREATE INDEX ""IX_INDEX"" on ""UniqueConstraint"" ( ""IndexProperty"" );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = dbModel.Tables.Single();
                        Assert.Equal(2, table.UniqueConstraints.Count);

                        var firstConstraint = table.UniqueConstraints.Single(c => c.Columns.Count == 1);
                        Assert.Equal("public", firstConstraint.Table.Schema);
                        Assert.Equal("UniqueConstraint", firstConstraint.Table.Name);
                        //Assert.StartsWith("UQ__UniqueCo", uniqueConstraint.Name);
                        Assert.Equal(new List<string> { "Name" }, firstConstraint.Columns.Select(ic => ic.Name).ToList());

                        var secondConstraint = table.UniqueConstraints.Single(c => c.Columns.Count == 2);
                        Assert.Equal("public", secondConstraint.Table.Schema);
                        Assert.Equal("UniqueConstraint", secondConstraint.Table.Name);
                        Assert.Equal(new List<string> { "Unq1", "Unq2" }, secondConstraint.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE ""UniqueConstraint""");
        }

        [Fact]
        public void Create_indexes()
        {
            Test(
                @"
CREATE TABLE ""IndexTable"" (
    ""Id"" int,
    ""Name"" int,
    ""IndexProperty"" int,
    ""ConstraintProperty"" int,
    UNIQUE (""ConstraintProperty"")
);

CREATE INDEX ""IX_NAME"" on ""IndexTable"" ( ""Name"" );
CREATE INDEX ""IX_INDEX"" on ""IndexTable"" ( ""IndexProperty"" );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = dbModel.Tables.Single();

                        // Unique constraints should *not* be modelled as indices
                        Assert.Equal(2, table.Indexes.Count);
                        Assert.All(
                            table.Indexes, c =>
                                {
                                    Assert.Equal("public", c.Table.Schema);
                                    Assert.Equal("IndexTable", c.Table.Name);
                                });

                        Assert.Single(table.Indexes.Where(c => c.Name == "IX_NAME"));
                        Assert.Single(table.Indexes.Where(c => c.Name == "IX_INDEX"));
                    },
                @"DROP TABLE ""IndexTable""");
        }

        [Fact]
        public void Create_foreign_keys()
        {
            Test(
                @"
CREATE TABLE ""PrincipalTable"" (
    ""Id"" int PRIMARY KEY
);

CREATE TABLE ""FirstDependent"" (
    ""Id"" int PRIMARY KEY,
    ""ForeignKeyId"" int,
    FOREIGN KEY (""ForeignKeyId"") REFERENCES ""PrincipalTable""(""Id"") ON DELETE CASCADE
);

CREATE TABLE ""SecondDependent"" (
    ""Id"" int PRIMARY KEY,
    FOREIGN KEY (""Id"") REFERENCES ""PrincipalTable""(""Id"") ON DELETE NO ACTION
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var firstFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "FirstDependent").ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", firstFk.Table.Schema);
                        Assert.Equal("FirstDependent", firstFk.Table.Name);
                        Assert.Equal("public", firstFk.PrincipalTable.Schema);
                        Assert.Equal("PrincipalTable", firstFk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId" }, firstFk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "Id" }, firstFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, firstFk.OnDelete);

                        var secondFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "SecondDependent").ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", secondFk.Table.Schema);
                        Assert.Equal("SecondDependent", secondFk.Table.Name);
                        Assert.Equal("public", secondFk.PrincipalTable.Schema);
                        Assert.Equal("PrincipalTable", secondFk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "Id" }, secondFk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "Id" }, secondFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.NoAction, secondFk.OnDelete);
                    },
                @"
DROP TABLE ""SecondDependent"";
DROP TABLE ""FirstDependent"";
DROP TABLE ""PrincipalTable"";");
        }

        #endregion

        #region ColumnFacets

        [Fact]
        public void Column_with_domain_assigns_underlying_store_type()
        {
            Fixture.TestStore.ExecuteNonQuery(
                @"
CREATE DOMAIN public.text_domain AS text;
CREATE DOMAIN db2.text_domain AS int;
CREATE DOMAIN public.char_domain AS char(3);
");

            Test(
                @"
CREATE TABLE domains (
    id int,
    text_domain public.text_domain NULL,
    char_domain public.char_domain NULL
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var textDomainColumn = Assert.Single(dbModel.Tables.Single().Columns.Where(c => c.Name == "text_domain"));
                        Assert.Equal("text_domain", textDomainColumn.StoreType);
                        Assert.Equal("text", textDomainColumn.GetUnderlyingStoreType());

                        var charDomainColumn = Assert.Single(dbModel.Tables.Single().Columns.Where(c => c.Name == "char_domain"));
                        Assert.Equal("char_domain", charDomainColumn.StoreType);
                        Assert.Equal("character(3)", charDomainColumn.GetUnderlyingStoreType());

                        var nonDomainColumn = Assert.Single(dbModel.Tables.Single().Columns.Where(c => c.Name == "id"));
                        Assert.Equal("integer", nonDomainColumn.StoreType);
                        Assert.Null(nonDomainColumn.GetUnderlyingStoreType());
                    },
                @"
DROP TABLE domains;
DROP DOMAIN public.text_domain;
DROP DOMAIN public.char_domain;
DROP DOMAIN db2.text_domain;");
        }

        [Fact]
        public void Decimal_numeric_types_have_precision_scale()
        {
            // Note: in PostgreSQL decimal is simply an alias for numeric
            Test(
                @"
CREATE TABLE ""NumericColumns"" (
    ""Id"" int,
    ""numericColumn"" numeric NOT NULL,
    ""numeric152Column"" numeric(15, 2) NOT NULL,
    ""numeric18Column"" numeric(18) NOT NULL
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.Equal("numeric", columns.Single(c => c.Name == "numericColumn").StoreType);
                        Assert.Equal("numeric(15,2)", columns.Single(c => c.Name == "numeric152Column").StoreType);
                        Assert.Equal("numeric(18,0)", columns.Single(c => c.Name == "numeric18Column").StoreType);
                    },
                @"DROP TABLE ""NumericColumns""");
        }

        [Fact]
        public void Specific_max_length_are_add_to_store_type()
        {
            Test(
                @"
CREATE TABLE ""LengthColumns"" (
    ""Id"" int,
    ""char10Column"" char(10) NULL,
    ""varchar66Column"" varchar(66) NULL,
    ""bit111Column"" bit(111) NULL,
    ""varbit123Column"" varbit(123) NULL
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.Equal("character(10)", columns.Single(c => c.Name == "char10Column").StoreType);
                        Assert.Equal("character varying(66)", columns.Single(c => c.Name == "varchar66Column").StoreType);
                        Assert.Equal("bit(111)", columns.Single(c => c.Name == "bit111Column").StoreType);
                        Assert.Equal("bit varying(123)", columns.Single(c => c.Name == "varbit123Column").StoreType);
                    },
                @"DROP TABLE ""LengthColumns""");
        }

        [Fact]
        public void Datetime_types_have_precision_if_non_null_scale()
        {
            Test(
                @"
CREATE TABLE ""LengthColumns"" (
    ""Id"" int,
    ""time1Column"" time(1) NULL,
    ""timetz2Column"" timetz(2) NULL,
    ""timestamp3Column"" timestamp(3) NULL,
    ""timestamptz4Column"" timestamptz(4) NULL,
    ""interval5Column"" interval(5) NULL
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.Equal("time(1) without time zone", columns.Single(c => c.Name == "time1Column").StoreType);
                        Assert.Equal("time(2) with time zone", columns.Single(c => c.Name == "timetz2Column").StoreType);
                        Assert.Equal("timestamp(3) without time zone", columns.Single(c => c.Name == "timestamp3Column").StoreType);
                        Assert.Equal("timestamp(4) with time zone", columns.Single(c => c.Name == "timestamptz4Column").StoreType);
                        Assert.Equal("interval(5)", columns.Single(c => c.Name == "interval5Column").StoreType);
                    },
                @"DROP TABLE ""LengthColumns""");
        }

        [Fact]
        public void Store_types_without_any_facets()
        {
            Test(
                @"
CREATE TABLE ""NoFacetTypes"" (
    ""Id"" int,
    ""boolColumn"" bool,
    ""byteaColumn"" bytea,
    ""floatColumn"" float4,
    ""doubleColumn"" float8,
    ""decimalColumn"" decimal,
    ""moneyColumn"" money,
    ""guidColumn"" uuid,
    ""shortColumn"" int2,
    ""intColumn"" int4,
    ""longColumn"" int8,
    ""textColumn"" text,
    ""jsonbColumn"" jsonb,
    ""jsonColumn"" json,
    ""timestampColumn"" timestamp,
    /* TODO: timestamptz */
    ""intervalColumn"" interval,
    ""timetzColumn"" timetz,
    ""macaddrColumn"" macaddr,
    ""inetColumn"" inet,
    ""pointColumn"" point,
    ""lineColumn"" line,
    ""xidColumn"" xid,
    ""textArrayColumn"" text[]
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single(t => t.Name == "NoFacetTypes").Columns;

                        Assert.Equal("boolean", columns.Single(c => c.Name == "boolColumn").StoreType);
                        Assert.Equal("bytea", columns.Single(c => c.Name == "byteaColumn").StoreType);
                        Assert.Equal("real", columns.Single(c => c.Name == "floatColumn").StoreType);
                        Assert.Equal("double precision", columns.Single(c => c.Name == "doubleColumn").StoreType);
                        Assert.Equal("numeric", columns.Single(c => c.Name == "decimalColumn").StoreType);
                        Assert.Equal("money", columns.Single(c => c.Name == "moneyColumn").StoreType);
                        Assert.Equal("uuid", columns.Single(c => c.Name == "guidColumn").StoreType);
                        Assert.Equal("smallint", columns.Single(c => c.Name == "shortColumn").StoreType);
                        Assert.Equal("integer", columns.Single(c => c.Name == "intColumn").StoreType);
                        Assert.Equal("bigint", columns.Single(c => c.Name == "longColumn").StoreType);
                        Assert.Equal("text", columns.Single(c => c.Name == "textColumn").StoreType);
                        Assert.Equal("jsonb", columns.Single(c => c.Name == "jsonbColumn").StoreType);
                        Assert.Equal("json", columns.Single(c => c.Name == "jsonColumn").StoreType);
                        Assert.Equal("timestamp without time zone", columns.Single(c => c.Name == "timestampColumn").StoreType);
                        Assert.Equal("interval", columns.Single(c => c.Name == "intervalColumn").StoreType);
                        Assert.Equal("time with time zone", columns.Single(c => c.Name == "timetzColumn").StoreType);
                        Assert.Equal("macaddr", columns.Single(c => c.Name == "macaddrColumn").StoreType);
                        Assert.Equal("inet", columns.Single(c => c.Name == "inetColumn").StoreType);
                        Assert.Equal("point", columns.Single(c => c.Name == "pointColumn").StoreType);
                        Assert.Equal("line", columns.Single(c => c.Name == "lineColumn").StoreType);
                        Assert.Equal("xid", columns.Single(c => c.Name == "xidColumn").StoreType);
                        Assert.Equal("text[]", columns.Single(c => c.Name == "textArrayColumn").StoreType);
                    },
                @"
DROP TABLE ""NoFacetTypes""");
        }

        [Fact]
        public void Default_and_computed_values_are_stored()
        {
            Test(
                @"
CREATE TABLE ""DefaultComputedValues"" (
    ""Id"" int,
    ""FixedDefaultValue"" timestamp NOT NULL DEFAULT ('1999-01-08')
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;
                        Assert.Equal("'1999-01-08 00:00:00'::timestamp without time zone", columns.Single(c => c.Name == "FixedDefaultValue").DefaultValueSql);
                    },
                @"DROP TABLE ""DefaultComputedValues""");
        }

        [Fact]
        public void Default_value_matching_clr_default_is_not_stored()
        {
            Test(
                @"
CREATE DOMAIN ""decimalDomain"" AS decimal(6);

CREATE TABLE ""DefaultValues"" (
    ""IgnoredDefault1"" int DEFAULT NULL,
    ""IgnoredDefault2"" int NOT NULL DEFAULT NULL,
    ""IgnoredDefault9"" int NOT NULL DEFAULT 0,
    ""IgnoredDefault14"" smallint NOT NULL DEFAULT 0,
    ""IgnoredDefault3"" bigint NOT NULL DEFAULT 0,
    ""IgnoredDefault15"" decimal NOT NULL DEFAULT 0,
    ""IgnoredDefault16"" decimal NOT NULL DEFAULT 0.0,
    ""IgnoredDefault17"" ""decimalDomain"" NOT NULL DEFAULT 0,
    ""IgnoredDefault10"" money NOT NULL DEFAULT 0,
    ""IgnoredDefault19"" money NOT NULL DEFAULT 0.0,
    ""IgnoredDefault21"" float4 NOT NULL DEFAULT 0.0,
    ""IgnoredDefault7"" float8 NOT NULL DEFAULT 0,
    ""IgnoredDefault18"" float8 NOT NULL DEFAULT 0.0,
    ""IgnoredDefault24"" float8 NOT NULL DEFAULT 0E0,
    ""IgnoredDefault4"" bool NOT NULL DEFAULT false,
    ""IgnoredDefault25"" date NOT NULL DEFAULT '0001-01-01',
    ""IgnoredDefault26"" timestamp NOT NULL DEFAULT '1900-01-01T00:00:00.000',
    ""IgnoredDefault27"" interval NOT NULL DEFAULT '00:00:00',
    ""IgnoredDefault32"" time NOT NULL DEFAULT '00:00:00',
    ""IgnoredDefault34"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.All(
                            columns,
                            t => Assert.Null(t.DefaultValueSql));
                    },
                @"
DROP TABLE ""DefaultValues"";
DROP DOMAIN ""decimalDomain"";");
        }

        [Fact]
        public void ValueGenerated_is_set_for_default_and_serial_column()
        {
            Test(
                @"
CREATE TABLE ""ValueGeneratedProperties"" (
    ""Id"" SERIAL,
    ""NoValueGenerationColumn"" text,
    ""FixedDefaultValue"" timestamp NOT NULL DEFAULT ('1999-01-08')
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.Equal(ValueGenerated.OnAdd, columns.Single(c => c.Name == "Id").ValueGenerated);
                        Assert.Null(columns.Single(c => c.Name == "NoValueGenerationColumn").ValueGenerated);
                        Assert.Null(columns.Single(c => c.Name == "FixedDefaultValue").ValueGenerated);
                    },
                @"DROP TABLE ""ValueGeneratedProperties""");
        }

        [Fact]
        public void Column_nullability_is_set()
        {
            Test(
                @"
CREATE TABLE ""NullableColumns"" (
    ""Id"" int,
    ""NullableInt"" int NULL,
    ""NonNullString"" text NOT NULL
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.True(columns.Single(c => c.Name == "NullableInt").IsNullable);
                        Assert.False(columns.Single(c => c.Name == "NonNullString").IsNullable);
                    },
                @"DROP TABLE ""NullableColumns""");
        }

        [Fact]
        public void System_columns_are_not_created()
        {
            Test(
                @"
CREATE TABLE ""SystemColumnsTable""
(
     ""Id"" int NOT NULL PRIMARY KEY
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.Equal(1, columns.Count);
                    },
                @"DROP TABLE ""SystemColumnsTable""");
        }

        #endregion

        #region PrimaryKeyFacets

        [Fact]
        public void Create_composite_primary_key()
        {
            Test(
                @"
CREATE TABLE ""CompositePrimaryKeyTable"" (
    ""Id1"" int,
    ""Id2"" int,
    PRIMARY KEY (""Id2"", ""Id1"")
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var pk = dbModel.Tables.Single().PrimaryKey;

                        Assert.Equal("public", pk.Table.Schema);
                        Assert.Equal("CompositePrimaryKeyTable", pk.Table.Name);
                        //Assert.StartsWith("PK__Composit", pk.Name);
                        Assert.Equal(new List<string> { "Id2", "Id1" }, pk.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE ""CompositePrimaryKeyTable""");
        }

        [Fact]
        public void Set_primary_key_name_from_index()
        {
            Test(
                @"
CREATE TABLE ""PrimaryKeyName"" (
    ""Id1"" int,
    ""Id2"" int,
    CONSTRAINT ""MyPK"" PRIMARY KEY ( ""Id2"" )
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var pk = dbModel.Tables.Single().PrimaryKey;

                        Assert.Equal("public", pk.Table.Schema);
                        Assert.Equal("PrimaryKeyName", pk.Table.Name);
                        Assert.StartsWith("MyPK", pk.Name);
                        Assert.Equal(new List<string> { "Id2" }, pk.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE ""PrimaryKeyName""");
        }

        #endregion

        #region UniqueConstraintFacets

        [Fact]
        public void Create_composite_unique_constraint()
        {
            Test(
                @"
CREATE TABLE ""CompositeUniqueConstraintTable"" (
    ""Id1"" int,
    ""Id2"" int,
    CONSTRAINT ""UX"" UNIQUE (""Id2"", ""Id1"")
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", uniqueConstraint.Table.Schema);
                        Assert.Equal("CompositeUniqueConstraintTable", uniqueConstraint.Table.Name);
                        Assert.Equal("UX", uniqueConstraint.Name);
                        Assert.Equal(new List<string> { "Id2", "Id1" }, uniqueConstraint.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE ""CompositeUniqueConstraintTable""");
        }

        [Fact]
        public void Set_unique_constraint_name_from_index()
        {
            Test(
                @"
CREATE TABLE ""UniqueConstraintName"" (
    ""Id1"" int,
    ""Id2"" int,
    CONSTRAINT ""MyUC"" UNIQUE ( ""Id2"" )
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                        var table = dbModel.Tables.Single();
                        var uniqueConstraint = Assert.Single(table.UniqueConstraints);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", uniqueConstraint.Table.Schema);
                        Assert.Equal("UniqueConstraintName", uniqueConstraint.Table.Name);
                        Assert.Equal("MyUC", uniqueConstraint.Name);
                        Assert.Equal(new List<string> { "Id2" }, uniqueConstraint.Columns.Select(ic => ic.Name).ToList());
                        Assert.Empty(table.Indexes);
                    },
                @"DROP TABLE ""UniqueConstraintName""");
        }

        #endregion

        #region IndexFacets

        [Fact]
        public void Create_composite_index()
        {
            Test(
                @"
CREATE TABLE ""CompositeIndexTable"" (
    ""Id1"" int,
    ""Id2"" int
);

CREATE INDEX ""IX_COMPOSITE"" ON ""CompositeIndexTable"" ( ""Id2"", ""Id1"" );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var index = Assert.Single(dbModel.Tables.Single().Indexes);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", index.Table.Schema);
                        Assert.Equal("CompositeIndexTable", index.Table.Name);
                        Assert.Equal("IX_COMPOSITE", index.Name);
                        Assert.Equal(new List<string> { "Id2", "Id1" }, index.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE ""CompositeIndexTable""");
        }

        [Fact]
        public void Set_unique_true_for_unique_index()
        {
            Test(
                @"
CREATE TABLE ""UniqueIndexTable"" (
    ""Id1"" int,
    ""Id2"" int
);

CREATE UNIQUE INDEX ""IX_UNIQUE"" ON ""UniqueIndexTable"" ( ""Id2"" );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var index = Assert.Single(dbModel.Tables.Single().Indexes);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", index.Table.Schema);
                        Assert.Equal("UniqueIndexTable", index.Table.Name);
                        Assert.Equal("IX_UNIQUE", index.Name);
                        Assert.True(index.IsUnique);
                        Assert.Null(index.Filter);
                        Assert.Equal(new List<string> { "Id2" }, index.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE ""UniqueIndexTable""");
        }

        [Fact]
        public void Set_filter_for_filtered_index()
        {
            Test(
                @"
CREATE TABLE ""FilteredIndexTable"" (
    ""Id1"" int,
    ""Id2"" int NULL
);

CREATE UNIQUE INDEX ""IX_UNIQUE"" ON ""FilteredIndexTable"" ( ""Id2"" ) WHERE ""Id2"" > 10;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var index = Assert.Single(dbModel.Tables.Single().Indexes);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", index.Table.Schema);
                        Assert.Equal("FilteredIndexTable", index.Table.Name);
                        Assert.Equal("IX_UNIQUE", index.Name);
                        Assert.Equal(@"(""Id2"" > 10)", index.Filter);
                        Assert.Equal(new List<string> { "Id2" }, index.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE ""FilteredIndexTable""");
        }

        #endregion

        #region ForeignKeyFacets

        [Fact]
        public void Create_composite_foreign_key()
        {
            Test(
                @"
CREATE TABLE ""PrincipalTable"" (
    ""Id1"" int,
    ""Id2"" int,
    PRIMARY KEY (""Id1"", ""Id2"")
);

CREATE TABLE ""DependentTable"" (
    ""Id"" int PRIMARY KEY,
    ""ForeignKeyId1"" int,
    ""ForeignKeyId2"" int,
    FOREIGN KEY (""ForeignKeyId1"", ""ForeignKeyId2"") REFERENCES ""PrincipalTable""(""Id1"", ""Id2"") ON DELETE CASCADE
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", fk.Table.Schema);
                        Assert.Equal("DependentTable", fk.Table.Name);
                        Assert.Equal("public", fk.PrincipalTable.Schema);
                        Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId1", "ForeignKeyId2" }, fk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "Id1", "Id2" }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                    },
                @"
DROP TABLE ""DependentTable"";
DROP TABLE ""PrincipalTable"";");
        }

        [Fact]
        public void Create_multiple_foreign_key_in_same_table()
        {
            Test(
                @"
CREATE TABLE ""PrincipalTable"" (
    ""Id"" int PRIMARY KEY
);

CREATE TABLE ""AnotherPrincipalTable"" (
    ""Id"" int PRIMARY KEY
);

CREATE TABLE ""DependentTable"" (
    ""Id"" int PRIMARY KEY,
    ""ForeignKeyId1"" int,
    ""ForeignKeyId2"" int,
    FOREIGN KEY (""ForeignKeyId1"") REFERENCES ""PrincipalTable""(""Id"") ON DELETE CASCADE,
    FOREIGN KEY (""ForeignKeyId2"") REFERENCES ""AnotherPrincipalTable""(""Id"") ON DELETE CASCADE
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var foreignKeys = dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys;

                        Assert.Equal(2, foreignKeys.Count);

                        var principalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "PrincipalTable"));

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", principalFk.Table.Schema);
                        Assert.Equal("DependentTable", principalFk.Table.Name);
                        Assert.Equal("public", principalFk.PrincipalTable.Schema);
                        Assert.Equal("PrincipalTable", principalFk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId1" }, principalFk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "Id" }, principalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, principalFk.OnDelete);

                        var anotherPrincipalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "AnotherPrincipalTable"));

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", anotherPrincipalFk.Table.Schema);
                        Assert.Equal("DependentTable", anotherPrincipalFk.Table.Name);
                        Assert.Equal("public", anotherPrincipalFk.PrincipalTable.Schema);
                        Assert.Equal("AnotherPrincipalTable", anotherPrincipalFk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId2" }, anotherPrincipalFk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "Id" }, anotherPrincipalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, anotherPrincipalFk.OnDelete);
                    },
                @"
DROP TABLE ""DependentTable"";
DROP TABLE ""AnotherPrincipalTable"";
DROP TABLE ""PrincipalTable"";");
        }

        [Fact]
        public void Create_foreign_key_referencing_unique_constraint()
        {
            Test(
                @"
CREATE TABLE ""PrincipalTable"" (
    ""Id1"" int,
    ""Id2"" int UNIQUE
);

CREATE TABLE ""DependentTable"" (
    ""Id"" int PRIMARY KEY,
    ""ForeignKeyId"" int,
    FOREIGN KEY (""ForeignKeyId"") REFERENCES ""PrincipalTable""(""Id2"") ON DELETE CASCADE
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", fk.Table.Schema);
                        Assert.Equal("DependentTable", fk.Table.Name);
                        Assert.Equal("public", fk.PrincipalTable.Schema);
                        Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId" }, fk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "Id2" }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                    },
                @"
DROP TABLE ""DependentTable"";
DROP TABLE ""PrincipalTable"";");
        }

        [Fact]
        public void Set_name_for_foreign_key()
        {
            Test(
                @"
CREATE TABLE ""PrincipalTable"" (
    ""Id"" int PRIMARY KEY
);

CREATE TABLE ""DependentTable"" (
    ""Id"" int PRIMARY KEY,
    ""ForeignKeyId"" int,
    CONSTRAINT ""MYFK"" FOREIGN KEY (""ForeignKeyId"") REFERENCES ""PrincipalTable""(""Id"") ON DELETE CASCADE
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", fk.Table.Schema);
                        Assert.Equal("DependentTable", fk.Table.Name);
                        Assert.Equal("public", fk.PrincipalTable.Schema);
                        Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId" }, fk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "Id" }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                        Assert.Equal("MYFK", fk.Name);
                    },
                @"
DROP TABLE ""DependentTable"";
DROP TABLE ""PrincipalTable"";");
        }

        [Fact]
        public void Set_referential_action_for_foreign_key()
        {
            Test(
                @"
CREATE TABLE ""PrincipalTable"" (
    ""Id"" int PRIMARY KEY
);

CREATE TABLE ""DependentTable"" (
    ""Id"" int PRIMARY KEY,
    ""ForeignKeyId"" int,
    FOREIGN KEY (""ForeignKeyId"") REFERENCES ""PrincipalTable""(""Id"") ON DELETE SET NULL
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable").ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("public", fk.Table.Schema);
                        Assert.Equal("DependentTable", fk.Table.Name);
                        Assert.Equal("public", fk.PrincipalTable.Schema);
                        Assert.Equal("PrincipalTable", fk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId" }, fk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "Id" }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.SetNull, fk.OnDelete);
                    },
                @"
DROP TABLE ""DependentTable"";
DROP TABLE ""PrincipalTable"";");
        }

        #endregion

        #region Warnings

        [Fact]
        public void Warn_missing_schema()
        {
            Test(
                @"
CREATE TABLE ""Blank"" (
    ""Id"" int
)",
                Enumerable.Empty<string>(),
                new[] { "MySchema" },
                dbModel =>
                    {
                        Assert.Empty(dbModel.Tables);

                        var (Level, Id, Message) = Assert.Single(_log.Where(t => t.Level == LogLevel.Warning));

                        Assert.Equal(NpgsqlStrings.LogMissingSchema.EventId, Id);
                        Assert.Equal(NpgsqlStrings.LogMissingSchema.GenerateMessage("MySchema"), Message);
                    },
                @"DROP TABLE ""Blank""");
        }

        [Fact]
        public void Warn_missing_table()
        {
            Test(
                @"
CREATE TABLE ""Blank"" (
    ""Id"" int
)",
                new[] { "MyTable" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        Assert.Empty(dbModel.Tables);

                        var (Level, Id, Message) = Assert.Single(_log.Where(t => t.Level == LogLevel.Warning));

                        Assert.Equal(NpgsqlStrings.LogMissingTable.EventId, Id);
                        Assert.Equal(NpgsqlStrings.LogMissingTable.GenerateMessage("MyTable"), Message);
                    },
                @"DROP TABLE ""Blank""");
        }

        [Fact]
        public void Warn_missing_principal_table_for_foreign_key()
        {
            Test(
                @"
CREATE TABLE ""PrincipalTable"" (
    ""Id"" int PRIMARY KEY
);

CREATE TABLE ""DependentTable"" (
    ""Id"" int PRIMARY KEY,
    ""ForeignKeyId"" int,
    CONSTRAINT ""MYFK"" FOREIGN KEY (""ForeignKeyId"") REFERENCES ""PrincipalTable""(""Id"") ON DELETE CASCADE
);",
                new[] { "DependentTable" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var (Level, Id, Message) = Assert.Single(_log.Where(t => t.Level == LogLevel.Warning));

                        Assert.Equal(NpgsqlStrings.LogPrincipalTableNotInSelectionSet.EventId, Id);
                        Assert.Equal(NpgsqlStrings.LogPrincipalTableNotInSelectionSet.GenerateMessage("MYFK", "public.DependentTable", "public.PrincipalTable"), Message);
                    },
                @"
DROP TABLE ""DependentTable"";
DROP TABLE ""PrincipalTable"";");
        }

        #endregion

        #region PostgreSQL-specific

        [Fact]
        public void SequenceSerial()
        {
            Test(
                @"
CREATE TABLE ""SerialSequence"" (""Id"" serial PRIMARY KEY)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    // Sequences which belong to a serial column should not get reverse engineered as separate sequences
                    Assert.Empty(dbModel.Sequences);

                    // Now make sure the field itself is properly reverse-engineered.
                    var column = dbModel.Tables.Single().Columns.Single();
                    Assert.Equal(ValueGenerated.OnAdd, column.ValueGenerated);
                    Assert.Null(column.DefaultValueSql);
                    Assert.Equal(NpgsqlValueGenerationStrategy.SerialColumn, (NpgsqlValueGenerationStrategy)column[NpgsqlAnnotationNames.ValueGenerationStrategy]);
                },
                @"DROP TABLE ""SerialSequence""");
        }

        [Fact]
        public void SequenceNonSerial()
        {
            Test(
                @"
CREATE SEQUENCE ""SomeSequence"";
CREATE TABLE ""NonSerialSequence"" (""Id"" integer PRIMARY KEY DEFAULT nextval('""SomeSequence""'))",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var column = dbModel.Tables.Single().Columns.Single();
                    Assert.Equal(@"nextval('""SomeSequence""'::regclass)", column.DefaultValueSql);
                    // Npgsql has special detection for serial columns (scaffolding them with ValueGenerated.OnAdd
                    // and removing the default), but not for non-serial sequence-driven columns, which are scaffolded
                    // with a DefaultValue. This is consistent with the SqlServer scaffolding behavior.
                    Assert.Null(column.ValueGenerated);

                    Assert.Single(dbModel.Sequences.Where(s => s.Name == "SomeSequence"));
                },
                @"
DROP TABLE ""NonSerialSequence"";
DROP SEQUENCE ""SomeSequence""");
        }

        [Fact]
        public void Identity()
        {
            Test(
                @"
CREATE TABLE identity (
    id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    a int GENERATED ALWAYS AS IDENTITY,
    b int GENERATED BY DEFAULT AS IDENTITY
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var idIdentityAlways = dbModel.Tables.Single().Columns.Single(c => c.Name == "id");
                    Assert.Equal(ValueGenerated.OnAdd, idIdentityAlways.ValueGenerated);
                    Assert.Null(idIdentityAlways.DefaultValueSql);
                    Assert.Equal(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn, (NpgsqlValueGenerationStrategy)idIdentityAlways[NpgsqlAnnotationNames.ValueGenerationStrategy]);

                    var identityAlways = dbModel.Tables.Single().Columns.Single(c => c.Name == "a");
                    Assert.Equal(ValueGenerated.OnAdd, identityAlways.ValueGenerated);
                    Assert.Null(identityAlways .DefaultValueSql);
                    Assert.Equal(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn, (NpgsqlValueGenerationStrategy)identityAlways[NpgsqlAnnotationNames.ValueGenerationStrategy]);

                    var identityByDefault = dbModel.Tables.Single().Columns.Single(c => c.Name == "b");
                    Assert.Equal(ValueGenerated.OnAdd, identityByDefault.ValueGenerated);
                    Assert.Null(identityByDefault .DefaultValueSql);
                    Assert.Equal(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn, (NpgsqlValueGenerationStrategy)identityByDefault[NpgsqlAnnotationNames.ValueGenerationStrategy]);
                },
                "DROP TABLE identity");
        }

        [Fact]
        public void Index_method()
        {
            Test(
                @"
CREATE TABLE ""IndexMethod"" (a int, b int);
CREATE INDEX ix_a ON ""IndexMethod"" USING hash (a);
CREATE INDEX ix_b ON ""IndexMethod"" (b);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = dbModel.Tables.Single();
                    Assert.Equal(2, table.Indexes.Count);

                    var methodIndex = table.Indexes.Single(i => i.Name == "ix_a");
                    Assert.Equal("hash", methodIndex.FindAnnotation(NpgsqlAnnotationNames.IndexMethod).Value);

                    var noMethodIndex = table.Indexes.Single(i => i.Name == "ix_b");
                    Assert.Equal("btree", noMethodIndex.FindAnnotation(NpgsqlAnnotationNames.IndexMethod).Value);
                },
                @"DROP TABLE ""IndexMethod""");
        }

        [Fact]
        public void Comments()
        {
            Test(
                @"
CREATE TABLE comment (a int);
COMMENT ON TABLE comment IS 'table comment';
COMMENT ON COLUMN comment.a IS 'column comment'",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = dbModel.Tables.Single();
                    Assert.Equal("table comment", table.FindAnnotation(NpgsqlAnnotationNames.Comment).Value);
                    Assert.Equal("column comment", table.Columns.Single().FindAnnotation(NpgsqlAnnotationNames.Comment).Value);
                },
                "DROP TABLE comment");
        }

        [Fact(Skip="Travis doesn't have PG10 which is required. Need to set up conditional fact.")]
        public void Sequence_types()
        {
            Test(
                @"
CREATE SEQUENCE ""SmallIntSequence"" AS smallint;
CREATE SEQUENCE ""IntSequence"" AS int;
CREATE SEQUENCE ""BigIntSequence"" AS bigint;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var smallSequence = dbModel.Sequences.Single(s => s.Name == "SmallIntSequence");
                    Assert.Equal("smallint", smallSequence.StoreType);
                    var intSequence = dbModel.Sequences.Single(s => s.Name == "IntSequence");
                    Assert.Equal("integer", intSequence.StoreType);
                    var bigSequence = dbModel.Sequences.Single(s => s.Name == "BigIntSequence");
                    Assert.Equal("bigint", bigSequence.StoreType);
                },
                @"
DROP SEQUENCE ""SmallIntSequence"";
DROP SEQUENCE ""IntSequence"";
DROP SEQUENCE ""BigIntSequence"";");
        }

        [Fact]
        public void Dropped_columns()
        {
            Test(
                @"
CREATE TABLE foo (id int primary key);
ALTER TABLE foo DROP COLUMN id;
ALTER TABLE foo ADD COLUMN id2 int primary key;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    Assert.Single(dbModel.Tables.Single().Columns);
                },
                "DROP TABLE foo");
        }

        [Fact]
        public void Postgres_extensions()
        {
            Test(
                @"
CREATE EXTENSION hstore;
CREATE EXTENSION pgcrypto;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var extensions = PostgresExtension.GetPostgresExtensions(dbModel).ToList();
                    Assert.Equal(2, extensions.Count);
                    var hstore = extensions.Single(e => e.Name == "hstore");
                    var pgcrypto = extensions.Single(e => e.Name == "pgcrypto");
                },
                @"DROP EXTENSION hstore; DROP EXTENSION pgcrypto");
        }

        [Fact]
        public void Enums()
        {
            Test(
                @"
CREATE TYPE mood AS ENUM ('happy', 'sad');
CREATE TYPE db2.mood AS ENUM ('excited', 'depressed');
CREATE TABLE foo (mood mood);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var enums = PostgresEnum.GetPostgresEnums(dbModel).ToList();
                    Assert.Equal(2, enums.Count);

                    var mood = enums.Single(e => e.Schema == null);
                    Assert.Equal("mood", mood.Name);
                    Assert.Equal(new[] { "happy", "sad" }, mood.Labels);

                    var mood2 = enums.Single(e => e.Schema == "db2");
                    Assert.Equal("mood", mood2.Name);
                    Assert.Equal(new[] { "excited", "depressed" }, mood2.Labels);

                    // Enum columns are left out of the model for now (a warning is logged).
                    Assert.Equal(0, dbModel.Tables.Single().Columns.Count);
                },
                "DROP TABLE foo; DROP TYPE mood; DROP TYPE db2.mood;");
        }

        [Fact]
        public void Column_default_type_names_are_scaffolded()
        {
            Test(
                @"
CREATE TABLE column_types (
    smallint smallint,
    integer integer,
    bigint bigint,
    real real,
    ""double precision"" double precision,
    money money,
    numeric numeric,
    boolean boolean,
    bytea bytea,
    uuid uuid,
    text text,
    jsonb jsonb,
    json json,
    ""character varying"" character varying,
    ""character(1)"" character,
    ""character(2)"" character(2),
    ""timestamp without time zone"" timestamp,
    ""timestamp with time zone"" timestamptz,
    ""time without time zone"" time,
    ""time with time zone"" timetz,
    interval interval,
    macaddr macaddr,
    inet inet,
    ""bit(1)"" bit,
    ""bit varying"" varbit,
    point point,
    line line
)",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var typeMappingSource = new NpgsqlTypeMappingSource(
                        new TypeMappingSourceDependencies(
                            new ValueConverterSelector(new ValueConverterSelectorDependencies())
                        ),
                        new RelationalTypeMappingSourceDependencies()
                    );

                    foreach (var column in dbModel.Tables.Single().Columns)
                    {
                        Assert.Equal(column.Name, column.StoreType);
                        Assert.Equal(
                            column.StoreType,
                            typeMappingSource.FindMapping(column.StoreType).StoreType
                        );
                    }
                },
                "DROP TABLE column_types");
        }

        [Fact]
        public void System_tables_are_ignored()
        {
            Test(@"CREATE EXTENSION postgis",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    Assert.Empty(dbModel.Tables);
                },
                "DROP EXTENSION postgis");
        }

        #endregion

        readonly List<(LogLevel Level, EventId Id, string Message)> _log = new List<(LogLevel Level, EventId Id, string Message)>();

        void Test(string createSql, IEnumerable<string> tables, IEnumerable<string> schemas, Action<DatabaseModel> asserter, string cleanupSql)
        {
            Fixture.TestStore.ExecuteNonQuery(createSql);

            try
            {
                var databaseModelFactory = new NpgsqlDatabaseModelFactory(
                    new DiagnosticsLogger<DbLoggerCategory.Scaffolding>(
                        new ListLoggerFactory(_log),
                        new LoggingOptions(),
                        new DiagnosticListener("Fake")));

                var databaseModel = databaseModelFactory.Create(Fixture.TestStore.ConnectionString, tables, schemas);
                Assert.NotNull(databaseModel);
                asserter(databaseModel);
            }
            finally
            {
                if (!string.IsNullOrEmpty(cleanupSql))
                {
                    Fixture.TestStore.ExecuteNonQuery(cleanupSql);
                }
            }
        }

        public class NpgsqlDatabaseModelFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = nameof(NpgsqlDatabaseModelFactoryTest);
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public new NpgsqlTestStore TestStore => (NpgsqlTestStore)base.TestStore;

            public NpgsqlDatabaseModelFixture()
            {
                TestStore.ExecuteNonQuery("CREATE SCHEMA IF NOT EXISTS db2");
                TestStore.ExecuteNonQuery(@"CREATE SCHEMA IF NOT EXISTS ""db.2""");
            }
        }
    }
}
