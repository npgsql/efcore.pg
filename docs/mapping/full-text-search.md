# Full Text Search

PostgreSQL has [built-in support for full-text search](https://www.postgresql.org/docs/current/static/textsearch.html), which allows you to conveniently and efficiently query natural language documents.

## Mapping

PostgreSQL full text search types are mapped onto .NET types built-in to Npgsql. The `tsvector` type is mapped to `NpgsqlTsVector` and `tsquery` is mapped to `NpgsqlTsQuery`. This means you can use properties of type `NpgsqlTsVector` directly in your model to create `tsvector` columns. The `NpgsqlTsQuery` type on the other hand, is used in LINQ queries.

```c#
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

```c#
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public NpgsqlTsVector SearchVector { get; set; }
}
```

Setting up the column to be auto-updated depends on your PostgreSQL version. On PostgreSQL 12 and above, the column can be a simple [generated column](../modeling/generated-properties.md#computed-columns-on-add-or-update), and version 5.0.0 contains sugar for setting that up. In previous versions, you must manually set up database triggers that update the column instead.

#### [PostgreSQL 12+](#tab/pg12)

> [!NOTE]
> The below only works on PostgreSQL 12 and version 5.0.0 of the EF Core provider.

The following will set up a generated `tsvector` column, over which you can easily create an index:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .HasGeneratedTsVectorColumn(
            p => p.TsVector,
            "english",  // Text search config
            p => new { p.Name, p.Description })  // Included properties
        .HasIndex(p => p.TsVector)
        .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)
}
```

#### [Older Versions](#tab/older)

First, modify the `OnModelCreating()` of your context class to add an index as follows:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)
}
```

Now generate a migration (`dotnet ef migrations add ....`), and open it with your favorite editor, adding the following:

```c#
public partial class CreateProductTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Migrations for creation of the column and the index will appear here, all we need to do is set up the trigger to update the column:

        migrationBuilder.Sql(
            @"CREATE TRIGGER product_search_vector_update BEFORE INSERT OR UPDATE
              ON ""Products"" FOR EACH ROW EXECUTE PROCEDURE
              tsvector_update_trigger(""SearchVector"", 'pg_catalog.english', ""Name"", ""Description"");");

        // If you were adding a tsvector to an existing table, you should populate the column using an UPDATE
        // migrationBuilder.Sql("UPDATE \"Products\" SET \"Name\" = \"Name\";");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Migrations for dropping of the column and the index will appear here, all we need to do is drop the trigger:
        migrationBuilder.Sql("DROP TRIGGER product_search_vector_update");
    }
}
```

***

Once your auto-updated `tsvector` column is set up, any inserts or updates on the `Products` table will now update the `SearchVector` column and maintain it automatically. You can query it as follows:

```c#
var context = new ProductDbContext();
var npgsql = context.Products
    .Where(p => p.SearchVector.Matches("Npgsql"))
    .ToList();
```

### Method 2: Expression index

Version 5.0.0 of the provider includes sugar for defining the appropriate expression index; if you're using an older version, you'll have to define a raw SQL migration yourself.

#### [Version 5.0.0](#tab/v5)

```c#
modelBuilder.Entity<Blog>()
    .HasIndex(b => new { b.Title, b.Description })
    .IsTsVectorExpressionIndex("english");
```

#### [Older Versions](#tab/older)

Create a migration which will contain the index creation SQL (`dotnet ef migrations add ...`). At this point, open the generated migration with your editor and add the following:

```c#
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"CREATE INDEX fts_idx ON ""Product"" USING GIN (to_tsvector('english', ""Name"" || ' ' || ""Description""));");
}

protected override void Down(MigrationBuilder migrationBuilder)
    migrationBuilder.Sql(@"DROP INDEX fts_idx;");
}
```

***

Once the index is created on the `Title` and `Description` columns, you can query as follows:

```c#
var context = new ProductDbContext();
var npgsql = context.Products
    .Where(p => EF.Functions.ToTsVector("english", p.Title + " " + p.Description)
        .Matches("Npgsql"))
    .ToList();
```

## Operation translation

Almost all PostgreSQL full text search functions can be called through LINQ queries. All supported EF Core LINQ methods are defined in extension classes in the `Microsoft.EntityFrameworkCore` namespace, so simply referencing the Npgsql provider will light up these methods. Here is a table showing translations for some operations; if an operation you need is missing, please open an issue to request for it.

.NET                                                                        | SQL
----------------------------------------------------------------------------|-----
EF.Functions.ToTsVector("english", string)                                  | [to_tsvector('english'::regconfig, string)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-DOCUMENTS)
NpgsqlTsVector.Parse(string))                                               | [CAST(string AS tsvector)](https://www.postgresql.org/docs/current/static/sql-expressions.html#SQL-SYNTAX-TYPE-CASTS)
EF.Functions.ToTsQuery("english", queryString))                             | [to_tsquery('english'::regconfig, queryString)](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
NpgsqlTsQuery.Parse(queryString))                                           | [CAST(queryString AS tsquery)](https://www.postgresql.org/docs/current/static/sql-expressions.html#SQL-SYNTAX-TYPE-CASTS)
tsVector.Matches(queryString))                                              | [tsVector @@ queryString](https://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING)
tsVector.Matches(tsQuery)                                                   | [tsVector @@ tsQuery](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
tsQuery.ToNegative()                                                        | [!! tsQuery](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
tsVector.SetWeight(NpgsqlTsVector.Lexeme.Weight.A))                         | [setweight(tsVector, 'A')](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR)
tsVector.Rank(tsQuery)                                                      | [ts_rank(tsVector, tsQuery)](https://www.postgresql.org/docs/current/textsearch-controls.html#TEXTSEARCH-RANKING)
EF.Functions.Unaccent(string)                                               | [unaccent(string)](https://www.postgresql.org/docs/current/unaccent.html)
EF.Functions.Unaccent(regdictionary, string)                                | [unaccent(regdictionary, string)](https://www.postgresql.org/docs/current/unaccent.html)
