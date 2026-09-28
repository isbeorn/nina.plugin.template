[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Manifest,
    [Parameter(Mandatory)][string]$Fork,
    [Parameter(Mandatory)][string]$Tag,
    [string]$TargetRepository = 'isbeorn/nina.plugin.manifests',
    [switch]$DryRun
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. "$PSScriptRoot/Release-Settings.ps1"
$settings = Get-ReleaseSettings -Tag $Tag -Repository $Fork
if ($TargetRepository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') { throw 'Invalid target repository.' }
$manifestText = Get-Content -LiteralPath $Manifest -Raw
if (!(Test-Json -Json $manifestText -SchemaFile "$PSScriptRoot/vendor/manifest.schema.json")) { throw 'Invalid manifest.' }
$data = $manifestText | ConvertFrom-Json
$id = [guid]::Parse($data.Identifier).ToString('D')
$version = "$($data.Version.Major).$($data.Version.Minor).$($data.Version.Patch).$($data.Version.Build)"
if ($version -ne $settings.Version) { throw 'Manifest version does not match the release tag.' }
$name = $data.Name.Replace('\', '-').Replace('/', '-') -replace '[<>:"|?*\x00-\x1f]', '_'
if ([string]::IsNullOrWhiteSpace($name) -or $name -in @('.', '..')) { throw 'Invalid plugin name for manifest path.' }
$letter = $name.Substring(0,1).ToLowerInvariant()
$minimum = "$($data.MinimumApplicationVersion.Major).$($data.MinimumApplicationVersion.Minor).$($data.MinimumApplicationVersion.Patch).$($data.MinimumApplicationVersion.Build)"
$relativePath = "manifests/$letter/$name/$minimum/manifest.$version.json"
$branch = "plugin/$id/$Tag"
if ($DryRun) {
    [pscustomobject]@{ Fork=$Fork; Target=$TargetRepository; Branch=$branch; Path=$relativePath }
    return
}
if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN)) { throw 'Set MANIFEST_TOKEN and the MANIFEST_FORK repository variable to enable manifest PR submission.' }
function Invoke-Gh([string[]]$Arguments) {
    $result = & gh @Arguments
    if ($LASTEXITCODE) { throw "GitHub request failed (exit $LASTEXITCODE)." }
    return $result
}
# GitHub's API avoids tokens in clone URLs and force-pushing release branches.
$forkBase = Invoke-Gh @('api', "repos/$Fork", '--jq', '.default_branch')
$targetBase = Invoke-Gh @('api', "repos/$TargetRepository", '--jq', '.default_branch')
$forkOwner = $Fork.Split('/')[0]
$heads = Invoke-Gh @('api', "repos/$Fork/git/matching-refs/heads/$branch") | ConvertFrom-Json
if (!(@($heads) | Where-Object ref -eq "refs/heads/$branch")) {
    $baseSha = Invoke-Gh @('api', "repos/$Fork/git/ref/heads/$forkBase", '--jq', '.object.sha')
    Invoke-Gh @('api', '--method', 'POST', "repos/$Fork/git/refs", '-f', "ref=refs/heads/$branch", '-f', "sha=$baseSha") | Out-Null
}
$encodedPath = ($relativePath.Split('/') | ForEach-Object { [uri]::EscapeDataString($_) }) -join '/'
$existingResult = & gh api "repos/$Fork/contents/$encodedPath`?ref=$([uri]::EscapeDataString($branch))" 2>$null
$existing = if ($LASTEXITCODE -eq 0) { $existingResult | ConvertFrom-Json } else { $null }
$content = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($manifestText))
$existingContent = if ($existing) { $existing.content -replace '\s','' } else { '' }
if ($existingContent -ne $content) {
    $body = @{ message="Update $($data.Name) $version"; branch=$branch; content=$content }
    if ($existing) { $body.sha = $existing.sha }
    $temporary = [IO.Path]::GetTempFileName()
    try {
        $body | ConvertTo-Json | Set-Content -LiteralPath $temporary -Encoding utf8
        Invoke-Gh @('api', '--method', 'PUT', "repos/$Fork/contents/$encodedPath", '--input', $temporary) | Out-Null
    } finally { Remove-Item -LiteralPath $temporary }
}
$prs = Invoke-Gh @('pr','list','--repo',$TargetRepository,'--head',"${forkOwner}:$branch",'--state','all','--json','url,state') | ConvertFrom-Json
if (@($prs).Count -gt 0) { Write-Host "Manifest PR already exists: $($prs[0].url)"; return }
Invoke-Gh @('pr','create','--repo',$TargetRepository,'--head',"${forkOwner}:$branch",'--base',$targetBase,'--title',"$($data.Name) $version",'--body',"Manifest for published release $Tag. Generated from the released archive.")
