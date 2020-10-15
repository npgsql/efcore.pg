# Other

## PostgreSQL extensions

The Npgsql EF Core provider allows you to specify PostgreSQL extensions that should be set up in your database.
Simply use `HasPostgresExtension` in your context's `OnModelCreating` method:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.HasPostgresExtension("hstore");
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

## Comments

PostgreSQL allows you to [attach comments](https://www.postgresql.org/docs/current/static/sql-syntax-lexical.html#SQL-SYNTAX-COMMENTS) to database objects, which can help explain their purpose for someone examining the schema. The Npgsql EF Core provider supports this for tables or columns, simply set the comment in your model's `OnModelCreating` as follows:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<MyEntity>()
                   .HasComment("Some comment");
```

## CockroachDB Interleave In Parent

If you're using CockroachDB, the Npgsql EF Core provider exposes its ["interleave in parent" feature](https://www.cockroachlabs.com/docs/stable/interleave-in-parent.html). Use the following code:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<Customer>()
                   .UseCockroachDbInterleaveInParent(
                        typeof(ParentEntityType),
                        new List<string> { "prefix_column_1", "prefix_column_2" });
```
