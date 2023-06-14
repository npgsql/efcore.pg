# Enum Type Mapping

By default, any enum properties in your model will be mapped to database integers. EF Core 2.1 also allows you to map these to strings in the database with value converters.

However, the Npgsql provider also allows you to map your CLR enums to [database enum types](https://www.postgresql.org/docs/current/static/datatype-enum.html). This option, unique to PostgreSQL, provides the best of both worlds: the enum is internally stored in the database as a number (minimal storage), but is handled like a string (more usable, no need to remember numeric values) and has type safety.

## Creating your database enum

First, you must specify the PostgreSQL enum type on your model, just like you would with tables, sequences or other databases objects:

```c#
protected override void OnModelCreating(ModelBuilder builder)
    => builder.HasPostgresEnum<Mood>();
```

This causes the EF Core provider to create your enum type, `mood`, with two labels: `happy` and `sad`. This will cause the appropriate migration to be created.

If you are using `context.Database.Migrate()` to create your enums, you need to instruct Npgsql to reload all types after applying your migrations:

```c#
context.Database.Migrate();

using (var conn = (NpgsqlConnection)context.Database.GetDbConnection())
{
    conn.Open();
    conn.ReloadTypes();
}
```

## Mapping your enum

Even if your database enum is created, Npgsql has to know about it, and especially about your CLR enum type that should be mapped to it:

### [NpgsqlDataSource](#tab/with-datasource)

Since version 7.0, NpgsqlDataSource is the recommended way to use Npgsql. When using NpgsqlDataSource, map your enum when building your data source:

```c#
// Call UseNodaTime() when building your data source:
var dataSourceBuilder = new NpgsqlDataSourceBuilder(/* connection string */);
dataSourceBuilder.MapEnum<Mood>();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(dataSource));
```

### [Without NpgsqlDatasource](#tab/without-datasource)

Since version 7.0, NpgsqlDataSource is the recommended way to use Npgsql. However, if you're not yet using NpgsqlDataSource, map enums by adding the following code, *before* any EF Core operations take place. An appropriate place for this is in the static constructor on your DbContext class:

```c#
static MyDbContext()
    => NpgsqlConnection.GlobalTypeMapper.MapEnum<Mood>();
```

***

This code lets Npgsql know that your CLR enum type, `Mood`, should be mapped to a database enum called `mood`. Note that if your enum is in a custom schema (not `public`), you must specify that schema in the call to `MapEnum`.

If you're curious as to inner workings, this code maps the enum with the ADO.NET provider - [see here for the full docs](http://www.npgsql.org/doc/types/enums_and_composites.html). When the Npgsql EF Core first initializes, it calls into the ADO.NET provider to get all mapped enums, and sets everything up internally at the EF Core layer as well.

> [!NOTE]
> If you have multiple context types, all `MapEnum` invocations must be done before *any* of them is used; this means that the code cannot be in your static constructors, but must be moved to the program start.

## Using enum properties

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

## Altering enum definitions

The Npgsql provider only allow adding new values to existing enums, and the appropriate migrations will be automatically created as you add values to your CLR enum type. However, PostgreSQL itself doesn't support removing enum values (since these may be in use), and while renaming values is supported, it isn't automatically done by the provider to avoid using unreliable detection heuristics. Renaming an enum value can be done by including [raw SQL](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli#arbitrary-changes-via-raw-sql) in your migrations as follows:

```c#
migrationBuilder.Sql(@"ALTER TYPE mood RENAME VALUE 'happy' TO 'thrilled';");
```
  
As always, test your migrations carefully before running them on production databases.

## Scaffolding from an existing database

If you're creating your model from an existing database, the provider will recognize enums in your database, and scaffold the appropriate `HasPostgresEnum()` lines in your model. However, the scaffolding process has no knowledge of your CLR type, and will therefore skip your enum columns (warnings will be logged). You will have to create the CLR type, add the global mapping and add the properties to your entities.

In the future it may be possible to scaffold the actual enum type (and with it the properties), but this doesn't happen at the moment.
