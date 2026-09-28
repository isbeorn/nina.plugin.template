[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
function Assert($Condition, [string]$Message) { if (!$Condition) { throw $Message } }
function Should-Fail([scriptblock]$Action, [string]$Expected) {
    $failure = $null
    try { & $Action | Out-Null } catch { $failure = $_.Exception.Message }
    Assert ($failure -and $failure.Contains($Expected)) "Expected failure containing '$Expected', got '$failure'."
}
$root = Split-Path $PSScriptRoot
$projectPath = Join-Path $root 'NINA.Plugin.Templates.csproj'
$projectHash = (Get-FileHash -LiteralPath $projectPath -Algorithm SHA256).Hash
$version = & dotnet msbuild $projectPath -nologo -getProperty:PackageVersion
if ($LASTEXITCODE) { throw 'Could not evaluate the template package version.' }
$tag = 'v' + $version
$release = & "$PSScriptRoot/Get-TemplateRelease.ps1" -Tag $tag
$packageDirectory = Join-Path $root 'artifacts/packages'
$package = Join-Path $packageDirectory $release.PackageFileName
& "$PSScriptRoot/Get-TemplateRelease.ps1" -Tag $tag -Package $package | Out-Null
foreach ($releaseTag in @('v1.0.0','v2.4.6')) {
    $fromTag = & "$PSScriptRoot/Get-TemplateRelease.ps1" -Tag $releaseTag
    Assert ($fromTag.Version -eq $releaseTag.Substring(1)) 'The tag must determine the package version.'
}
foreach ($invalid in @('3.0.0','V3.0.0','v3.0','v3.0.0.0','v03.0.0','v3.0.0;echo','v3.0.0-rc.1')) {
    Should-Fail { & "$PSScriptRoot/Get-TemplateRelease.ps1" -Tag $invalid } 'Template release tags'
}
$temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar)
$temporary = Join-Path $temporaryRoot ('nina-template-release-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporary | Out-Null
$savedKey = $env:NUGET_API_KEY
try {
    foreach ($releaseTag in @('v1.0.0','v2.4.6')) {
        $fromTag = & "$PSScriptRoot/Get-TemplateRelease.ps1" -Tag $releaseTag
        $output = Join-Path $temporary $fromTag.Version
        & dotnet pack $projectPath --nologo --verbosity quiet "-p:TemplatePackageVersion=$($fromTag.Version)" -o $output
        if ($LASTEXITCODE) { throw "Packing $releaseTag failed." }
        & "$PSScriptRoot/Get-TemplateRelease.ps1" -Tag $releaseTag -Package (Join-Path $output $fromTag.PackageFileName) | Out-Null
    }
    Assert ((Get-FileHash -LiteralPath $projectPath -Algorithm SHA256).Hash -eq $projectHash) 'Release packing modified the project file.'
    $fixture = Join-Path $temporary $release.PackageFileName
    foreach ($fault in @('version','identity','template')) {
        $archive = [IO.Compression.ZipFile]::Open($fixture, [IO.Compression.ZipArchiveMode]::Create)
        try {
            $id = if ($fault -eq 'identity') { 'Wrong.Package' } else { 'NINA.Plugin.Templates' }
            $version = if ($fault -eq 'version') { '99999.0.0' } else { $release.Version }
            $writer = [IO.StreamWriter]::new($archive.CreateEntry('NINA.Plugin.Templates.nuspec').Open())
            try { $writer.Write("<package><metadata><id>$id</id><version>$version</version></metadata></package>") } finally { $writer.Dispose() }
            if ($fault -ne 'template') { $archive.CreateEntry('content/nina-plugin/.template.config/template.json') | Out-Null }
        } finally { $archive.Dispose() }
        $expected = if ($fault -eq 'template') { 'does not contain the template' } else { 'identity or version differs' }
        Should-Fail { & "$PSScriptRoot/Get-TemplateRelease.ps1" -Tag $tag -Package $fixture } $expected
        Remove-Item -LiteralPath $fixture
    }
    # Replace only the publishing CLI boundary. No test contacts NuGet.org or uploads a package.
    $publishCalls = [Collections.Generic.List[object]]::new()
    $publishExitCode = 0
    function dotnet {
        $publishCalls.Add(@($args))
        $global:LASTEXITCODE = $publishExitCode
    }
    $env:NUGET_API_KEY = ''
    Should-Fail { & "$PSScriptRoot/Publish-Template.ps1" -Tag $tag -Directory $packageDirectory } 'Configure the NUGET_API_KEY'
    Assert ($publishCalls.Count -eq 0) 'Publishing started without credentials.'
    $env:NUGET_API_KEY = 'fake-key-used-only-by-local-tests'
    Should-Fail { & "$PSScriptRoot/Publish-Template.ps1" -Tag v1.2 -Directory $packageDirectory } 'Template release tags'
    Assert ($publishCalls.Count -eq 0) 'Publishing started with an invalid release tag.'
    & "$PSScriptRoot/Publish-Template.ps1" -Tag $tag -Directory $packageDirectory
    & "$PSScriptRoot/Publish-Template.ps1" -Tag $tag -Directory $packageDirectory
    Assert ($publishCalls.Count -eq 2) 'Expected two repeatable publication attempts.'
    foreach ($call in $publishCalls) {
        Assert ($call[0] -eq 'nuget' -and $call[1] -eq 'push' -and $call[2] -eq $package) 'Wrong publication command or package.'
        Assert ($call -contains '--skip-duplicate') 'Repeat publication must tolerate an existing version.'
        Assert ($call -contains 'https://api.nuget.org/v3/index.json') 'Wrong NuGet source.'
    }
    $publishExitCode = 17
    Should-Fail { & "$PSScriptRoot/Publish-Template.ps1" -Tag $tag -Directory $packageDirectory } 'NuGet publishing failed with exit code 17'
} finally {
    $env:NUGET_API_KEY = $savedKey
    $resolved = (Resolve-Path -LiteralPath $temporary).Path
    if (!$resolved.StartsWith($temporaryRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or (Split-Path $resolved -Leaf) -notmatch '^nina-template-release-[a-f0-9]{32}$') { throw 'Unexpected test cleanup path.' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
Write-Host 'PASS: tag-derived package versions, package identity/version, missing credentials, publication reruns and failed uploads.'
