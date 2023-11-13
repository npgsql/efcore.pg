using System.Text;
using Microsoft.EntityFrameworkCore.TestModels.StoreValueGenerationModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update;

#nullable enable

public class StoreValueGenerationNpgsqlTest : StoreValueGenerationTestBase<
    StoreValueGenerationNpgsqlTest.StoreValueGenerationNpgsqlFixture>
{
    public StoreValueGenerationNpgsqlTest(StoreValueGenerationNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override bool ShouldCreateImplicitTransaction(
        EntityState firstOperationType,
        EntityState? secondOperationType,
        GeneratedValues generatedValues,
        bool withSameEntityType)
        => secondOperationType is not null;

    protected override int ShouldExecuteInNumberOfCommands(
        EntityState firstOperationType,
        EntityState? secondOperationType,
        GeneratedValues generatedValues,
        bool withDatabaseGenerated)
        => 1;

    #region Single operation

    public override async Task Add_with_generated_values(bool async)
    {
        await base.Add_with_generated_values(async);

        AssertSql(
            """
@p0='1000'

INSERT INTO "WithSomeDatabaseGenerated" ("Data2")
VALUES (@p0)
RETURNING "Id", "Data1";
""");
    }

    public override async Task Add_with_no_generated_values(bool async)
    {
        await base.Add_with_no_generated_values(async);

        AssertSql(
            """
@p0='100'
@p1='1000'
@p2='1000'

INSERT INTO "WithNoDatabaseGenerated" ("Id", "Data1", "Data2")
VALUES (@p0, @p1, @p2);
""");
    }

    public override async Task Add_with_all_generated_values(bool async)
    {
        await base.Add_with_all_generated_values(async);

        AssertSql(
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES
RETURNING "Id", "Data1", "Data2";
""");
    }

    public override async Task Modify_with_generated_values(bool async)
    {
        await base.Modify_with_generated_values(async);

        AssertSql(
            """
@p1='1'
@p0='1000'

UPDATE "WithSomeDatabaseGenerated" SET "Data2" = @p0
WHERE "Id" = @p1
RETURNING "Data1";
""");
    }

    public override async Task Modify_with_no_generated_values(bool async)
    {
        await base.Modify_with_no_generated_values(async);

        AssertSql(
            """
@p2='1'
@p0='1000'
@p1='1000'

UPDATE "WithNoDatabaseGenerated" SET "Data1" = @p0, "Data2" = @p1
WHERE "Id" = @p2;
""");
    }

    public override async Task Delete(bool async)
    {
        await base.Delete(async);

        AssertSql(
            """
@p0='1'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0;
""");
    }

    #endregion Single operation

    #region Two operations with same entity type

    public override async Task Add_Add_with_same_entity_type_and_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_generated_values(async);

        AssertSql(
            """
@p0='1000'
@p1='1001'

INSERT INTO "WithSomeDatabaseGenerated" ("Data2")
VALUES (@p0)
RETURNING "Id", "Data1";
INSERT INTO "WithSomeDatabaseGenerated" ("Data2")
VALUES (@p1)
RETURNING "Id", "Data1";
""");
    }

    public override async Task Add_Add_with_same_entity_type_and_no_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_no_generated_values(async);

        AssertSql(
            """
@p0='100'
@p1='1000'
@p2='1000'
@p3='101'
@p4='1001'
@p5='1001'

INSERT INTO "WithNoDatabaseGenerated" ("Id", "Data1", "Data2")
VALUES (@p0, @p1, @p2);
INSERT INTO "WithNoDatabaseGenerated" ("Id", "Data1", "Data2")
VALUES (@p3, @p4, @p5);
""");
    }

    public override async Task Add_Add_with_same_entity_type_and_all_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_all_generated_values(async);

        AssertSql(
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES
RETURNING "Id", "Data1", "Data2";
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES
RETURNING "Id", "Data1", "Data2";
""");
    }

    public override async Task Modify_Modify_with_same_entity_type_and_generated_values(bool async)
    {
        await base.Modify_Modify_with_same_entity_type_and_generated_values(async);

        AssertSql(
            """
@p1='1'
@p0='1000'
@p3='2'
@p2='1001'

UPDATE "WithSomeDatabaseGenerated" SET "Data2" = @p0
WHERE "Id" = @p1
RETURNING "Data1";
UPDATE "WithSomeDatabaseGenerated" SET "Data2" = @p2
WHERE "Id" = @p3
RETURNING "Data1";
""");
    }

    public override async Task Modify_Modify_with_same_entity_type_and_no_generated_values(bool async)
    {
        await base.Modify_Modify_with_same_entity_type_and_no_generated_values(async);

        AssertSql(
            """
@p2='1'
@p0='1000'
@p1='1000'
@p5='2'
@p3='1001'
@p4='1001'

UPDATE "WithNoDatabaseGenerated" SET "Data1" = @p0, "Data2" = @p1
WHERE "Id" = @p2;
UPDATE "WithNoDatabaseGenerated" SET "Data1" = @p3, "Data2" = @p4
WHERE "Id" = @p5;
""");
    }

    public override async Task Delete_Delete_with_same_entity_type(bool async)
    {
        await base.Delete_Delete_with_same_entity_type(async);

        AssertSql(
            """
@p0='1'
@p1='2'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0;
DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p1;
""");
    }

    #endregion Two operations with same entity type

    #region Two operations with different entity types

    public override async Task Add_Add_with_different_entity_types_and_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_generated_values(async);

        AssertSql(
            """
@p0='1000'
@p1='1001'

INSERT INTO "WithSomeDatabaseGenerated" ("Data2")
VALUES (@p0)
RETURNING "Id", "Data1";
INSERT INTO "WithSomeDatabaseGenerated2" ("Data2")
VALUES (@p1)
RETURNING "Id", "Data1";
""");
    }

    public override async Task Add_Add_with_different_entity_types_and_no_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_no_generated_values(async);

        AssertSql(
            """
@p0='100'
@p1='1000'
@p2='1000'
@p3='101'
@p4='1001'
@p5='1001'

INSERT INTO "WithNoDatabaseGenerated" ("Id", "Data1", "Data2")
VALUES (@p0, @p1, @p2);
INSERT INTO "WithNoDatabaseGenerated2" ("Id", "Data1", "Data2")
VALUES (@p3, @p4, @p5);
""");
    }

    public override async Task Add_Add_with_different_entity_types_and_all_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_all_generated_values(async);

        AssertSql(
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES
RETURNING "Id", "Data1", "Data2";
INSERT INTO "WithAllDatabaseGenerated2"
DEFAULT VALUES
RETURNING "Id", "Data1", "Data2";
""");
    }

    public override async Task Modify_Modify_with_different_entity_types_and_generated_values(bool async)
    {
        await base.Modify_Modify_with_different_entity_types_and_generated_values(async);

        AssertSql(
            """
@p1='1'
@p0='1000'
@p3='2'
@p2='1001'

UPDATE "WithSomeDatabaseGenerated" SET "Data2" = @p0
WHERE "Id" = @p1
RETURNING "Data1";
UPDATE "WithSomeDatabaseGenerated2" SET "Data2" = @p2
WHERE "Id" = @p3
RETURNING "Data1";
""");
    }

    public override async Task Modify_Modify_with_different_entity_types_and_no_generated_values(bool async)
    {
        await base.Modify_Modify_with_different_entity_types_and_no_generated_values(async);

        AssertSql(
            """
@p2='1'
@p0='1000'
@p1='1000'
@p5='2'
@p3='1001'
@p4='1001'

UPDATE "WithNoDatabaseGenerated" SET "Data1" = @p0, "Data2" = @p1
WHERE "Id" = @p2;
UPDATE "WithNoDatabaseGenerated2" SET "Data1" = @p3, "Data2" = @p4
WHERE "Id" = @p5;
""");
    }

    public override async Task Delete_Delete_with_different_entity_types(bool async)
    {
        await base.Delete_Delete_with_different_entity_types(async);

        AssertSql(
            """
@p0='1'
@p1='2'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0;
DELETE FROM "WithSomeDatabaseGenerated2"
WHERE "Id" = @p1;
""");
    }

    #endregion Two operations with different entity types

    public class StoreValueGenerationNpgsqlFixture : StoreValueGenerationFixtureBase
    {
        private string? _cleanDataSql;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            foreach (var name in new[]
                     {
                         nameof(StoreValueGenerationContext.WithSomeDatabaseGenerated),
                         nameof(StoreValueGenerationContext.WithSomeDatabaseGenerated2),
                         nameof(StoreValueGenerationContext.WithAllDatabaseGenerated),
                         nameof(StoreValueGenerationContext.WithAllDatabaseGenerated2)
                     })
            {
                ConfigureComputedColumn(modelBuilder.SharedTypeEntity<StoreValueGenerationData>(name).Property(w => w.Data1));
            }

            foreach (var name in new[]
                     {
                         nameof(StoreValueGenerationContext.WithAllDatabaseGenerated),
                         nameof(StoreValueGenerationContext.WithAllDatabaseGenerated2)
                     })
            {
                ConfigureComputedColumn(modelBuilder.SharedTypeEntity<StoreValueGenerationData>(name).Property(w => w.Data2));
            }

            void ConfigureComputedColumn(PropertyBuilder builder)
            {
                if (TestEnvironment.PostgresVersion >= new Version(12, 0))
                {
                    // PG 12+ supports computed columns, but only stored (must be explicitly specified)
                    builder.Metadata.SetIsStored(true);
                }
                else
                {
                    // Before PG 12, disable computed columns (but leave OnAddOrUpdate)
                    builder
                        .HasComputedColumnSql(null)
                        .HasDefaultValue(100)
                        .Metadata
                        .ValueGenerated = ValueGenerated.OnAddOrUpdate;
                }
            }
        }

        public override void CleanData()
        {
            using var context = CreateContext();
            context.Database.ExecuteSqlRaw(_cleanDataSql ??= GetCleanDataSql());
        }

        private string GetCleanDataSql()
        {
            var context = CreateContext();
            var builder = new StringBuilder();

            var helper = context.GetService<ISqlGenerationHelper>();
            var tables = context.Model.GetEntityTypes()
                .SelectMany(e => e.GetTableMappings().Select(m => helper.DelimitIdentifier(m.Table.Name, m.Table.Schema)));

            foreach (var table in tables)
            {
                builder.AppendLine($"TRUNCATE TABLE {table} RESTART IDENTITY;");
            }

            return builder.ToString();
        }
    }
}
