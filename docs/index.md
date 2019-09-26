[![stable](https://img.shields.io/nuget/v/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=stable)](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/)
[![unstable](https://img.shields.io/myget/npgsql-unstable/vpre/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=unstable)](https://www.myget.org/feed/npgsql-unstable/package/nuget/Npgsql.EntityFrameworkCore.PostgreSQL)
[![next patch](https://img.shields.io/myget/npgsql/v/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=next%20patch)](https://www.myget.org/feed/npgsql/package/nuget/Npgsql.EntityFrameworkCore.PostgreSQL)
[![appveyor](https://img.shields.io/appveyor/ci/roji/npgsql-entityframeworkcore-postgresql/dev.svg?label=appveyor)](https://ci.appveyor.com/project/roji/npgsql-entityframeworkcore-postgresql)
[![travis](https://img.shields.io/travis/npgsql/npgsql.svg?label=travis)](https://travis-ci.org/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL)
[![gitter](https://img.shields.io/badge/gitter-join%20chat-brightgreen.svg)](https://gitter.im/npgsql/npgsql)

# Getting Started

Npgsql has an Entity Framework (EF) Core provider. It behaves like other EF Core providers (e.g. SQL Server), so the [general EF Core docs](https://docs.microsoft.com/ef/core/index) apply here as well. If you're just getting started with EF Core, those docs are the best place to start.

Development happens in the [Npgsql.EntityFrameworkCore.PostgreSQL](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL) repository, all issues should be reported there.

## Configuring the project file

To use the Npgsql EF Core provider, add a dependency on `Npgsql.EntityFrameworkCore.PostgreSQL`. You can follow the instructions in the general [EF Core Getting Started docs](https://docs.microsoft.com/ef/core/get-started/).

Below is a `.csproj` file for a console application that uses the Npgsql EF Core provider:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netcoreapp2.2</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="2.2.0" />
  </ItemGroup>
</Project>
```

## Defining a `DbContext`

```c#
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp.PostgreSQL
{
    public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=my_host;Database=my_db;Username=my_user;Password=my_pw");
    }

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
}
```

## Additional configuration for ASP.NET Core applications

Modify the `ConfigureServices` method in `Startup.cs`:

```c#
public IServiceProvider ConfigureServices(IServiceCollection services)
    => services.AddEntityFrameworkNpgsql()
               .AddDbContext<BloggingContext>()
               .BuildServiceProvider();
```
## Using an Existing Database (Database-First)

The Npgsql EF Core provider also supports reverse-engineering a code model from an existing PostgreSQL database ("database-first"). To do so, use dotnet CLI to execute the following:

```bash
dotnet ef dbcontext scaffold "Host=my_host;Database=my_db;Username=my_user;Password=my_pw" Npgsql.EntityFrameworkCore.PostgreSQL
```
