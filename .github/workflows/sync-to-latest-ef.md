---
description: |
  This workflow periodically updates EFCore.PG to reference the latest EF daily build, and makes any
  changes to make it pass.

on:
  schedule: weekly
  workflow_dispatch:

permissions:
  contents: read
  issues: read
  pull-requests: read

engine:
  id: copilot
  model: claude-opus-4.6

strict: false # Required since we're using custom domains in network below

network:
  allowed:
    - defaults
    - dotnet # Allow nuget so we can check the latest released preview
    # Azure URLs for getting the daily build nugets, prerelease SDK, etc.
    - "https://pkgs.dev.azure.com"
    - "https://*.vsblob.vsassets.io"
    - "https://dotnetcli.azureedge.net"
    # In case we use an Npgsql prerelease version
    - "https://myget.org"

safe-outputs:
  create-pull-request:
    title-prefix: "[EF Sync] "
    labels: [ef-sync]
    reviewers: [roji]
    expires: 1w
    draft: false

timeout-minutes: 90

# We run PostgreSQL as a service container instead of using the one built into the image since those
# tend to be old (e.g. PG18 is now out, but github still has PG16 on its image)
services:
  postgres:
    image: postgres
    env:
      POSTGRES_PASSWORD: postgres
    options: >-
      --health-cmd pg_isready
      --health-interval 10s
      --health-timeout 5s
      --health-retries 5
    ports:
      - 5432:5432

env:
  Test__Npgsql__DefaultConnection: "Host=postgres;Username=postgres;Password=postgres"
---

# Synchronize to latest EF

We're going to sync the EF Core PostgreSQL provider (EFCore.PG) in this repo to reference the latest daily build of EF Core, and make any necessary adjustment. Try your best to arrive at a fully 100% passing build after syncing, but even if you're unable to do so, submit a PR with whatever you did manage to do, as a basic for further manual work.

## 1. Find out and depend on the latest EF Core daily build of the current preview release

1. First, check what the latest *released* preview/rc of EF on nuget.org.
2. Then, find the latest EF Core daily build version **for the upcoming preview/RC**; we do not want to sync to a daily build that's for the next preview. For example, if the current released EF preview is preview.3, we want to update to the latest daily build for preview.4, and not to preview.5 even if packages already exist for that.
3. Note that after preview.7, the naming switches to rc.1 and rc.2.
4. Once you've found the latest daily build package we're syncing to, update the EF Core version in Directory.Packages.props to use it. The EF Core daily build nuget feed is https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet11/nuget/v3/index.json (note that you may need to update dotnet11 to dotnet12 depending on the version we're looking for).
5. There's no need to switch to the latest daily build of the dotnet SDK though - only of EF; please just always use the latest released preview version of the dotnet SDK (not daily build), unless there's a specific issue requiring the very latest SDK (very rare/unlikely).
  * To get the appropriate dotnet sdk, use the dotnet-install.sh script, and install it to a temporarily location in /tmp, executing from there (as the filesystem is otherwise readonly).

## 2. Build

Build EFCore.PG and resolve any errors that are reported.

The EF Core source code can be found in https://github.com/dotnet/efcore - you can explore that repo to understand what changes happened there from the last version the EFCore.PG used to the version we're upgrading towards. Focus especially on changes that occured before the current daily build referenced by EFCore.PG (before your change), since everything is still working correctly with it.

Generally try to align EFCore.PG to whatever practices and patterns are in use within EF Core itself.

## 3. Run Tests

Once everything builds, run all the tests under test - both unit and functional tests - and make sure they all pass. You should have a PostgreSQL database available for running the functional tests - run them all, always, as a final validation for your changes - even if this takes a long time.

* If you get failures from Check_all_tests_overridden, that means that new tests have been added - or possibly that existing ones have been renamed. Find out if a rename occurred by finding missing overridden tests that resemble the new test, and explore the EF code base (including its git history) as needed.
  * When adding new test overrides, add them in the appropriate place in the source file, following their placement in the base class. Do not simply add them at the end of the file.
  * Also, always add AssertSql(), following the pattern for other tests in the class.
* If you get failures from test NpgsqlComplianceTest.All_test_bases_must_be_implemented, that means that new test base classes have been added - or possibly that existing ones have been renamed. Find out if a rename occurred by finding missing base classes that resemble the new base class, and explore the EF code base (including its git history) as needed. Do not simply add the missing test suite to IgnoredTestBases.

## 4. Commit and submit PR

* Commit all changes in a single commit (no need for separate commits), calling it simply "Sync to EF 11.0.0-preview.1.26104.118" (substituting the actual version). No need for a commit body message.
* Create a PR in https://github.com/npgsql/efcore.pg with all the changes.
* Include a section in the PR body with a list of all PRs on the EF side that caused changes that required updates in EFCore.PG. You can find these by looking at the git history in the EF repo and finding all commits that occurred between the last version we were synced to and the new version we're syncing to, and then finding the PR associated with each commit (commits generally reference the PR in their title).
* Once again, even if you've failed to arrive at a 100% passing build, submit whatever work you did do as a basis for further manual work.
