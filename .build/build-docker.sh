#!/bin/sh
set -e

mkdir -p ./docker/11
mkdir -p ./docker/10
mkdir -p ./docker/9.6

cp -r ./docker/current/. ./docker/11
cp -r ./docker/current/. ./docker/10
cp -r ./docker/current/. ./docker/9.6

sed -i -E 's/FROM postgres:[0-9.]+/FROM postgres:11/'  ./docker/11/Dockerfile
sed -i -E 's/FROM postgres:[0-9.]+/FROM postgres:10/'  ./docker/10/Dockerfile
sed -i -E 's/FROM postgres:[0-9.]+/FROM postgres:9.6/' ./docker/9.6/Dockerfile

sudo docker build -t austindrenski/npgsql-postgres:11  ./docker/11
sudo docker build -t austindrenski/npgsql-postgres:10  ./docker/10
sudo docker build -t austindrenski/npgsql-postgres:9.6 ./docker/9.6

sudo docker push austindrenski/npgsql-postgres:11
sudo docker push austindrenski/npgsql-postgres:10
sudo docker push austindrenski/npgsql-postgres:9.6
