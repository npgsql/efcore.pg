# Full Text Search

PostgreSQL has [built-in support for full-text search](https://www.postgresql.org/docs/current/static/textsearch.html), which allows you to conveniently and efficiently query natural language documents.

## Mapping

PostgreSQL full text search types are mapped onto .NET types built-in to Npgsql. The `tsvector` type is mapped to `NpgsqlTsVector` and `tsquery` is mapped to `NpgsqlTsQuery`. This means you can use properties of type `NpgsqlTsVector` directly in your model to create `tsvector` columns. The `NpgsqlTsQuery` type on the other hand, is used in LINQ queries.

```csharp
public class Product
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public NpgsqlTsVector SearchVector { get; set; }
}
```

## Setting up and querying a full text search index on an entity

[As the PostgreSQL documentation explains](https://www.postgresql.org/docs/current/static/textsearch-tables.html), full-text search requires an index to run efficiently. This section will show two ways to do this, each having its benefits and drawbacks. Please read the PostgreSQL docs for more information on the two different approaches.

### Method 1: tsvector column

This method adds a `tsvector` column to your table, that is automatically updated when the row is modified. First, add an `NpgsqlTsVector` property to your entity:

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public NpgsqlTsVector SearchVector { get; set; }
}
```

Then, configure the property to be a [stored generated column](../modeling/generated-properties.md#computed-generated-columns): the provider has an API for setting that up:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .HasGeneratedTsVectorColumn(
            p => p.SearchVector,
            "english",  // Text search config
            p => new { p.Name, p.Description })  // Included properties
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)
}
```

Note that `HasGeneratedTsVectorColumn()` is simply a bit of sugar of EF's standard `HasComputedColumnSql()`; if there's anything you need which isn't covered by `HasGeneratedTsVectorColumn()`, simply use `HasComputedColumnSql()`.

Once your generated `tsvector` column is set up, any inserts or updates on the `Products` table will update the `SearchVector` column and maintain it automatically. You can query it as follows:

```csharp
var context = new ProductDbContext();
var npgsql = context.Products
    .Where(p => p.SearchVector.Matches("Npgsql"))
    .ToList();
```

### Method 2: Expression index

If you prefer to have an expression index rather than a generated column, use the following API to do so:

```csharp
modelBuilder.Entity<Blog>()
    .HasIndex(b => new { b.Title, b.Description })
    .HasMethod("GIN")
    .IsTsVectorExpressionIndex("english");
```

Once the index is created on the `Title` and `Description` columns, you can query as follows:

```csharp
var context = new ProductDbContext();
var npgsql = context.Products
    .Where(p => EF.Functions.ToTsVector("english", p.Title + " " + p.Description)
        .Matches("Npgsql"))
    .ToList();
```

## Computed column over JSON columns

The provider can also create computed `tsvector` columns over JSON columns. Simply use `HasGeneratedTsVectorColumn()` as shown above, and when applied to JSON columns, the provider will automatically generate [`json_to_tsvector/jsonb_to_tsvector`](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-FUNCTIONS-TABLE) as appropriate.

Note that this will pass the filter `all` to these functions, meaning that all values in the JSON document will be included. To customize the filter - or to create the computed column on older versions of the provider - simply specify the function yourself via [`HasComputedColumnSql`](https://docs.microsoft.com/ef/core/modeling/generated-properties?tabs=data-annotations#computed-generated-columns).

## Operation translation

Almost all PostgreSQL full text search functions can be called through LINQ queries. All supported EF Core LINQ methods are defined in extension classes in the `Microsoft.EntityFrameworkCore` namespace, so simply referencing the Npgsql provider will light up these methods. The following table lists all supported operations; if an operation you need is missing, please open an issue to request for it.

.NET                                                                        | SQL
----------------------------------------------------------------------------|-----
EF.Functions.ToTsVector(string)                                             | [to_tsvector(string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-DOCUMENTS)
EF.Functions.ToTsVector("english", string)                                  | [to_tsvector('english'::regconfig, string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-DOCUMENTS)
EF.Functions.ToTsQuery(string)                                              | [to_tsquery(string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
EF.Functions.ToTsQuery("english", string )                                  | [to_tsquery('english'::regconfig, string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
EF.Functions.PlainToTsQuery(string)                                         | [plainto_tsquery(string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
EF.Functions.PlainToTsQuery("english", string)                              | [plainto_tsquery('english'::regconfig, string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
EF.Functions.PhraseToTsQuery(string)                                        | [phraseto_tsquery(string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
EF.Functions.PhraseToTsQuery("english", string)                             | [phraseto_tsquery('english'::regconfig, string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
EF.Functions.WebSearchToTsQuery(string)                                     | [websearch_to_tsquery(string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
EF.Functions.WebSearchToTsQuery("english", string)                          | [websearch_to_tsquery('english'::regconfig, string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
EF.Functions.ArrayToTsVector(new[] { "a", "b" })                            | [array_to_tsvector(ARRAY\['a', 'b'\])](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-FUNCTIONS-TABLE)
NpgsqlTsVector.Parse(string)                                                | [CAST(string AS tsvector)](https://www.postgresql.org/docs/current/static/sql-expressions.html#SQL-SYNTAX-TYPE-CASTS)
NpgsqlTsQuery.Parse(string)                                                 | [CAST(queryString AS tsquery)](https://www.postgresql.org/docs/current/static/sql-expressions.html#SQL-SYNTAX-TYPE-CASTS)
tsvector.Matches(string)                                                    | [tsvector @@ plainto_tsquery(string)](https://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING)
tsvector.Matches(tsquery)                                                   | [tsvector @@ tsquery](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
tsquery1.And(tsquery2)                                                      | [tsquery1 && tsquery2](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
tsquery1.Or(tsquery2)                                                       | [tsquery1 \|\| tsquery2](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
tsquery.ToNegative()                                                        | [!! tsquery](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
tsquery1.Contains(tsquery2)                                                 | [tsquery1 @> tsquery2](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
tsquery1.IscontainedIn(tsquery2)                                            | [tsquery1 <@ tsquery2](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
tsquery.GetNodeCount()                                                      | [numnode(query)](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
tsquery.GetQueryTree()                                                      | [querytree(query)](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
tsquery.GetResultHeadline("a b c")                                          | [ts_headline('a b c', query)](https://www.postgresql.org/docs/current/textsearch-controls.html#TEXTSEARCH-HEADLINE)
tsquery.GetResultHeadline("a b c", "MinWords=1, MaxWords=2")                | [ts_headline('a b c', query, 'MinWords=1, MaxWords=2')](https://www.postgresql.org/docs/current/textsearch-controls.html#TEXTSEARCH-HEADLINE)
tsquery.Rewrite(targetQuery, substituteQuery)                               | [ts_rewrite(to_tsquery(tsquery), to_tsquery(targetQuery), to_tsquery(substituteQuery))](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-FUNCTIONS-TABLE)
tsquery1.ToPhrase(tsquery2)                                                 | [tsquery_phrase(tsquery1, tsquery2)](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-FUNCTIONS-TABLE)
tsquery1.ToPhrase(tsquery2, distance)                                       | [tsquery_phrase(tsquery1, tsquery2, distance)](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-FUNCTIONS-TABLE)
tsvector1.Concat(tsvector2)                                                 | [tsvector1 \|\| tsvector2](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-OPERATORS-TABLE)
tsvector.Delete("x")                                                        | [ts_delete(tsvector, 'x')](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-FUNCTIONS-TABLE)
tsvector.Delete(new[] { "x", "y" })                                         | [ts_delete(tsvector, ARRAY\['x', 'y'\])](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-FUNCTIONS-TABLE)
tsvector.Filter(new[] { "x", "y" })                                         | [ts_filter(tsvector, ARRAY\['x', 'y'\])](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-FUNCTIONS-TABLE)
tsvector.GetLength()                                                        | [length(tsvector)](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-FUNCTIONS-TABLE)
tsvector.Rank(tsquery)                                                      | [ts_rank(tsvector, tsquery)](https://www.postgresql.org/docs/current/textsearch-controls.html#TEXTSEARCH-RANKING)
tsvector.RankCoverDensity(tsquery)                                          | [ts_rank_cd(tsvector, tsquery)](https://www.postgresql.org/docs/current/textsearch-controls.html#TEXTSEARCH-RANKING)
tsvector.SetWeight(NpgsqlTsVector.Lexeme.Weight.A)                          | [setweight(tsvector, 'A')](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR)
tsvector.ToStripped()                                                       | [strip(tsvector)](https://www.postgresql.org/docs/current/functions-textsearch.html#TEXTSEARCH-FUNCTIONS-TABLE)
EF.Functions.Unaccent(string)                                               | [unaccent(string)](https://www.postgresql.org/docs/current/unaccent.html)
EF.Functions.Unaccent(regdictionary, string)                                | [unaccent(regdictionary, string)](https://www.postgresql.org/docs/current/unaccent.html)
