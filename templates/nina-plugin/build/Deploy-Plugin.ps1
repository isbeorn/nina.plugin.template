[CmdletBinding()]
param([Parameter(Mandatory)][string]$Stage, [Parameter(Mandatory)][string]$AssemblyName, [string]$Destination)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (!(Test-Path -LiteralPath (Join-Path $Stage '.nina-template-stage'))) { throw 'Use the DeployPlugin target to stage files first.' }
if ([string]::IsNullOrWhiteSpace($Destination)) {
    $metadata = [Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $Stage $AssemblyName))
    $folder = $metadata.FileDescription.Replace('\', '-').Replace('/', '-')
    foreach ($character in [IO.Path]::GetInvalidFileNameChars()) { $folder = $folder.Replace([string]$character, '_') }
    if ([string]::IsNullOrWhiteSpace($folder) -or $folder -in @('.', '..')) { throw 'Plugin display name does not form a valid folder name.' }
    $Destination = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) "NINA/Plugins/3.0.0/$folder"
}
$files = @(Get-ChildItem -LiteralPath $Stage -File -Recurse | Where-Object Name -ne '.nina-template-stage')
# Check locks before copying anything. Recheck errors during copying as well.
foreach ($file in $files) {
    $target = Join-Path $Destination ([IO.Path]::GetRelativePath($Stage, $file.FullName))
    if (Test-Path -LiteralPath $target) {
        try { $handle = [IO.File]::Open($target, 'Open', 'ReadWrite', 'None'); $handle.Dispose() }
        catch { throw "Cannot replace '$target'. Close NINA and retry. $($_.Exception.Message)" }
    }
}
foreach ($file in $files) {
    $target = Join-Path $Destination ([IO.Path]::GetRelativePath($Stage, $file.FullName))
    New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $target -Force -ErrorAction Stop
}
Write-Host "Deployed plugin to $Destination. Restart NINA to load it."
