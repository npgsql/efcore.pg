# Npgsql Entity Framework Core provider for PostgreSQL

Npgsql.EntityFrameworkCore.PostgreSQL is the open source EF Core provider for PostgreSQL. It allows you to interact with PostgreSQL via the most widely-used .NET O/RM from Microsoft, and use familiar LINQ syntax to express queries.

This package is a plugin which allows you to interact with spatial data provided by the PostgreSQL [PostGIS extension](https://postgis.net); PostGIS is a mature, standard extension considered to provide top-of-the-line database spatial features. On the .NET side, the plugin adds support for the types from the [NetTopologySuite library](https://github.com/NetTopologySuite/NetTopologySuite), allowing you to read and write them directly to PostgreSQL.

To use the plugin, simply add `UseNetTopologySuite` as below and use NetTopologySuite types in your entity properties:

```csharp
await using var ctx = new BlogContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

// Insert a Blog
ctx.Cities.Add(new()
{
    Name = "FooCity",
    Center = new Point(10, 10)
});
await ctx.SaveChangesAsync();

// Query all cities with the given center point
var newBlogs = await ctx.Cities.Where(b => b.Center == new Point(10, 10)).ToListAsync();

public class BlogContext : DbContext
{
    public DbSet<City> Cities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(
            @"Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase",
            o => o.UseNetTopologySuite());
}

public class City
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Point Center { get; set; }
}
```

The plugin also supports translating many NetTopologySuite methods and properties into corresponding PostGIS operations. For more information, see the [NetTopologySuite plugin documentation page](https://www.npgsql.org/efcore/mapping/nts.html).
