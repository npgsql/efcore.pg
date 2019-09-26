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
sed -i 's/<VersionPrefix>[^<]*<\/VersionPrefix>/<VersionPrefix>'$v'<\/VersionPrefix>/' Directory.Build.props
sed -i 's/^name: [^-]*-/name: '$v'-/' azure-pipelines.yml

git add .appveyor.yml
git add azure-pipelines.yml
git add Directory.Build.props

git commit -m "Bump version to $v"
