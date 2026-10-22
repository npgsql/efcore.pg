using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Migrations;

public class NpgsqlMigrationsSqlGeneratorTest() : MigrationsSqlGeneratorTestBase(
    NpgsqlTestHelpers.Instance,
    new ServiceCollection().AddEntityFrameworkNpgsqlNetTopologySuite(),
    NpgsqlTestHelpers.Instance.AddProviderOptions(
        ((IRelationalDbContextOptionsBuilderInfrastructure)
            new NpgsqlDbContextOptionsBuilder(new DbContextOptionsBuilder()).UseNetTopologySuite())
        .OptionsBuilder).Options)
{
    #region Database

    [Fact]
    public virtual void CreateDatabaseOperation()
    {
        Generate(new NpgsqlCreateDatabaseOperation { Name = "Northwind" });

        AssertSql(
            """
CREATE DATABASE "Northwind";

""");
    }

    [ConditionalFact]
    public virtual void CreateDatabaseOperation_with_collation()
    {
        Generate(
            new NpgsqlCreateDatabaseOperation { Name = "Northwind", Collation = "POSIX" });

        AssertSql(
            """
CREATE DATABASE "Northwind"
LC_COLLATE "POSIX";

""");
    }

    [Theory]
    [InlineData("UTF8", "'UTF8'")]
    [InlineData("LATIN1", "'LATIN1'")]
    [InlineData("UTF'8", "'UTF''8'")]
    public virtual void CreateDatabaseOperation_with_encoding(string encoding, string literal)
    {
        Generate(new NpgsqlCreateDatabaseOperation { Name = "Northwind", Encoding = encoding, Template = "template0" });

        AssertSql(
            $"""
CREATE DATABASE "Northwind"
ENCODING {literal}
TEMPLATE template0;

""");
    }

    [Fact]
    public virtual void CreateDatabaseOperation_with_template()
    {
        Generate(new NpgsqlCreateDatabaseOperation { Name = "Northwind", Template = "MyTemplate" });

        AssertSql(
            """
CREATE DATABASE "Northwind"
TEMPLATE "MyTemplate";

""");
    }

    [Fact]
    public virtual void CreateDatabaseOperation_with_tablespace()
    {
        Generate(new NpgsqlCreateDatabaseOperation { Name = "some_db", Tablespace = "MyTablespace" });

        AssertSql(
            """
CREATE DATABASE some_db
TABLESPACE "MyTablespace";

""");
    }

    #endregion

    [ConditionalTheory, MinimumPostgresVersion(12, 0)]
    [InlineData("app", "other", false, null, false)]
    [InlineData(null, "app", false, null, false)]
    [InlineData("app", null, false, null, false)]
    [InlineData("app.schema", "other\"schema", false, null, false)]
    [InlineData("app", "other", true, null, false)]
    [InlineData("app", null, false, "search_path_schema,public", false)]
    [InlineData("app", null, false, "search_path_schema,public", true)]
    public async Task Collation_schema_changes_preserve_dependencies(
        string? sourceSchema, string? targetSchema, bool explicitSchema, string? searchPath, bool idempotent)
    {
        await using var store = await NpgsqlTestStore.CreateInitializedAsync("CollationSchemaChanges");
        var connectionString = new NpgsqlConnectionStringBuilder(store.ConnectionString) { SearchPath = searchPath }.ConnectionString;
        await using var source = new CollationContext(sourceSchema, explicitSchema, connectionString);
        await using var target = new CollationContext(targetSchema, explicitSchema, connectionString);
        if (searchPath is not null)
        {
            await source.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS search_path_schema");
        }

        await source.Database.EnsureCreatedAsync();
        await source.Database.ExecuteSqlRawAsync("""INSERT INTO public."People" ("Name") VALUES ('ABC')""");
        var oid = await source.Database.SqlQueryRaw<long>(
            """SELECT oid::bigint AS "Value" FROM pg_collation WHERE collname = 'case.insensitive'""").SingleAsync();

        (CollationContext Source, CollationContext Target)[] steps = [(source, target), (target, source)];
        foreach (var (from, to) in steps)
        {
            var model = to.GetService<IDesignTimeModel>().Model;
            var operations = to.GetService<IMigrationsModelDiffer>().GetDifferences(
                from.GetService<IDesignTimeModel>().Model.GetRelationalModel(), model.GetRelationalModel());
            Assert.Single(operations.OfType<AlterDatabaseOperation>());
            var sql = string.Concat(to.GetService<IMigrationsSqlGenerator>().Generate(
                operations, model, idempotent ? MigrationsSqlGenerationOptions.Idempotent : MigrationsSqlGenerationOptions.Default)
                .Select(c => c.CommandText));
            Assert.DoesNotContain("CREATE COLLATION", sql);
            Assert.DoesNotContain("DROP COLLATION", sql);
            if (idempotent)
            {
                sql = $"DO $EF$\nBEGIN\n{sql}\nEND $EF$;";
            }

            await to.Database.ExecuteSqlRawAsync(sql);

            Assert.Equal(oid, await to.Database.SqlQueryRaw<long>(
                """SELECT oid::bigint AS "Value" FROM pg_collation WHERE collname = 'case.insensitive'""").SingleAsync());
            var expectedSchema = to.Schema
                ?? await to.Database.SqlQueryRaw<string>("""SELECT current_schema() AS "Value" """).SingleAsync();
            Assert.Equal(expectedSchema, await to.Database.SqlQueryRaw<string>(
                """
SELECT n.nspname AS "Value"
FROM pg_collation AS c
JOIN pg_namespace AS n ON n.oid = c.collnamespace
WHERE c.collname = 'case.insensitive'
""").SingleAsync());
            Assert.Equal(1, await to.Database.SqlQueryRaw<string>("""SELECT "Name" AS "Value" FROM public."People" """)
                .Where(name => EF.Functions.Collate(name, to.CollationReference) == "abc").CountAsync());
        }
    }

    private class CollationContext(string? schema, bool explicitSchema, string connectionString) : DbContext
    {
        public string? Schema => schema;

        public string CollationReference
            => explicitSchema && schema is not null
                ? $"\"{schema.Replace("\"", "\"\"")}\".\"case.insensitive\""
                : "case.insensitive";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(connectionString).EnableServiceProviderCaching(false);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(explicitSchema ? null : schema);
            modelBuilder.HasCollation(
                explicitSchema ? schema : null, "case.insensitive", locale: "en-u-ks-primary", provider: "icu", deterministic: false);
            modelBuilder.Entity("People", b =>
            {
                b.ToTable("People", "public");
                b.Property<int>("Id");
                b.Property<string>("Name").UseCollation(CollationReference);
                b.HasIndex("Name").UseCollation(CollationReference);
            });
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Collation_legacy_migration_uses_target_model_default_schema(bool drop)
    {
        Generate(
            b => b.HasDefaultSchema("app"),
            m =>
            {
                var operation = m.AlterDatabase();
                if (drop)
                {
                    operation.OldAnnotation("Npgsql:CollationDefinition:dummy", "POSIX,POSIX,libc,");
                }
                else
                {
                    operation.Annotation("Npgsql:CollationDefinition:dummy", "POSIX,POSIX,libc,");
                }
            });

        AssertSql(
            drop
                ? "DROP COLLATION app.dummy;"
                : """
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'app') THEN
        CREATE SCHEMA app;
    END IF;
END $EF$;
GO

CREATE COLLATION app.dummy (LOCALE = 'POSIX',
    PROVIDER = libc
);
""");
    }

    [Fact]
    public void Collation_legacy_schema_change_keeps_create_drop_semantics()
    {
        Generate(
            null,
            m => m.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:other.dummy", "POSIX,POSIX,libc,")
                .OldAnnotation("Npgsql:CollationDefinition:app.dummy", "POSIX,POSIX,libc,"));

        AssertSql(
            """
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'other') THEN
        CREATE SCHEMA other;
    END IF;
END $EF$;
GO

CREATE COLLATION other.dummy (LOCALE = 'POSIX',
    PROVIDER = libc
);
DROP COLLATION app.dummy;
""");
    }

    [Fact]
    public void Collation_drop_uses_resolved_source_schema_not_target_default()
    {
        var operation = new AlterDatabaseOperation();
        operation.OldDatabase.AddAnnotation(NpgsqlAnnotationNames.CollationSchemasResolved, true);
        PostgresCollation.GetOrAddCollation(operation.OldDatabase, null, "dummy", "POSIX", "POSIX", "libc");

        Generate(b => b.HasDefaultSchema("app"), operation);

        AssertSql("DROP COLLATION dummy;");
    }

    [Fact]
    public void Collation_move_matches_existing_schemas_before_names()
    {
        var operation = new AlterDatabaseOperation();
        operation.AddAnnotation(NpgsqlAnnotationNames.CollationSchemasResolved, true);
        operation.OldDatabase.AddAnnotation(NpgsqlAnnotationNames.CollationSchemasResolved, true);
        PostgresCollation.GetOrAddCollation(operation, "other", "dummy", "POSIX", "POSIX", "libc");
        PostgresCollation.GetOrAddCollation(operation, "fixed", "dummy", "POSIX", "POSIX", "libc");
        PostgresCollation.GetOrAddCollation(operation.OldDatabase, "fixed", "dummy", "POSIX", "POSIX", "libc");
        PostgresCollation.GetOrAddCollation(operation.OldDatabase, "app", "dummy", "POSIX", "POSIX", "libc");

        Generate(operation);

        AssertSql(
            """
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'other') THEN
        CREATE SCHEMA other;
    END IF;
END $EF$;
GO

ALTER COLLATION app.dummy SET SCHEMA other;
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Collation_move_to_search_path_schema(bool idempotent)
    {
        var operation = new AlterDatabaseOperation();
        operation.AddAnnotation(NpgsqlAnnotationNames.CollationSchemasResolved, true);
        operation.OldDatabase.AddAnnotation(NpgsqlAnnotationNames.CollationSchemasResolved, true);
        PostgresCollation.GetOrAddCollation(operation, null, "dummy", "POSIX", "POSIX", "libc");
        PostgresCollation.GetOrAddCollation(operation.OldDatabase, "app", "dummy", "POSIX", "POSIX", "libc");

        Generate(operation, idempotent ? MigrationsSqlGenerationOptions.Idempotent : MigrationsSqlGenerationOptions.Default);

        AssertSql(
            idempotent
                ? "    EXECUTE format('ALTER COLLATION %s SET SCHEMA %I', 'app.dummy', current_schema());"
                : """
DO $EF$
BEGIN
    EXECUTE format('ALTER COLLATION %s SET SCHEMA %I', 'app.dummy', current_schema());
END $EF$;
""");
    }

    public override void AddColumnOperation_without_column_type()
    {
        base.AddColumnOperation_without_column_type();

        AssertSql(
            """
ALTER TABLE "People" ADD "Alias" text NOT NULL;

""");
    }

    public override void AddColumnOperation_with_unicode_overridden()
    {
        base.AddColumnOperation_with_unicode_overridden();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Name" text;

""");
    }

    public override void AddColumnOperation_with_unicode_no_model()
    {
        base.AddColumnOperation_with_unicode_no_model();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Name" text;

""");
    }

    public override void AddColumnOperation_with_fixed_length_no_model()
    {
        base.AddColumnOperation_with_fixed_length_no_model();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Name" character(100);

""");
    }

    public override void AddColumnOperation_with_maxLength_overridden()
    {
        base.AddColumnOperation_with_maxLength_overridden();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Name" character varying(32);

""");
    }

    public override void AddColumnOperation_with_maxLength_no_model()
    {
        base.AddColumnOperation_with_maxLength_no_model();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Name" character varying(30);

""");
    }

    public override void AddColumnOperation_with_precision_and_scale_overridden()
    {
        base.AddColumnOperation_with_precision_and_scale_overridden();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Pi" numeric(15,10) NOT NULL;

""");
    }

    public override void AddColumnOperation_with_precision_and_scale_no_model()
    {
        base.AddColumnOperation_with_precision_and_scale_no_model();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Pi" numeric(20,7) NOT NULL;

""");
    }

    public override void AddForeignKeyOperation_without_principal_columns()
    {
        base.AddForeignKeyOperation_without_principal_columns();

        AssertSql(
            """
ALTER TABLE "People" ADD FOREIGN KEY ("SpouseId") REFERENCES "People";

""");
    }

    public override void AlterColumnOperation_without_column_type()
    {
        base.AlterColumnOperation_without_column_type();

        AssertSql(
            """
ALTER TABLE "People" ALTER COLUMN "LuckyNumber" TYPE integer;

""");
    }

    public override void RenameTableOperation_legacy()
    {
        base.RenameTableOperation_legacy();

        AssertSql(
            """
ALTER TABLE dbo."People" RENAME TO "Person";

""");
    }

    public override void RenameTableOperation()
    {
        base.RenameTableOperation();

        AssertSql(
            """
ALTER TABLE dbo."People" RENAME TO "Person";

""");
    }

    public override void SqlOperation()
    {
        base.SqlOperation();

        AssertSql(
            """
-- I <3 DDL

""");
    }

    public override void InsertDataOperation_all_args_spatial()
    {
        base.InsertDataOperation_all_args_spatial();

        AssertSql(
            """
INSERT INTO dbo."People" ("Id", "Full Name", "Geometry")
VALUES (0, NULL, NULL);
INSERT INTO dbo."People" ("Id", "Full Name", "Geometry")
VALUES (1, 'Daenerys Targaryen', NULL);
INSERT INTO dbo."People" ("Id", "Full Name", "Geometry")
VALUES (2, 'John Snow', NULL);
INSERT INTO dbo."People" ("Id", "Full Name", "Geometry")
VALUES (3, 'Arya Stark', NULL);
INSERT INTO dbo."People" ("Id", "Full Name", "Geometry")
VALUES (4, 'Harry Strickland', NULL);
INSERT INTO dbo."People" ("Id", "Full Name", "Geometry")
VALUES (5, 'The Imp', NULL);
INSERT INTO dbo."People" ("Id", "Full Name", "Geometry")
VALUES (6, 'The Kingslayer', NULL);
INSERT INTO dbo."People" ("Id", "Full Name", "Geometry")
VALUES (7, 'Aemon Targaryen', GEOMETRY 'SRID=4326;GEOMETRYCOLLECTION Z(LINESTRING Z(1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 7.1 7.2 NaN), LINESTRING Z(7.1 7.2 NaN, 20.2 20.2 NaN, 20.2 1.1 NaN, 70.1 70.2 NaN), MULTIPOINT Z((1.1 2.2 NaN), (2.2 2.2 NaN), (2.2 1.1 NaN)), POLYGON Z((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 1.1 2.2 NaN)), POLYGON Z((10.1 20.2 NaN, 20.2 20.2 NaN, 20.2 10.1 NaN, 10.1 20.2 NaN)), POINT Z(1.1 2.2 3.3), MULTILINESTRING Z((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 7.1 7.2 NaN), (7.1 7.2 NaN, 20.2 20.2 NaN, 20.2 1.1 NaN, 70.1 70.2 NaN)), MULTIPOLYGON Z(((10.1 20.2 NaN, 20.2 20.2 NaN, 20.2 10.1 NaN, 10.1 20.2 NaN)), ((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 1.1 2.2 NaN))))');

""");
    }

    public override void InsertDataOperation_required_args()
    {
        base.InsertDataOperation_required_args();

        AssertSql(
            """
INSERT INTO dbo."People" ("First Name")
VALUES ('John');

""");
    }

    public override void InsertDataOperation_required_args_composite()
    {
        base.InsertDataOperation_required_args_composite();

        AssertSql(
            """
INSERT INTO dbo."People" ("First Name", "Last Name")
VALUES ('John', 'Snow');

""");
    }

    public override void InsertDataOperation_required_args_multiple_rows()
    {
        base.InsertDataOperation_required_args_multiple_rows();

        AssertSql(
            """
INSERT INTO dbo."People" ("First Name")
VALUES ('John');
INSERT INTO dbo."People" ("First Name")
VALUES ('Daenerys');

""");
    }

    public override void DeleteDataOperation_all_args()
    {
        base.DeleteDataOperation_all_args();

        AssertSql(
            """
DELETE FROM "People"
WHERE "First Name" = 'Hodor';
DELETE FROM "People"
WHERE "First Name" = 'Daenerys';
DELETE FROM "People"
WHERE "First Name" = 'John';
DELETE FROM "People"
WHERE "First Name" = 'Arya';
DELETE FROM "People"
WHERE "First Name" = 'Harry';

""");
    }

    public override void DeleteDataOperation_all_args_composite()
    {
        base.DeleteDataOperation_all_args_composite();

        AssertSql(
            """
DELETE FROM "People"
WHERE "First Name" = 'Hodor' AND "Last Name" IS NULL;
DELETE FROM "People"
WHERE "First Name" = 'Daenerys' AND "Last Name" = 'Targaryen';
DELETE FROM "People"
WHERE "First Name" = 'John' AND "Last Name" = 'Snow';
DELETE FROM "People"
WHERE "First Name" = 'Arya' AND "Last Name" = 'Stark';
DELETE FROM "People"
WHERE "First Name" = 'Harry' AND "Last Name" = 'Strickland';

""");
    }

    public override void DeleteDataOperation_required_args()
    {
        base.DeleteDataOperation_required_args();

        AssertSql(
            """
DELETE FROM "People"
WHERE "Last Name" = 'Snow';

""");
    }

    public override void DeleteDataOperation_required_args_composite()
    {
        base.DeleteDataOperation_required_args_composite();

        AssertSql(
            """
DELETE FROM "People"
WHERE "First Name" = 'John' AND "Last Name" = 'Snow';

""");
    }

    public override void UpdateDataOperation_all_args()
    {
        base.UpdateDataOperation_all_args();

        AssertSql(
            """
UPDATE "People" SET "Birthplace" = 'Winterfell', "House Allegiance" = 'Stark', "Culture" = 'Northmen'
WHERE "First Name" = 'Hodor';
UPDATE "People" SET "Birthplace" = 'Dragonstone', "House Allegiance" = 'Targaryen', "Culture" = 'Valyrian'
WHERE "First Name" = 'Daenerys';

""");
    }

    public override void UpdateDataOperation_all_args_composite()
    {
        base.UpdateDataOperation_all_args_composite();

        AssertSql(
            """
UPDATE "People" SET "House Allegiance" = 'Stark'
WHERE "First Name" = 'Hodor' AND "Last Name" IS NULL;
UPDATE "People" SET "House Allegiance" = 'Targaryen'
WHERE "First Name" = 'Daenerys' AND "Last Name" = 'Targaryen';

""");
    }

    public override void UpdateDataOperation_all_args_composite_multi()
    {
        base.UpdateDataOperation_all_args_composite_multi();

        AssertSql(
            """
UPDATE "People" SET "Birthplace" = 'Winterfell', "House Allegiance" = 'Stark', "Culture" = 'Northmen'
WHERE "First Name" = 'Hodor' AND "Last Name" IS NULL;
UPDATE "People" SET "Birthplace" = 'Dragonstone', "House Allegiance" = 'Targaryen', "Culture" = 'Valyrian'
WHERE "First Name" = 'Daenerys' AND "Last Name" = 'Targaryen';

""");
    }

    public override void UpdateDataOperation_all_args_multi()
    {
        base.UpdateDataOperation_all_args_multi();

        AssertSql(
            """
UPDATE "People" SET "Birthplace" = 'Dragonstone', "House Allegiance" = 'Targaryen', "Culture" = 'Valyrian'
WHERE "First Name" = 'Daenerys';

""");
    }

    public override void UpdateDataOperation_required_args()
    {
        base.UpdateDataOperation_required_args();

        AssertSql(
            """
UPDATE "People" SET "House Allegiance" = 'Targaryen'
WHERE "First Name" = 'Daenerys';

""");
    }

    public override void UpdateDataOperation_required_args_multiple_rows()
    {
        base.UpdateDataOperation_required_args_multiple_rows();

        AssertSql(
            """
UPDATE "People" SET "House Allegiance" = 'Stark'
WHERE "First Name" = 'Hodor';
UPDATE "People" SET "House Allegiance" = 'Targaryen'
WHERE "First Name" = 'Daenerys';

""");
    }

    public override void UpdateDataOperation_required_args_composite()
    {
        base.UpdateDataOperation_required_args_composite();

        AssertSql(
            """
UPDATE "People" SET "House Allegiance" = 'Targaryen'
WHERE "First Name" = 'Daenerys' AND "Last Name" = 'Targaryen';

""");
    }

    public override void UpdateDataOperation_required_args_composite_multi()
    {
        base.UpdateDataOperation_required_args_composite_multi();

        AssertSql(
            """
UPDATE "People" SET "Birthplace" = 'Dragonstone', "House Allegiance" = 'Targaryen', "Culture" = 'Valyrian'
WHERE "First Name" = 'Daenerys' AND "Last Name" = 'Targaryen';

""");
    }

    public override void UpdateDataOperation_required_args_multi()
    {
        base.UpdateDataOperation_required_args_multi();

        AssertSql(
            """
UPDATE "People" SET "Birthplace" = 'Dragonstone', "House Allegiance" = 'Targaryen', "Culture" = 'Valyrian'
WHERE "First Name" = 'Daenerys';

""");
    }

    public override void DefaultValue_with_line_breaks(bool isUnicode)
    {
        // https://github.com/npgsql/efcore.pg/issues/1478
    }

    public override void DefaultValue_with_line_breaks_2(bool isUnicode)
    {
        // https://github.com/npgsql/efcore.pg/issues/1478
    }

    public override void Sequence_restart_operation(long? startsAt)
    {
        base.Sequence_restart_operation(startsAt);

        AssertSql(
            startsAt.HasValue
                ? $"""
ALTER SEQUENCE dbo."TestRestartSequenceOperation" START WITH {startsAt};
ALTER SEQUENCE dbo."TestRestartSequenceOperation" RESTART;
"""
                : """ALTER SEQUENCE dbo."TestRestartSequenceOperation" RESTART;""");
    }

    [Theory]
    [InlineData(MigrationsSqlGenerationOptions.Default)]
    [InlineData(MigrationsSqlGenerationOptions.Idempotent)]
    public void Alter_column_change_serial_to_identity_idempotent(MigrationsSqlGenerationOptions options)
    {
        Generate(
            modelBuilder =>
            {
                modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, "3.1.0");
                modelBuilder.Entity<Person>().Property<int>("Id").UseSerialColumn();
            },
            [
                new AlterColumnOperation
                {
                    Table = "Person",
                    Name = "Id",
                    ClrType = typeof(int),
                    [NpgsqlAnnotationNames.ValueGenerationStrategy] =
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn,
                    OldColumn = new AddColumnOperation
                    {
                        Table = "Person",
                        Name = "Id",
                        ClrType = typeof(int),
                        [NpgsqlAnnotationNames.ValueGenerationStrategy] =
                            NpgsqlValueGenerationStrategy.SerialColumn,
                    }
                }
            ],
            options);

        AssertSql(
            $"""
ALTER SEQUENCE "Person_Id_seq" RENAME TO "Person_Id_old_seq";
ALTER TABLE "Person" ALTER COLUMN "Id" DROP DEFAULT;
ALTER TABLE "Person" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY;
{(options == MigrationsSqlGenerationOptions.Idempotent ? "PERFORM" : "SELECT")} * FROM setval('"Person_Id_seq"', nextval('"Person_Id_old_seq"'), false);
DROP SEQUENCE "Person_Id_old_seq";

""");
    }

    [Fact]
    public void Create_schema_idempotent()
    {
        Generate(
            _ => { },
            [new EnsureSchemaOperation { Name = "some_schema" }],
            MigrationsSqlGenerationOptions.Idempotent);

        AssertSql(
            """
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'some_schema') THEN
        CREATE SCHEMA some_schema;
    END IF;

""");
    }

    [Theory]
    [InlineData(false, MigrationsSqlGenerationOptions.Default)]
    [InlineData(false, MigrationsSqlGenerationOptions.Idempotent)]
    [InlineData(true, MigrationsSqlGenerationOptions.Default)]
    [InlineData(true, MigrationsSqlGenerationOptions.Script)]
    [InlineData(true, MigrationsSqlGenerationOptions.Script | MigrationsSqlGenerationOptions.NoTransactions)]
    public void Create_index_with_concurrent_creation(bool concurrently, MigrationsSqlGenerationOptions options)
    {
        Generate(
            new CreateIndexOperation
            {
                Name = "IX_People_Age",
                Table = "People",
                Columns = ["Age"],
                [NpgsqlAnnotationNames.CreatedConcurrently] = concurrently
            },
            options);

        AssertSql($"""CREATE INDEX{(concurrently ? " CONCURRENTLY" : "")} "IX_People_Age" ON "People" ("Age");""");
    }

    [Theory]
    [InlineData(MigrationsSqlGenerationOptions.Idempotent)]
    [InlineData(MigrationsSqlGenerationOptions.Idempotent | MigrationsSqlGenerationOptions.Script)]
    [InlineData(MigrationsSqlGenerationOptions.Idempotent | MigrationsSqlGenerationOptions.Script | MigrationsSqlGenerationOptions.NoTransactions)]
    public void Create_index_concurrently_idempotent_throws(MigrationsSqlGenerationOptions options)
        => Assert.Equal(
            "Cannot generate an idempotent migration script for index 'IX_People_Age' because "
            + "PostgreSQL does not allow CREATE INDEX CONCURRENTLY inside the PL/pgSQL DO blocks used to check migration history. "
            + "Generate a non-idempotent script, or apply migrations using 'dotnet ef database update' or a migration bundle.",
            Assert.Throws<NotSupportedException>(
                () => Generate(
                    new CreateIndexOperation
                    {
                        Name = "IX_People_Age",
                        Table = "People",
                        Columns = ["Age"],
                        [NpgsqlAnnotationNames.CreatedConcurrently] = true
                    },
                    options)).Message);

    #region JSON indexes

    [Theory]
    [InlineData("Details.Slug", """("Details" ->> 'Slug')""")]
    [InlineData("Details.Rating", """(CAST("Details" ->> 'Rating' AS integer))""")]
    [InlineData("Details.Enabled", """(CAST("Details" ->> 'Enabled' AS boolean))""")]
    [InlineData("Details.Token", """(CAST("Details" ->> 'Token' AS uuid))""")]
    [InlineData("Details.Bytes", """(decode("Details" ->> 'Bytes', 'base64'))""")]
    [InlineData("Details.Tags", """("Details" -> 'Tags')""")]
    [InlineData("Details.Nested", """("Details" -> 'Nested')""")]
    [InlineData("Details.Nested.Value", """("Details" #>> '{Nested,Value}')""")]
    [InlineData("Details.Nested.Number", """(CAST("Details" #>> '{Nested,Number}' AS integer))""")]
    public void Create_json_member_index(string property, string expression)
    {
        Generate(
            b =>
            {
                BuildJsonIndexModel(b);
                b.Entity<JsonIndexBlog>().HasIndex(property).HasDatabaseName("IX_Json");
            },
            new CreateIndexOperation { Name = "IX_Json", Table = "Blogs", Columns = ["Details"] });

        AssertSql($"""CREATE INDEX "IX_Json" ON "Blogs" ({expression});""");
    }

    [Fact]
    public void Create_json_member_index_with_mapped_names()
    {
        Generate(
            b =>
            {
                BuildJsonIndexModel(b);
                var json = b.Entity<JsonIndexBlog>().ComplexProperty(e => e.Details);
                json.ComplexProperty(e => e.Nested).HasJsonPropertyName("odd,key");
                json.ComplexProperty(e => e.Nested).Property(e => e.Value).HasJsonPropertyName("v'alue");
                b.Entity<JsonIndexBlog>().HasIndex(e => e.Details.Nested.Value).HasDatabaseName("IX_Json");
            },
            new CreateIndexOperation { Name = "IX_Json", Table = "Blogs", Columns = ["Details"] });

        AssertSql("""CREATE INDEX "IX_Json" ON "Blogs" (("Details" #>> ARRAY['odd,key','v''alue']::text[]));""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_json_member_composite_index(bool descending)
    {
        Generate(
            b =>
            {
                BuildJsonIndexModel(b);
                var index = b.Entity<JsonIndexBlog>().HasIndex(e => new { e.Details.Slug, e.Details.Rating })
                    .HasDatabaseName("IX_Json");
                if (descending)
                {
                    index.IsDescending();
                }
            },
            new CreateIndexOperation
            {
                Name = "IX_Json",
                Table = "Blogs",
                Columns = ["Details"],
                IsDescending = descending ? [] : null
            });

        var direction = descending ? " DESC" : "";
        AssertSql(
            $"""CREATE INDEX "IX_Json" ON "Blogs" (("Details" ->> 'Slug'){direction}, (CAST("Details" ->> 'Rating' AS integer)){direction});""");
    }

    [Fact]
    public void Create_json_member_index_with_options()
    {
        Generate(
            b =>
            {
                BuildJsonIndexModel(b);
                b.Entity<JsonIndexBlog>().HasIndex(e => e.Details.Slug).HasDatabaseName("IX_Json");
            },
            new CreateIndexOperation
            {
                Name = "IX_Json",
                Table = "Blogs",
                Columns = ["Details"],
                IsUnique = true,
                IsDescending = [true],
                Filter = "\"Id\" > 0",
                [NpgsqlAnnotationNames.CreatedConcurrently] = true,
                [NpgsqlAnnotationNames.IndexOperators] = new[] { "text_ops" },
                [RelationalAnnotationNames.Collation] = new[] { "C" },
                [NpgsqlAnnotationNames.IndexNullSortOrder] = new[] { NullSortOrder.NullsLast },
                [NpgsqlAnnotationNames.IndexInclude] = new[] { "Id" },
                [NpgsqlAnnotationNames.NullsDistinct] = false
            });

        AssertSql(
            """CREATE UNIQUE INDEX CONCURRENTLY "IX_Json" ON "Blogs" (("Details" ->> 'Slug') COLLATE "C" text_ops DESC NULLS LAST) INCLUDE ("Id") NULLS NOT DISTINCT WHERE "Id" > 0;""");
    }

    [Fact]
    public void Create_json_member_composite_index_with_per_key_options()
    {
        Generate(
            b =>
            {
                BuildJsonIndexModel(b);
                b.Entity<JsonIndexBlog>().HasIndex(e => new { e.Details.Slug, e.Details.Rating }).HasDatabaseName("IX_Json");
            },
            new CreateIndexOperation
            {
                Name = "IX_Json",
                Table = "Blogs",
                Columns = ["Details"],
                [RelationalAnnotationNames.Collation] = new[] { "C", null! },
                [NpgsqlAnnotationNames.IndexOperators] = new[] { "text_pattern_ops", "int4_ops" },
                [NpgsqlAnnotationNames.IndexNullSortOrder] = new[] { NullSortOrder.NullsLast, NullSortOrder.NullsFirst }
            });

        AssertSql(
            """CREATE INDEX "IX_Json" ON "Blogs" (("Details" ->> 'Slug') COLLATE "C" text_pattern_ops NULLS LAST, (CAST("Details" ->> 'Rating' AS integer)) int4_ops NULLS FIRST);""");
    }

    [Theory]
    [InlineData("Details", "\"Details\"")]
    [InlineData("Details.Tags", """("Details" -> 'Tags')""")]
    [InlineData("Details.Nested", """("Details" -> 'Nested')""")]
    public void Create_json_gin_index(string property, string expression)
    {
        Generate(
            b =>
            {
                BuildJsonIndexModel(b);
                b.Entity<JsonIndexBlog>().HasIndex(property).HasDatabaseName("IX_Json").HasMethod("gin");
            },
            new CreateIndexOperation
            {
                Name = "IX_Json",
                Table = "Blogs",
                Columns = ["Details"],
                [NpgsqlAnnotationNames.IndexMethod] = "gin",
                [NpgsqlAnnotationNames.IndexOperators] = new[] { "jsonb_path_ops" }
            });

        AssertSql($"""CREATE INDEX "IX_Json" ON "Blogs" USING gin ({expression} jsonb_path_ops);""");
    }

    [Theory]
    [InlineData("json")]
    [InlineData("jsonb")]
    public void Create_json_member_index_with_container_type(string storeType)
    {
        Generate(
            b =>
            {
                BuildJsonIndexModel(b);
                b.Entity<JsonIndexBlog>().ComplexProperty(e => e.Details).HasColumnType(storeType);
                b.Entity<JsonIndexBlog>().HasIndex(e => e.Details.Slug).HasDatabaseName("IX_Json");
            },
            new CreateIndexOperation { Name = "IX_Json", Table = "Blogs", Columns = ["Details"] });

        AssertSql("""CREATE INDEX "IX_Json" ON "Blogs" (("Details" ->> 'Slug'));""");
    }

    [Theory]
    [InlineData("Details.Items[1].Value", """("Details" #>> '{Items,1,Value}')""")]
    [InlineData("Details.Items[1].Number", """(CAST("Details" #>> '{Items,1,Number}' AS integer))""")]
    [InlineData("Details.Items[1]", """("Details" #> '{Items,1}')""")]
    public void Create_json_index_with_fixed_collection_position(string property, string expression)
    {
        Generate(
            b =>
            {
                BuildJsonIndexModel(b);
                b.Entity<JsonIndexBlog>().HasIndex(property).HasDatabaseName("IX_Json");
            },
            new CreateIndexOperation { Name = "IX_Json", Table = "Blogs", Columns = ["Details"] });

        AssertSql($"""CREATE INDEX "IX_Json" ON "Blogs" ({expression});""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Json_member_index_uses_operation_or_target_model_metadata(bool removeAnnotation)
    {
        using var context = new JsonIndexContext(b => b.Entity<JsonIndexBlog>().HasIndex(e => e.Details.Rating));
        var model = context.GetService<IDesignTimeModel>().Model;
        var operation = Assert.Single(
            context.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel())
                .OfType<CreateIndexOperation>());
        Assert.NotNull(operation[RelationalAnnotationNames.JsonIndex]);
        if (removeAnnotation)
        {
            operation.RemoveAnnotation(RelationalAnnotationNames.JsonIndex);
        }

        var command = Assert.Single(context.GetService<IMigrationsSqlGenerator>().Generate([operation], model));
        Assert.Equal(
            """CREATE INDEX "IX_Blogs_Details_Rating" ON "Blogs" ((CAST("Details" ->> 'Rating' AS integer)));""",
            command.CommandText.TrimEnd());
        Assert.Contains(
            """CAST(b."Details" ->> 'Rating' AS integer)""",
            context.Set<JsonIndexBlog>().Where(b => b.Details.Rating > 0).ToQueryString());
    }

    [Fact]
    public void Json_member_index_without_metadata_does_not_fall_back_to_document_index()
        => Assert.Equal(
            "Cannot generate SQL for JSON member index 'IX_Json' without its JSON index metadata. Supply the migration target model.",
            Assert.Throws<InvalidOperationException>(
                () => Generate(
                    new CreateIndexOperation
                    {
                        Name = "IX_Json",
                        Table = "Blogs",
                        Columns = ["Details"],
                        [NpgsqlAnnotationNames.JsonIndexTypes] = new[] { "integer" }
                    })).Message);

    [ConditionalFact]
    public async Task Json_member_index_enforces_uniqueness()
    {
        await using var store = await NpgsqlTestStore.CreateInitializedAsync("JsonMemberIndexUniqueness");
        await using var context = new JsonIndexContext(
            b => b.Entity<JsonIndexBlog>().HasIndex(e => e.Details.Slug).IsUnique(), store.ConnectionString);
        await context.Database.EnsureCreatedAsync();

        context.Add(new JsonIndexBlog { Details = new JsonIndexDetails { Slug = "same", Rating = 1 } });
        await context.SaveChangesAsync();
        context.Add(new JsonIndexBlog { Details = new JsonIndexDetails { Slug = "same", Rating = 2 } });
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(exception.InnerException).SqlState);

        context.ChangeTracker.Clear();
        context.AddRange(
            new JsonIndexBlog { Details = new JsonIndexDetails { Slug = null, Rating = 3 } },
            new JsonIndexBlog { Details = new JsonIndexDetails { Slug = null, Rating = 4 } });
        await context.SaveChangesAsync();
        Assert.Equal(3, await context.Set<JsonIndexBlog>().CountAsync());
    }

    [ConditionalTheory]
    [InlineData("json")]
    [InlineData("jsonb")]
    public async Task Json_member_indexes_support_queries(string storeType)
    {
        await using var store = await NpgsqlTestStore.CreateInitializedAsync("JsonMemberIndexTypes_" + storeType);
        await using var context = new JsonIndexContext(
            b =>
            {
                var blog = b.Entity<JsonIndexBlog>();
                blog.ComplexProperty(e => e.Details).HasColumnType(storeType);
                blog.HasIndex(e => e.Details.Rating);
                blog.HasIndex(e => e.Details.Token);
                blog.HasIndex(e => e.Details.Bytes);
                blog.HasIndex(e => e.Details.Nested.Value);
                blog.HasIndex(e => e.Details.Items[1].Number);
                if (storeType == "jsonb")
                {
                    blog.HasIndex(e => e.Details.Tags).HasMethod("gin").HasOperators("jsonb_path_ops");
                    blog.HasIndex(e => e.Details.Nested).HasMethod("gin");
                    blog.HasIndex(e => e.Details.Items).HasMethod("gin");
                }
            },
            store.ConnectionString);
        await context.Database.EnsureCreatedAsync();

        var token = Guid.NewGuid();
        byte[] bytes = [1, 2, 3];
        var blog = new JsonIndexBlog
        {
            Details = new JsonIndexDetails
            {
                Rating = 42,
                Token = token,
                Bytes = bytes,
                Tags = ["tag"],
                Nested = new JsonIndexNested { Value = "nested" },
                Items = [new JsonIndexNested(), new JsonIndexNested { Number = 7 }]
            }
        };
        context.Add(blog);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        Assert.Equal(
            blog.Id,
            await context.Set<JsonIndexBlog>()
                .Where(b => b.Details.Rating == 42 && b.Details.Token == token && b.Details.Bytes == bytes
                    && b.Details.Nested.Value == "nested" && b.Details.Items[1].Number == 7)
                .Select(b => b.Id).SingleAsync());
        if (storeType == "jsonb")
        {
            Assert.Equal(
                blog.Id,
                await context.Set<JsonIndexBlog>().Where(b => EF.Functions.JsonContains(b.Details.Tags, "[\"tag\"]"))
                    .Select(b => b.Id).SingleAsync());
        }
    }

    [ConditionalFact]
    public async Task Json_member_index_migrations_up_and_down()
    {
        await using var store = await NpgsqlTestStore.CreateInitializedAsync("JsonMemberIndexMigrations");
        await using var source = new JsonIndexContext(
            b => b.Entity<JsonIndexBlog>().HasIndex(e => e.Details.Slug).HasDatabaseName("IX_Json"), store.ConnectionString);
        await using var target = new JsonIndexContext(
            b => b.Entity<JsonIndexBlog>().HasIndex(e => e.Details.Rating).HasDatabaseName("IX_Json"), store.ConnectionString);
        await source.Database.EnsureCreatedAsync();

        (JsonIndexContext Source, JsonIndexContext Target)[] steps = [(source, target), (target, source)];
        foreach (var (from, to) in steps)
        {
            var model = to.GetService<IDesignTimeModel>().Model;
            var operations = to.GetService<IMigrationsModelDiffer>().GetDifferences(
                from.GetService<IDesignTimeModel>().Model.GetRelationalModel(), model.GetRelationalModel());
            Assert.Collection(operations, o => Assert.IsType<DropIndexOperation>(o), o => Assert.IsType<CreateIndexOperation>(o));
            Assert.IsType<CreateIndexOperation>(operations[1]).RemoveAnnotation(RelationalAnnotationNames.JsonIndex);
            var sql = string.Concat(to.GetService<IMigrationsSqlGenerator>().Generate(operations, model).Select(c => c.CommandText));
            await to.Database.ExecuteSqlRawAsync(sql);
            var definition = await to.Database.SqlQueryRaw<string>(
                    """SELECT indexdef AS "Value" FROM pg_indexes WHERE indexname = 'IX_Json'""")
                .SingleAsync();
            Assert.Contains(ReferenceEquals(to, target) ? "Rating" : "Slug", definition);
        }
    }

    private static void BuildJsonIndexModel(ModelBuilder builder)
        => builder.Entity<JsonIndexBlog>(
            b =>
            {
                b.ToTable("Blogs");
                b.ComplexProperty(
                    e => e.Details, d =>
                    {
                        d.ToJson();
                        d.ComplexProperty(e => e.Nested);
                        d.ComplexCollection(e => e.Items);
                    });
            });

    private class JsonIndexContext(Action<ModelBuilder> configureModel, string? connectionString = null) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(connectionString).EnableServiceProviderCaching(false);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildJsonIndexModel(modelBuilder);
            configureModel(modelBuilder);
        }
    }

    private class JsonIndexBlog
    {
        public int Id { get; set; }
        public JsonIndexDetails Details { get; set; } = new();
    }

    private class JsonIndexDetails
    {
        public string? Slug { get; set; }
        public int Rating { get; set; }
        public bool Enabled { get; set; }
        public Guid Token { get; set; }
        public byte[] Bytes { get; set; } = [];
        public List<string> Tags { get; set; } = [];
        public JsonIndexNested Nested { get; set; } = new();
        public List<JsonIndexNested> Items { get; set; } = [];
    }

    private class JsonIndexNested
    {
        public string Value { get; set; } = "";
        public int Number { get; set; }
    }

    #endregion JSON indexes

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
                PrimaryKey = new AddPrimaryKeyOperation { Columns = ["Id"] }
            };

        var interleaveInParent = new CockroachDbInterleaveInParent(op);
        interleaveInParent.ParentTableSchema = "my_schema";
        interleaveInParent.ParentTableName = "my_parent";
        interleaveInParent.InterleavePrefix = ["col_a", "col_b"];

        Generate(op);

        AssertSql(
            """
CREATE TABLE dbo."People" (
    "Id" integer NOT NULL,
    PRIMARY KEY ("Id")
)
INTERLEAVE IN PARENT my_schema.my_parent (col_a, col_b);

""");
    }

    #endregion CockroachDB interleave-in-parent

#pragma warning disable 618
    [Fact]
    public virtual void AddColumnOperation_serial_old_annotation_throws()
        => Assert.Throws<NotSupportedException>(
            () =>
                Generate(
                    new AddColumnOperation
                    {
                        Table = "People",
                        Name = "foo",
                        ClrType = typeof(int),
                        ColumnType = "int",
                        IsNullable = false,
                        [NpgsqlAnnotationNames.ValueGeneratedOnAdd] = true
                    }));

    public override void InsertDataOperation_throws_for_unsupported_column_types()
        => Assert.Equal(
            RelationalStrings.UnsupportedDataOperationStoreType("foo", "dbo.People.First Name"),
            Assert.Throws<InvalidOperationException>(
                () =>
                    Generate(
                        new InsertDataOperation
                        {
                            Table = "People",
                            Schema = "dbo",
                            Columns = ["First Name"],
                            ColumnTypes = ["foo"],
                            Values = new object?[,] { { null } }
                        })).Message);

#pragma warning restore 618

    [Theory]
    [InlineData(MigrationsSqlGenerationOptions.Default)]
    [InlineData(MigrationsSqlGenerationOptions.Script)]
    [InlineData(MigrationsSqlGenerationOptions.Script | MigrationsSqlGenerationOptions.NoTransactions)]
    public virtual void DropIndexOperation_concurrently(MigrationsSqlGenerationOptions options)
    {
        Generate(
            new DropIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                [NpgsqlAnnotationNames.CreatedConcurrently] = true
            },
            options);

        AssertSql(
            """
DROP INDEX CONCURRENTLY dbo."IX_People_Name";

""");
    }

    [Theory]
    [InlineData(null, MigrationsSqlGenerationOptions.Default)]
    [InlineData(null, MigrationsSqlGenerationOptions.Idempotent)]
    [InlineData(false, MigrationsSqlGenerationOptions.Default)]
    [InlineData(false, MigrationsSqlGenerationOptions.Idempotent)]
    public virtual void DropIndexOperation_not_concurrently(bool? concurrently, MigrationsSqlGenerationOptions options)
    {
        Generate(
            new DropIndexOperation
            {
                Name = "IX_People_Name",
                Table = "People",
                Schema = "dbo",
                [NpgsqlAnnotationNames.CreatedConcurrently] = concurrently
            },
            options);

        AssertSql(
            """
DROP INDEX dbo."IX_People_Name";

""");
    }

    [Theory]
    [InlineData(MigrationsSqlGenerationOptions.Idempotent)]
    [InlineData(MigrationsSqlGenerationOptions.Idempotent | MigrationsSqlGenerationOptions.Script)]
    [InlineData(MigrationsSqlGenerationOptions.Idempotent | MigrationsSqlGenerationOptions.Script | MigrationsSqlGenerationOptions.NoTransactions)]
    public void DropIndexOperation_concurrently_idempotent_throws(MigrationsSqlGenerationOptions options)
        => Assert.Equal(
            "Cannot generate an idempotent migration script for index 'IX_People_Name' because "
            + "PostgreSQL does not allow DROP INDEX CONCURRENTLY inside the PL/pgSQL DO blocks used to check migration history. "
            + "Generate a non-idempotent script, or apply migrations using 'dotnet ef database update' or a migration bundle.",
            Assert.Throws<NotSupportedException>(
                () => Generate(
                    new DropIndexOperation
                    {
                        Name = "IX_People_Name",
                        Table = "People",
                        Schema = "dbo",
                        [NpgsqlAnnotationNames.CreatedConcurrently] = true
                    },
                    options)).Message);

    protected override string GetGeometryCollectionStoreType()
        => "GEOMETRY(GEOMETRYCOLLECTION)";
}
