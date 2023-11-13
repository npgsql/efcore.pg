using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update;

#nullable enable

[MinimumPostgresVersion(14, 0)]
public class StoredProcedureUpdateNpgsqlTest : StoredProcedureUpdateTestBase
{
    public override async Task Insert_with_output_parameter(bool async)
    {
        await base.Insert_with_output_parameter(
            async,
            """
CREATE PROCEDURE "Entity_Insert"(name text, OUT id int) LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO "Entity" ("Name") VALUES (name) RETURNING "Id" INTO id;
END $$
""");

        AssertSql(
            """
@p0='New'

CALL "Entity_Insert"(@p0, NULL);
""");
    }

    public override async Task Insert_twice_with_output_parameter(bool async)
    {
        await base.Insert_twice_with_output_parameter(
            async,
            """
CREATE PROCEDURE "Entity_Insert"(name text, OUT id int) LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO "Entity" ("Name") VALUES (name) RETURNING "Id" INTO id;
END $$
""");

        AssertSql(
            """
@p0='New1'
@p1='New2'

CALL "Entity_Insert"(@p0, NULL);
CALL "Entity_Insert"(@p1, NULL);
""");
    }

    public override async Task Insert_with_result_column(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Insert_with_result_column(async, createSprocSql: ""));

        Assert.Equal(NpgsqlStrings.StoredProcedureResultColumnsNotSupported(nameof(Entity), nameof(Entity) + "_Insert"), exception.Message);
    }

    public override async Task Insert_with_two_result_columns(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Insert_with_two_result_columns(async, createSprocSql: ""));

        Assert.Equal(
            NpgsqlStrings.StoredProcedureResultColumnsNotSupported(
                nameof(EntityWithAdditionalProperty), nameof(EntityWithAdditionalProperty) + "_Insert"), exception.Message);
    }

    public override async Task Insert_with_output_parameter_and_result_column(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Insert_with_output_parameter_and_result_column(async, createSprocSql: ""));

        Assert.Equal(
            NpgsqlStrings.StoredProcedureResultColumnsNotSupported(
                nameof(EntityWithAdditionalProperty), nameof(EntityWithAdditionalProperty) + "_Insert"), exception.Message);
    }

    public override async Task Update(bool async)
    {
        await base.Update(
            async,
            """
CREATE PROCEDURE "Entity_Update"(id int, name text) LANGUAGE plpgsql AS $$
BEGIN
    UPDATE "Entity" SET "Name" = name WHERE "Id" = id;
END $$
""");

        AssertSql(
            """
@p0='1'
@p1='Updated'

CALL "Entity_Update"(@p0, @p1);
""");
    }

    public override async Task Update_partial(bool async)
    {
        await base.Update_partial(
            async,
            """
CREATE PROCEDURE "EntityWithAdditionalProperty_Update"(id int, name text, additional_property int) LANGUAGE plpgsql AS $$
BEGIN
    UPDATE "EntityWithAdditionalProperty" SET "Name" = name, "AdditionalProperty" = additional_property WHERE "Id" = id;
END $$
""");

        AssertSql(
            """
@p0='1'
@p1='Updated'
@p2='8'

CALL "EntityWithAdditionalProperty_Update"(@p0, @p1, @p2);
""");
    }

    public override async Task Update_with_output_parameter_and_rows_affected_result_column(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Update_with_output_parameter_and_rows_affected_result_column(async, createSprocSql: ""));

        Assert.Equal(
            NpgsqlStrings.StoredProcedureResultColumnsNotSupported(
                nameof(EntityWithAdditionalProperty), nameof(EntityWithAdditionalProperty) + "_Update"), exception.Message);
    }

    public override async Task Update_with_output_parameter_and_rows_affected_result_column_concurrency_failure(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Update_with_output_parameter_and_rows_affected_result_column_concurrency_failure(async, createSprocSql: ""));

        Assert.Equal(
            NpgsqlStrings.StoredProcedureResultColumnsNotSupported(
                nameof(EntityWithAdditionalProperty), nameof(EntityWithAdditionalProperty) + "_Update"), exception.Message);
    }

    public override async Task Delete(bool async)
    {
        await base.Delete(
            async,
            """
CREATE PROCEDURE "Entity_Delete"(id int) LANGUAGE plpgsql AS $$
BEGIN
    DELETE FROM "Entity" WHERE "Id" = id;
END $$
""");

        AssertSql(
            """
@p0='1'

CALL "Entity_Delete"(@p0);
""");
    }

    public override async Task Delete_and_insert(bool async)
    {
        await base.Delete_and_insert(
            async,
            """
CREATE PROCEDURE "Entity_Insert"(name text, OUT id int) LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO "Entity" ("Name") VALUES (name) RETURNING "Id" INTO id;
END $$;

CREATE PROCEDURE "Entity_Delete"(id int) LANGUAGE plpgsql AS $$
BEGIN
    DELETE FROM "Entity" WHERE "Id" = id;
END $$;
""");

        AssertSql(
            """
@p0='1'
@p1='Entity2'

CALL "Entity_Delete"(@p0);
CALL "Entity_Insert"(@p1, NULL);
""");
    }

    public override async Task Rows_affected_parameter(bool async)
    {
        await base.Rows_affected_parameter(
            async,
            """
CREATE PROCEDURE "Entity_Update"(id int, name text, OUT rows_affected int) LANGUAGE plpgsql AS $$
BEGIN
    UPDATE "Entity" SET "Name" = name WHERE "Id" = id;
    GET DIAGNOSTICS rows_affected = ROW_COUNT;
END $$
""");

        AssertSql(
            """
@p0='1'
@p1='Updated'

CALL "Entity_Update"(@p0, @p1, NULL);
""");
    }

    public override async Task Rows_affected_parameter_and_concurrency_failure(bool async)
    {
        await base.Rows_affected_parameter_and_concurrency_failure(
            async,
            """
CREATE PROCEDURE "Entity_Update"(id int, name text, OUT rows_affected int) LANGUAGE plpgsql AS $$
BEGIN
    UPDATE "Entity" SET "Name" = name WHERE "Id" = id;
    GET DIAGNOSTICS rows_affected = ROW_COUNT;
END $$
""");

        AssertSql(
            """
@p0='1'
@p1='Updated'

CALL "Entity_Update"(@p0, @p1, NULL);
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Rows_affected_parameter_with_another_output_parameter(bool async)
    {
        // PG doesn't supposed non-stored computed columns, so we need to duplicate the test code
        var createSprocSql =
            """
CREATE PROCEDURE "EntityWithAdditionalProperty_Update"(id int, OUT rows_affected int, OUT additional_property int, name text) LANGUAGE plpgsql AS $$
BEGIN
    UPDATE "EntityWithAdditionalProperty" SET "Name" = name WHERE "Id" = id RETURNING "AdditionalProperty" INTO additional_property;
    GET DIAGNOSTICS rows_affected = ROW_COUNT;
END $$
""";

        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<EntityWithAdditionalProperty>()
                .UpdateUsingStoredProcedure(
                    nameof(EntityWithAdditionalProperty) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasRowsAffectedParameter()
                        .HasParameter(w => w.AdditionalProperty, pb => pb.IsOutput())
                        .HasParameter(w => w.Name))
                .Property(w => w.AdditionalProperty).HasComputedColumnSql("8", stored: true),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new EntityWithAdditionalProperty { Name = "Initial" };
        context.Set<EntityWithAdditionalProperty>().Add(entity);
        await context.SaveChangesAsync();

        ClearLog();

        entity.Name = "Updated";
        entity.AdditionalProperty = 10;

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            // ReSharper disable once MethodHasAsyncOverload
            context.SaveChanges();
        }

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var loadedEntity = await context.Set<EntityWithAdditionalProperty>().SingleAsync(w => w.Id == entity.Id);
            Assert.Equal("Updated", loadedEntity.Name);
            Assert.Equal(8, loadedEntity.AdditionalProperty);
        }

        AssertSql(
            """
@p0='1'
@p1='Updated'

CALL "EntityWithAdditionalProperty_Update"(@p0, NULL, NULL, @p1);
""");
    }

    public override async Task Rows_affected_result_column(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Rows_affected_result_column(async, createSprocSql: ""));

        Assert.Equal(NpgsqlStrings.StoredProcedureResultColumnsNotSupported(nameof(Entity), nameof(Entity) + "_Update"), exception.Message);
    }

    public override async Task Rows_affected_result_column_and_concurrency_failure(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Rows_affected_result_column_and_concurrency_failure(async, createSprocSql: ""));

        Assert.Equal(NpgsqlStrings.StoredProcedureResultColumnsNotSupported(nameof(Entity), nameof(Entity) + "_Update"), exception.Message);
    }

    public override async Task Rows_affected_return_value(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Rows_affected_return_value(async, createSprocSql: ""));

        Assert.Equal(NpgsqlStrings.StoredProcedureReturnValueNotSupported(nameof(Entity), nameof(Entity) + "_Update"), exception.Message);
    }

    public override async Task Rows_affected_return_value_and_concurrency_failure(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Rows_affected_return_value(async, createSprocSql: ""));

        Assert.Equal(NpgsqlStrings.StoredProcedureReturnValueNotSupported(nameof(Entity), nameof(Entity) + "_Update"), exception.Message);
    }

    public override async Task Store_generated_concurrency_token_as_in_out_parameter(bool async)
    {
        await base.Store_generated_concurrency_token_as_in_out_parameter(
            async,
            """
CREATE PROCEDURE "Entity_Update"(id int, INOUT concurrency_token xid, name text, OUT rows_affected int) LANGUAGE plpgsql AS $$
BEGIN
    UPDATE "Entity" SET "Name" = name WHERE "Id" = id AND xmin = concurrency_token RETURNING xmin INTO concurrency_token;
    GET DIAGNOSTICS rows_affected = ROW_COUNT;
END $$
""");

        AssertSql(
            """
@p0='1'
@p1=NULL (Direction = InputOutput) (DbType = Object)
@p2='Updated'

CALL "Entity_Update"(@p0, @p1, @p2, NULL);
""");
    }

    public override async Task Store_generated_concurrency_token_as_two_parameters(bool async)
    {
        await base.Store_generated_concurrency_token_as_two_parameters(
            async,
            """
CREATE PROCEDURE "Entity_Update"(id int, concurrency_token_in xid, name text, OUT concurrency_token_out xid, OUT rows_affected int) LANGUAGE plpgsql AS $$
BEGIN
    UPDATE "Entity" SET "Name" = name WHERE "Id" = id AND xmin = concurrency_token_in RETURNING xmin INTO concurrency_token_out;
    GET DIAGNOSTICS rows_affected = ROW_COUNT;
END $$
""");

        // Can't assert SQL baseline as usual because the concurrency token changes
        Assert.Equal(
            """
@p2='Updated'

CALL "Entity_Update"(@p0, @p1, @p2, NULL, NULL);
""",
            TestSqlLoggerFactory.Sql.Substring(TestSqlLoggerFactory.Sql.IndexOf("@p2", StringComparison.Ordinal)),
            ignoreLineEndingDifferences: true);
    }

    public override async Task User_managed_concurrency_token(bool async)
    {
        await base.User_managed_concurrency_token(
            async,
            """
CREATE PROCEDURE "EntityWithAdditionalProperty_Update"(id int, concurrency_token_original int, name text, concurrency_token_current int, OUT rows_affected int) LANGUAGE plpgsql AS $$
BEGIN
    UPDATE "EntityWithAdditionalProperty" SET "Name" = name, "AdditionalProperty" = concurrency_token_current WHERE "Id" = id AND "AdditionalProperty" = concurrency_token_original;
    GET DIAGNOSTICS rows_affected = ROW_COUNT;
END $$
""");

        AssertSql(
            """
@p0='1'
@p1='8'
@p2='Updated'
@p3='9'

CALL "EntityWithAdditionalProperty_Update"(@p0, @p1, @p2, @p3, NULL);
""");
    }

    public override async Task Original_and_current_value_on_non_concurrency_token(bool async)
    {
        await base.Original_and_current_value_on_non_concurrency_token(
            async,
            """
CREATE PROCEDURE "Entity_Update"(id int, name_current text, name_original text) LANGUAGE plpgsql AS $$
BEGIN
    IF name_current <> name_original THEN
        UPDATE "Entity" SET "Name" = name_current WHERE "Id" = id;
    END IF;
END $$
""");

        AssertSql(
            """
@p0='1'
@p1='Updated'
@p2='Initial'

CALL "Entity_Update"(@p0, @p1, @p2);
""");
    }

    public override async Task Input_or_output_parameter_with_input(bool async)
    {
        await base.Input_or_output_parameter_with_input(
            async,
            """
CREATE PROCEDURE "Entity_Insert"(OUT id int, INOUT name text) LANGUAGE plpgsql AS $$
BEGIN
    IF name IS NULL THEN
        INSERT INTO "Entity" ("Name") VALUES ('Some default value') RETURNING "Id", "Name" INTO id, name;
    ELSE
        INSERT INTO "Entity" ("Name") VALUES (name) RETURNING "Id" INTO id;
        name = NULL;
    END IF;
END $$
""");

        AssertSql(
            """
@p0='1' (Direction = InputOutput) (DbType = String)

CALL "Entity_Insert"(NULL, @p0);
""");
    }

    public override async Task Input_or_output_parameter_with_output(bool async)
    {
        await base.Input_or_output_parameter_with_output(
            async,
            """
CREATE PROCEDURE "Entity_Insert"(OUT id int, INOUT name text) LANGUAGE plpgsql AS $$
BEGIN
    IF name IS NULL THEN
        INSERT INTO "Entity" ("Name") VALUES ('Some default value') RETURNING "Id", "Name" INTO id, name;
    ELSE
        INSERT INTO "Entity" ("Name") VALUES (name) RETURNING "Id" INTO id;
        name = NULL;
    END IF;
END $$
""");

        AssertSql(
            """
@p0='1' (Direction = InputOutput) (DbType = String)

CALL "Entity_Insert"(NULL, @p0);
""");
    }

    public override async Task Tph(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Rows_affected_result_column(async, createSprocSql: ""));

        Assert.Equal(NpgsqlStrings.StoredProcedureResultColumnsNotSupported(nameof(Entity), nameof(Entity) + "_Update"), exception.Message);
    }

    public override async Task Tpt(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Rows_affected_result_column(async, createSprocSql: ""));

        Assert.Equal(NpgsqlStrings.StoredProcedureResultColumnsNotSupported(nameof(Entity), nameof(Entity) + "_Update"), exception.Message);
    }

    public override async Task Tpt_mixed_sproc_and_non_sproc(bool async)
    {
        await base.Tpt_mixed_sproc_and_non_sproc(
            async,
            """
CREATE PROCEDURE "Parent_Insert"(OUT id int, name text) LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO "Parent" ("Name") VALUES (name) RETURNING "Id" INTO id;
END $$
""");

        AssertSql(
            """
@p0='Child'

CALL "Parent_Insert"(NULL, @p0);
""",
            //
            """
@p1='1'
@p2='8'

INSERT INTO "Child1" ("Id", "Child1Property")
VALUES (@p1, @p2);
""");
    }

    public override async Task Tpc(bool async)
    {
        await base.Tpc(
            async,
            """
CREATE PROCEDURE "Child1_Insert"(OUT id int, name text, child1_property int) LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO "Child1" ("Name", "Child1Property") VALUES (name, child1_property) RETURNING "Id" INTO id;
END $$
""");

        AssertSql(
            """
@p0='Child'
@p1='8'

CALL "Child1_Insert"(NULL, @p0, @p1);
""");
    }

    public override async Task Non_sproc_followed_by_sproc_commands_in_the_same_batch(bool async)
    {
        await base.Non_sproc_followed_by_sproc_commands_in_the_same_batch(
            async,
            """
CREATE PROCEDURE "EntityWithAdditionalProperty_Insert"(name text, OUT id int, additional_property int) LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO "EntityWithAdditionalProperty" ("Name", "AdditionalProperty") VALUES (name, additional_property) RETURNING "Id" INTO id;
END $$
""");

        AssertSql(
            """
@p2='1'
@p0='2'
@p3='1'
@p1='Entity1_Modified'
@p4='Entity2'
@p5='0'

UPDATE "EntityWithAdditionalProperty" SET "AdditionalProperty" = @p0, "Name" = @p1
WHERE "Id" = @p2 AND "AdditionalProperty" = @p3;
CALL "EntityWithAdditionalProperty_Insert"(@p4, NULL, @p5);
""");
    }

    protected override void ConfigureStoreGeneratedConcurrencyToken(EntityTypeBuilder entityTypeBuilder, string propertyName)
        => entityTypeBuilder.Property<uint?>(propertyName)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
