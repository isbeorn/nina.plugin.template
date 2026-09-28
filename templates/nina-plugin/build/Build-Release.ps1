[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Tag,
    [Parameter(Mandatory)][string]$Repository,
    [ValidateSet('Release','Beta')][string]$Channel = 'Release'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. "$PSScriptRoot/Release-Settings.ps1"
$settings = Get-ReleaseSettings -Tag $Tag -Repository $Repository -Channel $Channel
function Invoke-Dotnet([string[]]$Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE) { throw "dotnet failed with exit code $LASTEXITCODE." }
}
$root = Split-Path $PSScriptRoot
$solution = @(Get-ChildItem -LiteralPath $root -Filter *.slnx)
$project = @(Get-ChildItem -LiteralPath (Join-Path $root 'src') -Filter *.csproj -Recurse)
if ($solution.Count -ne 1 -or $project.Count -ne 1) { throw 'Expected one solution and one plugin project under src.' }
Invoke-Dotnet @('restore', $solution[0].FullName)
Invoke-Dotnet @('build', $solution[0].FullName, '-c', 'Release', '--no-restore', "-p:Version=$($settings.Version)", "-p:RepositoryUrl=https://github.com/$Repository")
if (Test-Path -LiteralPath (Join-Path $root 'tests')) {
    Invoke-Dotnet @('test', $solution[0].FullName, '-c', 'Release', '--no-build', '--no-restore')
}
# Query the already evaluated project rather than guessing the DLL name from the display name.
$assemblyName = (& dotnet msbuild $project[0].FullName -getProperty:AssemblyName).Trim()
if ($LASTEXITCODE -or [string]::IsNullOrWhiteSpace($assemblyName)) { throw 'Could not read AssemblyName.' }
$url = "https://github.com/$Repository/releases/download/$Tag/$assemblyName.$($settings.Version).zip"
Invoke-Dotnet @('msbuild', $project[0].FullName, '-t:PackagePlugin', '-p:Configuration=Release', "-p:Version=$($settings.Version)", "-p:RepositoryUrl=https://github.com/$Repository", "-p:PluginInstallerUrl=$url", "-p:PluginChannel=$Channel")
$manifestPath = Join-Path $root "artifacts/release/$assemblyName.$($settings.Version).manifest.json"
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.Repository -ne "https://github.com/$Repository" -or $manifest.Installer.URL -ne $url) { throw 'Release metadata does not match the repository and tag.' }
if ($manifest.Name -eq 'My NINA Plugin' -or $manifest.Author -eq 'Plugin Author') { throw 'Set your plugin display name and author in Plugin.props before releasing.' }
Write-Host "Validated release $Tag ($Channel). Assets: $root/artifacts/release"
