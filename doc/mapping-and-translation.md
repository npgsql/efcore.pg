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

## Translating Regular Expressions

PostgreSQL supports [regular expression operations in the database](http://www.postgresql.org/docs/current/static/functions-matching.html#FUNCTIONS-POSIX-REGEXP), and the Npgsql EF Core provider provides some support for evaluating C# regex operations at the backend. All you have to do is use `Regex.IsMatch` in your where clause:

```c#
var customersStartingWithA = context.Customers.Where(c => Regex.IsMatch(c.CompanyName, "^A"));
```

Since this regular expression is evaluated at the server, the EF Core provider doesn't need to load all the customers from the database, saving lots of transfer bandwidth.
