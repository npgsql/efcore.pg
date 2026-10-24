using System.Collections.Immutable;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class HstoreTranslationsTest : IClassFixture<HstoreTranslationsTest.HstoreQueryFixture>
{
    private HstoreQueryFixture Fixture { get; }

    public HstoreTranslationsTest(HstoreQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags["Variant"] == "SD").Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" -> 'Variant' = 'SD'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_parameter_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var key = "Variant";
        var query = context.Entities.Where(e => e.Tags[EF.Parameter(key)] == "SD").Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1], results);
        AssertSql(
            """
@key='Variant'

SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" -> @key = 'SD'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_column_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags[e.Key] == "SD").Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" -> e."Key" = 'SD'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_computed_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.Tags[e.Key + "Suffix"] == "value")
            .Select(e => e.Tags[e.Key + "Suffix"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(["value"], results);
        AssertSql(
            """
SELECT e."Tags" -> (e."Key" || 'Suffix')
FROM "Entities" AS e
WHERE e."Tags" -> (e."Key" || 'Suffix') = 'value'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_projection(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.Tags["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", "HD", null, null }, results);
        AssertSql(
            """
SELECT e."Tags" -> 'Variant'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.NullableTags!["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", null, null, null }, results);
        AssertSql(
            """
SELECT e."NullableTags" -> 'Variant'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_string_length(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.Tags["Variant"]!.Length == 2)
            .Select(e => e.Tags["Variant"]!.Length);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([2, 2], results);
        AssertSql(
            """
SELECT length(e."Tags" -> 'Variant')::int
FROM "Entities" AS e
WHERE length(e."Tags" -> 'Variant')::int = 2
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_string_methods(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags["Variant"]!.StartsWith("S")).Select(e => e.Tags["Variant"]!.ToUpper());
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(["SD"], results);
        AssertSql(
            """
SELECT upper(e."Tags" -> 'Variant')
FROM "Entities" AS e
WHERE e."Tags" -> 'Variant' LIKE 'S%'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_null(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags["Variant"] == null).OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" -> 'Variant' IS NULL
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_inequality(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags["Variant"] != "SD").OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([2, 3, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" -> 'Variant' <> 'SD' OR e."Tags" -> 'Variant' IS NULL
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.ImmutableTags[e.Key] == "SD").Select(e => e.ImmutableTags["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(["SD"], results);
        AssertSql(
            """
SELECT e."ImmutableTags" -> 'Variant'
FROM "Entities" AS e
WHERE e."ImmutableTags" -> e."Key" = 'SD'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable_parameter_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var key = "Variant";
        var query = context.Entities.Where(e => e.ImmutableTags[EF.Parameter(key)] == "SD").Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1], results);
        AssertSql(
            """
@key='Variant'

SELECT e."Id"
FROM "Entities" AS e
WHERE e."ImmutableTags" -> @key = 'SD'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable_computed_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.ImmutableTags[e.Key + "Suffix"]!.StartsWith("val"))
            .Select(e => e.ImmutableTags[e.Key + "Suffix"]!.Length);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([5], results);
        AssertSql(
            """
SELECT length(e."ImmutableTags" -> (e."Key" || 'Suffix'))::int
FROM "Entities" AS e
WHERE e."ImmutableTags" -> (e."Key" || 'Suffix') LIKE 'val%'
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable_projection(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.ImmutableTags["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", "HD", null, null }, results);
        AssertSql(
            """
SELECT e."ImmutableTags" -> 'Variant'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.NullableImmutableTags!["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", null, null, null }, results);
        AssertSql(
            """
SELECT e."NullableImmutableTags" -> 'Variant'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_immutable_inequality(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.ImmutableTags["Variant"] != "SD").OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([2, 3, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."ImmutableTags" -> 'Variant' <> 'SD' OR e."ImmutableTags" -> 'Variant' IS NULL
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexer_ordering(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Tags["Variant"]).ThenBy(e => e.ImmutableTags["Variant"]).ThenBy(e => e.Id)
            .Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3, 4, 2, 1], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
ORDER BY e."Tags" -> 'Variant' NULLS FIRST, e."ImmutableTags" -> 'Variant' NULLS FIRST, e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Mutable = e.Tags.ContainsKey("Variant"), Immutable = e.ImmutableTags.ContainsKey("Variant") });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([true, true, false, true], results.Select(e => e.Mutable));
        Assert.Equal([true, true, false, true], results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT e."Tags" ? 'Variant' AS "Mutable", e."ImmutableTags" ? 'Variant' AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey_parameter_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var key = "Variant";
        var query = context.Entities
            .Where(e => e.Tags.ContainsKey(EF.Parameter(key)) && e.ImmutableTags.ContainsKey(EF.Parameter(key)))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2, 4], results);
        AssertSql(
            """
@key='Variant'

SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" ? @key AND e."ImmutableTags" ? @key
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey_column_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.Tags.ContainsKey(e.Key) && e.ImmutableTags.ContainsKey(e.Key))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" ? e."Key" AND e."ImmutableTags" ? e."Key"
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey_computed_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.Tags.ContainsKey(e.Key + "Suffix") && e.ImmutableTags.ContainsKey(e.Key + "Suffix"))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" ? (e."Key" || 'Suffix') AND e."ImmutableTags" ? (e."Key" || 'Suffix')
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey_negated(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => !e.Tags.ContainsKey("Variant") && !e.ImmutableTags.ContainsKey("Variant"))
            .Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE NOT (e."Tags" ? 'Variant') AND NOT (e."ImmutableTags" ? 'Variant')
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsKey_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Mutable = (bool?)e.NullableTags!.ContainsKey("Variant"),
                Immutable = (bool?)e.NullableImmutableTags!.ContainsKey("Variant")
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new bool?[] { true, null, false, true }, results.Select(e => e.Mutable));
        Assert.Equal(new bool?[] { true, null, false, true }, results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT e."NullableTags" ? 'Variant' AS "Mutable", e."NullableImmutableTags" ? 'Variant' AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Count(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags.Count == 2 && e.ImmutableTags.Count == 2).OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE cardinality(akeys(e."Tags")) = 2 AND cardinality(akeys(e."ImmutableTags")) = 2
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Count_projection(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => new { Mutable = e.Tags.Count, Immutable = e.ImmutableTags.Count });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([2, 2, 0, 1], results.Select(e => e.Mutable));
        Assert.Equal([2, 2, 0, 1], results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT cardinality(akeys(e."Tags")) AS "Mutable", cardinality(akeys(e."ImmutableTags")) AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Count_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Mutable = (int?)e.NullableTags!.Count, Immutable = (int?)e.NullableImmutableTags!.Count });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new int?[] { 1, null, 0, 1 }, results.Select(e => e.Mutable));
        Assert.Equal(new int?[] { 1, null, 0, 1 }, results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT cardinality(akeys(e."NullableTags")) AS "Mutable", cardinality(akeys(e."NullableImmutableTags")) AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsEmpty(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.ImmutableTags.IsEmpty).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE cardinality(akeys(e."ImmutableTags")) = 0
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsEmpty_negated(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => !e.ImmutableTags.IsEmpty).OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE cardinality(akeys(e."ImmutableTags")) <> 0
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsEmpty_projection(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.ImmutableTags.IsEmpty);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([false, false, true, false], results);
        AssertSql(
            """
SELECT cardinality(akeys(e."ImmutableTags")) = 0
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsEmpty_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.NullableImmutableTags!.IsEmpty).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE cardinality(akeys(e."NullableImmutableTags")) = 0
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Remove(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Original = e.ImmutableTags, Removed = e.ImmutableTags.Remove("Variant") });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([2, 2, 0, 1], results.Select(e => e.Original.Count));
        Assert.Equal([1, 1, 0, 0], results.Select(e => e.Removed.Count));
        Assert.All(results, e => Assert.False(e.Removed.ContainsKey("Variant")));
        Assert.Equal(["value", "other"], results.Take(2).Select(e => e.Removed["VariantSuffix"]));
        Assert.True(results[3].Original.ContainsKey("Variant"));
        Assert.Null(results[3].Original["Variant"]);
        AssertSql(
            """
SELECT e."ImmutableTags" AS "Original", e."ImmutableTags" - 'Variant'::text AS "Removed"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Remove_parameter_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var key = "Variant";
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.ImmutableTags.Remove(EF.Parameter(key)));
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 1, 0, 0], results.Select(e => e.Count));
        Assert.All(results, e => Assert.False(e.ContainsKey("Variant")));
        AssertSql(
            """
@key='Variant'

SELECT e."ImmutableTags" - @key
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Remove_column_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.ImmutableTags.Remove(e.Key).Count);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 1, 0, 0], results);
        AssertSql(
            """
SELECT cardinality(akeys(e."ImmutableTags" - e."Key"))
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Remove_computed_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.ImmutableTags.Remove(e.Key + "Suffix").Count);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 1, 0, 1], results);
        AssertSql(
            """
SELECT cardinality(akeys(e."ImmutableTags" - (e."Key" || 'Suffix')))
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Remove_composition(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.ImmutableTags.Remove("VariantSuffix").Count == 1).OrderBy(e => e.Id)
            .Select(e => e.ImmutableTags.Remove("VariantSuffix")["Variant"]);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", "HD", null }, results);
        AssertSql(
            """
SELECT e."ImmutableTags" - 'VariantSuffix'::text -> 'Variant'
FROM "Entities" AS e
WHERE cardinality(akeys(e."ImmutableTags" - 'VariantSuffix'::text)) = 1
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Remove_missing_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Original = e.ImmutableTags, Removed = e.ImmutableTags.Remove("Missing") });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        foreach (var result in results)
        {
            Assert.Equal(result.Original.OrderBy(e => e.Key), result.Removed.OrderBy(e => e.Key));
        }

        AssertSql(
            """
SELECT e."ImmutableTags" AS "Original", e."ImmutableTags" - 'Missing'::text AS "Removed"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Remove_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Single = e.NullableImmutableTags!.Remove("Variant"),
                Multiple = e.NullableImmutableTags!.RemoveRange(new[] { "Variant" })
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new int?[] { 0, null, 0, 0 }, results.Select(e => e.Single?.Count));
        Assert.Equal(new int?[] { 0, null, 0, 0 }, results.Select(e => e.Multiple?.Count));
        AssertSql(
            """
SELECT e."NullableImmutableTags" - 'Variant'::text AS "Single", e."NullableImmutableTags" - ARRAY['Variant']::text[] AS "Multiple"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Remove_chained(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.ImmutableTags.Remove("Variant").RemoveRange(new[] { "VariantSuffix" }).IsEmpty)
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2, 3, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE cardinality(akeys((e."ImmutableTags" - 'Variant'::text) - ARRAY['VariantSuffix']::text[])) = 0
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveRange(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.ImmutableTags.RemoveRange(new[] { "Variant", "Missing" }));
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 1, 0, 0], results.Select(e => e.Count));
        Assert.All(results, e => Assert.False(e.ContainsKey("Variant")));
        Assert.Equal(["value", "other"], results.Take(2).Select(e => e["VariantSuffix"]));
        AssertSql(
            """
SELECT e."ImmutableTags" - ARRAY['Variant','Missing']::text[]
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveRange_parameter_keys(bool async)
    {
        using var context = Fixture.CreateContext();
        string[] keys = ["Variant", "Missing"];
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.ImmutableTags.RemoveRange(EF.Parameter(keys)));
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 1, 0, 0], results.Select(e => e.Count));
        Assert.All(results, e => Assert.False(e.ContainsKey("Variant")));
        AssertSql(
            """
@keys={ 'Variant'
'Missing' } (DbType = Object)

SELECT e."ImmutableTags" - @keys
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveRange_column_keys(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.ImmutableTags.RemoveRange(e.Keys));
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(4, results.Length);
        Assert.All(results, e => Assert.Empty(e));
        AssertSql(
            """
SELECT e."ImmutableTags" - e."Keys"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveRange_computed_keys(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => e.ImmutableTags.RemoveRange(new[] { e.Key, e.Key + "Suffix" }).IsEmpty);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([true, true, true, true], results);
        AssertSql(
            """
SELECT cardinality(akeys(e."ImmutableTags" - ARRAY[e."Key",e."Key" || 'Suffix']::text[])) = 0
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveRange_empty_keys(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Original = e.ImmutableTags, Removed = e.ImmutableTags.RemoveRange(Array.Empty<string>()) });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        foreach (var result in results)
        {
            Assert.Equal(result.Original.OrderBy(e => e.Key), result.Removed.OrderBy(e => e.Key));
        }

        AssertSql(
            """
SELECT e."ImmutableTags" AS "Original", e."ImmutableTags" - ARRAY[]::text[] AS "Removed"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SetItems(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Original = e.ImmutableTags, Updated = e.ImmutableTags.SetItems(e.Updates) });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([2, 2, 0, 1], results.Select(e => e.Original.Count));
        Assert.Equal(new string?[] { "SD", "HD", null, null }, results.Select(e => e.Original.GetValueOrDefault("Variant")));
        Assert.Equal([4, 4, 3, 3], results.Select(e => e.Updated.Count));
        Assert.All(results, e =>
        {
            Assert.Equal("Updated", e.Updated["Variant"]);
            Assert.Equal("New", e.Updated["Added"]);
            Assert.True(e.Updated.ContainsKey("NullValue"));
            Assert.Null(e.Updated["NullValue"]);
        });
        Assert.Equal(["value", "other"], results.Take(2).Select(e => e.Updated["VariantSuffix"]));
        AssertSql(
            """
SELECT e."ImmutableTags" AS "Original", e."ImmutableTags" || e."Updates" AS "Updated"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SetItems_parameter(bool async)
    {
        var items = new Dictionary<string, string?> { ["Variant"] = "Updated", ["Added"] = "New", ["NullValue"] = null };
        var immutableItems = items.ToImmutableDictionary();
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Mutable = e.ImmutableTags.SetItems(items), Immutable = e.ImmutableTags.SetItems(immutableItems) });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([4, 4, 3, 3], results.Select(e => e.Mutable.Count));
        Assert.Equal([4, 4, 3, 3], results.Select(e => e.Immutable.Count));
        Assert.All(results.SelectMany(e => new[] { e.Mutable, e.Immutable }), e =>
        {
            Assert.Equal("Updated", e["Variant"]);
            Assert.Equal("New", e["Added"]);
            Assert.True(e.ContainsKey("NullValue"));
            Assert.Null(e["NullValue"]);
        });
        AssertSql(
            """
@items='System.Collections.Generic.Dictionary`2[System.String,System.String]' (DbType = Object)
@immutableItems='System.Collections.Immutable.ImmutableDictionary`2[System.String,System.String]' (DbType = Object)

SELECT e."ImmutableTags" || @items AS "Mutable", e."ImmutableTags" || @immutableItems AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SetItems_empty_parameter(bool async)
    {
        var items = ImmutableDictionary<string, string?>.Empty;
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Original = e.ImmutableTags, Updated = e.ImmutableTags.SetItems(items) });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.All(results, e => Assert.Equal(e.Original.OrderBy(p => p.Key), e.Updated.OrderBy(p => p.Key)));
        AssertSql(
            """
@items='System.Collections.Immutable.ImmutableDictionary`2[System.String,System.String]' (DbType = Object)

SELECT e."ImmutableTags" AS "Original", e."ImmutableTags" || @items AS "Updated"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SetItems_composed(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Value = e.ImmutableTags.SetItems(e.Updates).Remove("Added").SetItems(e.ImmutableTags)["Variant"],
                Count = e.ImmutableTags.SetItems(e.Updates).Count,
                RemovedCount = e.ImmutableTags.SetItems(e.Updates).Remove("Added").Count,
                HasNull = e.ImmutableTags.SetItems(e.Updates).ContainsKey("NullValue"),
                IsEmpty = e.ImmutableTags.SetItems(e.Updates).IsEmpty
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", "HD", "Updated", null }, results.Select(e => e.Value));
        Assert.Equal([4, 4, 3, 3], results.Select(e => e.Count));
        Assert.Equal([3, 3, 2, 2], results.Select(e => e.RemovedCount));
        Assert.All(results, e =>
        {
            Assert.True(e.HasNull);
            Assert.False(e.IsEmpty);
        });
        AssertSql(
            """
SELECT ((e."ImmutableTags" || e."Updates") - 'Added'::text || e."ImmutableTags") -> 'Variant' AS "Value", cardinality(akeys(e."ImmutableTags" || e."Updates")) AS "Count", cardinality(akeys((e."ImmutableTags" || e."Updates") - 'Added'::text)) AS "RemovedCount", (e."ImmutableTags" || e."Updates") ? 'NullValue' AS "HasNull", cardinality(akeys(e."ImmutableTags" || e."Updates")) = 0 AS "IsEmpty"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SetItems_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id).Select(e => e.NullableImmutableTags!.SetItems(e.Updates));
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Null(results[1]);
        Assert.Equal("Updated", results[0]["Variant"]);
        Assert.Equal("Updated", results[2]["Variant"]);
        Assert.Equal("Updated", results[3]["Variant"]);
        Assert.Equal(3, results[0].Count);
        Assert.Equal(3, results[2].Count);
        Assert.Equal(3, results[3].Count);
        AssertSql(
            """
SELECT e."NullableImmutableTags" || e."Updates"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetValueOrDefault(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Mutable = e.Tags.GetValueOrDefault("Variant"), Immutable = e.ImmutableTags.GetValueOrDefault("Variant") });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", "HD", null, null }, results.Select(e => e.Mutable));
        Assert.Equal(new string?[] { "SD", "HD", null, null }, results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT e."Tags" -> 'Variant' AS "Mutable", e."ImmutableTags" -> 'Variant' AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetValueOrDefault_fallback(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Mutable = e.Tags.GetValueOrDefault("Variant", "fallback"),
                Immutable = e.ImmutableTags.GetValueOrDefault("Variant", "fallback")
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", "HD", "fallback", null }, results.Select(e => e.Mutable));
        Assert.Equal(new string?[] { "SD", "HD", "fallback", null }, results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT CASE
    WHEN e."Tags" ? 'Variant' THEN e."Tags" -> 'Variant'
    ELSE 'fallback'
END AS "Mutable", CASE
    WHEN e."ImmutableTags" ? 'Variant' THEN e."ImmutableTags" -> 'Variant'
    ELSE 'fallback'
END AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetValueOrDefault_parameter(bool async)
    {
        var key = "Variant";
        var fallback = "fallback";
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Mutable = e.Tags.GetValueOrDefault(EF.Parameter(key)),
                Immutable = e.ImmutableTags.GetValueOrDefault(EF.Parameter(key)),
                MutableFallback = e.Tags.GetValueOrDefault(EF.Parameter(key), fallback),
                ImmutableFallback = e.ImmutableTags.GetValueOrDefault(EF.Parameter(key), fallback)
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", "HD", null, null }, results.Select(e => e.Mutable));
        Assert.Equal(new string?[] { "SD", "HD", null, null }, results.Select(e => e.Immutable));
        Assert.Equal(new string?[] { "SD", "HD", "fallback", null }, results.Select(e => e.MutableFallback));
        Assert.Equal(new string?[] { "SD", "HD", "fallback", null }, results.Select(e => e.ImmutableFallback));
        AssertSql(
            """
@key='Variant'
@fallback='fallback'

SELECT e."Tags" -> @key AS "Mutable", e."ImmutableTags" -> @key AS "Immutable", CASE
    WHEN e."Tags" ? @key THEN e."Tags" -> @key
    ELSE @fallback
END AS "MutableFallback", CASE
    WHEN e."ImmutableTags" ? @key THEN e."ImmutableTags" -> @key
    ELSE @fallback
END AS "ImmutableFallback"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetValueOrDefault_computed_key_and_column_fallback(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Mutable = e.Tags.GetValueOrDefault(e.Key + "Suffix", e.Key),
                Immutable = e.ImmutableTags.GetValueOrDefault(e.Key + "Suffix", e.Key),
                Length = e.ImmutableTags.GetValueOrDefault(e.Key + "Suffix", e.Key)!.Length
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(["value", "other", "Variant", "Variant"], results.Select(e => e.Mutable));
        Assert.Equal(["value", "other", "Variant", "Variant"], results.Select(e => e.Immutable));
        Assert.Equal([5, 5, 7, 7], results.Select(e => e.Length));
        AssertSql(
            """
SELECT CASE
    WHEN e."Tags" ? (e."Key" || 'Suffix') THEN e."Tags" -> (e."Key" || 'Suffix')
    ELSE e."Key"
END AS "Mutable", CASE
    WHEN e."ImmutableTags" ? (e."Key" || 'Suffix') THEN e."ImmutableTags" -> (e."Key" || 'Suffix')
    ELSE e."Key"
END AS "Immutable", length(CASE
    WHEN e."ImmutableTags" ? (e."Key" || 'Suffix') THEN e."ImmutableTags" -> (e."Key" || 'Suffix')
    ELSE e."Key"
END)::int AS "Length"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetValueOrDefault_nullable_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Mutable = e.NullableTags!.GetValueOrDefault("Variant"),
                Immutable = e.NullableImmutableTags!.GetValueOrDefault("Variant")
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(new string?[] { "SD", null, null, null }, results.Select(e => e.Mutable));
        Assert.Equal(new string?[] { "SD", null, null, null }, results.Select(e => e.Immutable));
        AssertSql(
            """
SELECT e."NullableTags" -> 'Variant' AS "Mutable", e."NullableImmutableTags" -> 'Variant' AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetValueOrDefault_predicate(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => e.Tags.GetValueOrDefault("Variant", "fallback") == "fallback"
                && e.ImmutableTags.GetValueOrDefault("Variant") == null)
            .Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE CASE
    WHEN e."Tags" ? 'Variant' THEN e."Tags" -> 'Variant'
    ELSE 'fallback'
END = 'fallback' AND e."ImmutableTags" -> 'Variant' IS NULL
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetValueOrDefault_parameter_hstore(bool async)
    {
        var tags = new Dictionary<string, string?> { ["Variant"] = "SD" };
        var immutableTags = tags.ToImmutableDictionary();
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Mutable = tags.GetValueOrDefault(e.Key),
                Immutable = immutableTags.GetValueOrDefault(e.Key),
                Fallback = tags.GetValueOrDefault(e.Key + "Suffix", "fallback")
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.All(results, e =>
        {
            Assert.Equal("SD", e.Mutable);
            Assert.Equal("SD", e.Immutable);
            Assert.Equal("fallback", e.Fallback);
        });
        AssertSql(
            """
@tags='System.Collections.Generic.Dictionary`2[System.String,System.String]' (DbType = Object)
@immutableTags='System.Collections.Immutable.ImmutableDictionary`2[System.String,System.String]' (DbType = Object)

SELECT @tags -> e."Key" AS "Mutable", @immutableTags -> e."Key" AS "Immutable", CASE
    WHEN @tags ? (e."Key" || 'Suffix') THEN @tags -> (e."Key" || 'Suffix')
    ELSE 'fallback'
END AS "Fallback"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Contains_all_keys(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => new[] { "Variant", "VariantSuffix" }.All(key => e.Tags.ContainsKey(key))
                && new[] { "Variant", "VariantSuffix" }.All(key => e.ImmutableTags.ContainsKey(key)))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" ?& ARRAY['Variant','VariantSuffix']::text[] AND e."ImmutableTags" ?& ARRAY['Variant','VariantSuffix']::text[]
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Contains_any_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => new[] { "Variant", "Missing" }.Any(key => e.Tags.ContainsKey(key))
                && new[] { "Variant", "Missing" }.Any(key => e.ImmutableTags.ContainsKey(key)))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Tags" ?| ARRAY['Variant','Missing']::text[] AND e."ImmutableTags" ?| ARRAY['Variant','Missing']::text[]
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Contains_keys_parameter_projection(bool async)
    {
        string[] keys = ["Variant", "Missing"];
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                All = keys.All(key => e.Tags.ContainsKey(key)),
                Any = keys.Any(key => e.Tags.ContainsKey(key)),
                ImmutableAll = keys.All(key => e.ImmutableTags.ContainsKey(key)),
                ImmutableAny = keys.Any(key => e.ImmutableTags.ContainsKey(key))
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.All(results, e =>
        {
            Assert.False(e.All);
            Assert.False(e.ImmutableAll);
        });
        Assert.Equal([true, true, false, true], results.Select(e => e.Any));
        Assert.Equal([true, true, false, true], results.Select(e => e.ImmutableAny));
        AssertSql(
            """
@keys={ 'Variant'
'Missing' } (DbType = Object)

SELECT e."Tags" ?& @keys AS "All", e."Tags" ?| @keys AS "Any", e."ImmutableTags" ?& @keys AS "ImmutableAll", e."ImmutableTags" ?| @keys AS "ImmutableAny"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Contains_keys_column_projection(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                All = e.Keys.All(key => e.Tags.ContainsKey(key)),
                Any = e.Keys.Any(key => e.ImmutableTags.ContainsKey(key))
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([true, true, false, false], results.Select(e => e.All));
        Assert.Equal([true, true, false, true], results.Select(e => e.Any));
        AssertSql(
            """
SELECT e."Tags" ?& e."Keys" AS "All", e."ImmutableTags" ?| e."Keys" AS "Any"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Contains_keys_empty_parameter(bool async)
    {
        string[] keys = [];
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                All = keys.All(key => e.Tags.ContainsKey(key)),
                Any = keys.Any(key => e.ImmutableTags.ContainsKey(key))
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.All(results, e =>
        {
            Assert.True(e.All);
            Assert.False(e.Any);
        });
        AssertSql(
            """
@keys={  } (DbType = Object)

SELECT e."Tags" ?& @keys AS "All", e."ImmutableTags" ?| @keys AS "Any"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Contains_keys_negated(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => !new[] { "Variant" }.Any(key => e.Tags.ContainsKey(key))
                && !new[] { "Variant" }.All(key => e.ImmutableTags.ContainsKey(key)))
            .Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([3], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE NOT (e."Tags" ?| ARRAY['Variant']::text[]) AND NOT (e."ImmutableTags" ?& ARRAY['Variant']::text[])
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Contains_keys_nullable_hstore(bool async)
    {
        string[] keys = ["Variant"];
        using var context = Fixture.CreateContext();
        var query = context.Entities
            .Where(e => keys.All(key => e.NullableTags!.ContainsKey(key))
                && keys.Any(key => e.NullableImmutableTags!.ContainsKey(key)))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 4], results);
        AssertSql(
            """
@keys={ 'Variant' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE e."NullableTags" ?& @keys AND e."NullableImmutableTags" ?| @keys
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Contains_keys_composed_hstore(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Keys.Any(key => e.ImmutableTags.Remove("Variant").ContainsKey(key)))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."ImmutableTags" - 'Variant'::text ?| e."Keys"
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Contains_keys_hstore_depends_on_key(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                All = e.Keys.All(key => e.ImmutableTags.Remove(key).ContainsKey(key)),
                Any = e.Keys.Any(key => e.ImmutableTags.Remove(key).ContainsKey(key))
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal(4, results.Length);
        Assert.All(results, e =>
        {
            Assert.False(e.All);
            Assert.False(e.Any);
        });
        AssertSql(
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM unnest(e."Keys") AS k(value)
    WHERE NOT (e."ImmutableTags" - k.value ? k.value)) AS "All", EXISTS (
    SELECT 1
    FROM unnest(e."Keys") AS k0(value)
    WHERE e."ImmutableTags" - k0.value ? k0.value) AS "Any"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsValue(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags.ContainsValue("SD") && e.ImmutableTags.ContainsValue("SD")).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE array_position(avals(e."Tags"), 'SD') IS NOT NULL AND array_position(avals(e."ImmutableTags"), 'SD') IS NOT NULL
""");
    }

    [Theory]
    [InlineData(false, "SD")]
    [InlineData(true, "SD")]
    [InlineData(false, "Missing")]
    [InlineData(true, "Missing")]
    [InlineData(false, null)]
    [InlineData(true, null)]
    public async Task ContainsValue_parameter_projection(bool async, string? value)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new { Mutable = e.Tags.ContainsValue(value), Immutable = e.ImmutableTags.ContainsValue(value) });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        bool[] expected = value switch
        {
            "SD" => [true, false, false, false],
            null => [false, false, false, true],
            _ => [false, false, false, false]
        };
        Assert.Equal(expected, results.Select(e => e.Mutable));
        Assert.Equal(expected, results.Select(e => e.Immutable));
        AssertSql(
            value is null
                ? """
SELECT array_position(avals(e."Tags"), NULL) IS NOT NULL AS "Mutable", array_position(avals(e."ImmutableTags"), NULL) IS NOT NULL AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
"""
                : $"""
@value='{value}'

SELECT array_position(avals(e."Tags"), @value) IS NOT NULL AS "Mutable", array_position(avals(e."ImmutableTags"), @value) IS NOT NULL AS "Immutable"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsValue_null(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags.ContainsValue(null) && e.ImmutableTags.ContainsValue(null)).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE array_position(avals(e."Tags"), NULL) IS NOT NULL AND array_position(avals(e."ImmutableTags"), NULL) IS NOT NULL
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsValue_negated(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => !e.Tags.ContainsValue(null) && !e.ImmutableTags.ContainsValue(null))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2, 3], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE array_position(avals(e."Tags"), NULL) IS NULL AND array_position(avals(e."ImmutableTags"), NULL) IS NULL
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsValue_computed_value(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.Where(e => e.Tags.ContainsValue(e.Tags["Variant"]!.ToUpper())
                && e.ImmutableTags.ContainsValue(e.ImmutableTags["Variant"]!.ToUpper()))
            .OrderBy(e => e.Id).Select(e => e.Id);
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([1, 2, 4], results);
        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE array_position(avals(e."Tags"), upper(e."Tags" -> 'Variant')) IS NOT NULL AND array_position(avals(e."ImmutableTags"), upper(e."ImmutableTags" -> 'Variant')) IS NOT NULL
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContainsValue_nullable_hstore_projection(bool async)
    {
        using var context = Fixture.CreateContext();
        var query = context.Entities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Mutable = e.NullableTags!.ContainsValue("SD"),
                Immutable = e.NullableImmutableTags!.ContainsValue("SD"),
                MutableNull = e.NullableTags!.ContainsValue(null),
                ImmutableNull = e.NullableImmutableTags!.ContainsValue(null)
            });
        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.Equal([true, false, false, false], results.Select(e => e.Mutable));
        Assert.Equal([true, false, false, false], results.Select(e => e.Immutable));
        Assert.Equal([false, false, false, true], results.Select(e => e.MutableNull));
        Assert.Equal([false, false, false, true], results.Select(e => e.ImmutableNull));
        AssertSql(
            """
SELECT array_position(avals(e."NullableTags"), 'SD') IS NOT NULL AS "Mutable", array_position(avals(e."NullableImmutableTags"), 'SD') IS NOT NULL AS "Immutable", array_position(avals(e."NullableTags"), NULL) IS NOT NULL AS "MutableNull", array_position(avals(e."NullableImmutableTags"), NULL) IS NOT NULL AS "ImmutableNull"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class HstoreQueryContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }
    }

    public class Entity
    {
        public int Id { get; set; }
        public string Key { get; set; } = "Variant";
        public string[] Keys { get; set; } = ["Variant", "VariantSuffix"];
        public Dictionary<string, string?> Tags { get; set; } = new();
        public Dictionary<string, string?>? NullableTags { get; set; }
        public Dictionary<string, string?> Updates { get; set; } = new()
        {
            ["Variant"] = "Updated",
            ["Added"] = "New",
            ["NullValue"] = null
        };
        public ImmutableDictionary<string, string?> ImmutableTags { get; set; } = ImmutableDictionary<string, string?>.Empty;
        public ImmutableDictionary<string, string?>? NullableImmutableTags { get; set; }
    }

    public class HstoreQueryFixture : SharedStoreFixtureBase<HstoreQueryContext>
    {
        protected override string StoreName
            => "HstoreQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Entity>().Property(e => e.Tags).HasColumnType("hstore");
            modelBuilder.Entity<Entity>().Property(e => e.NullableTags).HasColumnType("hstore");
            modelBuilder.Entity<Entity>().Property(e => e.Updates).HasColumnType("hstore");
            modelBuilder.Entity<Entity>().Property(e => e.ImmutableTags).HasColumnType("hstore");
            modelBuilder.Entity<Entity>().Property(e => e.NullableImmutableTags).HasColumnType("hstore");
        }

        protected override async Task SeedAsync(HstoreQueryContext context)
        {
            Entity[] entities =
            [
                new Entity
                {
                    Id = 1,
                    Tags = new() { ["Variant"] = "SD", ["VariantSuffix"] = "value" },
                    NullableTags = new() { ["Variant"] = "SD" }
                },
                new Entity { Id = 2, Tags = new() { ["Variant"] = "HD", ["VariantSuffix"] = "other" } },
                new Entity { Id = 3, NullableTags = new() },
                new Entity { Id = 4, Tags = new() { ["Variant"] = null }, NullableTags = new() { ["Variant"] = null } }
            ];

            foreach (var entity in entities)
            {
                entity.ImmutableTags = entity.Tags.ToImmutableDictionary();
                entity.NullableImmutableTags = entity.NullableTags?.ToImmutableDictionary();
            }

            context.Entities.AddRange(entities);
            await context.SaveChangesAsync();
        }
    }
}
