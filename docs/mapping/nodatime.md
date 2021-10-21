# Date/Time Mapping with NodaTime

## What is NodaTime

By default, [the PostgreSQL date/time types](https://www.postgresql.org/docs/current/static/datatype-datetime.html) are mapped to the built-in .NET types (`DateTime`, `TimeSpan`). Unfortunately, these built-in types are flawed in many ways. The [NodaTime library](http://nodatime.org/) was created to solve many of these problems, and if your application handles dates and times in anything but the most basic way, you should consider using it. To learn more [read this blog post by Jon Skeet](http://blog.nodatime.org/2011/08/what-wrong-with-datetime-anyway.html).

Beyond NodaTime's general advantages, some specific advantages NodaTime for PostgreSQL date/time mapping include:

* NodaTime defines some types which are missing from the BCL, such as `LocalDate`, `LocalTime`, and `OffsetTime`. These cleanly correspond to PostgreSQL `date`, `time` and `timetz`.
* `Period` is much more suitable for mapping PostgreSQL `interval` than `TimeSpan`.
* NodaTime types can fully represent PostgreSQL's microsecond precision, and can represent dates outside the BCL's date limit (1AD-9999AD).

## Setup

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

## Operation translation

The provider knows how to translate many members and methods on mapped NodaTime types. For example, the following query will be translated to SQL and evaluated server-side:

```c#
// Get all events which occurred on a Monday
var mondayEvents = context.Events.Where(p => p.SomeDate.DayOfWeek == DayOfWeek.Monday);

// Get all events which occurred before the year 2000
var oldEvents = context.Events.Where(p => p.SomeDate.Year < 2000);
```

Following is the list of supported NodaTime translations; If an operation you need is missing, please open an issue to request for it.

> [!NOTE]
> Most translations on ZonedDateTime and Period were added in version 6.0

.NET                                                    | SQL                                                                      | Notes
------------------------------------------------------- |------------------------------------------------------------------------- | ---
SystemClock.Instance.GetCurrentInstant()                | [now()](https://www.postgresql.org/docs/current/functions-datetime.html)
LocalDateTime.Date                                      | [date_trunc('day', timestamp)](https://www.postgresql.org/docs/current/functions-datetime.html)
LocalDateTime.Second (also LocalTime, ZonedDateTime)    | [date_part('second', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
LocalDateTime.Minute (also LocalTime, ZonedDateTime)    | [date_part('minute', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
LocalDateTime.Hour (also LocalTime, ZonedDateTime)      | [date_part('hour', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
LocalDateTime.Day, (also LocalDate, ZonedDateTime)      | [date_part('day', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
LocalDateTime.Month (also LocalDate, ZonedDateTime)     | [date_part('month', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
LocalDateTime.Year (also LocalDate, ZonedDateTime)      | [date_part('year', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
LocalDateTime.DayOfWeek (also LocalDate, ZonedDateTime) | [floor(date_part('dow', timestamp))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
LocalDateTime.DayOfYear (also LocalDate, ZonedDateTime) | [date_part('doy', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.Seconds (also Duration)                          | [date_part('second', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.Minutes (also Duration)                          | [date_part('minute', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.Hours (also Duration)                            | [date_part('hour', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.Days (also Duration)                             | [date_part('day', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.Months                                           | [date_part('month', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.Years                                            | [date_part('year', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.FromSeconds                                      | [make_interval(seconds => int)](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.FromMinutes                                      | [make_interval(minutes => int)](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.FromHours                                        | [make_interval(hours => int)](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.FromDays                                         | [make_interval(days => int)](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.FromWeeks                                        | [make_interval(weeks => int)](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.FromMonths                                       | [make_interval(months => int)](https://www.postgresql.org/docs/current/functions-datetime.html)
Period.FromYears                                        | [make_interval(years => int)](https://www.postgresql.org/docs/current/functions-datetime.html)
Duration.TotalMilliseconds                              | [date_part('epoch', interval) / 0.001](https://www.postgresql.org/docs/current/functions-datetime.html)
Duration.TotalSeconds                                   | [date_part('epoch', interval)](https://www.postgresql.org/docs/current/functions-datetime.html)
Duration.TotalMinutes                                   | [date_part('epoch', interval) / 60.0](https://www.postgresql.org/docs/current/functions-datetime.html)
Duration.TotalDays                                      | [date_part('epoch', interval) / 86400.0](https://www.postgresql.org/docs/current/functions-datetime.html)
Duration.TotalHours                                     | [date_part('epoch', interval) / 3600.0](https://www.postgresql.org/docs/current/functions-datetime.html)
ZonedDateTime.LocalDateTime                             | [timestamptz AT TIME ZONE 'UTC'](https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT) | Added in 6.0
DateInterval.Length                                     | [upper(daterange) - lower(daterange)](https://www.postgresql.org/docs/current/functions-range.html) | Added in 6.0
DateInterval.Start                                      | [lower(daterange)](https://www.postgresql.org/docs/current/functions-range.html)                    | Added in 6.0
DateInterval.End                                        | [upper(daterange) - INTERVAL 'P1D'](https://www.postgresql.org/docs/current/functions-range.html)   | Added in 6.0
DateInterval.Contains(LocalDate)                        | [daterange @> date](https://www.postgresql.org/docs/current/functions-range.html)                   | Added in 6.0
DateInterval.Contains(DateInterval)                     | [daterange @> daterange](https://www.postgresql.org/docs/current/functions-range.html)              | Added in 6.0
DateInterval.Intersection(DateInterval)                 | [daterange * daterange](https://www.postgresql.org/docs/current/functions-range.html)               | Added in 6.0
DateInterval.Union(DateInterval)                        | [daterange + daterange](https://www.postgresql.org/docs/current/functions-range.html)               | Added in 6.0
Instant.InUtc                                           | No PG operation (.NET-side conversion from Instant to ZonedDateTime only)                           | Added in 6.0
Instant.ToDateTimeUtc                                   | No PG operation (.NET-side conversion from Instant to UTC DateTime only)                            | Added in 6.0

In addition to the above, most arithmetic operators are also translated (e.g. LocalDate + Period).
