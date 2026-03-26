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

Synchronize EFCore.PG to the latest daily build of EF, using the sync-to-latest-ef skill in this repo.
