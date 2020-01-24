///////////////////////////////////////////////////////////////////////////////////////
// THIS FILE WAS COPIED FROM THE EF CORE REPO, SINCE THE NON-FUNCTIONAL TESTS AREN'T
// DISTRIBUTED VIA NUGET.
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations
{
    public abstract class MigrationSqlGeneratorTestBase
    {
        protected static string EOL => Environment.NewLine;

        protected virtual string Sql { get; set; }

        [ConditionalFact]
        public virtual void AddColumnOperation_without_column_type()
            => Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Alias",
                    ClrType = typeof(string)
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_unicode_overridden()
            => Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<string>("Name").IsUnicode(false),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = true,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_unicode_no_model()
            => Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = false,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_fixed_length_no_model()
            => Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = false,
                    IsNullable = true,
                    IsFixedLength = true,
                    MaxLength = 100
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_maxLength_overridden()
            => Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<string>("Name").HasMaxLength(30),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 32,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddColumnOperation_with_maxLength_no_model()
            => Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true
                });

        [ConditionalFact]
        public virtual void AddForeignKeyOperation_without_principal_columns()
            => Generate(
                new AddForeignKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "SpouseId" },
                    PrincipalTable = "People"
                });

        [ConditionalFact]
        public virtual void AlterColumnOperation_without_column_type()
            => Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "LuckyNumber",
                    ClrType = typeof(int)
                });

        [ConditionalFact]
        public virtual void RenameTableOperation_legacy()
            => Generate(
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "Person"
                });

        [ConditionalFact]
        public virtual void RenameTableOperation()
            => Generate(
                modelBuilder => modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "2.1.0"),
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "dbo",
                    NewName = "Person",
                    NewSchema = "dbo"
                });

        [ConditionalFact]
        public virtual void SqlOperation()
            => Generate(
                new SqlOperation { Sql = "-- I <3 DDL" });

        protected TestHelpers TestHelpers { get; }

        protected MigrationSqlGeneratorTestBase(TestHelpers testHelpers)
        {
            TestHelpers = testHelpers;
        }

        protected virtual void Generate(params MigrationOperation[] operation)
            => Generate(_ => { }, operation);

        protected virtual void Generate(Action<ModelBuilder> buildAction, params MigrationOperation[] operation)
        {
            var modelBuilder = TestHelpers.CreateConventionBuilder();
            modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
            buildAction(modelBuilder);

            var batch = TestHelpers.CreateContextServices().GetRequiredService<IMigrationsSqlGenerator>()
                .Generate(operation, modelBuilder.Model);

            // Note that GO here is just a delimiter introduced in the tests to indicate a batch boundary
            Sql = string.Join(
                "GO" + EOL + EOL,
                batch.Select(b => b.CommandText));
        }

        protected void AssertSql(string expected)
            => Assert.Equal(expected, Sql, ignoreLineEndingDifferences: true);
    }
}
