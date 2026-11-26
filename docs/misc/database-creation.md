# Database Creation

## Specifying the administrative db

When the Npgsql EF Core provider creates or deletes a database (`EnsureCreated()`, `EnsureDeleted()`), it must connect to an administrative database which already exists (with PostgreSQL you always have to be connected to some database, even when creating/deleting another database). Up to now the `postgres` database was used, which is supposed to always be present.

However, there are some PostgreSQL-like databases where the `postgres` database is not available. For these cases you can specify the administrative database as follows:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseNpgsql(
        "<connection_string>",
        options => options.UseAdminDatabase("my_admin_db"));
```

## Using a database template

When creating a new database,
[PostgreSQL allows specifying another "template database"](http://www.postgresql.org/docs/current/static/manage-ag-templatedbs.html)
which will be copied as the basis for the new one. This can be useful for including database entities which are not managed by Entity Framework Core. You can trigger this by using `HasDatabaseTemplate` in your context's `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.UseDatabaseTemplate("my_template_db");
```

## Setting a tablespace

PostgreSQL allows you to locate your database in different parts of your filesystem, [via tablespaces](https://www.postgresql.org/docs/current/static/manage-ag-tablespaces.html). The Npgsql EF Core provider allows you to specify your database's namespace:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.UseTablespace("my_tablespace");
```

You must have created your tablespace prior to this via the `CREATE TABLESPACE` command - the Npgsql EF Core provider does not do this for you. Note also that specifying a tablespace on specific tables is not supported.
