# Enum Type Mapping

By default, any enum properties in your model will be mapped to database integers. EF Core 2.1 also allows you to map these to strings in the database with value converters.

However, the Npgsql provider also allows you to map your CLR enums to [database enum types](https://www.postgresql.org/docs/current/static/datatype-enum.html). This option, unique to PostgreSQL, provides the best of both worlds: the enum is internally stored in the database as a number (minimal storage), but is handled like a string (more usable, no need to remember numeric values) and has type safety.

## Setting up your enum with EF

> [!NOTE]
> Enum mapping has changed considerably in EF 9.0.

If you're using EF 9.0 or above, simply call `MapEnum` inside your `UseNpgsql` invocation.

### [With a connection string](#tab/with-connection-string)

If you're passing a connection string to `UseNpgsql`, simply add the `MapEnum` call as follows:

```c#
builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(
    "<connection string>",
    o => o.MapEnum<Mood>("mood")));
```

This configures all aspects of Npgsql to use your `Mood` enum - both at the EF and the lower-level Npgsql layer - and ensures that the enum is created in the database in EF migrations.

### [With an external NpgsqlDataSource](#tab/with-external-datasource)

If you're creating an external NpgsqlDataSource and passing it to `UseNpgsql`, you must make sure to map your enum on that data independently of the EF-level setup:

```c#
var dataSourceBuilder = new NpgsqlDataSourceBuilder("<connection string>");
dataSourceBuilder.MapEnum<Mood>();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(
    dataSource,
    o => o.MapEnum<Mood>("mood")));
```

***

### Older EF versions

On versions of EF prior to 9.0, enum setup is more involved and consists of several steps; enum mapping has to be done at the lower-level Npgsql layer, and also requires explicit configuration in the EF model for creation in the database via migrations.

#### Creating your database enum

First, you must specify the PostgreSQL enum type on your model, just like you would with tables, sequences or other databases objects:

```c#
protected override void OnModelCreating(ModelBuilder builder)
    => builder.HasPostgresEnum<Mood>();
```

This causes the EF Core provider to create your enum type, `mood`, with two labels: `happy` and `sad`. This will cause the appropriate migration to be created.

#### Mapping your enum

Even if your database enum is created, Npgsql has to know about it, and especially about your CLR enum type that should be mapped to it:

##### [NpgsqlDataSource](#tab/with-datasource)

Since version 7.0, NpgsqlDataSource is the recommended way to use Npgsql. When using NpgsqlDataSource, map your enum when building your data source:

```c#
// Call MapEnum() when building your data source:
var dataSourceBuilder = new NpgsqlDataSourceBuilder(/* connection string */);
dataSourceBuilder.MapEnum<Mood>();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(dataSource));
```

##### [Without NpgsqlDatasource](#tab/without-datasource)

Since version 7.0, NpgsqlDataSource is the recommended way to use Npgsql. However, if you're not yet using NpgsqlDataSource, map enums by adding the following code, *before* any EF Core operations take place. An appropriate place for this is in the static constructor on your DbContext class:

```c#
static MyDbContext()
    => NpgsqlConnection.GlobalTypeMapper.MapEnum<Mood>();
```

> [!NOTE]
> If you have multiple context types, all `MapEnum` invocations must be done before *any* of them is used; this means that the code cannot be in your static constructors, but must be moved to the program start.

***

This code lets Npgsql know that your CLR enum type, `Mood`, should be mapped to a database enum called `mood`. Note that if your enum is in a custom schema (not `public`), you must specify that schema in the call to `MapEnum`.

## Using enum properties

Once your enum is properly set up with EF, you can use your CLR enum type just like any other property:

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
migrationBuilder.Sql("ALTER TYPE mood RENAME VALUE 'happy' TO 'thrilled';");
```
  
As always, test your migrations carefully before running them on production databases.

## Scaffolding from an existing database

If you're creating your model from an existing database, the provider will recognize enums in your database, and scaffold the appropriate `HasPostgresEnum()` lines in your model. However, the scaffolding process has no knowledge of your CLR type, and will therefore skip your enum columns (warnings will be logged). You will have to create the CLR type and perform the proper setup as described above.

In the future it may be possible to scaffold the actual enum type (and with it the properties), but this isn't supported at the moment.
