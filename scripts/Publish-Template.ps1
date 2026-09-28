[CmdletBinding()]
param([Parameter(Mandatory)][string]$Tag, [Parameter(Mandatory)][string]$Directory)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$release = & "$PSScriptRoot/Get-TemplateRelease.ps1" -Tag $Tag
$package = Join-Path $Directory $release.PackageFileName
& "$PSScriptRoot/Get-TemplateRelease.ps1" -Tag $Tag -Package $package | Out-Null
if ([string]::IsNullOrWhiteSpace($env:NUGET_API_KEY)) {
    throw 'Configure the NUGET_API_KEY repository Actions secret with permission to publish NINA.Plugin.Templates.'
}
& dotnet nuget push $package --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
if ($LASTEXITCODE) { throw "NuGet publishing failed with exit code $LASTEXITCODE." }
