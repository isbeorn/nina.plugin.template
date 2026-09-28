[CmdletBinding()]
param([Parameter(Mandatory)][string]$Directory, [Parameter(Mandatory)][string]$Tag, [Parameter(Mandatory)][string]$Repository)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Release-Settings.ps1"
$settings = Get-ReleaseSettings -Tag $Tag -Repository $Repository
$manifests = @(Get-ChildItem -LiteralPath $Directory -Filter *.manifest.json)
$archives = @(Get-ChildItem -LiteralPath $Directory -Filter *.zip)
if ($manifests.Count -ne 1 -or $archives.Count -ne 1) { throw 'Expected exactly one archive and one manifest.' }
$json = Get-Content -LiteralPath $manifests[0].FullName -Raw
if (!(Test-Json -Json $json -SchemaFile "$PSScriptRoot/vendor/manifest.schema.json")) { throw 'Invalid manifest.' }
$manifest = $json | ConvertFrom-Json
$version = "$($manifest.Version.Major).$($manifest.Version.Minor).$($manifest.Version.Patch).$($manifest.Version.Build)"
if ($version -ne $settings.Version) { throw 'Manifest version differs from tag.' }
$expectedUrl = "https://github.com/$Repository/releases/download/$Tag/$($archives[0].Name)"
if ($manifest.Installer.URL -ne $expectedUrl -or $manifest.Installer.Type -ne 'ARCHIVE' -or $manifest.Installer.ChecksumType -ne 'SHA256') { throw 'Unexpected installer metadata.' }
if ((Get-FileHash -LiteralPath $archives[0].FullName -Algorithm SHA256).Hash -ne $manifest.Installer.Checksum) { throw 'Archive checksum differs from manifest.' }
Write-Host 'Verified release archive and manifest.'
