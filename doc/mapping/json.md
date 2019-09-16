# JSON Mapping

> [!NOTE]
> This capability was introduced in 3.0.0-preview8, and shouldn't be considered stable yet.

PostgreSQL has rich, built-in support for storing JSON columns and efficiently performing complex queries operations on them. Newcomers can read more about the PostgreSQL support on [the JSON types page](https://www.postgresql.org/docs/current/datatype-json.html), and on the [functions and operators page](https://www.postgresql.org/docs/current/functions-json.html).

## Mapping POCO types to JSON columns

The Npgsql provider allows you to seamlessly map your own complex .NET types to PostgreSQL JSON columns, and then use LINQ to query them efficiently.

```c#
// A regular EF entity object that will be mapped to a PostgreSQL table
public class CustomerEntry
{
    public int Id { get; set; }

    [Column(TypeName = "jsonb")]
    public Customer Customer { get; set; }    
}
```

The `[Column]` attribute tells EF Core to map `Customer` to a `jsonb` columnn, and not to another database table. `Customer` can be any regular .NET type, and can nest other types and arrays:

```
public class Customer
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Order[] Orders { get; set; }
}

public class Order
{
    public decimal Price { get; set; }
    public string ShippingAddress { get; set; }
}
```

You can now assign a `Customer` instance to the property, and once you call `SaveChanges()` it will be serialized to database using [the new `System.Text.Json` APIs](https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/). For example, your database column will contain a document similar to the following:

```json
{"Age": 25, "Name": "Joe", "Orders": [{"Price": 99.5, "ShippingAddress": "Some address 1"}, {"Price": 23, "ShippingAddress": "Some address 2"}]}
```

When you load your entities, the JSON documents will be seamlessly (and efficiently) materialized back into `Customer` and `Order` instances. 

Note that PostgreSQL supports two flavors of JSON types: the binary `jsonb` and the textual `json`. In almost all cases the `jsonb` type is preferred for efficiency reasons, more detail is available in [the JSON types page](https://www.postgresql.org/docs/current/datatype-json.html).

## Querying JSON columns

Saving and loading these documents wouldn't be much use without the ability to query them. You can express your queries via the same LINQ constructs you are already using in EF Core:

```c#
var joes = context.CustomerEntries
    .Where(e => e.Customer.Name == "Joe")
    .ToList();
```

This will produce the following PostgreSQL-specific SQL:

```sql
SELECT c.""Id"", c.""Customer""
FROM ""CustomerEntries"" AS c
WHERE c.""Customer""->>'Name' = 'Joe'
```

## Indexing JSON columns

> [!NOTE]
> A section on indices will be added. In the meantime consult the PostgreSQL documentation and other guides on the Internet.

These are early days for EF Core JSON support, and you'll likely run into some limitations. Please let us know how the current features are working for you and what you'd like to see.


