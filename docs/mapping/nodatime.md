# Date/Time Mapping with NodaTime

## What is NodaTime

By default, [the PostgreSQL date/time types](https://www.postgresql.org/docs/current/static/datatype-datetime.html) are mapped to the built-in .NET types (`DateTime`, `TimeSpan`). Unfortunately, these built-in types are flawed in many ways. The [NodaTime library](http://nodatime.org/) was created to solve many of these problems, and if your application handles dates and times in anything but the most basic way, you should consider using it. To learn more [read this blog post by Jon Skeet](http://blog.nodatime.org/2011/08/what-wrong-with-datetime-anyway.html).

Beyond NodaTime's general advantages, some specific advantages NodaTime for PostgreSQL date/time mapping include:

* NodaTime defines some types which are missing from the BCL, such as `LocalDate`, `LocalTime`, and `OffsetTime`. These cleanly correspond to PostgreSQL `date`, `time` and `timetz`.
* `Period` is much more suitable for mapping PostgreSQL `interval` than `TimeSpan`.
* NodaTime types can fully represent PostgreSQL's microsecond precision, and can represent dates outside the BCL's date limit (1AD-9999AD).

## Setup

To set up the NodaTime plugin, add the [Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime nuget](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime) to your project. Then, configure the NodaTime plugin as follows:

### [EF 9.0, with a connection string](#tab/ef9-with-connection-string)

If you're passing a connection string to `UseNpgsql`, simply add the `UseNodaTime` call as follows:

```csharp
builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(
    "<connection string>",
    o => o.UseNodaTime()));
```

This configures all aspects of Npgsql to use the NodaTime plugin - both at the EF and the lower-level Npgsql layer.

### [With an external NpgsqlDataSource](#tab/with-datasource)

If you're creating an external NpgsqlDataSource and passing it to `UseNpgsql`, you must call `UseNodaTime` on your NpgsqlDataSourceBuilder independently of the EF-level setup:

```csharp
var dataSourceBuilder = new NpgsqlDataSourceBuilder("<connection string>");
dataSourceBuilder.UseNodaTime();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(
    dataSource,
    o => o.UseNodaTime()));
```

### [Older EF versions, with a connection string](#tab/legacy-with-connection-string)

```csharp
// Configure UseNodaTime at the ADO.NET level.
// This code must be placed at the beginning of your application, before any other Npgsql API is called; an appropriate place for this is in the static constructor on your DbContext class:
static MyDbContext()
    => NpgsqlConnection.GlobalTypeMapper.UseNodaTime();

// Then, when configuring EF Core with UseNpgsql(), call UseNodaTime():
builder.Services.AddDbContext<MyContext>(options =>
    options.UseNpgsql("<connection string>", o => o.UseNodaTime()));
```

***

The above sets up all the necessary mappings and operation translators. You can now use NodaTime types as regular properties in your entities, and even perform some operations:

```csharp
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

```csharp
// Get all events which occurred on a Monday
var mondayEvents = context.Events.Where(p => p.SomeDate.DayOfWeek == DayOfWeek.Monday);

// Get all events which occurred before the year 2000
var oldEvents = context.Events.Where(p => p.SomeDate.Year < 2000);
```

Following is the list of supported NodaTime translations; If an operation you need is missing, please open an issue to request for it.

.NET                                                                                   | SQL                                                                                                                                        | Notes
-------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ | -----------
SystemClock.Instance.GetCurrentInstant()                                               | [now()](https://www.postgresql.org/docs/current/functions-datetime.html)                                                                   |
LocalDateTime.Date                                                                     | [date_trunc('day', timestamp)](https://www.postgresql.org/docs/current/functions-datetime.html)                                            |
LocalDateTime.Second (also LocalTime, ZonedDateTime)                                   | [date_part('second', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                     |
LocalDateTime.Minute (also LocalTime, ZonedDateTime)                                   | [date_part('minute', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                     |
LocalDateTime.Hour (also LocalTime, ZonedDateTime)                                     | [date_part('hour', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                       |
LocalDateTime.Day, (also LocalDate, ZonedDateTime)                                     | [date_part('day', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                        |
LocalDateTime.Month (also LocalDate, ZonedDateTime)                                    | [date_part('month', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                      |
LocalDateTime.Year (also LocalDate, ZonedDateTime)                                     | [date_part('year', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                       |
LocalDateTime.DayOfWeek (also LocalDate, ZonedDateTime)                                | [floor(date_part('dow', timestamp))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                 |
LocalDateTime.DayOfYear (also LocalDate, ZonedDateTime)                                | [date_part('doy', timestamp)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                        |
LocalDate.AtMidnight()                                                                 | [date + time](https://www.postgresql.org/docs/current/functions-datetime.html)                                                             | Added in 10.0
LocalDate.At(time)                                                                     | [date + '00:00:00'](https://www.postgresql.org/docs/current/functions-datetime.html)                                                       | Added in 10.0
Period.Seconds (also Duration)                                                         | [date_part('second', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                      |
Period.Minutes (also Duration)                                                         | [date_part('minute', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                      |
Period.Hours (also Duration)                                                           | [date_part('hour', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                        |
Period.Days (also Duration)                                                            | [date_part('day', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                         |
Period.Months                                                                          | [date_part('month', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                       |
Period.Years                                                                           | [date_part('year', interval)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                        |
Period.FromSeconds                                                                     | [make_interval(seconds => int)](https://www.postgresql.org/docs/current/functions-datetime.html)                                           |
Period.FromMinutes                                                                     | [make_interval(minutes => int)](https://www.postgresql.org/docs/current/functions-datetime.html)                                           |
Period.FromHours                                                                       | [make_interval(hours => int)](https://www.postgresql.org/docs/current/functions-datetime.html)                                             |
Period.FromDays                                                                        | [make_interval(days => int)](https://www.postgresql.org/docs/current/functions-datetime.html)                                              |
Period.FromWeeks                                                                       | [make_interval(weeks => int)](https://www.postgresql.org/docs/current/functions-datetime.html)                                             |
Period.FromMonths                                                                      | [make_interval(months => int)](https://www.postgresql.org/docs/current/functions-datetime.html)                                            |
Period.FromYears                                                                       | [make_interval(years => int)](https://www.postgresql.org/docs/current/functions-datetime.html)                                             |
Duration.TotalMilliseconds                                                             | [date_part('epoch', interval) / 0.001](https://www.postgresql.org/docs/current/functions-datetime.html)                                    |
Duration.TotalSeconds                                                                  | [date_part('epoch', interval)](https://www.postgresql.org/docs/current/functions-datetime.html)                                            |
Duration.TotalMinutes                                                                  | [date_part('epoch', interval) / 60.0](https://www.postgresql.org/docs/current/functions-datetime.html)                                     |
Duration.TotalDays                                                                     | [date_part('epoch', interval) / 86400.0](https://www.postgresql.org/docs/current/functions-datetime.html)                                  |
Duration.TotalHours                                                                    | [date_part('epoch', interval) / 3600.0](https://www.postgresql.org/docs/current/functions-datetime.html)                                   |
ZonedDateTime.LocalDateTime                                                            | [timestamptz AT TIME ZONE 'UTC'](https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT)           |
DateInterval.Length                                                                    | [upper(daterange) - lower(daterange)](https://www.postgresql.org/docs/current/functions-range.html)                                        |
DateInterval.Start                                                                     | [lower(daterange)](https://www.postgresql.org/docs/current/functions-range.html)                                                           |
DateInterval.End                                                                       | [upper(daterange) - INTERVAL 'P1D'](https://www.postgresql.org/docs/current/functions-range.html)                                          |
DateInterval.Contains(LocalDate)                                                       | [daterange @> date](https://www.postgresql.org/docs/current/functions-range.html)                                                          |
DateInterval.Contains(DateInterval)                                                    | [daterange @> daterange](https://www.postgresql.org/docs/current/functions-range.html)                                                     |
DateInterval.Intersection(DateInterval)                                                | [daterange * daterange](https://www.postgresql.org/docs/current/functions-range.html)                                                      |
DateInterval.Union(DateInterval)                                                       | [daterange + daterange](https://www.postgresql.org/docs/current/functions-range.html)                                                      |
Instant.InZone(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).LocalDateTime              | [timestamptz AT TIME ZONE 'Europe/Berlin'](https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT) |
LocalDateTime.InZoneLeniently(DateTimeZoneProviders.Tzdb["Europe/Berlin"]).ToInstant() | [timestamp AT TIME ZONE 'Europe/Berlin'](https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT)   |
ZonedDateTime.ToInstant                                                                | No PG operation (.NET-side conversion from ZonedDateTime to Instant only)                                                                  |
Instant.InUtc                                                                          | No PG operation (.NET-side conversion from Instant to ZonedDateTime only)                                                                  |
Instant.ToDateTimeUtc                                                                  | No PG operation (.NET-side conversion from Instant to UTC DateTime only)                                                                   |
EF.Functions.Sum(periods)                                                              | [sum(periods)](https://www.postgresql.org/docs/current/functions-aggregate.html#FUNCTIONS-AGGREGATE-TABLE)                                 | See [Aggregate functions](translations.md#aggregate-functions).
EF.Functions.Sum(durations)                                                            | [sum(durations)](https://www.postgresql.org/docs/current/functions-aggregate.html#FUNCTIONS-AGGREGATE-TABLE)                               | See [Aggregate functions](translations.md#aggregate-functions).
EF.Functions.Average(periods)                                                          | [avg(durations)](https://www.postgresql.org/docs/current/functions-aggregate.html#FUNCTIONS-AGGREGATE-TABLE)                               | See [Aggregate functions](translations.md#aggregate-functions).
EF.Functions.Average(durations)                                                        | [avg(durations)](https://www.postgresql.org/docs/current/functions-aggregate.html#FUNCTIONS-AGGREGATE-TABLE)                               | See [Aggregate functions](translations.md#aggregate-functions).

In addition to the above, most arithmetic operators are also translated (e.g. LocalDate + Period).
