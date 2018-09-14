#!/bin/bash

psql -c "CREATE USER npgsql_tests WITH PASSWORD 'npgsql_tests' SUPERUSER"
