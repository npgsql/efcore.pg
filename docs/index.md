# Npgsql Entity Framework Core Provider

[![stable](https://img.shields.io/nuget/v/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=stable)](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/)
[![next patch](https://img.shields.io/myget/npgsql/v/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=next%20patch)](https://www.myget.org/feed/npgsql/package/nuget/Npgsql.EntityFrameworkCore.PostgreSQL)
[![vnext](https://img.shields.io/myget/npgsql-vnext/vpre/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=vnext)](https://www.myget.org/feed/npgsql-vnext/package/nuget/Npgsql.EntityFrameworkCore.PostgreSQL)
[![build](https://img.shields.io/github/actions/workflow/status/npgsql/efcore.pg/build.yml?branch=main)](https://github.com/npgsql/efcore.pg/actions)
[![gitter](https://img.shields.io/badge/gitter-join%20chat-brightgreen.svg)](https://gitter.im/npgsql/npgsql)

Npgsql has an Entity Framework (EF) Core provider. It behaves like other EF Core providers (e.g. SQL Server), so the [general EF Core docs](https://docs.microsoft.com/ef/core/index) apply here as well. If you're just getting started with EF Core, those docs are the best place to start.

Development happens in the [Npgsql.EntityFrameworkCore.PostgreSQL](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL) repository, all issues should be reported there.

## Configuring the project file

To use the Npgsql EF Core provider, add a dependency on `Npgsql.EntityFrameworkCore.PostgreSQL`. You can follow the instructions in the general [EF Core Getting Started docs](https://docs.microsoft.com/ef/core/get-started/).

Below is a `.csproj` file for a console application that uses the Npgsql EF Core provider:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.4" />
  </ItemGroup>
</Project>
```

## Defining a model and a `DbContext`

Let's say you want to store blogs and their posts in their database; you can model these as .NET types as follows:

```csharp
public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; }

    public List<Post> Posts { get; set; }
}

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public int BlogId { get; set; }
    public Blog Blog { get; set; }
}
```

You then define a `DbContext` type which you'll use to interact with the database:

### [OnConfiguring](#tab/onconfiguring)

Using `OnConfiguring()` to configure your context is the easiest way to get started, but is discouraged for most production applications:

```csharp
public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("<connection string>");
}

// At the point where you need to perform a database operation:
using var context = new BloggingContext();
// Use the context...
```

### [DbContext pooling](#tab/context-pooling)

```csharp
var dbContextFactory = new PooledDbContextFactory<BloggingContext>(
    new DbContextOptionsBuilder<BloggingContext>()
        .UseNpgsql("<connection string>")
        .Options);

// At the point where you need to perform a database operation:
using var context = dbContextFactory.CreateDbContext();
// Use the context...
```

### [ASP.NET / DI](#tab/aspnet)

When using ASP.NET - or any application with dependency injection - the context instance will be injected into your code. Use the following to configure EF with your DI container:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<BloggingContext>(opt => 
    opt.UseNpgsql(builder.Configuration.GetConnectionString("BloggingContext")));

public class BloggingContext(DbContextOptions<BloggingContext> options) : DbContext(options)
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
}
```

***

For more information on getting started with EF, consult the [EF getting started documentation](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli).

## Additional Npgsql configuration

The Npgsql EF provider is built on top of the lower-level Npgsql ADO.NET provider ([docs](https://www.npgsql.org/doc/index.html)); these two separate components support various options you may want to configure.

If you're using EF 9.0 or above, the `UseNpgsql()` is a single point where you can configure everything related to Npgsql. For example:

```csharp
builder.Services.AddDbContextPool<BloggingContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("BloggingContext"),
        o => o
            .SetPostgresVersion(13, 0)
            .UseNodaTime()
            .MapEnum<Mood>("mood")));
```

The above configures the EF provider to produce SQL for PostgreSQL version 13 (avoiding newer incompatible features), adds a plugin allowing use of NodaTime for date/time type mapping, and maps a .NET enum type. Note that the last two also require configuration at the lower-level ADO.NET layer, which the code above does for you automatically.

If you need to configure something at the lower-level ADO.NET layer, use `ConfigureDataSource()` as follows:

```csharp
builder.Services.AddDbContextPool<BloggingContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("BloggingContext"),
            o => o.ConfigureDataSource(dataSourceBuilder => dataSourceBuilder.UseClientCertificate(certificate))));
```

`ConfigureDataSource()` provides access to a lower-level [`NpgsqlDataSourceBuilder`](../Npgsql/basic-usage.md#data-source) which you can use to configure all aspects of the Npgsql ADO.NET provider.

> [!WARNING]
> The EF provider internally creates an NpgsqlDataSource and uses that; for most configuration (e.g. connection string), the provider knows to switch between NpgsqlDataSources automatically.
> However, it's not possible to detect configuration differences within the `ConfigureDataSource()`; as a result, avoid performing varying configuration inside `ConfigureDataSource()`, since you may
> get the wrong NpgsqlDataSource. If you find yourself needing to vary Npgsql ADO.NET configuration, create an external NpgsqlDataSource yourself with the desired configuration and pass that to
> `UseNpgsql()` as described below.

### Using an external NpgsqlDataSource

If you're using a version of EF prior to 9.0, the above configuration methods aren't available. You can still create an `NpgsqlDataSource` yourself, and then pass it EF's `UseNpgsql()`:

```csharp
var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("BloggingContext"));
dataSourceBuilder.MapEnum<Mood>();
dataSourceBuilder.UseNodaTime();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContextPool<BloggingContext>(opt => opt.UseNpgsql(dataSource));
```

## Using an Existing Database (Database-First)

The Npgsql EF Core provider also supports reverse-engineering a code model from an existing PostgreSQL database ("database-first"). To do so, use dotnet CLI to execute the following:

```bash
dotnet ef dbcontext scaffold "Host=my_host;Database=my_db;Username=my_user;Password=my_pw" Npgsql.EntityFrameworkCore.PostgreSQL
```
