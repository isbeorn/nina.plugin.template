[CmdletBinding()]
param([Parameter(Mandatory)][string]$FileList, [Parameter(Mandatory)][string]$Destination)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$destinationRoot = [IO.Path]::GetFullPath($Destination)
$entries = foreach ($line in Get-Content -LiteralPath $FileList) {
    $parts = $line.Split('|')
    if ($parts.Count -ne 2 -or [string]::IsNullOrWhiteSpace($parts[1])) { throw "Invalid package file entry: $line" }
    $target = [IO.Path]::GetFullPath((Join-Path $destinationRoot $parts[1]))
    if ([IO.Path]::IsPathRooted($parts[1]) -or !$target.StartsWith($destinationRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw "Package path escapes staging: $($parts[1])" }
    if (!(Test-Path -LiteralPath $parts[0] -PathType Leaf)) { throw "Missing package file: $($parts[0])" }
    [pscustomobject]@{ Source=$parts[0]; Target=$target; Relative=$parts[1] }
}
if (@($entries.Target | Sort-Object -Unique).Count -ne @($entries).Count) { throw 'Duplicate package destination.' }
# Delete only a staging folder owned by this script. Never erase arbitrary user paths.
if (Test-Path -LiteralPath $destinationRoot) {
    if (!(Test-Path -LiteralPath (Join-Path $destinationRoot '.nina-template-stage'))) { throw "Refusing to clear unowned staging folder: $destinationRoot" }
    Remove-Item -LiteralPath $destinationRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $destinationRoot -Force | Out-Null
Set-Content -LiteralPath (Join-Path $destinationRoot '.nina-template-stage') -Value 'NINA plugin staging'
foreach ($entry in $entries) {
    New-Item -ItemType Directory -Path (Split-Path $entry.Target) -Force | Out-Null
    Copy-Item -LiteralPath $entry.Source -Destination $entry.Target
}
