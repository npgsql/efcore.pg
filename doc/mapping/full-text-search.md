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
Almost all PostgreSQL full text search functions can be called through LINQ queries. All supported EF Core LINQ methods are defined on the `NpgsqlFullTextSearchLinqExtensions` and `NpgsqlFullTextSearchDbFunctionsExtensions` types. Here is a table showing translations for some operations. If an operation you need is missing, please open an issue to request for it.

| This C# expression...                                    | ... gets translated to this SQL                                     |
|----------------------------------------------------------|---------------------------------------------------------------------|
| `.Select(c => EF.Functions.ToTsVector("english", c.Name))` | [`SELECT to_tsvector('english'::regconfig, c."Name")`](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-DOCUMENTS)
| `.Select(c => NpgsqlTsVector.Parse("b"))`                   | [`SELECT CAST('b' AS tsvector)`](https://www.postgresql.org/docs/current/static/sql-expressions.html#SQL-SYNTAX-TYPE-CASTS)
| `.Select(c => EF.Functions.ToTsQuery("english", "pgsql"))` | [`SELECT to_tsquery('english'::regconfig, 'pgsql')`](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
| `.Select(c => NpgsqlTsQuery.Parse("b"))`                   | [`SELECT CAST('b' AS tsquery)`](https://www.postgresql.org/docs/current/static/sql-expressions.html#SQL-SYNTAX-TYPE-CASTS)
| `.Where(c => c.SearchVector.Matches("Npgsql"))`            | [`WHERE c."SearchVector" @@ 'Npgsql'`](https://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING)
| `.Select(c => EF.Functions.ToTsQuery(c.SearchQuery).ToNegative())` | [`SELECT !! to_tsquery(c."SearchQuery")`](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
| `.Select(c => EF.Functions.ToTsVector(c.Name).SetWeight(NpgsqlTsVector.Lexeme.Weight.A))` | [`SELECT setweight(to_tsvector(c."Name"), 'A')`](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR)

## Setting up and querying a full text search index on an entity
This guide will help you setup the most common case of having a `tsvector` column on a table that updates itself with a trigger and querying it with EF Core.

### Setting up and maintaining the full text search index
First define an entity as shown below:

```c#
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public NpgsqlTsVector SearchVector { get; set; }
}
```

and modify the `OnModelCreating` of your `DbContext` class as follows:
```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
                .HasIndex(p => p.SearchVector)
                .ForNpgsqlHasMethod("GIN"); // Index method on the search vector (GIN or GIST)
}
```

Then use the EF Core tools (as shown below) or Visual Studio to add a migration to create this table in the database:
```
dotnet ef migrations add "CreateProductTable"
```

This will create the migration `CreateProductTable`:
```c#
public partial class CreateProductTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                Description = table.Column<string>(nullable: true),
                Name = table.Column<string>(nullable: true),
                SearchVector = table.Column<NpgsqlTsVector>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_SearchVector",
            table: "Products",
            column: "SearchVector")
            .Annotation("Npgsql:Npgsql:IndexMethod", "GIN");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Products");
    }
}
```

Currently, the Npgsql EF Core driver doesn't support creating full text search column triggers automatically. They can be created with raw SQL migrations instead. Edit the created migration to add the create trigger statement using [the PostgreSQL tsvector_update_trigger function](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-UPDATE-TRIGGERS) as follows:
```c#
public partial class CreateProductTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                Description = table.Column<string>(nullable: true),
                Name = table.Column<string>(nullable: true),
                SearchVector = table.Column<NpgsqlTsVector>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_SearchVector",
            table: "Products",
            column: "SearchVector")
            .Annotation("Npgsql:Npgsql:IndexMethod", "GIN");
            
        // Create full text search trigger
        migrationBuilder.Sql(
            @"CREATE TRIGGER product_search_vector_update BEFORE INSERT OR UPDATE
              ON ""Products"" FOR EACH ROW EXECUTE PROCEDURE
              tsvector_update_trigger(""SearchVector"", 'pg_catalog.english', ""Name"", ""Description"");");
              
        // If you were adding a tsvector to an existing table, you should populate the column using an UPDATE
        // migrationBuilder.Sql("UPDATE \"Products\" SET \"Name\" = \"Name\";");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Products");
    }
}
```
Now any inserts or updates on the `Products` table will populate the `SearchVector` column and maintain it automatically.

### Querying the full text search index
Here's an example of querying the `SearchVector` column:
```c#
var context = new ProductDbContext();
var npgsql = context.Query<Product>()
                    .Where(p => p.SearchVector.Matches(EF.Functions.ToTsQuery("Npgsql")))
                    .FirstOrDefault();
```
