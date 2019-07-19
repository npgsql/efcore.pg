# Npgsql Entity Framework Core provider for PostgreSQL

[![gitter](https://img.shields.io/badge/gitter-join%20chat-brightgreen.svg)](https://gitter.im/npgsql/npgsql)

Npgsql.EntityFrameworkCore.PostgreSQL is an Entity Framework Core provider built on top of
[Npgsql](https://github.com/npgsql/npgsql). It allows you to use the EF Core O/RM with
PostgreSQL.

Documentation is available on [the Npgsql site](http://www.npgsql.org/efcore/index.html).

# Build Status

| Branch                               | Build Status                                     | Stable                                 | Next Patch                                  | Unstable                                                    |
|:-------------------------------------|:-------------------------------------------------|:---------------------------------------|:--------------------------------------------|:------------------------------------------------------------|
| [`dev`][tree-dev]                    | [![status][build-dev]][ci-dev]                   | [![stable][nuget-vpre]][nuget-npgsql]  |                                             | [![unstable][npgsql-unstable-vpre]][myget-npgsql-unstable]  |
| [`master`][tree-master]              | [![status][build-master]][ci-master]             | [![stable][nuget-v]][nuget-npgsql]     | [![next patch][npgsql-vpre]][myget-npgsql]  |                                                             |
| [`hotfix/2.2.6`][tree-hotfix/2.2.*]  | [![status][build-hotfix/2.2.*]][ci-hotfix/2.2.*] | [![stable][nuget-2.2.*]][nuget-npgsql] | [![next patch][npgsql-2.2.*]][myget-npgsql] | [![unstable][npgsql-unstable-2.2.*]][myget-npgsql-unstable] |
| [`hotfix/2.1.11`][tree-hotfix/2.1.*] | [![status][build-hotfix/2.1.*]][ci-hotfix/2.1.*] | [![stable][nuget-2.1.*]][nuget-npgsql] | [![next patch][npgsql-2.1.*]][myget-npgsql] | [![unstable][npgsql-unstable-2.1.*]][myget-npgsql-unstable] |


[* Package Feeds]: <>
[nuget-npgsql]:          https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL
[myget-npgsql]:          https://www.myget.org/feed/npgsql/package/nuget/Npgsql.EntityFrameworkCore.PostgreSQL
[myget-npgsql-unstable]: https://www.myget.org/feed/npgsql-unstable/package/nuget/Npgsql.EntityFrameworkCore.PostgreSQL

[* Branches]: <>
[tree-dev]: https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/tree/dev
[tree-master]: https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/tree/master
[tree-hotfix/2.2.*]: https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/tree/hotfix/2.2.6
[tree-hotfix/2.1.*]: https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/tree/hotfix/2.1.11

[* Azure Pipelines (badges)]: <>
[build-dev]:          https://dev.azure.com/npgsql/EFCore.PG/_apis/build/status/npgsql.Npgsql.EntityFrameworkCore.PostgreSQL?branchName=dev
[build-master]:       https://dev.azure.com/npgsql/EFCore.PG/_apis/build/status/npgsql.Npgsql.EntityFrameworkCore.PostgreSQL?branchName=master
[build-hotfix/2.2.*]: https://dev.azure.com/npgsql/EFCore.PG/_apis/build/status/npgsql.Npgsql.EntityFrameworkCore.PostgreSQL?branchName=hotfix/2.2.6
[build-hotfix/2.1.*]: https://dev.azure.com/npgsql/EFCore.PG/_apis/build/status/npgsql.Npgsql.EntityFrameworkCore.PostgreSQL?branchName=hotfix/2.1.11

[* Azure Pipelines (links)]: <>
[ci-dev]:          https://dev.azure.com/npgsql/EFCore.PG/_build/latest?branchName=dev
[ci-master]:       https://dev.azure.com/npgsql/EFCore.PG/_build/latest?branchName=master
[ci-hotfix/2.2.*]: https://dev.azure.com/npgsql/EFCore.PG/_build/latest?branchName=hotfix/2.2.6
[ci-hotfix/2.1.*]: https://dev.azure.com/npgsql/EFCore.PG/_build/latest?branchName=hotfix/2.1.11

[* Dynamic Versions]: <>
[nuget-v]:              https://img.shields.io/nuget/v/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=nuget
[nuget-vpre]:           https://img.shields.io/nuget/vpre/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=nuget
[npgsql-v]:             https://img.shields.io/myget/npgsql/v/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=npgsql
[npgsql-vpre]:          https://img.shields.io/myget/npgsql/vpre/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=npgsql
[npgsql-unstable-v]:    https://img.shields.io/myget/npgsql-unstable/v/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=npgsql-unstable
[npgsql-unstable-vpre]: https://img.shields.io/myget/npgsql-unstable/vpre/Npgsql.EntityFrameworkCore.PostgreSQL.svg?label=npgsql-unstable

[* Static Versions]: <>
[nuget-2.2.*]:           https://img.shields.io/badge/nuget-v2.2.*-blue.svg
[nuget-2.1.*]:           https://img.shields.io/badge/nuget-v2.1.*-blue.svg
[npgsql-2.2.*]:          https://img.shields.io/badge/npgsql-v2.2.*-yellow.svg
[npgsql-2.1.*]:          https://img.shields.io/badge/npgsql-v2.1.*-yellow.svg
[npgsql-unstable-2.2.*]: https://img.shields.io/badge/npgsql--unstable-v2.2.*-yellow.svg
[npgsql-unstable-2.1.*]: https://img.shields.io/badge/npgsql--unstable-v2.1.*-yellow.svg
