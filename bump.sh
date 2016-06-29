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

echo "echo ##teamcity[buildNumber '$v-%1']" > teamcity_set_version.cmd

if [[ $v == *"-"* ]]; then
  # Prerelease version

  without_prerelease=`echo $v | cut -d- -f1`

  sed -i 's/^\(\s*\)"version": "[^"]*"/\1"version": "'$v'-*"/' src/Npgsql.EntityFrameworkCore.PostgreSQL/project.json
  sed -i 's/^\(\s*\)"version": "[^"]*"/\1"version": "'$v'-*"/' src/Npgsql.EntityFrameworkCore.PostgreSQL.Design/project.json

  sed -i 's/AssemblyVersion("[^"]*")/AssemblyVersion("'$without_prerelease'")/' src/Shared/CommonAssemblyInfo.cs
  sed -i 's/AssemblyFileVersion("[^"]*")/AssemblyFileVersion("'$without_prerelease'")/' src/Shared/CommonAssemblyInfo.cs
  sed -i 's/AssemblyInformationalVersion("[^"]*")/AssemblyInformationalVersion("'$v'")/' src/Shared/CommonAssemblyInfo.cs
else
  # Release version

  sed -i 's/^\(\s*\)"version": "[^"]*"/\1"version": "'$v'"/' src/Npgsql.EntityFrameworkCore.PostgreSQL/project.json
  sed -i 's/^\(\s*\)"version": "[^"]*"/\1"version": "'$v'"/' src/Npgsql.EntityFrameworkCore.PostgreSQL.Design/project.json

  sed -i 's/AssemblyVersion("[^"]*")/AssemblyVersion("'$v'")/' src/Shared/CommonAssemblyInfo.cs
  sed -i 's/AssemblyFileVersion("[^"]*")/AssemblyFileVersion("'$v'")/' src/Shared/CommonAssemblyInfo.cs
  sed -i 's/AssemblyInformationalVersion("[^"]*")/AssemblyInformationalVersion("'$v'")/' src/Shared/CommonAssemblyInfo.cs
fi

git add teamcity_set_version.cmd
git add src/Npgsql.EntityFrameworkCore.PostgreSQL/project.json
git add src/Npgsql.EntityFrameworkCore.PostgreSQL.Design/project.json
git add src/Shared/CommonAssemblyInfo.cs

git commit -m "Bump version to $v"
