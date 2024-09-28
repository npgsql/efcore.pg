# JSON Mapping

> [!NOTE]
> Version 8.0 of the Npgsql provider introduced support for EF's [JSON columns](https://learn.microsoft.com/ef/core/what-is-new/ef-core-7.0/whatsnew#json-columns), using `ToJson()`. That is the recommended way to map POCOs going forward.

PostgreSQL has rich, built-in support for storing JSON columns and efficiently performing complex queries operations on them. Newcomers can read more about the PostgreSQL support on [the JSON types page](https://www.postgresql.org/docs/current/datatype-json.html), and on the [functions and operators page](https://www.postgresql.org/docs/current/functions-json.html). Note that the below mapping mechanisms support both the `jsonb` and `json` types, although the former is almost always preferred for efficiency reasons.

The Npgsql EF Core provider allows you to map PostgreSQL JSON columns in three different ways:

1. As simple strings
2. As EF owned entities
3. As System.Text.Json DOM types (JsonDocument or JsonElement, [see docs](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/use-dom#use-jsondocument))
4. As strongly-typed user-defined types (POCOs) (deprecated)

## String mapping

The simplest form of mapping to JSON is via a regular string property, just like an ordinary text column:

### [Data Annotations](#tab/data-annotations)

```csharp
public class SomeEntity
{
    public int Id { get; set; }
    [Column(TypeName = "jsonb")]
    public string Customer { get; set; }
}
```

### [Fluent API](#tab/fluent-api)

```csharp
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

If your column JSON contains documents with a stable schema, you can map them to your own .NET types (or POCOs); EF will use System.Text.Json APIs under the hood to serialize instances of your types to JSON documents before sending them to the database, and to deserialize documents coming back from the database. This effectively allows mapping an arbitrary .NET type - or object graph - to a single column in the database.

EF 7.0 introduced the "JSON Columns" feature, which maps a database JSON column via EF's "owned entity" mapping concept, using `ToJson()`. In this approach, EF fully models the types within the JSON document - just like it models regular tables and columns - and uses that information to perform better queries and updates. Full support for ToJson has been added to version 8.0 of the Npgsql EF provider.

As an alternative, prior to version 8.0, the Npgsql EF provider has supported JSON POCO mapping by simply delegating serialization/deserialization to System.Text.Json; in this model, EF itself model the contents of the JSON document, and cannot take that structure into account for queries and updates. This approach can now be considered deprecated as it allows for less powerful mapping and supports less query types; using ToJson() is now the recommended way to map POCOs to JSON.

### ToJson (owned entity mapping)

Npgsql's support for `ToJson()` is fully aligned with the general EF support; see the [EF documentation for more information](https://learn.microsoft.com/ef/core/what-is-new/ef-core-7.0/whatsnew#json-columns).

To get you started quickly, assume that we have the following Customer type, with a Details property that we want to map to a single JSON column in the database:

```csharp
public class Customer
{
    public int Id { get; set; }
    public CustomerDetails Details { get; set; }
}

public class CustomerDetails    // Map to a JSON column in the table
{
    public string Name { get; set; }
    public int Age { get; set; }
    public List<Order> Orders { get; set; }
}

public class Order       // Part of the JSON column
{
    public decimal Price { get; set; }
    public string ShippingAddress { get; set; }
}
```

To instruct EF to map CustomerDetails - and within it, Order - to a JSON column, configure it as follows:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Customer>()
        .OwnsOne(c => c.Details, d =>
        {
            d.ToJson();
            d.OwnsMany(d => d.Orders);
        });
}
```

At this point you can interact with the Customer just like you would normally, and EF will seamlessly serialize and deserialize it to a JSON column in the database. You can also perform LINQ queries which reference properties inside the JSON document, and these will get translated to SQL.

## Traditional POCO mapping (deprecated)

Before version 8.0 introduced support for EF's ToJson (owned entity mapping), the provider had its own support for JSON POCO mapping, by simply delegating serialization/deserialization to System.Text.Json; in this model, EF itself model the contents of the JSON document, and cannot take that structure into account for queries and updates. This approach can now be considered deprecated as it allows for less powerful mapping and supports less query types; using ToJson() is now the recommended way to map POCOs to JSON.

To use traditional POCO mapping, configure a property a mapping to map to a `jsonb` column as follows:

### [Data Annotations](#tab/data-annotations)

```csharp
public class Customer
{
    public int Id { get; set; }
    [Column(TypeName = "jsonb")]
    public CustomerDetails Details { get; set; }
}

public class CustomerDetails    // Mapped to a JSON column in the table
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

```csharp
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

Note that when using this mapping, only limited forms of LINQ querying is supported; it's recommended to switch to ToJson() for full LINQ querying capabilities. The querying supported by traditional POCO mapping is documented [below](#querying-traditional-and-dom).

***

## JsonDocument DOM mapping

If your column JSON schema isn't stable, a strongly-typed POCO mapping may not be appropriate. The Npgsql provider also allows you to map the DOM document type provided by [System.Text.Json APIs](https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/).

```csharp
public class SomeEntity : IDisposable
{
    public int Id { get; set; }
    public JsonDocument Customer { get; set; }

    public void Dispose() => Customer?.Dispose();
}
```

Note that neither a data annotation nor the fluent API are required, as [JsonDocument](https://docs.microsoft.com/dotnet/api/system.text.json.jsondocument) is automatically recognized and mapped to `jsonb`. Note also that `JsonDocument` is disposable, so the entity type is made disposable as well; not dispose the `JsonDocument` will result in the memory not being returned to the pool, which will increase GC impact across various parts of the framework.

Once a document is loaded from the database, you can traverse it:

```csharp
var someEntity = context.Entities.First();
Console.WriteLine(someEntity.Customer.RootElement.GetProperty("Orders")[0].GetProperty("Price").GetInt32());
```

Note that when using this mapping, only limited forms of LINQ querying is supported; [see below](#querying-traditional-and-dom) for more details.

## <a name="querying-traditional-and-dom">Querying JSON columns (traditional JSON and DOM)

> [!NOTE]
> The below does not apply if you are using ToJson (owned entity mapping). ToJson supports

Saving and loading documents these documents wouldn't be much use without the ability to query them. You can express your queries via the same LINQ constructs you are already using in EF Core:

### [Classic POCO Mapping](#tab/poco)

```csharp
var joes = context.CustomerEntries
    .Where(e => e.Customer.Name == "Joe")
    .ToList();
```

### [JsonDocument Mapping](#tab/jsondocument)

```csharp
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
customer.Orders[1].Price                                                                | [customer#>>'{Orders,0,Price}'\[1\]](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-OP-TABLE)
customer.Orders.Length (or Count)                                                       | [jsonb_array_length(customer->'Orders')](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-PROCESSING-TABLE)
EF.Functions.JsonContains(customer, @"{""Name"": ""Joe"", ""Age"": 25}")<sup>1</sup>    | [customer @> '{"Name": "Joe", "Age": 25}'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonContained(@"{""Name"": ""Joe"", ""Age"": 25}", e.Customer)<sup>1</sup> | ['{"Name": "Joe", "Age": 25}' <@ customer](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExists(e.Customer, "Age")                                              | [customer ? 'Age'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExistsAny(e.Customer, "Age", "Address")                                | [customer ?\| ARRAY\['Age','Address'\]](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExistsAll(e.Customer, "Age", "Address")                                | [customer ?& ARRAY\['Age','Address'\]](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonTypeof(e.Customer.Age)                                                 | [jsonb_typeof(customer->'Age')](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-PROCESSING-TABLE)

### [JsonDocument Mapping](#tab/jsondocument)

.NET                                                                                  | SQL
------------------------------------------------------------------------------------- | ----
customer.RootElement.GetProperty("Name").GetString()                                  | [customer->>'Name' = 'Joe'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-OP-TABLE)
customer.RootElement.GetProperty("Orders")[1].GetProperty("Price").GetInt32()         | [CAST(customer #>> '{Orders,1,Price}' AS integer) = 8](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-OP-TABLE)
customer.RootElement.GetProperty("Orders").GetArrayLength()                           | [jsonb_array_length(customer->'Orders'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-PROCESSING-TABLE)
EF.Functions.JsonContains(customer, @"{""Name"": ""Joe"", ""Age"": 25}")<sup>1</sup>  | [customer @> '{"Name": "Joe", "Age": 25}'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonContained(@"{""Name"": ""Joe"", ""Age"": 25}", customer)<sup>1</sup> | ['{"Name": "Joe", "Age": 25}' <@ customer](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExists(customer, "Age")                                              | [customer ? 'Age'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExistsAny(customer, "Age", "Address")                                | [customer ?\| ARRAY\['Age','Address'\]](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonExistsAll(customer, "Age", "Address")                                | [customer ?& ARRAY\['Age','Address'\]](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSONB-OP-TABLE)
EF.Functions.JsonTypeof(customer.GetProperty("Age")) == "number"                      | [jsonb_typeof(customer->'Age') = 'number'](https://www.postgresql.org/docs/current/functions-json.html#FUNCTIONS-JSON-PROCESSING-TABLE)

***

<sup>1</sup> JSON functions which accept a .NET object will not accept .NET scalar values. For example, to pass a scalar to `JsonContains` wrap it in a `JsonElement` or alternatively wrap it in a string. Note: a root level JSON string value requires quotes and escaping `@"""Joe"""`, just as any nested JSON string value would.

## Indexing JSON columns

> [!NOTE]
> A section on indices will be added. In the meantime consult the PostgreSQL documentation and other guides on the Internet.
