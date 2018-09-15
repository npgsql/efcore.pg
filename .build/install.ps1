#!/usr/bin/pwsh

###################
## Setup PostGIS ##
###################

If (!(Test-Path $env:POSTGIS_EXE)) {
    Write-Host 'Downloading PostGIS';
    (New-Object Net.WebClient).DownloadFile("http://download.osgeo.org/postgis/windows/pg10/$env:POSTGIS_EXE", "$env:POSTGIS_EXE");
}

Write-Host 'Installing PostGIS';
Invoke-Expression ".\$env:POSTGIS_EXE /S /D='C:\Program Files\PostgreSQL\10'";

#####################
## Setup .NET Core ##
#####################

## The following can be used to install a custom version of .NET Core
# Invoke-WebRequest -Uri "https://raw.githubusercontent.com/dotnet/cli/master/scripts/obtain/dotnet-install.ps1" -OutFile "install-dotnet.ps1"
# ./install-dotnet.ps1 -Version 2.1.300-rc1-008673
