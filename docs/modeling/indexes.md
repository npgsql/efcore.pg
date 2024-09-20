# Indexes

PostgreSQL and the Npgsql provider support the standard index modeling described in [the EF Core docs](https://docs.microsoft.com/ef/core/modeling/indexes). This page describes some supported PostgreSQL-specific features.

## Covering indexes (INCLUDE)

PostgreSQL supports [covering indexes](https://paquier.xyz/postgresql-2/postgres-11-covering-indexes), which allow you to include "non-key" columns in your indexes. This allows you to perform index-only scans and can provide a significant performance boost:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<Blog>()
        .HasIndex(b => b.Id)
        .IncludeProperties(b => b.Name);
```

This will create an index for searching on `Id`, but containing also the column `Name`, so that reading the latter will not involve accessing the table. The SQL generated is as follows:

```sql
CREATE INDEX "IX_Blog_Id" ON blogs ("Id") INCLUDE ("Name");
```

## Treating nulls as non-distinct

> [!NOTE]
> This feature was introduced in version 7.0, and is available starting with PostgreSQL 15.

By default, when you create a unique index, PostgreSQL treats null values as distinct; this means that a unique index can contain multiple null values in a column. When creating an index, you can also instruct PostgreSQL that nulls should be treated as *non-distinct*; this causes a unique constraint violation to be raised if a column contains multiple null values:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<Blog>()
        .IsUnique()
        .AreNullsDistinct(false);
```

## Index methods

PostgreSQL supports a number of *index methods*, or *types*. These are specified at index creation time via the `USING <method>` clause, see the [PostgreSQL docs for `CREATE INDEX`](https://www.postgresql.org/docs/current/static/sql-createindex.html) and [this page](https://www.postgresql.org/docs/current/static/indexes-types.html) for information on the different types.

The Npgsql EF Core provider allows you to specify the index method to be used by calling `HasMethod()` on your index in your context's `OnModelCreating` method:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<Blog>()
        .HasIndex(b => b.Url)
        .HasMethod("gin");
```

## Index operator classes

PostgreSQL allows you to specify [operator classes on your indexes](https://www.postgresql.org/docs/current/indexes-opclass.html), to allow tweaking how the index should work. Use the following code to specify an operator class:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder builder)
    => modelBuilder.Entity<Blog>()
        .HasIndex(b => new { b.Id, b.Name })
        .HasOperators(null, "text_pattern_ops");
```

Note that each operator class is used for the corresponding index column, by order. In the example above, the `text_pattern_ops` class will be used for the `Name` column, while the `Id` column will use the default class (unspecified), producing the following SQL:

```sql
CREATE INDEX "IX_blogs_Id_Name" ON blogs ("Id", "Name" text_pattern_ops);
```

## Storage parameters

PostgreSQL allows configuring indexes with *storage parameters*, which can tweak their behaviors in various ways; which storage parameters are available depends on the chosen index method. [See the PostgreSQL documentation](https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-STORAGE-PARAMETERS) for more information.

To configure a storage parameter on an index, use the following code:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<Blog>()
        .HasIndex(b => b.Url)
        .HasStorageParameter("fillfactor", 70);
```

## Creating indexes concurrently

Creating an index can interfere with regular operation of a database. Normally PostgreSQL locks the table to be indexed against writes and performs the entire index build with a single scan of the table. Other transactions can still read the table, but if they try to insert, update, or delete rows in the table they will block until the index build is finished. This could have a severe effect if the system is a live production database. Very large tables can take many hours to be indexed, and even for smaller tables, an index build can lock out writers for periods that are unacceptably long for a production system.

The EF provider allows you to specify that an index should be created *concurrently*, partially mitigating the above issues:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.Entity<Blog>()
        .HasIndex(b => b.Url)
        .IsCreatedConcurrently();
```

> [!CAUTION]
> Do not enable this feature before reading the [PostgreSQL documentation](https://www.postgresql.org/docs/current/sql-createindex.html#SQL-CREATEINDEX-CONCURRENTLY) and understanding the full implications of concurrent index creation.

> [!NOTE]
> Prior to version 5.0, `IsCreatedConcurrently` erroneously defaulted to `false` - explicitly pass `true` to configure the index for concurrent creation
