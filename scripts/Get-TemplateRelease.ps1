[CmdletBinding()]
param([Parameter(Mandatory)][string]$Tag, [string]$Package)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($Tag -cnotmatch '^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$') {
    throw 'Template release tags must use vMAJOR.MINOR.PATCH, for example v1.0.0.'
}
[xml]$project = Get-Content -LiteralPath (Join-Path (Split-Path $PSScriptRoot) 'NINA.Plugin.Templates.csproj') -Raw
$version = $Tag.Substring(1)
$packageId = [string]$project.Project.PropertyGroup.PackageId
$fileName = "$packageId.$version.nupkg"
if ($Package) {
    $path = (Resolve-Path -LiteralPath $Package).Path
    if ([IO.Path]::GetFileName($path) -cne $fileName) { throw "Expected package filename $fileName." }
    $archive = [IO.Compression.ZipFile]::OpenRead($path)
    try {
        $entry = $archive.GetEntry("$packageId.nuspec")
        if (!$entry) { throw 'Template package metadata is missing.' }
        $reader = [IO.StreamReader]::new($entry.Open())
        try { [xml]$metadata = $reader.ReadToEnd() } finally { $reader.Dispose() }
        if ($metadata.package.metadata.id -cne $packageId -or $metadata.package.metadata.version -cne $version) {
            throw 'Package identity or version differs from the release tag.'
        }
        if (!$archive.GetEntry('content/nina-plugin/.template.config/template.json')) { throw 'Package does not contain the template.' }
    } finally { $archive.Dispose() }
}
[pscustomobject]@{ Tag=$Tag; Version=$version; PackageFileName=$fileName }
