# Ranges and Multiranges

PostgreSQL has the unique feature of supporting [*range data types*](https://www.postgresql.org/docs/current/static/rangetypes.html). Ranges represent a range of numbers, dates or other data types, and allow you to easily query ranges which contain a value, perform set operations (e.g. query ranges which contain other ranges), and other similar operations. The range operations supported by PostgreSQL are listed [in this page](https://www.postgresql.org/docs/current/static/functions-range.html). The Npgsql EF Core provider allows you to seamlessly map PostgreSQL ranges, and even perform operations on them that get translated to SQL for server evaluation.

In addition, PostgreSQL 14 introduced *multiranges*, which are basically sorted arrays of non-overlapping ranges with set-theoretic operations defined over them. Most range operators also work on multiranges, and they have a few functions of their own.

## Ranges

Npgsql maps PostgreSQL ranges to the generic CLR type `NpgsqlRange<T>`:

```csharp
public class Event
{
    public int Id { get; set; }
    public string Name { get; set; }
    public NpgsqlRange<DateTime> Duration { get; set; }
}
```

This will create a column of type `daterange` in your database. You can similarly have properties of type `NpgsqlRange<int>`, `NpgsqlRange<long>`, etc.

## User-defined ranges

PostgreSQL comes with 6 built-in ranges: `int4range`, `int8range`, `numrange`, `tsrange`, `tstzrange`, `daterange`; these can be used simply by adding the appropriate `NpgsqlRange<T>` property in your entities as shown above. You can also define your own range types over arbitrary types, and use those in EF Core as well.

To make the EF Core type mapper aware of your user-defined range, call the `MapRange()` method in your context's `OnConfiguring()` method as follows:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseNpgsql(
        "<connection_string>",
        options => options.MapRange<float>("floatrange"));
```

This allows you to have properties of type `NpgsqlRange<float>`, which will be mapped to PostgreSQL `floatrange`.

The above does *not* create the `floatrange` type for you. In order to do that, include the following in your context's `OnModelCreating()`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.HasPostgresRange("floatrange", "real");
```

This will cause the appropriate [`CREATE TYPE ... AS RANGE`](https://www.postgresql.org/docs/current/static/sql-createtype.html) statement to be generated in your migrations, ensuring that your range is created and ready for use. Note that `HasPostgresRange()` supports additional parameters as supported by PostgreSQL `CREATE TYPE`.

## Multiranges

Npgsql maps arrays or lists of `NpgsqlRange<T>` to PostgreSQL multiranges:

```csharp
public class Store
{
    public int Id { get; set; }
    public string Name { get; set; }
    public NpgsqlRange<DateTime>[] OpeningTimes { get; set; }
}
```

## Operation translation

Ranges can be queried via extensions methods on `NpgsqlRange`:

```csharp
var events = context.Events.Where(p => p.Duration.Contains(someDate));
```

This will translate to an SQL operation using the PostgreSQL `@>` operator, evaluating at the server and saving you from transferring the entire `Events` table to the client. Note that you can (and probably should) create indexes to make this operation more efficient, see the PostgreSQL docs for more info.

The following table lists the range operations that currently get translated. Most operations work on both ranges and multiranges (starting with version 6.0.0); the multirange version is omitted for brevity.

If you run into a missing operation, please open an issue.

.NET                                  | SQL
--------------------------------------|-----
range.LowerBound                      | [lower(range)](https://www.postgresql.org/docs/current/functions-range.html#RANGE-FUNCTIONS-TABLE)
range.UpperBound                      | [upper(range)](https://www.postgresql.org/docs/current/functions-range.html#RANGE-FUNCTIONS-TABLE)
range.LowerBoundIsInclusive           | [lower_inc(range)](https://www.postgresql.org/docs/current/functions-range.html#RANGE-FUNCTIONS-TABLE)
range.UpperBoundIsInclusive           | [upper_inc(range)](https://www.postgresql.org/docs/current/functions-range.html#RANGE-FUNCTIONS-TABLE)
range.LowerBoundIsInfinite            | [lower_inf(range)](https://www.postgresql.org/docs/current/functions-range.html#RANGE-FUNCTIONS-TABLE)
range.UpperBoundIsInfinite            | [upper_inf(range)](https://www.postgresql.org/docs/current/functions-range.html#RANGE-FUNCTIONS-TABLE)
range.IsEmpty                         | [isempty(range)](https://www.postgresql.org/docs/current/functions-range.html#RANGE-FUNCTIONS-TABLE)
multirange.Any()                      | [NOT is_empty(multirange)](https://www.postgresql.org/docs/current/functions-range.html#MULTIRANGE-FUNCTIONS-TABLE)
range.Contains(i)                     | [range @> i](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.Contains(range2)               | [range @> range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.ContainedBy(range2)            | [range1 <@ range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.Overlaps(range2)               | [range1 && range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.IsStrictlyLeftOf(range2)       | [range1 << range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.IsStrictlyRightOf(range2)      | [range1 >> range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.DoesNotExtendLeftOf(range2)    | [range1 &> range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.DoesNotExtendRightOf(range2)   | [range1 <& range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.IsAdjacentTo(range2)           | [range1 -\|- range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.Union(range2)                  | [range1 + range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.Intersect(range2)              | [range1 * range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.Except(range2)                 | [range1 - range2](https://www.postgresql.org/docs/current/static/functions-range.html#RANGE-OPERATORS-TABLE)
range1.Merge(range2)                  | [range_merge(range1, range2)](https://www.postgresql.org/docs/current/functions-range.html#RANGE-FUNCTIONS-TABLE)
multirange.Merge()                    | [range_merge(multirange)](https://www.postgresql.org/docs/current/functions-range.html#MULTIRANGE-FUNCTIONS-TABLE)
ranges.RangeAgg()                     | [range_agg(ranges)](https://www.postgresql.org/docs/current/functions-aggregate.html)
ranges.RangeIntersectAgg()            | [range_intersect_agg(ranges)](https://www.postgresql.org/docs/current/functions-aggregate.html)
multiranges.RangeIntersectAgg()       | [range_intersect_agg(multiranges)](https://www.postgresql.org/docs/current/functions-aggregate.html)                | See [Aggregate functions](translations.md#aggregate-functions).
