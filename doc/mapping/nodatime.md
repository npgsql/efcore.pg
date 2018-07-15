# Date/Time Mapping with NodaTime

# What is NodaTime?

By default, [the PostgreSQL date/time types](https://www.postgresql.org/docs/current/static/datatype-datetime.html) are mapped to the built-in .NET types (`DateTime`, `TimeSpan`). Unfortunately, these built-in types are flawed in many ways. The [NodaTime library](http://nodatime.org/) was created to solve many of these problems, and if your application handles dates and times in anything but the most basic way, you should consider using it. To learn more [read this blog post by Jon Skeet](http://blog.nodatime.org/2011/08/what-wrong-with-datetime-anyway.html).

Beyond NodaTime's general advantages, some specific advantages NodaTime for PostgreSQL date/time mapping include:

* NodaTime defines some types which are missing from the BCL, such as `LocalDate`, `LocalTime`, and `OffsetTime`. These cleanly correspond to PostgreSQL `date`, `time` and `timetz`.
* `Period` is much more suitable for mapping PostgreSQL `interval` than `TimeSpan`.
* NodaTime types can fully represent PostgreSQL's microsecond precision, and can represent dates outside the BCL's date limit (1AD-9999AD).

# Setup

To set up the NodaTime plugin, add the [Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime nuget](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime) to your project. Then, make the following modification to your `UseNpgsql()` line:

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

## Member translation

Currently, the EF Core provider knows how to translate the most date/time component members of NodaTime's `LocalDateTime`, `LocalDate`, `LocalTime` and `Period`. In other words, the following query will be translated to SQL and evaluated server-side:

```c#
// Get all events which occurred on a Monday
var mondayEvents = context.Events.Where(p => p.SomeDate.DayOfWeek == DayOfWeek.Monday);

// Get all events which occurred before the year 2000
var oldEvents = context.Events.Where(p => p.SomeDate.Year < 2000);
```

Note that the plugin is far from covering all translations. If a translation you need is missing, please open an issue to request for it.
