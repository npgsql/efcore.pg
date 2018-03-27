## Type Mapping

The EF Core provider can transparently map any type supported by Npgsql at the ADO.NET level. This means you can use PostgreSQL-specific types, such as `inet` or `circle`, directly in your entities - this wasn't possible in EF 6.x. Simply define your properties just as if they were a simple type, such as a string:

```c#
public class MyEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IPAddress IPAddress { get; set; }
    public NpgsqlCircle Circle { get; set; }
    public int[] SomeInts { get; set; }
}
```

Note that mapping array properties to [PostgreSQL arrays](https://www.postgresql.org/docs/current/static/arrays.html) is supported. However, operations such as indexing the array, searching for elements in it, etc. aren't yet translated to SQL and will be evaluated client-side. This will probably be fixed in 1.2.

[PostgreSQL composite types](https://www.postgresql.org/docs/current/static/rowtypes.html), while supported at the ADO.NET level, aren't yet supported in the EF Core provider. This is tracked by [#22](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/22).

## Explicitly Specifying Datatypes (e.g. JSON)

In some cases, your .NET property type can be mapped to several PostgreSQL datatypes; a good example is a string, which will be mapped to `text` by default, but can also be mapped to `jsonb`. You can explicitly specify the PostgreSQL datatype by adding the following to your model's `OnModelCreating`:

```c#
builder.Entity<Blog>()
    .Property(b => b.SomeStringProperty)
    .HasColumnType("jsonb");
```

Or, if you prefer annotations, use the `[Column]` attribute:

```c#
[Column(TypeName="jsonb")]
public string SomeStringProperty { get; set; }
```

## Operation Translation to SQL

Entity Framework Core allows providers to translate query expressions to SQL for database evaluation. For example, PostgreSQL supports [regular expression operations](http://www.postgresql.org/docs/current/static/functions-matching.html#FUNCTIONS-POSIX-REGEXP), and the Npgsql EF Core provider automatically translates .NET's `Regex.IsMatch()` to use this feature. Since evaluation happens at the server, table data doesn't need to be transferred to the client (saving bandwidth), and in some cases indices can be used to speed things up. The same C# code on other providers will trigger client evaluation.

Below are some Npgsql-specific translations, many additional standard ones are supported as well.

| This C# expression...                                    | ... gets translated to this SQL |
|----------------------------------------------------------|---------------------------------|
| .Where(c => Regex.IsMatch(c.Name, "^A+")                 | [WHERE "c"."Name" ~ '^A+'](http://www.postgresql.org/docs/current/static/functions-matching.html#FUNCTIONS-POSIX-REGEXP)
| .Where(c => c.SomeArray[1] = "foo")                      | [WHERE "c"."SomeArray"[1] = 'foo'](https://www.postgresql.org/docs/current/static/arrays.html#ARRAYS-ACCESSING)
| .Where(c => c.SomeArray.SequenceEqual(new[] { 1, 2, 3 }) | [WHERE "c"."SomeArray" = ARRAY[1, 2, 3])](https://www.postgresql.org/docs/current/static/arrays.html)
| .Where(c => c.SomeArray.Contains(3))                     | [WHERE 3 = ANY("c"."SomeArray")](https://www.postgresql.org/docs/current/static/functions-comparisons.html#AEN21104)
| .Where(c => c.SomeArray.Length == 3)                     | [WHERE array_length("c"."SomeArray, 1) == 3](https://www.postgresql.org/docs/current/static/functions-array.html#ARRAY-FUNCTIONS-TABLE)
| .Where(c => EF.Functions.Like(c.Name, "foo%")            | [WHERE "c"."Name" LIKE 'foo%'](https://www.postgresql.org/docs/current/static/functions-matching.html#FUNCTIONS-LIKE)
| .Where(c => EF.Functions.ILike(c.Name, "foo%")           | [WHERE "c"."Name" ILIKE 'foo%'](https://www.postgresql.org/docs/current/static/functions-matching.html#FUNCTIONS-LIKE) (case-insensitive LIKE)
| .Select(c => EF.Functions.ToTsVector("english", c.Name)) | [SELECT to_tsvector('english'::regconfig, "c"."Name")](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-DOCUMENTS)
| .Select(c => EF.Functions.ToTsQuery("english", "pgsql")) | [SELECT to_tsquery('english'::regconfig, 'pgsql')](https://www.postgresql.org/docs/current/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES)
| .Where(c => c.SearchVector.Matches("Npgsql"))            | [WHERE "c"."SearchVector" @@ 'Npgsql'](https://www.postgresql.org/docs/current/static/textsearch-intro.html#TEXTSEARCH-MATCHING)
| .Select(c => EF.Functions.ToTsQuery(c.SearchQuery).ToNegative()) | [SELECT (!! to_tsquery("c"."SearchQuery"))](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSQUERY)
| .Select(c => EF.Functions.ToTsVector(c.Name).SetWeight(NpgsqlTsVector.Lexeme.Weight.A)) | [SELECT setweight(to_tsvector("c"."Name"), 'A')](https://www.postgresql.org/docs/current/static/textsearch-features.html#TEXTSEARCH-MANIPULATE-TSVECTOR)