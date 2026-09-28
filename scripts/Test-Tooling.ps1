[CmdletBinding()]
param([Parameter(Mandatory)][string]$Run)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
function Assert($Condition, [string]$Message) { if (!$Condition) { throw $Message } }
function Should-Fail([scriptblock]$Action, [string]$Message) {
    $failed = $false
    try { & $Action | Out-Null } catch { $failed = $true; Write-Host "Expected failure: $($_.Exception.Message)" }
    Assert $failed $Message
}
function Invoke-Dotnet { & dotnet @args; if ($LASTEXITCODE) { throw "dotnet failed: $args" } }
$projectRoot = Join-Path $Run 'AllFeatures'
$build = Join-Path $projectRoot 'build'
$temporary = Join-Path $Run 'tooling'
New-Item -ItemType Directory -Path $temporary -Force | Out-Null
# Build and package an owned NuGet dependency through the generated project.
$pluginProjectPath = Join-Path $projectRoot 'src/AllFeatures/AllFeatures.csproj'
[xml]$pluginProject = Get-Content $pluginProjectPath -Raw
[xml]$testProject = Get-Content (Join-Path $projectRoot 'tests/AllFeatures.Tests/AllFeatures.Tests.csproj') -Raw
if (!$pluginProject.SelectSingleNode("//PackageReference[@Include='Moq']")) {
    $dependencyReference = $pluginProject.CreateElement('PackageReference')
    $dependencyReference.SetAttribute('Include', 'Moq')
    $dependencyReference.SetAttribute('Version', $testProject.SelectSingleNode("//PackageReference[@Include='Moq']").Version)
    $pluginProject.Project.SelectSingleNode('ItemGroup').AppendChild($dependencyReference) | Out-Null
}
$pluginProject.Save($pluginProjectPath)
$targetsPath = Join-Path $build 'Plugin.targets'
[xml]$targets = Get-Content $targetsPath -Raw
foreach ($dll in @('Moq.dll','Castle.Core.dll')) {
    if ($targets.SelectSingleNode("//PluginPackageFile[@Destination='$dll']")) { continue }
    $file = $targets.CreateElement('PluginPackageFile')
    $file.SetAttribute('Include', '$(TargetDir)' + $dll)
    $file.SetAttribute('Destination', $dll)
    $targets.Project.SelectSingleNode('ItemGroup').AppendChild($file) | Out-Null
}
$targets.Save($targetsPath)
. "$build/Release-Settings.ps1"
foreach ($tag in @('V1.2.3.4','1.2.3.4','v1.2.3','v1.2.3.4.5','v01.2.3.4','v-1.2.3.4','v65535.0.0.0','v999999999999.0.0.0','v1.2.3.4;echo bad')) {
    Should-Fail { Get-ReleaseSettings $tag example/plugin } "Accepted malformed tag $tag"
}
foreach ($tag in @('v0.0.0.0','v65534.65534.65534.65534')) {
    Assert ((Get-ReleaseSettings $tag example/plugin).Version -eq $tag.Substring(1)) 'Valid version boundary rejected.'
}
Should-Fail { Get-ReleaseSettings v1.2.3.4 'example/plugin;echo' } 'Accepted unsafe repository.'
# Failed restore, build or test must prevent packaging/publication.
foreach ($failureStep in @('restore','build','test')) {
    & {
        $releaseCalls = [Collections.Generic.List[string]]::new()
        function dotnet {
            $releaseCalls.Add([string]$args[0])
            $global:LASTEXITCODE = if ($args[0] -eq $failureStep) { 17 } else { 0 }
        }
        Should-Fail { & "$build/Build-Release.ps1" -Tag v1.2.3.4 -Repository example/plugin } "A failed $failureStep did not stop release preparation."
        Assert (!$releaseCalls.Contains('msbuild')) 'Packaging ran after a failed release check.'
    }
}
# This is the same release entry point used by the generated workflow. No uploads occur here.
& "$build/Build-Release.ps1" -Tag v1.2.3.4 -Repository example/plugin
$release = Join-Path $projectRoot 'artifacts/release'
& "$build/Verify-ReleaseArtifacts.ps1" -Directory $release -Tag v1.2.3.4 -Repository example/plugin
$manifestPath = (Get-ChildItem $release -Filter *.manifest.json).FullName
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
[xml]$props = Get-Content "$projectRoot/Plugin.props"
Assert ($manifest.Identifier -eq $props.Project.PropertyGroup.PluginId) 'Wrong package identity.'
Assert ($manifest.Name -eq 'Verification Plugin') 'Wrong display name.'
Assert ($manifest.MinimumApplicationVersion.Build -eq 1060) 'Wrong minimum host version.'
Assert (!($manifest.PSObject.Properties.Name -contains 'Channel')) 'Release channel must use the official default.'
$packageArchive = [IO.Compression.ZipFile]::OpenRead((Get-ChildItem $release -Filter *.zip).FullName)
try {
    $names = ($packageArchive.Entries.FullName | Sort-Object) -join ','
    Assert ($names -eq 'AllFeatures.dll,AllFeatures.pdb,Castle.Core.dll,LICENSE.txt,Moq.dll') "Unexpected package dependencies: $names"
} finally { $packageArchive.Dispose() }

$stage = Join-Path $temporary 'stage'
$asset = Join-Path $temporary 'asset.txt'
$fileList = Join-Path $temporary 'files.txt'
Set-Content $asset 'Owned asset'
$assembly = Join-Path $projectRoot 'src/AllFeatures/bin/Release/net10.0-windows/AllFeatures.dll'
# Dependencies must come from the plugin's build output, including transitive ones.
$dependency = Join-Path $projectRoot 'src/AllFeatures/bin/Release/net10.0-windows/Moq.dll'
$transitiveDependency = Join-Path $projectRoot 'src/AllFeatures/bin/Release/net10.0-windows/Castle.Core.dll'
Assert (Test-Path $dependency) 'Owned NuGet dependency was not copied to the plugin output.'
Assert (Test-Path $transitiveDependency) 'Transitive NuGet dependency was not copied to the plugin output.'
@("$assembly|AllFeatures.dll", "$dependency|Moq.dll", "$asset|assets/nested.txt") | Set-Content $fileList
& "$build/Stage-Plugin.ps1" -FileList $fileList -Destination $stage
$destination = Join-Path $temporary 'custom-plugin-directory'
& "$build/Deploy-Plugin.ps1" -Stage $stage -AssemblyName AllFeatures.dll -Destination $destination
Assert ((Get-FileHash "$destination/Moq.dll").Hash -eq (Get-FileHash $dependency).Hash) 'Dependency deployment mismatch.'
Set-Content "$destination/unrelated.txt" 'Keep me'
Set-Content $asset 'Updated asset'
& "$build/Stage-Plugin.ps1" -FileList $fileList -Destination $stage
$lock = [IO.File]::Open("$destination/AllFeatures.dll", 'Open', 'Read', 'None')
try {
    Should-Fail { & "$build/Deploy-Plugin.ps1" -Stage $stage -AssemblyName AllFeatures.dll -Destination $destination } 'Locked file deployment succeeded.'
    Assert ((Get-Content "$destination/assets/nested.txt") -eq 'Owned asset') 'Deployment changed files before detecting the lock.'
} finally { $lock.Dispose() }
& "$build/Deploy-Plugin.ps1" -Stage $stage -AssemblyName AllFeatures.dll -Destination $destination
Assert ((Get-Content "$destination/unrelated.txt") -eq 'Keep me') 'Deployment removed unrelated files.'
Assert ((Get-Content "$destination/assets/nested.txt") -eq 'Updated asset') 'Deployment did not update the asset.'
foreach ($entry in @("$asset|../escape.txt", "$temporary/missing|missing.txt")) {
    Set-Content $fileList $entry
    Should-Fail { & "$build/Stage-Plugin.ps1" -FileList $fileList -Destination $stage } 'Invalid package entry was accepted.'
}
@("$asset|same.txt", "$asset|assets/../same.txt") | Set-Content $fileList
Should-Fail { & "$build/Stage-Plugin.ps1" -FileList $fileList -Destination $stage } 'Duplicate file accepted.'
Set-Content $fileList "$asset|file.txt"
Should-Fail { & "$build/Stage-Plugin.ps1" -FileList $fileList -Destination $destination } 'An unowned directory was cleared.'

$beta = Join-Path $temporary 'beta'
$url = 'https://github.com/example/plugin/releases/download/v1.2.3.4/AllFeatures.1.2.3.4.zip'
& "$build/Package-Plugin.ps1" -Stage $stage -AssemblyName AllFeatures.dll -OutputDirectory $beta -InstallerUrl $url -Channel Beta
& "$build/Verify-ReleaseArtifacts.ps1" -Directory $beta -Tag v1.2.3.4 -Repository example/plugin
$betaManifest = Get-Content (Get-ChildItem $beta -Filter *.manifest.json).FullName -Raw | ConvertFrom-Json
Assert ($betaManifest.Channel -eq 'Beta') 'Beta channel missing.'
$zip = [IO.Compression.ZipFile]::OpenRead((Get-ChildItem $beta -Filter *.zip).FullName)
try {
    $entries = @($zip.Entries.FullName | ForEach-Object { $_.Replace('\','/') })
    Assert ((($entries | Sort-Object) -join ',') -eq 'AllFeatures.dll,assets/nested.txt,Moq.dll') "Unexpected archive contents: $entries"
} finally { $zip.Dispose() }
$archive = (Get-ChildItem $beta -Filter *.zip).FullName
[IO.File]::AppendAllText($archive, 'tamper')
Should-Fail { & "$build/Verify-ReleaseArtifacts.ps1" -Directory $beta -Tag v1.2.3.4 -Repository example/plugin } 'Tampered archive passed validation.'
# A second packaging run must replace its own artifacts cleanly.
& "$build/Package-Plugin.ps1" -Stage $stage -AssemblyName AllFeatures.dll -OutputDirectory $beta -InstallerUrl $url -Channel Release
& "$build/Verify-ReleaseArtifacts.ps1" -Directory $beta -Tag v1.2.3.4 -Repository example/plugin

$metadata = Get-Content "$Run/expected-metadata.json" -Raw | ConvertFrom-Json
$escaped = Join-Path $Run $metadata.Name
$escapedProject = (Get-ChildItem "$escaped/src" -Filter *.csproj -Recurse).FullName
$optionalMetadata = @{
    Copyright='Copyright 2026 A & B "Example"'
    PluginChangelogUrl='https://example.com/changes?version=1&format=html'
    PluginFeaturedImageUrl='https://example.com/logo.png?width=128&height=128'
    PluginScreenshotUrl='https://example.com/screenshot.png?width=800&height=600'
    PluginAltScreenshotUrl='https://example.com/other.png?width=800&height=600'
}
[xml]$escapedProps = Get-Content "$escaped/Plugin.props" -Raw
foreach ($property in $optionalMetadata.GetEnumerator()) {
    $node = $escapedProps.SelectSingleNode("/Project/PropertyGroup/$($property.Key)")
    Assert ($null -ne $node) "Missing editable metadata property: $($property.Key)"
    $node.InnerText = $property.Value
}
$escapedProps.Save((Join-Path $escaped 'Plugin.props'))
Invoke-Dotnet msbuild $escapedProject '-t:PackagePlugin' '-p:Configuration=Debug' '-verbosity:minimal'
$escapedManifest = Get-Content (Get-ChildItem "$escaped/artifacts/release" -Filter *.manifest.json).FullName -Raw | ConvertFrom-Json
Assert ($escapedManifest.Name -ceq $metadata.DisplayName) 'Display name did not survive escaping.'
Assert ($escapedManifest.Author -ceq $metadata.Author) 'Author did not survive escaping.'
Assert (($escapedManifest.Descriptions.ShortDescription -replace "`r`n", "`n") -ceq ($metadata.Description -replace "`r`n", "`n")) 'Description did not survive escaping.'
Assert ($escapedManifest.Descriptions.LongDescription -ceq $metadata.LongDescription) 'Multiline description did not survive escaping.'
Assert ($escapedManifest.Homepage -eq 'https://example.com/?a=1&b=2') 'Homepage was not XML decoded.'
Assert (($escapedManifest.Tags -join ',') -eq 'one,two') 'Tags were not normalized.'
Assert ($escapedManifest.ChangelogURL -ceq $optionalMetadata.PluginChangelogUrl) 'Changelog URL did not reach the manifest.'
Assert ($escapedManifest.Descriptions.FeaturedImageURL -ceq $optionalMetadata.PluginFeaturedImageUrl) 'Featured image URL did not reach the manifest.'
Assert ($escapedManifest.Descriptions.ScreenshotURL -ceq $optionalMetadata.PluginScreenshotUrl) 'Screenshot URL did not reach the manifest.'
Assert ($escapedManifest.Descriptions.AltScreenshotURL -ceq $optionalMetadata.PluginAltScreenshotUrl) 'Alternate screenshot URL did not reach the manifest.'
$escapedAssembly = (Get-ChildItem "$escaped/artifacts/stage/Debug" -Filter *.dll).FullName
$fileMetadata = [Diagnostics.FileVersionInfo]::GetVersionInfo($escapedAssembly)
Assert ($fileMetadata.ProductName -ceq $metadata.DisplayName) 'Assembly product did not use the display name.'
Assert ($fileMetadata.LegalCopyright -ceq $optionalMetadata.Copyright) 'Copyright did not reach the assembly.'

# Exercise the actual submission script against a stateful fake gh function.
# It never launches gh or sends an HTTP request, even if credentials exist locally.
$expectedPluginId = $manifest.Identifier.ToLowerInvariant()
$fakeGitHubState = @{ Branch=$false; Content=''; Branches=0; Updates=0; PRs=0; State='OPEN' }
function gh {
    $global:LASTEXITCODE = 0
    $arguments = @($args)
    $command = $arguments -join ' '
    if ($command -match '^api repos/[^/]+/[^/]+ --jq .default_branch$') { return 'main' }
    if ($command -like '*git/matching-refs*') {
        if (!$fakeGitHubState.Branch) { return '[]' }
        return @(@{ref="refs/heads/plugin/$expectedPluginId/v1.2.3.4"}) | ConvertTo-Json -AsArray
    }
    if ($command -like '*git/ref/heads/main*') { return '0123456789abcdef' }
    if ($command -like 'api --method POST *git/refs*') { $fakeGitHubState.Branch=$true; $fakeGitHubState.Branches++; return '{}' }
    if ($command -like 'api --method PUT *contents*') {
        $body = Get-Content $arguments[-1] -Raw | ConvertFrom-Json
        $fakeGitHubState.Content=$body.content; $fakeGitHubState.Updates++; return '{}'
    }
    if ($command -like 'api repos/*/contents/*') {
        if (!$fakeGitHubState.Content) { $global:LASTEXITCODE=1; return '' }
        return @{content=$fakeGitHubState.Content;sha='contentsha'} | ConvertTo-Json
    }
    if ($command -like 'pr list *') {
        if (!$fakeGitHubState.PRs) { return '[]' }
        return @(@{url='https://github.com/example/manifests/pull/1';state=$fakeGitHubState.State}) | ConvertTo-Json -AsArray
    }
    if ($command -like 'pr create *') { $fakeGitHubState.PRs++; return 'https://github.com/example/manifests/pull/1' }
    throw "Unexpected fake GitHub request: $command"
}
$savedToken = $env:GH_TOKEN
try {
    $env:GH_TOKEN = ''
    $dry = & "$build/Submit-Manifest.ps1" -Manifest $manifestPath -Fork example/manifests -Tag v1.2.3.4 -DryRun
    Assert ($dry.Path -eq 'manifests/v/Verification Plugin/3.3.0.1060/manifest.1.2.3.4.json') 'Incorrect manifest path.'
    Should-Fail { & "$build/Submit-Manifest.ps1" -Manifest $manifestPath -Fork example/manifests -Tag v1.2.3.4 } 'Missing credentials were accepted.'
    $env:GH_TOKEN = 'fake-token-for-local-tests'
    foreach ($state in @('OPEN','OPEN','MERGED')) {
        $fakeGitHubState.State = $state
        & "$build/Submit-Manifest.ps1" -Manifest $manifestPath -Fork example/manifests -Tag v1.2.3.4
    }
    Assert ($fakeGitHubState.Branches -eq 1 -and $fakeGitHubState.Updates -eq 1 -and $fakeGitHubState.PRs -eq 1) 'Reruns created duplicate branches, commits or PRs.'
} finally { $env:GH_TOKEN=$savedToken }
Write-Host 'PASS: release validation, metadata escaping, deployment, packaging, both channels and idempotent manifest submission.'
