[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path $PSScriptRoot
Set-Location $root
$files = @(Get-ChildItem scripts,templates/nina-plugin/build -Filter *.ps1 -Recurse)
foreach ($file in $files) {
    $errors = $null
    [Management.Automation.Language.Parser]::ParseFile($file.FullName, [ref]$null, [ref]$errors) | Out-Null
    if ($errors.Count) { throw "PowerShell syntax errors in $($file.FullName): $errors" }
}
$markdown = @(Get-ChildItem docs -Filter *.md) + @(Get-Item README.md)
foreach ($file in $markdown) {
    foreach ($match in [regex]::Matches((Get-Content $file.FullName -Raw), '\[[^\]]+\]\(([^)]+)\)')) {
        $link = $match.Groups[1].Value.Split('#')[0]
        if (!$link -or $link -match '^[a-z]+:') { continue }
        if (!(Test-Path -LiteralPath (Join-Path $file.DirectoryName $link))) { throw "Broken handbook link: $($file.Name) -> $link" }
    }
}
# Download a fixed upstream release and verify its archive before executing it.
$toolDirectory = Join-Path $root 'artifacts/tools/actionlint-1.7.12'
$executable = Join-Path $toolDirectory 'actionlint.exe'
$archive = Join-Path $root 'artifacts/tools/actionlint-1.7.12.zip'
New-Item -ItemType Directory -Path (Split-Path $archive) -Force | Out-Null
if (!(Test-Path -LiteralPath $archive)) {
    Invoke-WebRequest 'https://github.com/rhysd/actionlint/releases/download/v1.7.12/actionlint_1.7.12_windows_amd64.zip' -OutFile $archive
}
if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne '6E7241B51E6817EA6A047693D8E6FED13B31819C9A0DD6C5A726E1592D22F6E9') { throw 'actionlint checksum mismatch.' }
Expand-Archive -LiteralPath $archive -DestinationPath $toolDirectory -Force
# Regenerate workflow variants from the final package, even for a workflow-only edit.
function Invoke-Dotnet { & dotnet @args; if ($LASTEXITCODE) { throw "dotnet failed: $args" } }
Invoke-Dotnet pack NINA.Plugin.Templates.csproj --nologo --verbosity quiet
$package = Get-ChildItem artifacts/packages/*.nupkg | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$workflowRun = Join-Path $root ('artifacts/workflow-verification-' + [guid]::NewGuid().ToString('N'))
$hive = Join-Path $workflowRun 'hive'
Invoke-Dotnet new install $package.FullName '--debug:custom-hive' $hive
$workflows = @(Get-ChildItem .github/workflows -Filter *.yml)
foreach ($submission in @($false,$true)) {
    foreach ($tests in @($false,$true)) {
        $case = Join-Path $workflowRun "Submission-$submission-Tests-$tests"
        $options = @('--github-actions','--tests',$tests.ToString().ToLowerInvariant())
        if ($submission) { $options += '--manifest-pr' }
        Invoke-Dotnet new nina-plugin '--debug:custom-hive' $hive -n WorkflowCheck -o $case --no-restore @options
        $release = Get-Content (Join-Path $case '.github/workflows/release.yml') -Raw
        if (($release -match '(?m)^  manifest:') -ne $submission) { throw 'Incorrect conditional manifest job.' }
        $workflows += @(Get-ChildItem (Join-Path $case '.github/workflows') -Filter *.yml)
    }
}
& $executable '-shellcheck=' '-pyflakes=' @($workflows.FullName)
if ($LASTEXITCODE) { throw 'Workflow lint failed.' }
foreach ($workflow in $workflows) {
    $text = Get-Content $workflow.FullName -Raw
    foreach ($match in [regex]::Matches($text, 'uses: ([^\r\n]+)')) {
        if ($match.Groups[1].Value -match '^\./\.github/workflows/[a-z-]+\.yml$') {
            if (!(Test-Path -LiteralPath $match.Groups[1].Value)) { throw 'Missing reusable workflow.' }
            continue
        }
        if ($match.Groups[1].Value -notmatch '^[\w-]+/[\w-]+@[a-f0-9]{40} # v[0-9.]+$') { throw "Unpinned action in $($workflow.FullName)" }
    }
    if ($text -match '(?m)^#(?:if|endif)') { throw "Unresolved workflow condition: $($workflow.FullName)" }
}
Write-Host "PASS: $($files.Count) PowerShell files, local handbook links and $($workflows.Count) workflows."
