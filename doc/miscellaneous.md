# Miscellaneous

## PostgreSQL extensions

The Npgsql EF Core provider allows you to specify PostgreSQL extensions that should be set up in your database.
Simply use `HasPostgresExtension` in your context's `OnModelCreating` method:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.HasPostgresExtension("hstore");
```

## Optimistic Concurrency and Concurrency Tokens

Entity Framework Core supports the concept of optimistic concurrency - a property on your entity is designated as a concurrency token, and EF Core detects concurrent modifications by checking whether that token has changed since the entity was read. You can read more about this in the [EF Core docs](https://docs.microsoft.com/ef/core/modeling/concurrency).

Although applications can update concurrency tokens themselves, we frequently rely on the database automatically updating a column on update - a "last modified" timestamp, an SQL Server `rowversion`, etc. Unfortunately PostgreSQL doesn't have such auto-updating columns - but there is one feature that can be used for concurrency token. All PostgreSQL tables have a set of [implicit and hidden system columns](https://www.postgresql.org/docs/current/static/ddl-system-columns.htm://www.postgresql.org/docs/current/static/ddl-system-columns.html), among which `xmin` holds the ID of the latest updating transaction. Since this value automatically gets updated every time the row is changed, it is ideal for use as a concurrency token.

To enable this feature on an entity, insert the following code into your context's `OnModelCreating` method:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<Blog>()
                   .ForNpgsqlUseXminAsConcurrencyToken();
```

## Execution Strategy

Since 2.0.0, the Npgsql EF Core provider provides a retrying execution strategy, which will attempt to detect most transient PostgreSQL/network errors and will automatically retry your operation. To enable, place the following code in your context's `OnModelConfiguring`:

```c#
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseNpgsql(
        "<connection_string>",
        options => options.EnableRetryOnFailure());
```

This strategy relies on the `IsTransient` property of `NpgsqlException`. Both this property and the retrying strategy are new and should be considered somewhat experimental - please report any issues.

## Comments

PostgreSQL allows you to [attach comments](https://www.postgresql.org/docs/current/static/sql-syntax-lexical.html#SQL-SYNTAX-COMMENTS) to database objects, which can help explain their purpose for someone examining the schema. The Npgsql EF Core provider supports this for tables or columns, simply set the comment in your model's `OnModelCreating` as follows:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<MyEntity>()
                   .ForNpgsqlHasComment("Some comment");
```

## Certificate authentication

The Npgsql allows you to provide a callback for verifying the server-provided certificates, and to provide a callback for providing certificates to the server. The latter, if properly set up on the PostgreSQL side, allows you to do client certificate authentication - see [the Npgsql docs](http://www.npgsql.org/doc/security.html#encryption-ssltls) and also [the PostgreSQL docs](https://www.postgresql.org/docs/current/static/ssl-tcp.html#SSL-CLIENT-CERTIFICATES) on setting this up.

The Npgsql EF Core provider allows you to set these two callbacks on the `DbContextOptionsBuilder` as follows:

```c#
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseNpgsql(
        "<connection_string>",
        options =>
        {
            options.RemoteCertificateValidationCallback(MyCallback1);
            options.ProvideClientCertificatesCallback(MyCallback2);
        });
```

You may also consider passing `Trust Server Certificate=true` in your connection string to make Npgsql accept whatever certificate your PostgreSQL provides (useful for self-signed certificates).

## Database Creation

### Specifying the administrative db

When the Npgsql EF Core provider creates or deletes a database (`EnsureCreated()`, `EnsureDeleted()`), it must connect to an administrative database which already exists (with PostgreSQL you always have to be connected to some database, even when creating/deleting another database). Up to now the `postgres` database was used, which is supposed to always be present.

However, there are some PostgreSQL-like databases where the `postgres` database is not available. For these cases you can specify the administrative database as follows:

```c#
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseNpgsql(
        "<connection_string>",
        options => options.UseAdminDatabase("my_admin_db"));
```

### Using a database template

When creating a new database,
[PostgreSQL allows specifying another "template database"](http://www.postgresql.org/docs/current/static/manage-ag-templatedbs.html)
which will be copied as the basis for the new one. This can be useful for including database entities which are not managed by Entity Framework Core. You can trigger this by using `HasDatabaseTemplate` in your context's `OnModelCreating`:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.HasDatabaseTemplate("my_template_db");
```

### Setting a tablespace

PostgreSQL allows you to locate your database in different parts of your filesystem, [via tablespaces](https://www.postgresql.org/docs/current/static/manage-ag-tablespaces.html). The Npgsql EF Core provider allows you to specify your database's namespace:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.ForNpgsqlUseTablespace("my_tablespace");
```

You must have created your tablespace prior to this via the `CREATE TABLESPACE` command - the Npgsql EF Core provider does not do this for you. Note also that specifying a tablespace on specific tables is not supported.

## CockroachDB Interleave In Parent

If you're using CockroachDB, the Npgsql EF Core provider exposes its ["interleave in parent" feature](https://www.cockroachlabs.com/docs/stable/interleave-in-parent.html). Use the following code:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<Customer>()
                   .ForCockroachDbInterleaveInParent(
                        typeof(ParentEntityType),
                        new List<string> { "prefix_column_1", "prefix_column_2" });
```


