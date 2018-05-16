# Date/Time Mapping with NodaTime

> [!NOTE]
> This feature is only available in Npgsql EF Core 2.1, which is currently in preview.

By default, [the PostgreSQL date/time types](https://www.postgresql.org/docs/current/static/datatype-datetime.html) are mapped to the built-in .NET types (`DateTime`, `TimeSpan`). Unfortunately, these built-in types (`DateTime`, `DateTimeOffset`) are flawed in many ways; regardless of PostgreSQL or databases. The [NodaTime library](http://nodatime.org/) was created to solve many of these problems, and if your application handles dates and times in anything but the most basic way, you should seriously consider using NodaTime. To learn more [read this blog post by Jon Skeet](http://blog.nodatime.org/2011/08/what-wrong-with-datetime-anyway.html).

For PostgreSQL specifically, the NodaTime types map more naturally to the database types - everything is simpler and works in a more predictable way.

> [!NOTE]
> This plugin, which works at the Entity Framework Core level, is distinct from [the Npgsql ADO plugin for NodaTime](http://www.npgsql.org/doc/types/nodatime.html); the EF Core plugin references and relies on the ADO plugin.

# Setup

To set up the NodaTime plugin, add the [Npgsql.NodaTime nuget](https://www.nuget.org/packages/Npgsql.NodaTime) to your project. Then, make the following modification to your `UseNpgsql()` line:

```c#
protected override void OnConfiguring(DbContextOptionsBuilder builder)
{
    builder.UseNpgsql("Host=localhost;Database=test;Username=npgsql_tests;Password=npgsql_tests",
        o => o.UseNodaTime());
}
```

This will set up all the necessary mappings and operation translators. You can now use NodaTime types as regular properties in your entities, and even perform some operations:

```c#
public class Post
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Instant CreationTime { get; set; }
}

var recentPosts = context.Posts.Where(p => p.CreationTime > someInstant);
```

## Member Translation

Currently, the EF Core provider knows how to translate the most date/time component members of NodaTime's `LocalDateTime`, `LocalDate`, `LocalTime` and `Period`. In other words, the following query will be translated to SQL and evaluated server-side:

```c#
// Get all events which occurred on a Monday
var mondayEvents = context.Events.Where(p => p.SomeDate.DayOfWeek == DayOfWeek.Monday);

// Get all events which occurred before the year 2000
var oldEvents = context.Events.Where(p => p.SomeDate.Year < 2000);
```

Note that the plugin is far from covering all translations. If a translation you need is missing, please open an issue to request for it.
