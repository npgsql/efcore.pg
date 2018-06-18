# Getting Started

Npgsql has an Entity Framework (EF) Core provider. It behaves like other EF Core provider (e.g. SQL Server), so all of the information in the [general EF Core docs](https://docs.microsoft.com/en-us/ef/core/index) applies. If you're just getting started with EF Core, those docs are the best place to start.

Development happens in the [Npgsql.EntityFrameworkCore.PostgreSQL](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL) repository, all issues should be reported there.

## Configuring the project file

To use the Npgsql EF Core provider, add a dependencies for `Npgsql` and `Npgsql.EntityFrameworkCore.PostgreSQL`. You can follow the instructions in the general [EF Core Getting Started docs](https://docs.microsoft.com/en-us/ef/core/get-started/).

Below is a `.csproj` file for a console application that uses the Npgsql EF Core provider:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netcoreapp2.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="2.1.0" />
    <PackageReference Include="Npgsql" Version="4.0.0" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="2.1.0" />
  </ItemGroup>
</Project>
```

## Defining a `DbContext`

```c#
public class BlogContext : DbContext
{
    public BlogContext(DbContextOptions<BlogContext> options) : base(options) { }
        
    public DbSet<BlogPost> BlogPosts { get; set; }     
}
```

## Additional configuration for ASP.NET Core applications

From `Startup.cs` in `ConfigureServices(IServiceCollection)`:

```c#
public IServiceProvider ConfigureServices(IServiceCollection services)
    => services.AddEntityFrameworkNpgsql()
               .AddDbContext<BlogContext>(options => options.UseNpgsql(connectionString))
               .BuildServiceProvider();
```
## Using an Existing Database (Database-First)

The Npgsql EF Core provider also supports reverse-engineering a code model from an existing PostgreSQL database ("database-first"). To do so, use the `dotnet` CLI to execute the following:

```bash
dotnet ef dbcontext scaffold "Host=my_host;Database=my_db;Username=my_user;Password=my_pw" Npgsql.EntityFrameworkCore.PostgreSQL
```

Or with Powershell:

```powershell
Scaffold-DbContext "Host=my_host;Database=my_db;Username=my_user;Password=my_pw" Npgsql.EntityFrameworkCore.PostgreSQL
```
