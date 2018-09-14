#!/usr/bin/pwsh

If ($isWindows) {
    $env:PATH="$env:PATH;$env:ProgramFiles/PostgreSQL/10/bin/";
    psql -U postgres -c "CREATE USER npgsql_tests WITH PASSWORD 'npgsql_tests' SUPERUSER";
}
