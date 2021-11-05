# Npgsql Entity Framework Core provider for PostgreSQL

Npgsql.EntityFrameworkCore.PostgreSQL is the open source EF Core provider for PostgreSQL. It allows you to interact with PostgreSQL via the most widely-used .NET O/RM from Microsoft, and use familiar LINQ syntax to express queries.

This package is a plugin which allows you to use the [NodaTime](https://nodatime.org) date/time library when interacting with PostgreSQL; this provides a better and safer API for dealing with date and time data.

To use the plugin, simply add `UseNodaTime` as below and use NodaTime types in your entity properties:

```csharp
await using var ctx = new BlogContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

// Insert a Blog
ctx.Blogs.Add(new()
{
    Name = "FooBlog",
    CreationTime = SystemClock.Instance.GetCurrentInstant()
});
await ctx.SaveChangesAsync();

// Query all blogs created in 2020 or after
var newBlogs = await ctx.Blogs.Where(b => b.CreationTime >= Instant.FromUtc(2020, 1, 1, 0, 0, 0)).ToListAsync();

public class BlogContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(
            @"Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase",
            o => o.UseNodaTime());
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Instant CreationTime { get; set; }
}
```

The plugin also supports translating most NodaTime methods and properties into corresponding PostgreSQL date/time operations. For more information, see the [NodaTime plugin documentation page](https://www.npgsql.org/efcore/mapping/nodatime.html).
