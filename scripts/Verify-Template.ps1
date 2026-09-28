[CmdletBinding()]
param([switch]$Quick, [ValidateRange(1,8)][int]$Parallelism = 3)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path $PSScriptRoot
Set-Location $root
function Invoke-Dotnet { & dotnet @args; if ($LASTEXITCODE) { throw "dotnet failed: $args" } }
$run = Join-Path $root ('artifacts/verification-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $run -Force | Out-Null
Set-Content artifacts/latest-verification.txt $run
Invoke-Dotnet pack NINA.Plugin.Templates.csproj --nologo --verbosity quiet
$package = Get-ChildItem artifacts/packages/*.nupkg | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$hive = Join-Path $run 'hive'
Invoke-Dotnet new install $package.FullName '--debug:custom-hive' $hive
$features = @('instruction','condition','trigger','container','expressions','sequence-upgrader','settings','dock-panel','image-hooks','weather-provider','messaging','symbols','star-annotation','editor-integration')
$cases = [Collections.Generic.List[object]]::new()
function Add-Case($Name, $Features, $Options = @(), $Tests = $true, $CI = $false, $Submission = $false) {
    $cases.Add([pscustomobject]@{ Name=$Name; Features=@($Features); Options=@($Options); Tests=$Tests; CI=$CI; Submission=$Submission; Path=(Join-Path $run $Name) })
}
Add-Case 'Starter' @()
foreach ($feature in $features) { Add-Case ('Single_' + $feature.Replace('-','_')) @($feature) }
Add-Case 'AllFeatures' $features @('--manifest-pr','--displayName','Verification Plugin','--plugin-author','Template Maintainers','--repository','https://github.com/example/plugin') -CI $true -Submission $true
Add-Case 'NoTests' @() @('--tests','false') -Tests $false
Add-Case 'CIWithoutTests' $features @('--tests','false','--github-actions') -Tests $false -CI $true
Add-Case 'CIWithTests' @() @('--github-actions') -CI $true
Add-Case 'SubmissionWithoutTests' @() @('--tests','false','--manifest-pr') -Tests $false -CI $true -Submission $true
# These values must survive template replacement, MSBuild, generated C# and JSON.
$metadata = @{
    Name='Escaped Plugin'
    DisplayName='Nebula "星" & <sky>'
    Author='A;B $(NotAProperty) @(NotAnItem) 100% "C"'
    Description="First line & <xml>`nSecond line 'quoted'"
    LongDescription="Long description`r`nUnicode: 星 é"
}
Add-Case $metadata.Name @('settings','editor-integration') @('--namespace','Observatory.星.Plugin','--displayName',$metadata.DisplayName,'--plugin-author',$metadata.Author,'--description',$metadata.Description,'--longDescription',$metadata.LongDescription,'--repository','https://github.com/example/plugin','--homepage','https://example.com/?a=1&b=2','--tags','one, two')
$metadata | ConvertTo-Json | Set-Content (Join-Path $run 'expected-metadata.json')
if (!$Quick) {
    for ($i=0; $i -lt $features.Count; $i++) {
        for ($j=$i+1; $j -lt $features.Count; $j++) {
            Add-Case ("Pair_${i}_${j}") @($features[$i],$features[$j])
        }
    }
}
$ids = [Collections.Generic.HashSet[string]]::new()
foreach ($case in $cases) {
    $arguments = @('new','nina-plugin','--debug:custom-hive',$hive,'-n',$case.Name,'-o',$case.Path,'--no-restore')
    foreach ($feature in $case.Features) { $arguments += "--$feature" }
    & dotnet @arguments @($case.Options) *> (Join-Path $run ($case.Name + '.generation.log'))
    if ($LASTEXITCODE) { throw "Generation failed for $($case.Name). See $run." }
    if ((Test-Path (Join-Path $case.Path 'tests')) -ne $case.Tests) { throw "Incorrect tests selection: $($case.Name)" }
    if ((Test-Path (Join-Path $case.Path '.github')) -ne $case.CI) { throw "Incorrect CI selection: $($case.Name)" }
    if ((Test-Path (Join-Path $case.Path 'build/Submit-Manifest.ps1')) -ne $case.Submission) { throw "Incorrect submission selection: $($case.Name)" }
    [xml]$props = Get-Content (Join-Path $case.Path 'Plugin.props') -Raw
    $id = [guid]::Parse($props.Project.PropertyGroup.PluginId).ToString()
    if (!$ids.Add($id)) { throw "Duplicate generated GUID: $id" }
    $readme = Get-Content (Join-Path $case.Path 'README.md') -Raw
    foreach ($feature in $features) {
        $directory = $feature.Replace('-','_')
        $found = @(Get-ChildItem (Join-Path $case.Path 'src') -Directory -Recurse | Where-Object Name -eq $directory).Count -gt 0
        if ($found -ne ($feature -in $case.Features)) { throw "Incorrect feature selection $feature in $($case.Name)" }
        if ($readme.Contains("**${feature}**") -ne $found) { throw "Incorrect README selection $feature in $($case.Name)" }
    }
    $textFiles = Get-ChildItem $case.Path -Recurse -File -Force | Where-Object Extension -in '.cs','.csproj','.props','.xaml','.md','.yml','.slnx'
    foreach ($file in $textFiles) {
        if ((Get-Content $file.FullName -Raw) -match '(?m)TemplateNamespace|TemplateAssembly|META_[A-Z]+|<!--#|^#(?:if|endif)') { throw "Unresolved token in $($file.FullName)" }
    }
}
Write-Host "Generated $($cases.Count) configurations with unique identities. Building and testing with $Parallelism workers."
$results = @($cases | ForEach-Object -ThrottleLimit $Parallelism -Parallel {
    $case = $_
    $solution = (Get-ChildItem $case.Path -Filter *.slnx).FullName
    $log = Join-Path $case.Path 'verification.log'
    if ($case.Tests) {
        & dotnet test $solution --nologo --verbosity minimal --logger 'trx;LogFileName=results.trx' --results-directory (Join-Path $case.Path 'results') *> $log
    } else {
        & dotnet build $solution --nologo --verbosity minimal *> $log
    }
    $code = $LASTEXITCODE
    $count = 0
    if ($code -eq 0 -and $case.Tests) {
        [xml]$trx = Get-Content (Join-Path $case.Path 'results/results.trx')
        $count = [int]$trx.TestRun.ResultSummary.Counters.passed
    }
    Write-Host "$($case.Name): exit $code, $count tests"
    [pscustomobject]@{ Name=$case.Name; ExitCode=$code; Tests=$count; Log=$log }
})
$results | ConvertTo-Json | Set-Content (Join-Path $run 'results.json')
if (@($results | Where-Object ExitCode -ne 0).Count) { throw "Matrix failures. Inspect $run/results.json and per-project verification.log files." }
& "$PSScriptRoot/Test-Tooling.ps1" -Run $run
Write-Host "PASS: $($cases.Count) generated configurations, $(($results | Measure-Object Tests -Sum).Sum) test executions. Results: $run"

