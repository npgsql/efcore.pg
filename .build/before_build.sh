#!/bin/bash

sudo -u postgres psql -c "CREATE USER npgsql_tests WITH PASSWORD 'npgsql_tests' SUPERUSER"
