#requires -Version 7.0
[CmdletBinding()]
param([switch]$Check)

$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$sourcePath = Join-Path $repo 'doc/使用手册.md'
$templatePath = Join-Path $PSScriptRoot 'docs/template.html'
$outputPath = Join-Path $repo 'docs/index.html'
$source = [IO.File]::ReadAllText($sourcePath).Replace("`r`n", "`n")
# The web navigation replaces the Markdown's own table of contents.
$source = [regex]::Replace($source, '(?ms)^## 目录\n.*?(?=^## )', '')
$html = (ConvertFrom-Markdown -InputObject $source).Html.Replace("`r`n", "`n")
$html = [regex]::Replace($html, '(?s)^<h1\b.*?</h1>\s*', '')
$headings = [Collections.Generic.List[object]]::new()
$usedIds = @{}
$html = [regex]::Replace($html, '<h([2-4])(?:\s[^>]*)?>(.*?)</h\1>', {
    param($match)
    $level = [int]$match.Groups[1].Value
    $label = [Net.WebUtility]::HtmlDecode([regex]::Replace($match.Groups[2].Value, '<[^>]+>', ''))
    # Unlike the Markdown converter's ASCII-only IDs, retain Chinese title text.
    $id = [regex]::Replace($label.ToLowerInvariant(), '[^\p{L}\p{Nd}\s-]', '')
    $id = [regex]::Replace($id.Trim(), '\s+', '-')
    if (!$id) { throw "无法为标题生成链接：$label" }
    $baseId = $id
    $suffix = 1
    while ($usedIds.ContainsKey($id)) { $id = "$baseId-$suffix"; $suffix++ }
    $usedIds[$id] = $true
    $headings.Add(@{ Level = $level; Label = $label; Id = $id })
    $safeLabel = [Net.WebUtility]::HtmlEncode("链接到：$label")
    return "<h$level id=`"$id`">$($match.Groups[2].Value)<a class=`"heading-link`" href=`"#$id`" aria-label=`"$safeLabel`">#</a></h$level>"
})

# Source references work from file:// and when docs/ is hosted on its own.
$html = [regex]::Replace($html, 'href="\.\./([^"]+)"', {
    param($match)
    $relative = [Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
    $local = [IO.Path]::GetFullPath((Join-Path $repo $relative))
    if (!$local.StartsWith($repo + [IO.Path]::DirectorySeparatorChar) -or !(Test-Path -LiteralPath $local -PathType Leaf)) {
        throw "源码链接不存在或越界：$relative"
    }
    $url = 'https://github.com/RobertCXC/SUI/blob/main/' + ((($relative -split '/') | ForEach-Object { [Uri]::EscapeDataString($_) }) -join '/')
    return 'href="' + [Net.WebUtility]::HtmlEncode($url) + '"'
})

$html = [regex]::Replace($html, '(?s)<table>(.*?)</table>', '<div class="table-scroll" role="region" aria-label="参考表格，可横向滚动" tabindex="0"><table>$1</table></div>')
$html = [regex]::Replace($html, '(?s)<pre><code(?: class="language-([^"]+)")?>(.*?)</code></pre>', {
    param($match)
    $language = $match.Groups[1].Value
    if (!$language) { $language = 'text' }
    $safeLanguage = [Net.WebUtility]::HtmlEncode($language)
    return "<div class=`"code-block`"><div class=`"code-toolbar`"><span>$safeLanguage</span><button type=`"button`" class=`"copy-button`" hidden>复制代码</button></div><pre tabindex=`"0`" aria-label=`"$safeLanguage 代码示例`"><code class=`"language-$safeLanguage`">$($match.Groups[2].Value)</code></pre></div>"
})

$sections = [regex]::Split($html, '(?=<h2\b)')
$body = '<div class="manual-intro">' + $sections[0] + '</div>'
for ($i = 1; $i -lt $sections.Length; $i++) {
    $heading = $headings.Where({ $_.Level -eq 2 })[$i - 1]
    $number = '{0:d2}' -f $i
    $body += "`n<section class=`"doc-section`" aria-labelledby=`"$($heading.Id)`"><div class=`"section-number`">$number / SUI 手册</div>$($sections[$i])</section>"
}

$nav = '<a class="nav-link overview-link" href="#概览">概览<span>开始阅读</span></a>'
$groupStarts = @{
    '快速开始' = '开始使用'
    'suicard-卡片控件' = '控件与样式'
    '转换器' = '参考与支持'
}
foreach ($heading in $headings.Where({ $_.Level -eq 2 })) {
    if ($groupStarts.ContainsKey($heading.Id)) { $nav += '<p class="nav-group">' + $groupStarts[$heading.Id] + '</p>' }
    $nav += '<a class="nav-link" href="#' + $heading.Id + '">' + [Net.WebUtility]::HtmlEncode($heading.Label) + '</a>'
}

$template = [IO.File]::ReadAllText($templatePath).Replace("`r`n", "`n")
$result = $template.Replace('<!-- DOC_NAV -->', $nav).Replace('<!-- DOC_CONTENT -->', $body)
# Validate all local links, resources, unique IDs, and generation markers before writing.
$ids = @{}
foreach ($match in [regex]::Matches($result, '\sid="([^"]+)"')) {
    $id = $match.Groups[1].Value
    if ($ids.ContainsKey($id)) { throw "HTML 包含重复 id：$id" }
    $ids[$id] = $true
}
foreach ($match in [regex]::Matches($result, '(?:href|src)="([^"]+)"')) {
    $value = [Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
    if ($value.StartsWith('#')) {
        if (!$ids.ContainsKey($value.Substring(1))) { throw "章节链接不存在：$value" }
    } elseif ($value -notmatch '^[a-z]+:') {
        $path = Join-Path (Split-Path $outputPath -Parent) $value
        if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "本地资源不存在：$value" }
    }
}
if ($result.Contains('<!-- DOC_')) { throw 'HTML 模板仍有未替换的内容标记。' }
if ($Check) {
    if (!(Test-Path -LiteralPath $outputPath) -or [IO.File]::ReadAllText($outputPath).Replace("`r`n", "`n") -cne $result) {
        throw '网页说明书尚未更新。请执行 pwsh -File scripts/Build-Docs.ps1 后提交生成的 docs/index.html。'
    }
    Write-Host "说明书检查通过：$($headings.Where({ $_.Level -eq 2 }).Count) 个章节，链接与本地资源有效。"
} else {
    [IO.Directory]::CreateDirectory((Split-Path $outputPath -Parent)) | Out-Null
    [IO.File]::WriteAllText($outputPath, $result, [Text.UTF8Encoding]::new($false))
    Write-Host "已生成 $outputPath"
}
