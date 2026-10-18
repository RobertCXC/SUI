param(
    [Parameter(Mandatory)][string]$Tag,
    [string]$OutputPath = 'release-notes.md'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandArgumentPassing = 'Standard'
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [Console]::OutputEncoding

function Invoke-Git {
    param([string[]]$Arguments)
    $result = & git @Arguments
    if ($LASTEXITCODE -ne 0) { throw "git $($Arguments -join ' ') failed ($LASTEXITCODE)." }
    return $result
}

# Resolve the tag to a commit so tag names are never interpreted as log options.
$commit = Invoke-Git @('rev-parse', '--verify', "refs/tags/${Tag}^{commit}")
$parents = (Invoke-Git @('rev-list', '--parents', '-n', '1', $commit)) -split ' '
$previous = $null
if ($parents.Count -gt 1) {
    # Starting at the parent excludes this tag (including aliases on this commit).
    $previous = & git describe --tags --abbrev=0 $parents[1] 2>$null
    if ($LASTEXITCODE -ne 0) { $previous = $null }
}

$range = if ($previous) { "$previous..$commit" } else { $commit }
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# $Tag 变动日志")
$lines.Add('')
if ($previous) {
    $lines.Add("提交范围：$previous → $Tag（不含前一标签，包含当前标签）。")
} else {
    $lines.Add('首个标签：包含截至当前标签的全部提交。')
}
$lines.Add('')
# Enumerate every commit, including merges, then read each complete message separately.
$commits = @(Invoke-Git @('log', '--reverse', '--format=%H', $range, '--'))
foreach ($id in $commits) {
    $subject = Invoke-Git @('show', '-s', '--format=%s', $id, '--')
    $body = @(Invoke-Git @('show', '-s', '--format=%b', $id, '--'))
    $lines.Add("## $subject")
    $lines.Add('')
    $lines.Add(('提交：`{0}`' -f $id))
    $lines.Add('')
    if ($body.Count -gt 0) {
        $lines.Add(($body -join "`n").TrimEnd())
        $lines.Add('')
    }
}
[System.IO.File]::WriteAllText($ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath), ($lines -join "`n") + "`n", [System.Text.UTF8Encoding]::new($false))
Write-Host "已生成 $OutputPath：$($commits.Count) 条提交。"
