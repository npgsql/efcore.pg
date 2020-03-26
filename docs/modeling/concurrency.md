# Concurrency Tokens

> [!NOTE]
> Please read the general [Entity Framework Core docs on concurrency tokens](https://docs.microsoft.com/en-us/ef/core/modeling/concurrency).

Entity Framework Core supports the concept of optimistic concurrency - a property on your entity is designated as a concurrency token, and EF Core detects concurrent modifications by checking whether that token has changed since the entity was read.

## The PostgreSQL xmin system column

Although applications can update concurrency tokens themselves, we frequently rely on the database automatically updating a column on update - a "last modified" timestamp, an SQL Server `rowversion`, etc. Unfortunately PostgreSQL doesn't have such auto-updating columns - but there is one feature that can be used for concurrency token. All PostgreSQL tables have a set of [implicit and hidden system columns](https://www.postgresql.org/docs/current/static/ddl-system-columns.htm://www.postgresql.org/docs/current/static/ddl-system-columns.html), among which `xmin` holds the ID of the latest updating transaction. Since this value automatically gets updated every time the row is changed, it is ideal for use as a concurrency token.

To enable this feature on an entity, insert the following code into your context's `OnModelCreating` method:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<Blog>()
                   .UseXminAsConcurrencyToken();
```

Note that by default, this will set up a [shadow property](https://docs.microsoft.com/en-us/ef/core/modeling/shadow-properties) called `xmin` on your entity. This means that when you load an entity from the database, the value of `xmin` will be stored in the context, making it impossible to use that instance with another context (without reloading the entity). If you plan to pass instances between different contexts, consider adding an `xmin` property of type `uint` on your entity; this will automatically cause the concurrency token to be stored on the instance instead:

```c#
class Blog
{
	...
	public uint xmin { get; set; }
}