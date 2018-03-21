# Mapping Enums

By default, any enum properties in your model will be mapped to database integers. EF Core 2.1 also allows you to map these to strings in the database with value converters.

However, the Npgsql provider also allows you to map your CLR enums to [database  enum types](https://www.postgresql.org/docs/current/static/datatype-enum.html). This option, unique to PostgreSQL, provides the best of both worlds: the enum is stored in the database as a number, but is handled like a string and has type safety.

## Creating your Database Enum

First, you must specify the PostgreSQL enum type on your model, just like you would with tables, sequences or other databases objects:

```c#
protected override void OnModelCreating(ModelBuilder builder) {
    builder.ForNpgsqlHasEnum("Mood", new[] { "happy", "sad" });
}
```

This causes the EF Core provider to create your data enum type, `Mood`, with two labels: `happy` and `sad`. This will cause the appropriate migration to be created.

## Mapping your Enum

Even if your database enum is created, Npgsql has to know about it, and especially about your CLR enum type that should be mapped to it. This is done by adding the following code, *before* any EF Core operations take place. An appropriate place for this is in the static constructor on your DbContext class:

```c#
static MyDbContext()
{
    NpgsqlConnection.GlobalTypeMapper.MapEnum<Mood>("Mood");
}
```

This code lets Npgsql know that your CLR enum type, `Mood`, should be mapped to a database enum called `Mood`.

If you're curious as to inner workings, this code maps the enum with the ADO.NET provider - [see here for the full docs](http://www.npgsql.org/doc/types/enums_and_composites.html). When the Npgsql EF Core first initializes, it calls into the ADO.NET provider to get all mapped enums, and sets everything up internally at the EF Core layer as well.

## Using Enum Properties

Once your enum is mapped and created in the database, you can use your CLR enum type just like any other property:

```c#
public class Blog
{
    public int Id { get; set; }
    public Mood Mood { get; set; }
}

using (var ctx = new MyDbContext())
{
    // Insert
    ctx.Blogs.Add(new Blog { Mood = Mood.Happy });
    ctx.Blogs.SaveChanges();

    // Query
    var blog = ctx.Blogs.Single(b => b.Mood == Mood.Happy);
}
```

## Changing Enums

Although PostgreSQL allows [altering enum types](https://www.postgresql.org/docs/current/static/sql-altertype.html), the Npgsql provider currently does not generate SQL for those operations (beyond creating and dropping the entire type). If you to add, remove or rename enum values, you'll have to include raw SQL in your migrations (this is quite easy to do). As always, test your migrations carefully before running them on production databases.

## Scaffolding from an Existing Database

If you're creating your model from an existing database, the provider will recognize enums in your database, and scaffold the appropriate `ForNpgsqlHasEnum()` lines in your model. However, since the scaffolding process has no knowledge of your CLR type, and will therefore skip your enum columns (warnings will be logged). You will have to create the CLR type, add the global mapping and add the properties to your entities.

In the future it may be possible to scaffold the actual enum type (and with it the properties), but this doesn't happen at the moment.

