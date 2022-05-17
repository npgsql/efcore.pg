# JSON Mapping

PostgreSQL has rich, built-in support for storing JSON columns and efficiently performing complex queries operations on them. Newcomers can read more about the PostgreSQL support on [the JSON types page](https://www.postgresql.org/docs/current/datatype-json.html), and on the [functions and operators page](https://www.postgresql.org/docs/current/functions-json.html). Note that the below mapping mechanisms support both the `jsonb` and `json` types, although the former is almost always preferred for efficiency reasons.

The Npgsql EF Core provider allows you to map PostgreSQL JSON columns in three different ways:

1. As simple strings
2. As strongly-typed user-defined types (POCOs)
3. As [System.Text.Json](https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/) DOM types (JsonDocument or JsonElement)

> [!NOTE]
> Mapping to POCO or to System.Text.Json types was introduced in version 3.0.0

## String mapping

The simplest form of mapping to JSON is via a regular string property, just like an ordinary text column:

### [Data Annotations](#tab/data-annotations)

```c#
public class SomeEntity
{
    public int Id { get; set; }
    [Column(TypeName = "jsonb")]
    public string Customer { get; set; }
}
```

### [Fluent API](#tab/fluent-api)

```c#
class MyContext : DbContext
{
    public DbSet<SomeEntity> SomeEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SomeEntity>()
            .Property(b => b.Customer)
            .HasColumnType("jsonb");
    }
}

public class SomeEntity
{
    public int Id { get; set; }
    public string Customer { get; set; }
}
```

***

With string mapping, the EF Core provider will save and load properties to database JSON columns, but will not do any further serialization or parsing - it's the developer's responsibility to handle the JSON contents, possibly using System.Text.Json to parse them. This mapping approach is more limited compared to the others.

## POCO mapping

If your column's JSON documents have a stable schema, you can map them to your own .NET types (or POCOs). The provider will use [the new System.Text.Json APIs](https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/) under the hood to serialize instances to JSON documents before sending them to the database, and to deserialize documents coming from the database back. Just like EF Core can map a .NET type to rows in the table, this capability allows you to map a .NET type to a single JSON column.

Mapping POCOs is extremely easy: simply add a property with your custom POCO type and instruct the provider to map it to JSON:

### [Data Annotations](#tab/data-annotations)

```c#
public class SomeEntity
{
    public int Id { get; set; }
    [Column(TypeName = "jsonb")]
    public Customer Customer { get; set; }
}

public class Customer    // Mapped to a JSON column in the table
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Order[] Orders { get; set; }
}

public class Order       // Part of the JSON column
{
    public decimal Price { get; set; }
    public string ShippingAddress { get; set; }
}
```

### [Fluent API](#tab/fluent-api)

```c#
class MyContext : DbContext
{
    public DbSet<SomeEntity> SomeEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SomeEntity>()
            .Property(b => b.Customer)
            .HasColumnType("jsonb");
    }
}

public class SomeEntity  // Mapped to a database table
{
    public int Id { get; set; }
    public Customer Customer { get; set; }
}

public class Customer    // Mapped to a JSON column in the table
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Order[] Orders { get; set; }
}

public class Order       // Part of the JSON column
{
    [JsonPropertyName("OrderPrice")] // Controls the JSON property name
    public decimal Price { get; set; }
    public string ShippingAddress { get; set; }
}
```

***

You can now assign a regular `Customer` instance to the property, and once you call `SaveChanges()` it will be serialized to database, producing a  document such as the following:

```json
{
    "Age": 25,
    "Name": "Joe",
    "Orders": [
        { "OrderPrice": 9, "ShippingAddress": "Some address 1" },
        { "OrderPrice": 23, "ShippingAddress": "Some address 2" }
    ]
}
```

Reading is just as simple:

```c#
var someEntity = context.Entities.First();
Console.WriteLine(someEntity.Customer.Orders[0].Price)
```

This provides a seamless mapping approach, and supports embedding nested types and arrays, resulting in complex JSON document schemas as shown above. This approach also allows you to traverse loaded JSON documents in a type-safe way, using regular C# syntax, and to use LINQ to query inside database JSON documents (see [Querying JSON columns](#querying-json-columns) below).

## JsonDocument DOM mapping

If your column JSON schema isn't stable, a strongly-typed POCO mapping may not be appropriate. The Npgsql provider also allows you to map the DOM document type provided by [System.Text.Json APIs](https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/).

```c#
public class SomeEntity : IDisposable
{
    public int Id { get; set; }
    public JsonDocument Customer { get; set; }

    public void Dispose() => Customer?.Dispose();
}
```

Note that neither a data annotation nor the fluent API are required, as [JsonDocument](https://docs.microsoft.com/dotnet/api/system.text.json.jsondocument) is automatically recognized and mapped to `jsonb`. Note also that `JsonDocument` is disposable, so the entity type is made disposable as well; not dispose the `JsonDocument` will result in the memory not being returned to the pool, which will increase GC impact across various parts of the framework.

Once a document is loaded from the database, you can traverse it:

```c#
var someEntity = context.Entities.First();
Console.WriteLine(someEntity.Customer.RootElement.GetProperty("Orders")[0].GetProperty("Price").GetInt32());
```

## Querying JSON columns

Saving and loading documents these documents wouldn't be much use without the ability to query them. You can express your queries via the same LINQ constructs you are already using in EF Core:

### [POCO Mapping](#tab/poco)

```c#
var joes = context.CustomerEntries
    .Where(e => e.Customer.Name == "Joe")
    .ToList();
```

### [JsonDocument Mapping](#tab/jsondocument)

```c#
var joes = context.CustomerEntries
    .Where(e => e.Customer.RootElement.GetProperty("Name").GetString() == "Joe")
    .ToList();
```

***

The provider will recognize the traversal of a JSON document, and translate it to the correspond PostgreSQL JSON traversal operator, producing the following PostgreSQL-specific SQL:

```sql
SELECT c.""Id"", c.""Customer""
FROM ""CustomerEntries"" AS c
WHERE c.""Customer""->>'Name' = 'Joe'
```

[If indexes are set up properly](#indexing-json-columns), this can result in very efficient, server evaluation of searches with database JSON documents.

The following expression types and functions are translated:

### [POCO Mapping](#tab/poco)

.NET                                                                                    | SQL
--------------------------------------------------------------------------------------- | ----
customer.Name                                                                           | [customer->>'Name'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-OP-TABLE)
customer.Orders[1].Price                                                                | [customer#>>'{Orders,0,Price}'[1]](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-OP-TABLE)
customer.Orders.Length                                                                  | [jsonb_array_length(customer->'Orders')](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-PROCESSING-TABLE)
EF.Functions.JsonContains(customer, @"{""Name"": ""Joe"", ""Age"": 25}")<sup>1</sup>    | [customer @> '{"Name": "Joe", "Age": 25}'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonContained(@"{""Name"": ""Joe"", ""Age"": 25}", e.Customer)<sup>1</sup> | ['{"Name": "Joe", "Age": 25}' <@ customer](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExists(e.Customer, "Age")                                              | [customer ? 'Age'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExistsAny(e.Customer, "Age", "Address")                                | [customer ?\| ARRAY['Age','Address']](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExistsAll(e.Customer, "Age", "Address")                                | [customer ?& ARRAY['Age','Address']](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonTypeof(e.Customer.Age)                                                 | [jsonb_typeof(customer->'Age')](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-PROCESSING-TABLE)

### [JsonDocument Mapping](#tab/jsondocument)

.NET                                                                                  | SQL
------------------------------------------------------------------------------------- | ----
customer.RootElement.GetProperty("Name").GetString()                                  | [customer->>'Name' = 'Joe'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-OP-TABLE)
customer.RootElement.GetProperty("Orders")[1].GetProperty("Price").GetInt32()         | [customer#>>'{Orders,0,Price}'[1] = 8](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-OP-TABLE)
customer.RootElement.GetProperty("Orders").GetArrayLength()                           | [jsonb_array_length(customer->'Orders'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-PROCESSING-TABLE)
EF.Functions.JsonContains(customer, @"{""Name"": ""Joe"", ""Age"": 25}")<sup>1</sup>  | [customer @> '{"Name": "Joe", "Age": 25}'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonContained(@"{""Name"": ""Joe"", ""Age"": 25}", customer)<sup>1</sup> | ['{"Name": "Joe", "Age": 25}' <@ customer](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExists(customer, "Age")                                              | [customer ? 'Age'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExistsAny(customer, "Age", "Address")                                | [customer ?\| ARRAY['Age','Address']](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExistsAll(customer, "Age", "Address")                                | [customer ?& ARRAY['Age','Address']](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonTypeof(customer.GetProperty("Age")) == "number"                      | [jsonb_typeof(customer->'Age') = 'number'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-PROCESSING-TABLE)

***

<sup>1</sup> JSON functions which accept a .NET object require a JSON document, and do not accept scalar values. For example, to pass a scalar to `JsonContains`, wrap it in a `JsonElement`.

## Indexing JSON columns

> [!NOTE]
> A section on indices will be added. In the meantime consult the PostgreSQL documentation and other guides on the Internet.

These are early days for EF Core JSON support, and you'll likely run into some limitations. Please let us know how the current features are working for you and what you'd like to see.
