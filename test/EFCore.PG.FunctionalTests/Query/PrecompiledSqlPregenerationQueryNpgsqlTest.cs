using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

// ReSharper disable InconsistentNaming

public class PrecompiledSqlPregenerationQueryNpgsqlTest(
    PrecompiledSqlPregenerationQueryNpgsqlTest.PrecompiledSqlPregenerationQueryNpgsqlFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledSqlPregenerationQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledSqlPregenerationQueryNpgsqlTest.PrecompiledSqlPregenerationQueryNpgsqlFixture>
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    public override async Task No_parameters()
    {
        await base.No_parameters();

        AssertSql(
            """
SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = 'foo'
""");
    }

    public override async Task Non_nullable_value_type()
    {
        await base.Non_nullable_value_type();

        AssertSql(
            """
@id='8'

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Id" = @id
""");
    }

    public override async Task Nullable_value_type()
    {
        await base.Nullable_value_type();

        AssertSql(
            """
@id='8' (Nullable = true)

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Id" = @id
""");
    }

    public override async Task Nullable_reference_type()
    {
        await base.Nullable_reference_type();

        AssertSql(
            """
@name='bar'

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @name
""");
    }

    public override async Task Non_nullable_reference_type()
    {
        await base.Non_nullable_reference_type();

        AssertSql(
            """
@name='bar' (Nullable = false)

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @name
""");
    }

    public override async Task Nullable_and_non_nullable_value_types()
    {
        await base.Nullable_and_non_nullable_value_types();

        AssertSql(
            """
@id1='8' (Nullable = true)
@id2='9'

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Id" = @id1 OR b."Id" = @id2
""");
    }

    public override async Task Two_nullable_reference_types()
    {
        await base.Two_nullable_reference_types();

        AssertSql(
            """
@name1='foo'
@name2='bar'

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @name1 OR b."Name" = @name2
""");
    }

    public override async Task Two_non_nullable_reference_types()
    {
        await base.Two_non_nullable_reference_types();

        AssertSql(
            """
@name1='foo' (Nullable = false)
@name2='bar' (Nullable = false)

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @name1 OR b."Name" = @name2
""");
    }

    public override async Task Nullable_and_non_nullable_reference_types()
    {
        await base.Nullable_and_non_nullable_reference_types();

        AssertSql(
            """
@name1='foo'
@name2='bar' (Nullable = false)

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @name1 OR b."Name" = @name2
""");
    }

    public override async Task Too_many_nullable_parameters_prevent_pregeneration()
    {
        await base.Too_many_nullable_parameters_prevent_pregeneration();

        AssertSql(
            """
@name1='foo'
@name2='bar'
@name3='baz'
@name4='baq'

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @name1 OR b."Name" = @name2 OR b."Name" = @name3 OR b."Name" = @name4
""");
    }

    public override async Task Many_non_nullable_parameters_do_not_prevent_pregeneration()
    {
        await base.Many_non_nullable_parameters_do_not_prevent_pregeneration();

        AssertSql(
            """
@name1='foo' (Nullable = false)
@name2='bar' (Nullable = false)
@name3='baz' (Nullable = false)
@name4='baq' (Nullable = false)

SELECT b."Id", b."Name"
FROM "Blogs" AS b
WHERE b."Name" = @name1 OR b."Name" = @name2 OR b."Name" = @name3 OR b."Name" = @name4
""");
    }

    #region Tests for the different querying enumerables

    public override async Task Include_single_query()
    {
        await base.Include_single_query();

        AssertSql(
            """
SELECT b."Id", b."Name", p."Id", p."BlogId", p."Title"
FROM "Blogs" AS b
LEFT JOIN "Post" AS p ON b."Id" = p."BlogId"
ORDER BY b."Id" NULLS FIRST
""");
    }

    public override async Task Include_split_query()
    {
        await base.Include_split_query();

        AssertSql(
            """
SELECT b."Id", b."Name"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
""",
            //
            """
SELECT p."Id", p."BlogId", p."Title", b."Id"
FROM "Blogs" AS b
INNER JOIN "Post" AS p ON b."Id" = p."BlogId"
ORDER BY b."Id" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy()
    {
        await base.Final_GroupBy();

        AssertSql(
            """
SELECT b."Name", b."Id"
FROM "Blogs" AS b
ORDER BY b."Name" NULLS FIRST
""");
    }

    #endregion Tests for the different querying enumerables

    public class PrecompiledSqlPregenerationQueryNpgsqlFixture : PrecompiledSqlPregenerationQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            builder = base.AddOptions(builder);

            // TODO: Figure out if there's a nice way to continue using the retrying strategy
            var npgsqlOptionsBuilder = new NpgsqlDbContextOptionsBuilder(builder);
            npgsqlOptionsBuilder
                .ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
            return builder;
        }

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers => NpgsqlPrecompiledQueryTestHelpers.Instance;
    }
}
