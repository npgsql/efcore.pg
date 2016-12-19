# Misc

## Setting up PostgreSQL extensions

The provider allows you to specify PostgreSQL extensions that should be set up in your database.
Simply use HasPostgresExtension in your context's OnModelCreating:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.HasPostgresExtension("hstore");
}
```

## Optimistic Concurrency and Concurrency Tokens

Entity Framework supports the concept of optimistic concurrency - a property on your entity is designated as a concurrency token, and EF detects concurrent modifications by checking whether that token has changed since the entity was read. You can read more about this in the [EF docs](https://docs.microsoft.com/en-us/ef/core/modeling/concurrency).

Although applications can update concurrency tokens themselves, we frequently rely on the database automatically updating a column on update - a "last modified" timestamp, an SQL Server `rowversion`, etc. Unfortunately PostgreSQL doesn't have such auto-updating columns - but there is one feature that can be used for concurrency token. All PostgreSQL have a set of [implicit and hidden system columns](https://www.postgresql.org/docs/current/static/ddl-system-columns.htm://www.postgresql.org/docs/current/static/ddl-system-columns.html), among which `xmin` holds the ID of the latest updating transaction. Since this value automatically gets updated every time the row is changed, it is ideal for use as a concurrency token.

To enable this feature on an entity, insert the following code into your model's `OnModelCreating` method:

```c#
modelBuilder.Entity<MyEntity>().ForNpgsqlUseXminAsConcurrencyToken();
```

## Comments

PostgreSQL allows you to [attach comments](https://www.postgresql.org/docs/current/static/sql-syntax.html) to database objects, which can help explain their purpose for someone examining the schema. The EF Core provider supports this for tables or columns, simply set the comment in your model's `OnModelCreating` as follows:

```c#
modelBuilder.Entity<MyEntity>().ForNpgsqlHasComment("Some comment");
```

## Using a database template

When creating a new database,
[PostgreSQL allows specifying another "template database"](http://www.postgresql.org/docs/current/static/manage-ag-templatedbs.html)
which will be copied as the basis for the new one. This can be useful for including database entities which aren't managed by Entity Framework. You can trigger this by using HasDatabaseTemplate in your context's `OnModelCreating`:

```c#
modelBuilder.HasDatabaseTemplate("my_template_db");
```
