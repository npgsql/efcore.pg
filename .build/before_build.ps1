#!/usr/bin/pwsh

$env:PATH="$env:PATH;$env:ProgramFiles/PostgreSQL/10/bin/";
psql -U postgres -c "CREATE USER npgsql_tests WITH PASSWORD 'npgsql_tests' SUPERUSER";
