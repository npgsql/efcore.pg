# Value Generation

See [the general EF Docs on value generation](https://docs.microsoft.com/en-us/ef/core/modeling/generated-properties) to better understand the concepts described here.

## Serial (Autoincrement) Columns

In PostgreSQL, the standard autoincrement column type is called `serial`. This isn't really a special type like in some other databases (e.g. SQL Server's IDENTITY), but rather a shorthand for specifying that the column's default value should come from a sequence. See [the PostgreSQL docs](https://www.postgresql.org/docs/current/static/datatype-numeric.html#DATATYPE-SERIAL) for more info.

When `ValueGeneratedOnAdd` is specified on a short, int or long property, the Npgsql EF Core provider will automatically map it to a `serial` column. Note that EF Core will automatically recognize key properties by convention (e.g. a property called `Id` in your entity) and will implicitly set them to `ValueGeneratedOnAdd`.

Note that there was a significant and breaking change in 1.1. If you have existing migrations generated with 1.0, please read the [migration notes](migration/1.1.md).

## HiLo Autoincrement Generation

One disadvantage of database-generated values is that these values must be read back from the database after a row is inserted. If you're saving multiple related entities, this means you must perform multiple roundtrips as the first entity's generated key must be read before writing the second one. One solution to this problem is HiLo value generation: rather than relying on the database to generate each and every value, the application "allocates" a range of values, which it can then populate directly on new entities without any additional roundtrips. When the range is exhausted, a new range is allocated. In practical terms, this uses a sequence that increments by some large value (100 by default), allowing the application to insert 100 rows autonomously.

To use HiLo, specify `ForNpgsqlUseSequenceHiLo` on a property in your model's `OnModelCreating`:

```c#
modelBuilder.Entity<Blog>().Property(b => b.Id).ForNpgsqlUseSequenceHiLo();
```

You can also make your model use HiLo everywhere:

```c#
modelBuilder.ForNpgsqlUseSequenceHiLo();
```

## Guid/UUID Generation

By default, if you specify `ValueGeneratedOnAdd` on a Guid property, a random Guid value will be generated client-side and sent to the database.

If you prefer to generate values in the database instead, you can do so by specifying `HasDefaultValueSql` on your property. Note that PostgreSQL doesn't include any Guid/UUID generation functions, you must add an extension such as `uuid-ossp` or `pgcrypto`. This can be done by placing the following code in your model's `OnModelCreating`:

```c#
modelBuilder.HasPostgresExtension("uuid-ossp");
modelBuilder
	.Entity<Blog>()
	.Property(e => e.SomeGuidProperty)
	.HasDefaultValueSql("uuid_generate_v4()");
```

See [the PostgreSQL docs on UUID for more details](https://www.postgresql.org/docs/current/static/datatype-uuid.html).

## Computed Columns (On Add or Update)

PostgreSQL does not support computed columns.

