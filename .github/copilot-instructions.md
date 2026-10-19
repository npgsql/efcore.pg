# Copilot Instructions for Npgsql EF Core Provider (EFCore.PG)

This is the PostgreSQL provider for [Entity Framework Core](https://github.com/dotnet/efcore), built on top of [Npgsql](https://github.com/npgsql/npgsql). It translates EF Core LINQ queries into PostgreSQL SQL and handles PostgreSQL-specific features like arrays, JSON, ranges, full-text search, NodaTime, and PostGIS/NetTopologySuite.

## Build and Test

Use the SDK pinned in `global.json` and dependency versions centralized in `Directory.Packages.props`. Consult EF APIs and test bases matching the pinned version; do not upgrade dependencies just to make a change compile.

Tests use Microsoft.Testing.Platform; select projects with `--project` and use its native filters:

```bash
# Build
dotnet build

# Run all tests (requires PostgreSQL - see "Test Database" below)
dotnet test

# Run a specific test project
dotnet test --project test/EFCore.PG.FunctionalTests
dotnet test --project test/EFCore.PG.Tests

# Run a single test class
dotnet test --project test/EFCore.PG.FunctionalTests --filter-class '*GuidTranslationsNpgsqlTest'

# Run a single test method
dotnet test --project test/EFCore.PG.FunctionalTests --filter-method '*GuidTranslationsNpgsqlTest.ToString_projection'
```

### Test Database

Prefer the automatic, isolated PostgreSQL instance provided by Testcontainers for .NET. With a Docker-compatible container runtime running, tests start their own container when no connection string is configured. Each test process gets its own PostgreSQL instance, allowing multiple sessions to test concurrently without interfering with one another.

For normal test runs, leave `Test__Npgsql__DefaultConnection` unset and do not configure `Test:Npgsql:DefaultConnection` in `config.json` or `config.test.json`. Do not start or reuse a shared local PostgreSQL instance by default.

In the Copilot cloud agent, retain the job-isolated PostGIS service and exported connection string provided by `.github/workflows/copilot-setup-steps.yml`.

Only when an existing PostgreSQL instance is specifically needed, set the environment variable:

```bash
export Test__Npgsql__DefaultConnection="Server=localhost;Username=npgsql_tests;Password=npgsql_tests"
```

The default container uses `postgis/postgis` and includes PostGIS for spatial tests. It runs as `linux/amd64`, using emulation on ARM64 hosts such as Apple Silicon. When using an existing PostgreSQL instance, ensure PostGIS is installed if running spatial tests. Set `NPGSQL_TEST_POSTGIS=true` to enforce PostGIS availability (tests fail instead of skip if PostGIS is missing).

## Architecture

### Source Projects (src/)

- **EFCore.PG**: Main PostgreSQL provider. Contains query translation, type mappings, migrations, scaffolding, and service registration.
- **EFCore.PG.NodaTime**: Plugin for NodaTime date/time types.
- **EFCore.PG.NTS**: Plugin for PostGIS spatial types via NetTopologySuite.
- **Shared/**: Utility code shared across all source projects via `<Compile Include>`.

### Test Projects (test/)

- **EFCore.PG.FunctionalTests**: Integration tests requiring a PostgreSQL database. Inherits extensively from EF Core's `Relational.Specification.Tests` base classes.
- **EFCore.PG.Tests**: Unit tests that don't require a database.

### Key Source Directories (under src/EFCore.PG/)

- **Query/ExpressionTranslators/Internal/**: LINQ-to-SQL method/member translators (e.g. `NpgsqlStringMethodTranslator`, `NpgsqlArrayMethodTranslator`). Each translates .NET method calls into PostgreSQL SQL expressions.
- **Query/Expressions/Internal/**: Custom PostgreSQL SQL expression nodes (`PgArrayIndexExpression`, `PgJsonTraversalExpression`, `PgRegexMatchExpression`, `PgILikeExpression`, `PgAnyExpression`, `PgUnnestExpression`, etc.).
- **Query/Internal/**: Query pipeline components — SQL generation (`NpgsqlQuerySqlGenerator`), translation visitors, postprocessors.
- **Storage/Internal/**: Type mapping between .NET types and PostgreSQL types (e.g. `NpgsqlStringTypeMapping`). Each mapping stores an `NpgsqlDbType` and configures `NpgsqlParameter` at execution time.
- **Migrations/**: Migration SQL generation, including PostgreSQL-specific DDL (extensions, enums, sequences).
- **Extensions/**: Extension methods for configuring the provider (`UseNpgsql()`) and PostgreSQL-specific model building (e.g. `HasPostgresExtension()`, `HasPostgresEnum()`).

### Plugin Architecture

NodaTime and NTS plugins hook into the main provider via EF Core's plugin interfaces (`IMethodCallTranslatorPlugin`, `IMemberTranslatorPlugin`, `IRelationalTypeMappingSourcePlugin`, etc.) registered through DI. This allows extending translations and type mappings without modifying the core provider.

## Test Patterns

Query tests should assert both results (typically via `AssertQuery`) and exact generated SQL (`AssertSql`). Follow the suite's sync/async patterns and cover constants, parameters, and null semantics where relevant to the change.

### Choosing Where to Add Tests

- First look for an existing, focused test suite with a robust, shared model. For query translation tests, prefer the suites under `test/EFCore.PG.FunctionalTests/Query/Translations/`, which are organized around specific types, operators, and features.
- Avoid adding new tests to broad legacy suites such as Northwind and GearsOfWar; their models and tests are generally messy and not focused on a specific area.
- Where practical, extend a fixture's shared model with features that can support multiple tests rather than introducing a one-off model. Fixtures inherited from upstream EF Core are often difficult or impossible to extend, so do not force this approach when it is impractical.
- Suites with `AdHoc` or `NonShared` in their names are generally for issues requiring a very specific model. Use them only as a last resort, after trying to cover the scenario in an existing shared-model suite.

### Inherited Specification Tests

Most functional tests inherit from EF Core base test classes and override methods to add PostgreSQL-specific SQL assertions:

```csharp
public class GuidTranslationsNpgsqlTest : GuidTranslationsTestBase<BasicTypesQueryNpgsqlFixture>
{
    public override async Task ToString_projection()
    {
        await base.ToString_projection();

        AssertSql(
            """
SELECT b."Guid"::text
FROM "BasicTypesEntities" AS b
""");
    }
}
```

* All inherited query tests systematically override all methods in the EF-provided base class, in order to call `AssertSql()` to assert on the exact generated SQL. Always do this when e.g. inheriting a new EF functional test class.
* Such test overrides must be placed in the same position in the file as its base test method in EF, to maintain consistency between EF and EFCore.PG. To do that, check the source for the base test class in the EF repo (https://github.com/dotnet/efcore); note that you may need to consult a specific commit in the EF repo that corresponds to what EFCore.PG is referencing.
* To regenerate SQL baselines, run only the affected tests with `EF_TEST_REWRITE_BASELINES=1`. Review the rewritten SQL for correctness, then rerun without the variable.

### Conditional Execution

- `[MinimumPostgresVersion(major, minor)]`: Skips test if PostgreSQL version is too low.
- `[RequiresPostgis]`: Skips test if PostGIS extension is unavailable.

## Conventions

- Targets the latest .NET (currently `net11.0`) with C# `latest` language version. Nullable reference types are enabled. Warnings are treated as errors.
- Prefer `var` everywhere, but prefer collection expressions even more (which require the type on the left-hand side).
- Expression-bodied members are preferred for properties, indexers, and accessors.
- Switch expressions are used whenever possible. When not possible, switch statements are always preferable to if condition chains.
- Primary constructors are used whenever possible.
- Private fields use `_camelCase`; parameters/locals use `camelCase`; types/methods/properties use `PascalCase`.
- PostgreSQL-specific SQL expression classes are prefixed with `Pg` (e.g. `PgArrayIndexExpression`, `PgBinaryExpression`).
- Services are registered via `TryAdd` in `EntityFrameworkNpgsqlServicesBuilder` to allow user overrides.
- Global implicit usings are configured in `Directory.Build.props` for common EF Core and Npgsql namespaces — these do not need to be repeated in individual files.
- The functional test project uses `RootNamespace: Microsoft.EntityFrameworkCore` to align with EF Core's specification test base classes.
