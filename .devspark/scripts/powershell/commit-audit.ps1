#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Build commit-history audit context for /devspark.commit-audit
# Usage:
#   .\commit-audit.ps1 [-Json] [-Scope full] [-Since YYYY-MM-DD] [-Until YYYY-MM-DD] [-Branch NAME]
#
# Emits a JSON document on stdout with anonymized contributor data,
# per-commit change stats, and monthly velocity aggregates. Contributor
# identities are replaced with role-based IDs (Contributor A = most
# commits) — real names and emails never appear in the output.

[CmdletBinding(PositionalBinding = $false)]
param(
    [switch]$Json,
    [string]$Scope = "full",
    [string]$Since = "",
    [string]$Until = "",
    [string]$Branch = "",
    [int]$MaxCommits = 1000,
    [switch]$Help,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Rest
)

if ($Help) {
    Write-Output @"
Usage: commit-audit.ps1 [-Json] [-Scope <name>] [-Since YYYY-MM-DD] [-Until YYYY-MM-DD] [-Branch NAME]

Emits commit-audit context JSON on stdout:
- repo_root, branch, total_commits, date_range
- contributors (anonymized role IDs with commit counts and shares)
- commits (sha, date, author_role, message, files_changed, insertions, deletions)
- monthly velocity aggregates and report_path

Options:
    -Json                Accepted for command-contract compatibility (output is always JSON)
    -Scope <name>        full | velocity | hygiene | dora | contributors | ai | architecture
    -Since YYYY-MM-DD    Limit analysis to commits after this date
    -Until YYYY-MM-DD    Limit analysis to commits before this date
    -Branch NAME         Analyze a specific branch (default: current branch)
    -MaxCommits <n>      Cap per-commit detail entries (default: 1000)

GNU-style flags forwarded from the command prompt (--scope=..., --since=...,
--until=..., --branch=..., --json) are also accepted.
"@
    exit 0
}

$ErrorActionPreference = 'Stop'

# Multi-app support
if (-not (Get-Command Detect-DevSparkMode -ErrorAction SilentlyContinue)) {
    . "$PSScriptRoot/common.ps1"
}

# The invoking prompt forwards raw user input, so GNU-style tokens arrive via
# $Rest — fold recognized ones into the typed parameters and ignore the rest.
foreach ($token in ($Rest ?? @())) {
    switch -Regex ($token) {
        '^--json$' { }
        '^--scope=(.+)$' { $Scope = $Matches[1] }
        '^--since=(.+)$' { $Since = $Matches[1] }
        '^--until=(.+)$' { $Until = $Matches[1] }
        '^--branch=(.+)$' { $Branch = $Matches[1] }
        '^--max-commits=(\d+)$' { $MaxCommits = [int]$Matches[1] }
        default { Write-Warning "ignoring unrecognized argument: $token" }
    }
}

git rev-parse --show-toplevel *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Not inside a git repository."
    exit 1
}
$repoRoot = (git rev-parse --show-toplevel).Trim()
Set-Location $repoRoot

if (-not $Branch) {
    $Branch = (git rev-parse --abbrev-ref HEAD).Trim()
}
git rev-parse --verify --quiet $Branch *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Error "branch not found: $Branch"
    exit 1
}

$logArgs = @($Branch, '--date=short', '--numstat', '--format=@%H%x09%ad%x09%an%x09%s')
if ($Since) { $logArgs += "--since=$Since" }
if ($Until) { $logArgs += "--until=$Until" }
$logLines = git log @logArgs

$commits = [System.Collections.Generic.List[hashtable]]::new()
$current = $null
foreach ($raw in ($logLines ?? @())) {
    if ($raw.StartsWith('@')) {
        $parts = $raw.Substring(1).Split("`t", 4)
        $current = @{
            sha           = $parts[0]
            date          = if ($parts.Count -gt 1) { $parts[1] } else { "" }
            author        = if ($parts.Count -gt 2) { $parts[2] } else { "" }
            message       = if ($parts.Count -gt 3) { $parts[3] } else { "" }
            files_changed = 0
            insertions    = 0
            deletions     = 0
        }
        $commits.Add($current)
    }
    elseif ($raw.Trim() -and $null -ne $current) {
        $cols = $raw.Split("`t")
        if ($cols.Count -ge 3) {
            $current.files_changed += 1
            if ($cols[0] -match '^\d+$') { $current.insertions += [int]$cols[0] }
            if ($cols[1] -match '^\d+$') { $current.deletions += [int]$cols[1] }
        }
    }
}

# Anonymize: order authors by commit count desc, then Contributor A, B, ...
$counts = @{}
foreach ($c in $commits) {
    $counts[$c.author] = ($counts[$c.author] ?? 0) + 1
}
$ordered = $counts.GetEnumerator() | Sort-Object -Property @{Expression = { $_.Value }; Descending = $true }, @{Expression = { $_.Key }; Descending = $false }

function Get-RoleLabel([int]$Index) {
    $label = ""
    $n = $Index + 1
    while ($n -gt 0) {
        $rem = ($n - 1) % 26
        $n = [math]::Floor(($n - 1) / 26)
        $label = [char]([int][char]'A' + $rem) + $label
    }
    return "Contributor $label"
}

$roleByAuthor = @{}
$i = 0
foreach ($entry in $ordered) {
    $roleByAuthor[$entry.Key] = Get-RoleLabel $i
    $i += 1
}
$total = $commits.Count

$contributors = @()
foreach ($entry in $ordered) {
    $authorCommits = @($commits | Where-Object { $_.author -eq $entry.Key })
    $dates = @($authorCommits | ForEach-Object { $_.date }) | Sort-Object
    $sharePct = if ($total) { [math]::Round(100.0 * $entry.Value / $total, 1) } else { 0.0 }
    $contributors += [ordered]@{
        role         = $roleByAuthor[$entry.Key]
        commits      = $entry.Value
        share_pct    = $sharePct
        first_commit = $dates[0]
        last_commit  = $dates[-1]
    }
}

$monthly = [ordered]@{}
foreach ($c in ($commits | Sort-Object -Property @{Expression = { $_.date } })) {
    $month = $c.date.Substring(0, [math]::Min(7, $c.date.Length))
    if (-not $monthly.Contains($month)) {
        $monthly[$month] = [ordered]@{ commits = 0; files_changed = 0; insertions = 0; deletions = 0 }
    }
    $monthly[$month].commits += 1
    $monthly[$month].files_changed += $c.files_changed
    $monthly[$month].insertions += $c.insertions
    $monthly[$month].deletions += $c.deletions
}

$detail = @()
foreach ($c in ($commits | Select-Object -First $MaxCommits)) {
    $detail += [ordered]@{
        sha           = $c.sha
        date          = $c.date
        author_role   = $roleByAuthor[$c.author]
        message       = $c.message
        files_changed = $c.files_changed
        insertions    = $c.insertions
        deletions     = $c.deletions
    }
}

$dates = @($commits | ForEach-Object { $_.date }) | Sort-Object
$reportPath = ".devspark.work/audit/commit-audit-$((Get-Date).ToUniversalTime().ToString('yyyy-MM-dd')).md"

$result = [ordered]@{
    generated_at     = (Get-Date).ToUniversalTime().ToString("o")
    audit_parameters = [ordered]@{
        scope       = $Scope
        since       = if ($Since) { $Since } else { $null }
        until       = if ($Until) { $Until } else { $null }
        max_commits = $MaxCommits
    }
    repo_root         = $repoRoot
    branch            = $Branch
    total_commits     = $total
    date_range        = [ordered]@{
        first = if ($dates.Count) { $dates[0] } else { $null }
        last  = if ($dates.Count) { $dates[-1] } else { $null }
    }
    contributors      = $contributors
    commits           = $detail
    commits_truncated = ($total -gt $MaxCommits)
    monthly_velocity  = $monthly
    report_path       = $reportPath
}

$result | ConvertTo-Json -Depth 6
