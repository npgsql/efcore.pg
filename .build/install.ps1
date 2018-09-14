#!/usr/bin/pwsh

###################
## Setup PostGIS ##
###################

If ($isWindows) {
    If (!(Test-Path $env:POSTGIS_EXE)) {
        Write-Host 'Downloading PostGIS'
        (New-Object Net.WebClient).DownloadFile("http://download.osgeo.org/postgis/windows/pg10/$env:POSTGIS_EXE", "$env:POSTGIS_EXE")
    }
    Write-Host 'Installing PostGIS'
    iex ".\$env:POSTGIS_EXE /S /D='C:\Program Files\PostgreSQL\10'"
}

#####################
## Setup .NET Core ##
#####################

If ($isWindows) {
    ## The following can be used to install a custom version of .NET Core
    # - ps: Invoke-WebRequest -Uri "https://raw.githubusercontent.com/dotnet/cli/master/scripts/obtain/dotnet-install.ps1" -OutFile "install-dotnet.ps1"
    # - ps: .\install-dotnet.ps1 -Version 2.1.300-rc1-008673
}

###########################
## Set version variables ##
###########################

Set-Variable -Name TruncatedSha1 -Value $env:APPVEYOR_REPO_COMMIT.subString(0, 9)

if ($env:APPVEYOR_REPO_TAG -eq 'true' -and $env:APPVEYOR_REPO_TAG_NAME -match '^v\d+\.\d+\.\d+(-(\w+))?')
{
    if ($matches[2]) {
        Write-Host "Prerelease tag detected ($env:APPVEYOR_REPO_TAG_NAME), version suffix set to $($matches[2])."
        Set-AppveyorBuildVariable -Name VersionSuffix -Value $matches[2]
    } else {
        Write-Host "Release tag detected ($env:APPVEYOR_REPO_TAG_NAME), no version suffix will be set."
    }
    Set-AppveyorBuildVariable -Name deploy_github_release -Value true
}
#elseif (Test-Path env:APPVEYOR_PULL_REQUEST_NUMBER)
#{
#    Set-AppveyorBuildVariable -Name deploy_myget_unstable -Value true
#    Set-Variable -Name VersionSuffix -Value "pr$($env:APPVEYOR_PULL_REQUEST_NUMBER).$($env:APPVEYOR_BUILD_NUMBER)+sha.$TruncatedSha1"
#    Write-Host "Pull request detected (#$env:APPVEYOR_PULL_REQUEST_NUMBER), setting version suffix to $VersionSuffix"
#    Set-AppveyorBuildVariable -Name VersionSuffix -Value $VersionSuffix
#}
else
{
    # Set which myget feed we deploy to
    if ($env:APPVEYOR_REPO_BRANCH.StartsWith("hotfix/")) {
        Set-AppveyorBuildVariable -Name deploy_myget_stable -Value true
    } else {
        Set-AppveyorBuildVariable -Name deploy_myget_unstable -Value true
    }

    Set-Variable -Name VersionSuffix -Value "ci.$($env:APPVEYOR_BUILD_NUMBER)+sha.$TruncatedSha1"
    Write-Host "Setting version suffix to $VersionSuffix"
    Set-AppveyorBuildVariable -Name VersionSuffix -Value $VersionSuffix
}
