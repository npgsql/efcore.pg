# Type mapping

The EF Core provider transparently maps the types supported by Npgsql at the ADO.NET level - see [the Npgsql ADO type mapping page](/doc/types/basic.html).

This means that you can use PostgreSQL-specific types, such as `inet` or `circle`, directly in your entities. Simply define your properties just as if they were a simple type, such as a `string`:

```csharp
public class MyEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IPAddress IPAddress { get; set; }
    public NpgsqlCircle Circle { get; set; }
    public int[] SomeInts { get; set; }
}
```

Special types such as [arrays](array.md) and [enums](enum.md) have their own documentation pages with more details.

[PostgreSQL composite types](https://www.postgresql.org/docs/current/static/rowtypes.html), while supported at the ADO.NET level, aren't yet supported in the EF Core provider. This is tracked by [#22](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/22).

## Explicitly specifying data types

In some cases, your .NET property type can be mapped to several PostgreSQL data types; a good example is a `string`, which will be mapped to `text` by default, but can also be mapped to `jsonb`. You can use either Data Annotation attributes or the Fluent API to configure the PostgreSQL data type:

## [Data Annotations](#tab/data-annotations)

```csharp
[Column(TypeName="jsonb")]
public string SomeStringProperty { get; set; }
```

## [Fluent API](#tab/fluent-api)

```csharp
builder.Entity<Blog>()
       .Property(b => b.SomeStringProperty)
       .HasColumnType("jsonb");
```

***
