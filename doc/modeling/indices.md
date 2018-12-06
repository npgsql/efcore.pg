# Indices

## PostgreSQL covering indices (INCLUDE)

Since version 11, PostgreSQL supports [covering indexes](https://paquier.xyz/postgresql-2/postgres-11-covering-indexes), which allow you to include "non-key" columns in your indices. This allows you to perform index-only scans and can provide a significant performance boost:

```c#
protected override void OnConfiguring(DbContextOptionsBuilder builder)
{
    modelBuilder.Entity<Blog>()
        .ForNpgsqlHasIndex(b => b.Id)
        .ForNpgsqlInclude(b => b.Name);
}
```

This will create an index for searching on `Id`, but containing also the column `Name`, so that reading the latter will not involve accessing the table. The SQL generated is as follows:

## PostgreSQL index methods

PostgreSQL supports a number of _index methods_, or _types_. These are specified at index creation time via the `USING _method_` clause, see the [PostgreSQL docs for `CREATE INDEX`](https://www.postgresql.org/docs/current/static/sql-createindex.html) and [this page](https://www.postgresql.org/docs/current/static/indexes-types.html) for information on the different types.

The Npgsql EF Core provider allows you to specify the index method to be used by specifying `ForNpgsqlHasMethod()` on your index in your context's `OnModelCreating` method:
```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    => modelBuilder.Entity<Blog>()
                   .HasIndex(b => b.Url)
                   .ForNpgsqlHasMethod("gin");
}
```

## PostgreSQL index operator classes

PostgreSQL allows you to specify [operator classes on your indices](https://www.postgresql.org/docs/current/indexes-opclass.html), to allow tweaking how the index should work. Use the following code to specify an operator class:

```c#
modelBuilder.Entity<Blog>()
    .ForNpgsqlHasIndex(b => new { b.Id, b.Name })
    .ForNpgsqlHasOperators(null, "text_pattern_ops");
```

Note that each operator class is used for the corresponding index column, by order. In the example above, the `text_pattern_ops` class will be used for the `Name` column, whereas the `Id` column will have the default class (unspecified)
