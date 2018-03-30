#!/bin/bash

if [ "$#" -ne 1 ]; then
  echo "usage: bump.sh <version>"
  exit 1
fi

v=$1
if [[ $v == *"-" ]]; then
  echo "Version must not end with -"
  exit 1
fi

sed -i 's/^version: .*/version: '$v'-{build}/' .appveyor.yml
sed -i 's/<VersionPrefix>[^<]*<\/VersionPrefix>/<VersionPrefix>'$v'<\/VersionPrefix>/' {src,test}/*/*.csproj

git add .appveyor.yml
git add {src,test}/*/*.csproj

git commit -m "Bump version to $v"
