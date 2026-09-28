# Dot-source this helper to validate inputs without building or contacting GitHub.
function Get-ReleaseSettings {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Tag, [Parameter(Mandatory)][string]$Repository, [ValidateSet('Release','Beta')][string]$Channel = 'Release')
    if ($Tag -cnotmatch '^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$') {
        throw 'Release tag must be vMAJOR.MINOR.PATCH.BUILD, for example v1.2.3.4.'
    }
    $version = $Tag.Substring(1)
    foreach ($part in $version.Split('.')) {
        $number = 0
        if (![int]::TryParse($part, [ref]$number) -or $number -gt 65534) { throw 'Each assembly version component must be between 0 and 65534.' }
    }
    if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') { throw 'Repository must be owner/repository.' }
    [pscustomobject]@{ Tag=$Tag; Version=$version; Repository=$Repository; Channel=$Channel }
}
