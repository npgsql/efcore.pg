using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

public class PrecompiledQueryNpgsqlTest(
    PrecompiledQueryNpgsqlTest.PrecompiledQueryNpgsqlFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledQueryNpgsqlTest.PrecompiledQueryNpgsqlFixture>
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    #region Expression types

    public override async Task BinaryExpression()
    {
        await base.BinaryExpression();

        AssertSql(
            """
@id='3'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" > @id
""");
    }

    public override async Task Conditional_no_evaluatable()
    {
        await base.Conditional_no_evaluatable();

        AssertSql(
            """
SELECT CASE
    WHEN b."Id" = 2 THEN 'yes'
    ELSE 'no'
END
FROM "Blogs" AS b
""");
    }

    public override async Task Conditional_contains_captured_variable()
    {
        await base.Conditional_contains_captured_variable();

        AssertSql(
            """
@yes='yes'

SELECT CASE
    WHEN b."Id" = 2 THEN @yes
    ELSE 'no'
END
FROM "Blogs" AS b
""");
    }

    public override async Task Invoke_no_evaluatability_is_not_supported()
    {
        await base.Invoke_no_evaluatability_is_not_supported();

        AssertSql();
    }

     public override async Task ListInit_no_evaluatability()
     {
         await base.ListInit_no_evaluatability();

         AssertSql(
             """
SELECT b."Id", b."Id" + 1
FROM "Blogs" AS b
""");
     }

     public override async Task ListInit_with_evaluatable_with_captured_variable()
     {
         await base.ListInit_with_evaluatable_with_captured_variable();

         AssertSql(
             """
SELECT b."Id"
FROM "Blogs" AS b
""");
     }

    public override async Task ListInit_with_evaluatable_without_captured_variable()
    {
        await base.ListInit_with_evaluatable_without_captured_variable();

        AssertSql(
            """
SELECT b."Id"
FROM "Blogs" AS b
""");
    }

    public override async Task ListInit_fully_evaluatable()
    {
        await base.ListInit_fully_evaluatable();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" IN (7, 8)
LIMIT 2
""");
    }

     public override async Task MethodCallExpression_no_evaluatability()
     {
         await base.MethodCallExpression_no_evaluatability();

         AssertSql(
             """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Name" IS NOT NULL AND left(b."Name", length(b."Name")) = b."Name"
""");
     }

    public override async Task MethodCallExpression_with_evaluatable_with_captured_variable()
    {
        await base.MethodCallExpression_with_evaluatable_with_captured_variable();

        AssertSql(
            """
@pattern_startswith='foo%'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Name" LIKE @pattern_startswith
""");
    }

    public override async Task MethodCallExpression_with_evaluatable_without_captured_variable()
    {
        await base.MethodCallExpression_with_evaluatable_without_captured_variable();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Name" LIKE 'foo%'
""");
    }

    public override async Task MethodCallExpression_fully_evaluatable()
    {
        await base.MethodCallExpression_fully_evaluatable();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task New_with_no_arguments()
    {
        await base.New_with_no_arguments();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 0
""");
    }

    public override async Task Where_New_with_captured_variable()
    {
        await base.Where_New_with_captured_variable();

        AssertSql();
    }

    public override async Task Select_New_with_captured_variable()
    {
        await base.Select_New_with_captured_variable();

        AssertSql(
            """
SELECT b."Name"
FROM "Blogs" AS b
""");
    }

    public override async Task MemberInit_no_evaluatable()
    {
        await base.MemberInit_no_evaluatable();

        AssertSql(
            """
SELECT b."Id", b."Name"
FROM "Blogs" AS b
""");
    }

    public override async Task MemberInit_contains_captured_variable()
    {
        await base.MemberInit_contains_captured_variable();

        AssertSql(
            """
@id='8'

SELECT @id AS "Id", b."Name"
FROM "Blogs" AS b
""");
    }

    public override async Task MemberInit_evaluatable_as_constant()
    {
        await base.MemberInit_evaluatable_as_constant();

        AssertSql(
            """
SELECT 1 AS "Id", 'foo' AS "Name"
FROM "Blogs" AS b
""");
    }

    public override async Task MemberInit_evaluatable_as_parameter()
    {
        await base.MemberInit_evaluatable_as_parameter();

        AssertSql(
            """
SELECT 1
FROM "Blogs" AS b
""");
    }

    public override async Task NewArray()
    {
        await base.NewArray();

        AssertSql(
            """
@i='8'

SELECT ARRAY[b."Id",b."Id" + @i]::integer[]
FROM "Blogs" AS b
""");
    }

    public override async Task Unary()
    {
        await base.Unary();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id"::smallint = 8
""");
    }

    public virtual async Task Collate()
    {
        await Test("""_ = context.Blogs.Where(b => EF.Functions.Collate(b.Name, "German_PhoneBook_CI_AS") == "foo").ToList();""");

        AssertSql();
    }

    #endregion Expression types

    #region Regular operators

    public override async Task OrderBy()
    {
        await base.OrderBy();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Name" NULLS FIRST
""");
    }

    public override async Task Skip_with_constant()
    {
        await base.Skip_with_constant();

        AssertSql(
            """
@p='1'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Name" NULLS FIRST
OFFSET @p
""");
    }

    public override async Task Skip_with_parameter()
    {
        await base.Skip_with_parameter();

        AssertSql(
            """
@p='1'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Name" NULLS FIRST
OFFSET @p
""");
    }

    public override async Task Take_with_constant()
    {
        await base.Take_with_constant();

        AssertSql(
            """
@p='1'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Name" NULLS FIRST
LIMIT @p
""");
    }

    public override async Task Take_with_parameter()
    {
        await base.Take_with_parameter();

        AssertSql(
            """
@p='1'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Name" NULLS FIRST
LIMIT @p
""");
    }

    public override async Task Select_changes_type()
    {
        await base.Select_changes_type();

        AssertSql(
            """
SELECT b."Name"
FROM "Blogs" AS b
""");
    }

    public override async Task Select_anonymous_object()
    {
        await base.Select_anonymous_object();

        AssertSql(
            """
SELECT COALESCE(b."Name", '') || 'Foo' AS "Foo"
FROM "Blogs" AS b
""");
    }

    public override async Task Include_single()
    {
        await base.Include_single();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json", p."Id", p."BlogId", p."Title"
FROM "Blogs" AS b
LEFT JOIN "Posts" AS p ON b."Id" = p."BlogId"
WHERE b."Id" > 8
ORDER BY b."Id" NULLS FIRST
""");
    }

    public override async Task Include_split()
    {
        await base.Include_split();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
""",
            //
            """
SELECT p."Id", p."BlogId", p."Title", b."Id"
FROM "Blogs" AS b
INNER JOIN "Posts" AS p ON b."Id" = p."BlogId"
ORDER BY b."Id" NULLS FIRST
""");
    }

    public override async Task Final_GroupBy()
    {
        await base.Final_GroupBy();

        AssertSql(
            """
SELECT b."Name", b."Id", b."Json"
FROM "Blogs" AS b
ORDER BY b."Name" NULLS FIRST
""");
    }

    #endregion Regular operators

    #region Terminating operators

    public override async Task Terminating_AsEnumerable()
    {
        await base.Terminating_AsEnumerable();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_AsAsyncEnumerable_on_DbSet()
    {
        await base.Terminating_AsAsyncEnumerable_on_DbSet();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_AsAsyncEnumerable_on_IQueryable()
    {
        await base.Terminating_AsAsyncEnumerable_on_IQueryable();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" > 8
""");
    }

    public override async Task Foreach_sync_over_operator()
    {
        await base.Foreach_sync_over_operator();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" > 8
""");
    }

    public override async Task Terminating_ToArray()
    {
        await base.Terminating_ToArray();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ToArrayAsync()
    {
        await base.Terminating_ToArrayAsync();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ToDictionary()
    {
        await base.Terminating_ToDictionary();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ToDictionaryAsync()
    {
        await base.Terminating_ToDictionaryAsync();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task ToDictionary_over_anonymous_type()
    {
        await base.ToDictionary_over_anonymous_type();

        AssertSql(
            """
SELECT b."Id", b."Name"
FROM "Blogs" AS b
""");
    }

    public override async Task ToDictionaryAsync_over_anonymous_type()
    {
        await base.ToDictionaryAsync_over_anonymous_type();

        AssertSql(
            """
SELECT b."Id", b."Name"
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ToHashSet()
    {
        await base.Terminating_ToHashSet();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ToHashSetAsync()
    {
        await base.Terminating_ToHashSetAsync();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ToLookup()
    {
        await base.Terminating_ToLookup();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ToList()
    {
        await base.Terminating_ToList();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ToListAsync()
    {
        await base.Terminating_ToListAsync();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task Foreach_sync_over_DbSet_property_is_not_supported()
    {
        await base.Foreach_sync_over_DbSet_property_is_not_supported();

        AssertSql();
    }

    public override async Task Foreach_async_is_not_supported()
    {
        await base.Foreach_async_is_not_supported();

        AssertSql();
    }

    #endregion Terminating operators

    #region Reducing terminating operators

    public override async Task Terminating_All()
    {
        await base.Terminating_All();

        AssertSql(
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" <= 7)
""",
            //
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" <= 8)
""");
    }

    public override async Task Terminating_AllAsync()
    {
        await base.Terminating_AllAsync();
AssertSql(
    """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" <= 7)
""",
    //
    """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" <= 8)
""");
    }

    public override async Task Terminating_Any()
    {
        await base.Terminating_Any();
AssertSql(
    """
SELECT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" > 7)
""",
    //
    """
SELECT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" < 7)
""",
    //
    """
SELECT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" > 7)
""",
    //
    """
SELECT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" < 7)
""");
    }

    public override async Task Terminating_AnyAsync()
    {
        await base.Terminating_AnyAsync();

        AssertSql(
            """
SELECT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" > 7)
""",
            //
            """
SELECT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" < 7)
""",
            //
            """
SELECT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" > 7)
""",
            //
            """
SELECT EXISTS (
    SELECT 1
    FROM "Blogs" AS b
    WHERE b."Id" < 7)
""");
    }

    public override async Task Terminating_Average()
    {
        await base.Terminating_Average();

        AssertSql(
            """
SELECT avg(b."Id"::double precision)
FROM "Blogs" AS b
""",
            //
            """
SELECT avg(b."Id"::double precision)
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_AverageAsync()
    {
        await base.Terminating_AverageAsync();

        AssertSql(
            """
SELECT avg(b."Id"::double precision)
FROM "Blogs" AS b
""",
            //
            """
SELECT avg(b."Id"::double precision)
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_Contains()
    {
        await base.Terminating_Contains();

        AssertSql(
            """
@p='8'

SELECT @p IN (
    SELECT b."Id"
    FROM "Blogs" AS b
)
""",
            //
            """
@p='7'

SELECT @p IN (
    SELECT b."Id"
    FROM "Blogs" AS b
)
""");
    }

    public override async Task Terminating_ContainsAsync()
    {
        await base.Terminating_ContainsAsync();

        AssertSql(
            """
@p='8'

SELECT @p IN (
    SELECT b."Id"
    FROM "Blogs" AS b
)
""",
            //
            """
@p='7'

SELECT @p IN (
    SELECT b."Id"
    FROM "Blogs" AS b
)
""");
    }

    public override async Task Terminating_Count()
    {
        await base.Terminating_Count();

        AssertSql(
            """
SELECT count(*)::int
FROM "Blogs" AS b
""",
            //
            """
SELECT count(*)::int
FROM "Blogs" AS b
WHERE b."Id" > 8
""");
    }

    public override async Task Terminating_CountAsync()
    {
        await base.Terminating_CountAsync();

        AssertSql(
            """
SELECT count(*)::int
FROM "Blogs" AS b
""",
            //
            """
SELECT count(*)::int
FROM "Blogs" AS b
WHERE b."Id" > 8
""");
    }

    public override async Task Terminating_ElementAt()
    {
        await base.Terminating_ElementAt();

        AssertSql(
            """
@p='1'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
LIMIT 1 OFFSET @p
""",
            //
            """
@p='3'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
LIMIT 1 OFFSET @p
""");
    }

    public override async Task Terminating_ElementAtAsync()
    {
        await base.Terminating_ElementAtAsync();

        AssertSql(
            """
@p='1'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
LIMIT 1 OFFSET @p
""",
            //
            """
@p='3'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
LIMIT 1 OFFSET @p
""");
    }

    public override async Task Terminating_ElementAtOrDefault()
    {
        await base.Terminating_ElementAtOrDefault();

        AssertSql(
            """
@p='1'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
LIMIT 1 OFFSET @p
""",
            //
            """
@p='3'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
LIMIT 1 OFFSET @p
""");
    }

    public override async Task Terminating_ElementAtOrDefaultAsync()
    {
        await base.Terminating_ElementAtOrDefaultAsync();

        AssertSql(
            """
@p='1'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
LIMIT 1 OFFSET @p
""",
            //
            """
@p='3'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" NULLS FIRST
LIMIT 1 OFFSET @p
""");
    }

    public override async Task Terminating_First()
    {
        await base.Terminating_First();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 1
""");
    }

    public override async Task Terminating_FirstAsync()
    {
        await base.Terminating_FirstAsync();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 1
""");
    }

    public override async Task Terminating_FirstOrDefault()
    {
        await base.Terminating_FirstOrDefault();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 1
""");
    }

    public override async Task Terminating_FirstOrDefaultAsync()
    {
        await base.Terminating_FirstOrDefaultAsync();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 1
""");
    }

    public override async Task Terminating_GetEnumerator()
    {
        await base.Terminating_GetEnumerator();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
""");
    }

    public override async Task Terminating_Last()
    {
        await base.Terminating_Last();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""");
    }

    public override async Task Terminating_LastAsync()
    {
        await base.Terminating_LastAsync();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""");
    }

    public override async Task Terminating_LastOrDefault()
    {
        await base.Terminating_LastOrDefault();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""");
    }

    public override async Task Terminating_LastOrDefaultAsync()
    {
        await base.Terminating_LastOrDefaultAsync();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
ORDER BY b."Id" DESC NULLS LAST
LIMIT 1
""");
    }

    public override async Task Terminating_LongCount()
    {
        await base.Terminating_LongCount();

        AssertSql(
            """
SELECT count(*)
FROM "Blogs" AS b
""",
            //
            """
SELECT count(*)
FROM "Blogs" AS b
WHERE b."Id" = 8
""");
    }

    public override async Task Terminating_LongCountAsync()
    {
        await base.Terminating_LongCountAsync();

        AssertSql(
            """
SELECT count(*)
FROM "Blogs" AS b
""",
            //
            """
SELECT count(*)
FROM "Blogs" AS b
WHERE b."Id" = 8
""");
    }

    public override async Task Terminating_Max()
    {
        await base.Terminating_Max();
AssertSql(
    """
SELECT max(b."Id")
FROM "Blogs" AS b
""",
    //
    """
SELECT max(b."Id")
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_MaxAsync()
    {
        await base.Terminating_MaxAsync();

        AssertSql(
            """
SELECT max(b."Id")
FROM "Blogs" AS b
""",
            //
            """
SELECT max(b."Id")
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_Min()
    {
        await base.Terminating_Min();

        AssertSql(
            """
SELECT min(b."Id")
FROM "Blogs" AS b
""",
            //
            """
SELECT min(b."Id")
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_MinAsync()
    {
        await base.Terminating_MinAsync();

        AssertSql(
            """
SELECT min(b."Id")
FROM "Blogs" AS b
""",
            //
            """
SELECT min(b."Id")
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_Single()
    {
        await base.Terminating_Single();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 2
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 2
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 2
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 2
""");
    }

    public override async Task Terminating_SingleAsync()
    {
        await base.Terminating_SingleAsync();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 2
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 2
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 2
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 2
""");
    }

    public override async Task Terminating_SingleOrDefault()
    {
        await base.Terminating_SingleOrDefault();

        AssertSql(
    """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 2
""",
    //
    """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 2
""",
    //
    """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 2
""",
    //
    """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 2
""");
    }

    public override async Task Terminating_SingleOrDefaultAsync()
    {
        await base.Terminating_SingleOrDefaultAsync();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 2
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 2
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 2
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 2
""");
    }

    public override async Task Terminating_Sum()
    {
        await base.Terminating_Sum();

        AssertSql(
    """
SELECT COALESCE(sum(b."Id"), 0)::int
FROM "Blogs" AS b
""",
    //
    """
SELECT COALESCE(sum(b."Id"), 0)::int
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_SumAsync()
    {
        await base.Terminating_SumAsync();

        AssertSql(
            """
SELECT COALESCE(sum(b."Id"), 0)::int
FROM "Blogs" AS b
""",
            //
            """
SELECT COALESCE(sum(b."Id"), 0)::int
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ExecuteDelete()
    {
        await base.Terminating_ExecuteDelete();

        AssertSql(
            """
DELETE FROM "Blogs" AS b
WHERE b."Id" > 8
""",
            //
            """
SELECT count(*)::int
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ExecuteDeleteAsync()
    {
        await base.Terminating_ExecuteDeleteAsync();

        AssertSql(
            """
DELETE FROM "Blogs" AS b
WHERE b."Id" > 8
""",
            //
            """
SELECT count(*)::int
FROM "Blogs" AS b
""");
    }

    public override async Task Terminating_ExecuteUpdate_with_lambda()
    {
        await base.Terminating_ExecuteUpdate_with_lambda();

        AssertSql(
            """
@suffix='Suffix'

UPDATE "Blogs" AS b
SET "Name" = COALESCE(b."Name", '') || @suffix
WHERE b."Id" > 8
""",
            //
            """
SELECT count(*)::int
FROM "Blogs" AS b
WHERE b."Id" = 9 AND b."Name" = 'Blog2Suffix'
""");
    }

    public override async Task Terminating_ExecuteUpdate_without_lambda()
    {
        await base.Terminating_ExecuteUpdate_without_lambda();

        AssertSql(
            """
@newValue='NewValue'

UPDATE "Blogs" AS b
SET "Name" = @newValue
WHERE b."Id" > 8
""",
            //
            """
SELECT count(*)::int
FROM "Blogs" AS b
WHERE b."Id" = 9 AND b."Name" = 'NewValue'
""");
    }

    public override async Task Terminating_ExecuteUpdateAsync_with_lambda()
    {
        await base.Terminating_ExecuteUpdateAsync_with_lambda();

        AssertSql(
            """
@suffix='Suffix'

UPDATE "Blogs" AS b
SET "Name" = COALESCE(b."Name", '') || @suffix
WHERE b."Id" > 8
""",
            //
            """
SELECT count(*)::int
FROM "Blogs" AS b
WHERE b."Id" = 9 AND b."Name" = 'Blog2Suffix'
""");
    }

    public override async Task Terminating_ExecuteUpdateAsync_without_lambda()
    {
        await base.Terminating_ExecuteUpdateAsync_without_lambda();

        AssertSql(
            """
@newValue='NewValue'

UPDATE "Blogs" AS b
SET "Name" = @newValue
WHERE b."Id" > 8
""",
            //
            """
SELECT count(*)::int
FROM "Blogs" AS b
WHERE b."Id" = 9 AND b."Name" = 'NewValue'
""");
    }

    public override async Task Terminating_with_cancellation_token()
    {
        await base.Terminating_with_cancellation_token();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 8
LIMIT 1
""",
            //
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = 7
LIMIT 1
""");
    }

    #endregion Reducing terminating operators

    #region SQL expression quotability

    public override async Task Union()
    {
        await base.Union();

        AssertSql(
            """
SELECT u."Id", u."BlogId", u."Title"
FROM (
    SELECT p."Id", p."BlogId", p."Title"
    FROM "Posts" AS p
    WHERE p."Id" > 11
    UNION
    SELECT p0."Id", p0."BlogId", p0."Title"
    FROM "Posts" AS p0
    WHERE p0."Id" < 21
) AS u
ORDER BY u."Id" NULLS FIRST
""");
    }

    public override async Task UnionOnEntitiesWithJson()
    {
        await base.UnionOnEntitiesWithJson();

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[Json]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[Json]
    FROM [Blogs] AS [b]
    WHERE [b].[Id] > 7
    UNION
    SELECT [b0].[Id], [b0].[Name], [b0].[Json]
    FROM [Blogs] AS [b0]
    WHERE [b0].[Id] < 10
) AS [u]
ORDER BY [u].[Id]
""");
    }

    public override async Task Concat()
    {
        await base.Concat();

        AssertSql(
            """
SELECT u."Id", u."BlogId", u."Title"
FROM (
    SELECT p."Id", p."BlogId", p."Title"
    FROM "Posts" AS p
    WHERE p."Id" > 11
    UNION ALL
    SELECT p0."Id", p0."BlogId", p0."Title"
    FROM "Posts" AS p0
    WHERE p0."Id" < 21
) AS u
ORDER BY u."Id" NULLS FIRST
""");
    }

    public override async Task ConcatOnEntitiesWithJson()
    {
        await base.ConcatOnEntitiesWithJson();

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[Json]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[Json]
    FROM [Blogs] AS [b]
    WHERE [b].[Id] > 7
    UNION ALL
    SELECT [b0].[Id], [b0].[Name], [b0].[Json]
    FROM [Blogs] AS [b0]
    WHERE [b0].[Id] < 10
) AS [u]
ORDER BY [u].[Id]
""");
    }

    public override async Task Intersect()
    {
        await base.Intersect();

        AssertSql(
            """
SELECT i."Id", i."BlogId", i."Title"
FROM (
    SELECT p."Id", p."BlogId", p."Title"
    FROM "Posts" AS p
    WHERE p."Id" > 11
    INTERSECT
    SELECT p0."Id", p0."BlogId", p0."Title"
    FROM "Posts" AS p0
    WHERE p0."Id" < 22
) AS i
ORDER BY i."Id" NULLS FIRST
""");
    }

    public override async Task IntersectOnEntitiesWithJson()
    {
        await base.IntersectOnEntitiesWithJson();

        AssertSql(
            """
SELECT [i].[Id], [i].[Name], [i].[Json]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[Json]
    FROM [Blogs] AS [b]
    WHERE [b].[Id] > 7
    INTERSECT
    SELECT [b0].[Id], [b0].[Name], [b0].[Json]
    FROM [Blogs] AS [b0]
    WHERE [b0].[Id] > 8
) AS [i]
ORDER BY [i].[Id]
""");
    }

    public override async Task Except()
    {
        await base.Except();

        AssertSql(
            """
SELECT e."Id", e."BlogId", e."Title"
FROM (
    SELECT p."Id", p."BlogId", p."Title"
    FROM "Posts" AS p
    WHERE p."Id" > 11
    EXCEPT
    SELECT p0."Id", p0."BlogId", p0."Title"
    FROM "Posts" AS p0
    WHERE p0."Id" > 21
) AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    public override async Task ExceptOnEntitiesWithJson()
    {
        await base.ExceptOnEntitiesWithJson();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[Json]
FROM (
    SELECT [b].[Id], [b].[Name], [b].[Json]
    FROM [Blogs] AS [b]
    WHERE [b].[Id] > 7
    EXCEPT
    SELECT [b0].[Id], [b0].[Name], [b0].[Json]
    FROM [Blogs] AS [b0]
    WHERE [b0].[Id] > 8
) AS [e]
ORDER BY [e].[Id]
""");
    }

    public override async Task ValuesExpression()
    {
        await base.ValuesExpression();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE (
    SELECT count(*)::int
    FROM (VALUES (7::int), (b."Id")) AS v("Value")
    WHERE v."Value" > 8) = 2
""");
    }

    public override async Task Contains_with_parameterized_collection()
    {
        await base.Contains_with_parameterized_collection();

        AssertSql(
            """
@ids={ '1', '2', '3' } (DbType = Object)

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = ANY (@ids)
""");
    }

    public override async Task FromSqlRaw()
    {
        await base.FromSqlRaw();

AssertSql(
    """
SELECT m."Id", m."Name", m."Json"
FROM (
    SELECT * FROM "Blogs" WHERE "Id" > 8
) AS m
ORDER BY m."Id" NULLS FIRST
""");
    }

    public override async Task FromSql_with_FormattableString_parameters()
    {
        await base.FromSql_with_FormattableString_parameters();

        AssertSql(
            """
p0='8'
p1='9'

SELECT m."Id", m."Name", m."Json"
FROM (
    SELECT * FROM "Blogs" WHERE "Id" > @p0 AND "Id" < @p1
) AS m
ORDER BY m."Id" NULLS FIRST
""");
    }

    #endregion SQL expression quotability

    #region Different query roots

    public override async Task DbContext_as_local_variable()
    {
        await base.DbContext_as_local_variable();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task DbContext_as_field()
    {
        await base.DbContext_as_field();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task DbContext_as_property()
    {
        await base.DbContext_as_property();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task DbContext_as_captured_variable()
    {
        await base.DbContext_as_captured_variable();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    public override async Task DbContext_as_method_invocation_result()
    {
        await base.DbContext_as_method_invocation_result();

        AssertSql(
            """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    #endregion Different query roots

    #region Captured variable handling

    public override async Task Two_captured_variables_in_same_lambda()
    {
        await base.Two_captured_variables_in_same_lambda();

        AssertSql(
            """
@yes='yes'
@no='no'

SELECT CASE
    WHEN b."Id" = 3 THEN @yes
    ELSE @no
END
FROM "Blogs" AS b
""");
    }

    public override async Task Two_captured_variables_in_different_lambdas()
    {
        await base.Two_captured_variables_in_different_lambdas();

        AssertSql(
            """
@starts_startswith='Blog%'
@ends_endswith='%2'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Name" LIKE @starts_startswith AND b."Name" LIKE @ends_endswith
LIMIT 2
""");
    }

    public override async Task Same_captured_variable_twice_in_same_lambda()
    {
        await base.Same_captured_variable_twice_in_same_lambda();

        AssertSql(
            """
@foo_startswith='X%'
@foo_endswith='%X'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Name" LIKE @foo_startswith AND b."Name" LIKE @foo_endswith
""");
    }

    public override async Task Same_captured_variable_twice_in_different_lambdas()
    {
        await base.Same_captured_variable_twice_in_different_lambdas();

        AssertSql(
            """
@foo_startswith='X%'
@foo_endswith='%X'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Name" LIKE @foo_startswith AND b."Name" LIKE @foo_endswith
""");
    }

    public override async Task Multiple_queries_with_captured_variables()
    {
        await base.Multiple_queries_with_captured_variables();

        AssertSql(
            """
@id1='8'
@id2='9'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = @id1 OR b."Id" = @id2
""",
            //
            """
@id1='8'

SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Id" = @id1
LIMIT 2
""");
    }

    #endregion Captured variable handling

    #region Negative cases

    public override async Task Dynamic_query_does_not_get_precompiled()
    {
        await base.Dynamic_query_does_not_get_precompiled();

        AssertSql();
    }

    public override async Task ToList_over_objects_does_not_get_precompiled()
    {
        await base.ToList_over_objects_does_not_get_precompiled();

        AssertSql();
    }

    public override async Task Query_compilation_failure()
    {
        await base.Query_compilation_failure();

        AssertSql();
    }

    public override async Task EF_Constant_is_not_supported()
    {
        await base.EF_Constant_is_not_supported();

        AssertSql();
    }

    public override async Task NotParameterizedAttribute_with_constant()
    {
        await base.NotParameterizedAttribute_with_constant();
AssertSql(
    """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
WHERE b."Name" = 'Blog2'
LIMIT 2
""");
    }

    public override async Task NotParameterizedAttribute_is_not_supported_with_non_constant_argument()
    {
        await base.NotParameterizedAttribute_is_not_supported_with_non_constant_argument();

        AssertSql();
    }

    public override async Task Query_syntax_is_not_supported()
    {
        await base.Query_syntax_is_not_supported();

        AssertSql();
    }

    #endregion Negative cases

    public override async Task Unsafe_accessor_gets_generated_once_for_multiple_queries()
    {
        await base.Unsafe_accessor_gets_generated_once_for_multiple_queries();

        AssertSql(
    """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""",
    //
    """
SELECT b."Id", b."Name", b."Json"
FROM "Blogs" AS b
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public class PrecompiledQueryNpgsqlFixture : PrecompiledQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            builder = base.AddOptions(builder);

            // TODO: Figure out if there's a nice way to continue using the retrying strategy
            var npgsqlOptionsBuilder = new NpgsqlDbContextOptionsBuilder(builder);
            npgsqlOptionsBuilder.ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
            return builder;
        }

        protected override async Task SeedAsync(PrecompiledQueryContext context)
        {
            var blog1 = new Blog { Id = 8, Name = "Blog1", Json = [] };
            var blog2 = new Blog
            {
                Id = 9,
                Name = "Blog2",
                Json =
                [
                    new JsonRoot { Number = 1, Text = "One", Inner = new JsonBranch { Date = new DateTime(2001, 1, 1,0, 0, 0, DateTimeKind.Utc) } },
                    new JsonRoot { Number = 2, Text = "Two", Inner = new JsonBranch { Date = new DateTime(2002, 2, 2,0, 0, 0, DateTimeKind.Utc) } },
                ]
            };

            context.Blogs.AddRange(blog1, blog2);

            var post11 = new Post { Id = 11, Title = "Post11", Blog = blog1 };
            var post12 = new Post { Id = 12, Title = "Post12", Blog = blog1 };
            var post21 = new Post { Id = 21, Title = "Post21", Blog = blog2 };
            var post22 = new Post { Id = 22, Title = "Post22", Blog = blog2 };
            var post23 = new Post { Id = 23, Title = "Post23", Blog = blog2 };

            context.Posts.AddRange(post11, post12, post21, post22, post23);
            await context.SaveChangesAsync();
        }

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers => NpgsqlPrecompiledQueryTestHelpers.Instance;
    }
}
