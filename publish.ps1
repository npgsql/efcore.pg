<#
.SYNOPSIS
    A script used to publish efcore.pg to our internal NuGet repository
.DESCRIPTION
    TeamCity will build from the tip of the specified branch and publish efcore.pg
    as a NuGet package to Pathmatics' private NuGet repository.

    Please reference examples for syntax help.
.EXAMPLE
    TeamCity Publish Examples
    
    Current Branch:
    -> C:\PS> .\publish.ps1

    Specified Branch:
    -> C:\PS> .\publish.ps1 -branch master

    Specified Branch (contains spaces):
    -> Wrap branch name in single quotes
    -> C:\PS> .\publish.ps1 -branch 'branch name with spaces'

.LINK
    [1] https://confluence.jetbrains.com/display/TCD18/REST+API#RESTAPI-TriggeringaBuild
#>

param(
    [Parameter(HelpMessage = "[Optional] Name of branch TeamCity will build (will build from tip of branch)")]
    [string]$BRANCH = $null
)

#region [ FUNCTIONS ]

function Test-BranchExists ([string]$branchName) {
    $branchExists = "$(git ls-remote --heads origin | findstr "refs/heads/$branchName")"

    if (!$branchExists) {
        Write-Host "`nERROR: Branch not found -> $branchName" -ForegroundColor Red
        Write-Host "- Please check your spelling" -ForegroundColor Red
        Write-Host "- If your branch contains spaces, please wrap the name with single quotes" -ForegroundColor Red
        Exit
    }
}

function New-XmlRequestBody ([string]$branch, [string]$teamCityBuildId) {

    Write-Host "`nBuilding XML Request Body..."

    [xml]$xmlDoc = New-Object System.Xml.XmlDocument
    $xmlRoot = $xmlDoc.CreateElement("build")
    $xmlRoot.SetAttribute("branchName", "refs/heads/$branch")

    $buildType = $xmlDoc.CreateElement("buildType")
    $buildType.SetAttribute("id", $teamCityBuildId)

    $xmlRoot.AppendChild($buildType) | Out-Null
    $xmlDoc.AppendChild($xmlRoot) | Out-Null

    return $xmlDoc
}

#endregion

#region [ START ]

# Get branch or test branch if given
If ([string]::IsNullOrEmpty($BRANCH)) {
    $BRANCH = "$(git rev-parse --abbrev-ref HEAD)"
} Else {
    Test-BranchExists $BRANCH
}


$TEAMCITY_ID = "Pathmatics_EFCorePG_Build"
$requestBody = New-XmlRequestBody $BRANCH $TEAMCITY_ID

$params = @{
    Headers     = @{
        Origin        = "http://teamcity.aws.pathmatics.com"
        Authorization = "Bearer eyJ0eXAiOiAiVENWMiJ9.QkpvYWVhTXNpcll2TnRueDFrUWdZUHowRWs4.ZGIyNDFlYmMtMjQyZC00YTY4LWFhMjctN2QwMDJmMmExZGZm" # Access Token from Apps account
    }
    Method      = 'POST'
    ContentType = 'application/xml'
    Uri         = 'http://teamcity.aws.pathmatics.com/app/rest/buildQueue'
    Body        = $requestBody
}

try {
    # Reference [1]
    Write-Host "Triggering TeamCity build/publish from the tip of $BRANCH..."
    Invoke-RestMethod @params | Out-Null
    Write-Host "`nSuccessfully triggered from the tip of $BRANCH!" -ForegroundColor Green
    Write-Host "-> Review builds here: http://teamcity.aws.pathmatics.com/buildConfiguration/Pathmatics_EFCorePG_Build?branch=&buildTypeTab=overview"
}
catch {
    Write-Host $_.Exception | Format-List -force
    throw $_.Exception
}

#endregion

