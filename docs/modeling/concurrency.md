# Concurrency Tokens

> [!NOTE]
> Please read the general [Entity Framework Core docs on concurrency tokens](https://docs.microsoft.com/en-us/ef/core/modeling/concurrency).

Entity Framework Core supports the concept of optimistic concurrency - a property on your entity is designated as a concurrency token, and EF Core detects concurrent modifications by checking whether that token has changed since the entity was read.

## The PostgreSQL xmin system column

Although applications can update concurrency tokens themselves, we frequently rely on the database automatically updating a column on update - a "last modified" timestamp, an SQL Server `rowversion`, etc. Unfortunately PostgreSQL doesn't have such auto-updating columns - but there is one feature that can be used for concurrency token. All PostgreSQL tables have a set of [implicit and hidden system columns](https://www.postgresql.org/docs/current/ddl-system-columns.html), among which `xmin` holds the ID of the latest updating transaction. Since this value automatically gets updated every time the row is changed, it is ideal for use as a concurrency token.

Starting with version 7.0, you can map a `uint` property to the PostgreSQL `xmin` system column using the standard EF Core mechanisms:

### [Data Annotations](#tab/data-annotations)

```csharp
public class SomeEntity
{
    public int Id { get; set; }

    [Timestamp]
    public uint Version { get; set; }
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
            .Property(b => b.Version)
            .IsRowVersion();
    }
}

public class SomeEntity
{
    public int Id { get; set; }
    public uint Version { get; set; }
}
```

***

In older version of the provider, use the following instead:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>().UseXminAsConcurrencyToken();
}
```
