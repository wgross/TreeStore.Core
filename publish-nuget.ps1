[CmdletBinding()]
param()
process {
    # Fetch the current version from the project for later identification 
    # of the nuget packages to publish
    $version = Get-DotNetProjectItem -Path .\src\TreeStore.Core\|Get-DotNetProjectVersion
    "Version is: $($version.VersionPrefix)"|Write-Debug

    # Get the api key for nuget.org 
    $apiKey = Get-Secret -Name "nuget" -AsPlainText

    # identify the package and uplod it to nuget.org
    $packagePath = "$PsScriptRoot\..\packages\TreeStore.Core.$($Version.VersionPrefix).nupkg"|Resolve-Path -Relative
    "Package is: $packagePath"|Write-Debug

    nuget push $packagePath $apiKey
}