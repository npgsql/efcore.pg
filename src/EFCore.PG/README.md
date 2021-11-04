# Npgsql Entity Framework Core provider for PostgreSQL

Npgsql.EntityFrameworkCore.PostgreSQL is the open source EF Core provider for PostgreSQL. It allows you to interact with PostgreSQL via the most widely-used .NET O/RM from Microsoft, and use familiar LINQ syntax to express queries. It's built on top of [Npgsql](https://github.com/npgsql/npgsql).

The provider looks and feels just like any other Entity Framework Core provider. Here's a quick sample to get you started:

```csharp
await using var ctx = new BlogContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

// Insert a Blog
ctx.Blogs.Add(new() { Name = "FooBlog" });
await ctx.SaveChangesAsync();

// Query all blogs who's name starts with F
var fBlogs = await ctx.Blogs.Where(b => b.Name.StartsWith("F")).ToListAsync();

public class BlogContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(@"Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase");
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

Aside from providing general EF Core support for PostgreSQL, the provider also exposes some PostgreSQL-specific capabilities, allowing you to query JSON, array or range columns, as well as many other advanced features. For more information, see the [the Npgsql site](http://www.npgsql.org/efcore/index.html). For information about EF Core in general, see the [EF Core website](https://docs.microsoft.com/ef/core/).

