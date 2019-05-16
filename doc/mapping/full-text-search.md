# Full Text Search

PostgreSQL has [built-in support for full-text search](https://www.postgresql.org/docs/current/static/textsearch.html), which allows you to conveniently and efficiently query natural language documents.

## Mapping

PostgreSQL full text search types are mapped onto .NET types built-in to Npgsql. The `tsvector` type is mapped to `NpgsqlTsVector` and `tsquery` is mapped to `NpgsqlTsQuery`. This means you can use properties of type `NpgsqlTsVector` directly in your model to create `tsvector` columns. The `NpgsqlTsQuery` type on the other hand, is used in LINQ queries.

```c#
public class BlogPost
{
    public string Title { get; set; }
    public string Content { get; set; }
    public NpgsqlTsVector SearchVector { get; set; }
}
```

## Operation translation

Almost all PostgreSQL full text search functions can be called through LINQ queries. All supported EF Core LINQ methods are defined in extension classes in the `Microsoft.EntityFrameworkCore` namespace, so simply referencing the Npgsql provider will light up these methods. Here is a table showing translations for some operations; if an operation you need is missing, please open an issue to request for it.

| This C# expression...                                    | ... gets translated to this SQL                                     |
|----------------------------------------------------------|---------------------------------------------------------------------|
| .Select(c => EF.Functions.ToTsVector("english", c.Name)) | [SELECT to_tsvector('english'::regconfig, c."Name")](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-DOCUMENTS)
| .Select(c => NpgsqlTsVector.Parse("b"))                  | [SELECT CAST('b' AS tsvector)](https://www.postgresql.org/docs/current/static/sql-expressions.html#SQL-SYNTAX-TYPE-CASTS)
| .Select(c => EF.Functions.ToTsQuery("english", "pgsql")) | [SELECT to_tsquery('english'::regconfig, 'pgsql')`](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
| .Select(c => NpgsqlTsQuery.Parse("b"))                   | [SELECT CAST('b' AS tsquery)](https://www.postgresql.org/docs/current/static/sql-expressions.html#SQL-SYNTAX-TYPE-CASTS)
| .Where(c => c.SearchVector.Matches("Npgsql"))            | [WHERE c."SearchVector" @@ 'Npgsql'](https://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING)
| .Select(c => EF.Functions.ToTsQuery(c.SearchQuery).ToNegative()) | [SELECT !! to_tsquery(c."SearchQuery")](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
| .Select(c => EF.Functions.ToTsVector(c.Name).SetWeight(NpgsqlTsVector.Lexeme.Weight.A)) | [SELECT setweight(to_tsvector(c."Name"), 'A')](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR)

## Setting up and querying a full text search index on an entity

As [the PostgreSQL documentation](https://www.postgresql.org/docs/current/static/textsearch-tables.html) explains, full-text search requires an index to run efficiently. This section will show two ways to do this, both (currently) requiring raw SQL in your migrations. Read the PostgreSQL docs for more information on the different approaches.

### Method 1: Expression index

The simpler method to use full-text search is to set up an expression index. Let's take the following entity:

```c#
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
```

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

This will create a full-text search index on the `Name` and `Description` columns. You can query as follows:

```c#
var context = new ProductDbContext();
var npgsql = context.Products
    .Where(p => EF.Functions.ToTsVector("english", p.Name + " " + p.Description).Matches("Npgsql"))
    .ToList();
```

### Method 2: tsvector column

Instead of an expression index, this method will add a `tsvector` column on your table that updates itself with a trigger.

First, add an `NpgsqlTsVector` property to your entity:

```c#
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public NpgsqlTsVector SearchVector { get; set; }
}
```

and modify the `OnModelCreating()` of your context class to add an index as follows:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .HasIndex(p => p.SearchVector)
        .ForNpgsqlHasMethod("GIN"); // Index method on the search vector (GIN or GIST)
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

Any inserts or updates on the `Products` table will now update the `SearchVector` column and maintain it automatically. You can query it as follows:

```c#
var context = new ProductDbContext();
var npgsql = context.Products
    .Where(p => p.SearchVector.Matches("Npgsql"))
    .ToList();
```
