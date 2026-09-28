[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Stage,
    [Parameter(Mandatory)][string]$AssemblyName,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$InstallerUrl,
    [ValidateSet('Release','Beta')][string]$Channel = 'Release'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$Stage = (Resolve-Path -LiteralPath $Stage).Path
$assembly = Join-Path $Stage $AssemblyName
$version = [Diagnostics.FileVersionInfo]::GetVersionInfo($assembly).FileVersion
if ($version -notmatch '^\d+\.\d+\.\d+\.\d+$') { throw "Invalid plugin file version: $version" }
$name = [IO.Path]::GetFileNameWithoutExtension($AssemblyName)
$archiveName = "$name.$version"
if ([string]::IsNullOrWhiteSpace($InstallerUrl)) {
    # A local validation artifact only. A real release must provide its immutable download URL.
    $InstallerUrl = "https://example.invalid/releases/$archiveName.zip"
}
$uri = [uri]$InstallerUrl
if (!$uri.IsAbsoluteUri -or $uri.Scheme -ne 'https') { throw 'InstallerUrl must be an absolute HTTPS URL.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path
Push-Location $OutputDirectory
try {
    # The staging ownership marker must not enter the distributable archive.
    $zip = Join-Path $OutputDirectory "$archiveName.zip"
    if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip }
    $files = @(Get-ChildItem -LiteralPath $Stage -Force | Where-Object Name -ne '.nina-template-stage')
    Compress-Archive -LiteralPath $files.FullName -DestinationPath $zip
    & "$PSScriptRoot/vendor/CreateManifest.ps1" -file $assembly -installerUrl $InstallerUrl -beta:($Channel -eq 'Beta') | Out-Null
    $manifest = Get-Content -LiteralPath 'manifest.json' -Raw | ConvertFrom-Json
    $manifest.Tags = @($manifest.Tags | Where-Object { ![string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_.Trim() })
    $manifest.Installer.Type = 'ARCHIVE'
    $manifest.Installer.Checksum = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
    $manifest.Installer.ChecksumType = 'SHA256'
    $json = $manifest | ConvertTo-Json -Depth 20
    if (!(Test-Json -Json $json -SchemaFile "$PSScriptRoot/vendor/manifest.schema.json" -ErrorAction Stop)) { throw 'Manifest schema validation failed.' }
    $manifestPath = Join-Path $OutputDirectory "$archiveName.manifest.json"
    Set-Content -LiteralPath $manifestPath -Value $json -Encoding utf8
    Remove-Item -LiteralPath 'manifest.json'
    Write-Host "Created $zip and $manifestPath"
} finally { Pop-Location }
