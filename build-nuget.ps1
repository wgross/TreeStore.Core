<#
.SYNOPSIS
	Build the nuget package of the project
	Requires some local-onel modules
#>

Import-Module fs-dirs
Import-Module dotnet-fs
Import-Module dotnet-cli-pack

"$PSScriptRoot/src/TreeStore.Core" | fs-dirs\Invoke-AtContainer {
	# Clean older builds
	Remove-Item -Path *.nupkg
	Remove-Item -Path *.snupkg

	# Create new package/symbol package
 	dotnet-fs\Get-DotNetProjectItem -CSproj | dotnet-cli-pack\Invoke-DotNetPack
}



