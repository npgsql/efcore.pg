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

Although applications can update concurrency tokens themselves, we frequently rely on the database automatically updating a column on update - a "last modified" timestamp, an SQL Server `rowversion`, etc. Unfortunately PostgreSQL doesn't have such auto-updating columns - but there is one feature that can be used for concurrency token. All PostgreSQL tables have a set of [implicit and hidden system columns](https://www.postgresql.org/docs/current/static/ddl-system-columns.htm://www.postgresql.org/docs/current/static/ddl-system-columns.html), among which `xmin` holds the ID of the latest updating transaction. Since this value automatically gets updated every time the row is changed, it is ideal for use as a concurrency token.

To enable this feature on an entity, insert the following code into your model's `OnModelCreating` method:

```c#
modelBuilder.Entity<MyEntity>().ForNpgsqlUseXminAsConcurrencyToken();
```

## Execution Strategy

Since 2.0.0, the Npgsql provider provides a retrying execution strategy, which will attempt to detect most transient PostgreSQL/network errors and will automatically retry your operation. To enable, place the following code in your context's `OnModelConfiguring`:

```c#
.UseNpgsql(
    "<connection string>",
    options => options.EnableRetryOnFailure());
```

This strategy relies on NpgsqlException's `IsTransient` property. Both this property and the retrying strategy are new and should be considered somewhat experimental - please report any issues.

## Comments

PostgreSQL allows you to [attach comments](https://www.postgresql.org/docs/current/static/sql-syntax.html) to database objects, which can help explain their purpose for someone examining the schema. The EF Core provider supports this for tables or columns, simply set the comment in your model's `OnModelCreating` as follows:

```c#
modelBuilder.Entity<MyEntity>().ForNpgsqlHasComment("Some comment");
```

## Specifying the administrative db

When the Npgsql EF Core provider creates or deletes a database (EnsureCreated(), EnsureDeleted()), it must connect to an administrative database which already exists (with PostgreSQL you always have to be connected to some database, even when creating/deleting another database). Up to now the `postgres` database was used, which is supposed to always be present.

However, there are some PostgreSQL-like databases where the postgres database isn't available. For these cases you can specify the administrative database as follows:

```c#
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseNpgsql("<connection_string>",
        b => b.UseAdminDatabase("<admin_db>"));
}
```

## Using a database template

When creating a new database,
[PostgreSQL allows specifying another "template database"](http://www.postgresql.org/docs/current/static/manage-ag-templatedbs.html)
which will be copied as the basis for the new one. This can be useful for including database entities which aren't managed by Entity Framework. You can trigger this by using HasDatabaseTemplate in your context's `OnModelCreating`:

```c#
modelBuilder.HasDatabaseTemplate("my_template_db");
```

# CockroachDB Interleave In Parent

If you're using CockroachDB, the Npgsql provider exposes its ["interleave in parent" feature](https://www.cockroachlabs.com/docs/stable/interleave-in-parent.html). Use the following code:

```c#
modelBuilder.Entity<Customer>()
    .ForCockroachDbInterleaveInParent(typeof(ParentEntityType), new List<string> { "prefix_column_1", "prefix_column_2" });
```
