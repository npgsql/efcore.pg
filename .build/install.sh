#!/bin/bash

export DEBIAN_FRONTEND=noninteractive
lsb_release -irc

###################
## Setup PostGIS ##
###################

echo 'Registering PostgreSQL Apt Repository'
sudo sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt/ xenial-pgdg main" >> /etc/apt/sources.list'
wget -q -O - http://apt.postgresql.org/pub/repos/apt/ACCC4CF8.asc | sudo apt-key add -
sudo apt-get -qq update

echo 'Installing PostGIS'
sudo apt-get -qq install postgresql-10-postgis-2.4 > /dev/null
sudo apt-get -qq install postgresql-10-postgis-2.4-scripts > /dev/null

#####################
## Setup .NET Core ##
#####################

## This adds ~4 minutes to the build time.
# echo 'Upgrading .NET Core SDK'
# sudo apt-get -qq upgrade dotnet-sdk-2.1 > /dev/null