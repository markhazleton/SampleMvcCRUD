#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
#requires -Version 7.0
# Helper script for /devspark.merge-from-main
# Default: fetch <Remote>/<Base> and merge it into the current branch.
# -Continue: verify conflict resolution, stage resolved paths, and commit the merge.
# -Abort: abort an in-progress merge.
# Exit codes: 0 done, 2 needs agent/developer action, 1 error.

param(
    [string]$Base = 'main',
    [string]$Remote = 'origin',
    [switch]$Continue,
    [switch]$Abort,
    [switch]$Push,
    [switch]$Json
)

$ErrorActionPreference = 'Stop'

$result = [ordered]@{
    status    = ''
    branch    = ''
    base      = "$Remote/$Base"
    behind    = 0
    ahead     = 0
    files     = 0
    pushed    = $false
    conflicts = @()
    markers   = @()
    dirty     = @()
    message   = ''
}

function Complete-Run {
    param([string]$Status, [int]$Code, [string]$Message = '')
    $result.status = $Status
    if ($Message) { $result.message = $Message }
    if ($Json) {
        $result | ConvertTo-Json -Depth 5 -Compress
    }
    else {
        Write-Output "STATUS: $($result.status)"
        Write-Output "BRANCH: $($result.branch)  BASE: $($result.base)  BEHIND: $($result.behind)  AHEAD: $($result.ahead)"
        if ($result.files) { Write-Output "FILES_CHANGED: $($result.files)" }
        if ($result.pushed) { Write-Output 'PUSHED: true' }
        if ($result.conflicts.Count) {
            Write-Output 'CONFLICTS:'
            foreach ($c in $result.conflicts) {
                $hunks = if ($c.hunks.Count) { ' ' + ($c.hunks -join ',') } else { '' }
                Write-Output "  $($c.state) $($c.path)$hunks"
            }
        }
        if ($result.markers.Count) {
            Write-Output 'MARKERS_REMAIN:'
            $result.markers | ForEach-Object { Write-Output "  $_" }
        }
        if ($result.dirty.Count) {
            Write-Output 'DIRTY:'
            $result.dirty | ForEach-Object { Write-Output "  $_" }
        }
        if ($result.message) { Write-Output "MESSAGE: $($result.message)" }
    }
    exit $Code
}

function Invoke-Git {
    $out = & git @args 2>&1 | ForEach-Object { "$_" }
    return [pscustomobject]@{ Code = $LASTEXITCODE; Output = @($out) }
}

function Test-MergeInProgress {
    & git rev-parse -q --verify MERGE_HEAD *> $null
    return $LASTEXITCODE -eq 0
}

function Get-UnmergedPaths {
    return @(git diff --name-only --diff-filter=U 2>$null | Where-Object { $_ })
}

function Get-ConflictReport {
    $states = @{}
    foreach ($line in @(git status --porcelain=v1 --untracked-files=no 2>$null)) {
        if ($line.Length -gt 3) { $states[$line.Substring(3)] = $line.Substring(0, 2) }
    }
    $report = @()
    foreach ($path in Get-UnmergedPaths) {
        $hunks = @()
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            $start = 0
            $n = 0
            foreach ($text in [System.IO.File]::ReadLines((Resolve-Path -LiteralPath $path).Path)) {
                $n++
                if ($text.StartsWith('<<<<<<< ') -or $text -eq '<<<<<<<') { $start = $n }
                elseif ($start -and ($text.StartsWith('>>>>>>> ') -or $text -eq '>>>>>>>')) {
                    $hunks += "L$start-$n"
                    $start = 0
                }
            }
        }
        $state = if ($states.ContainsKey($path)) { $states[$path] } else { 'UU' }
        $report += [ordered]@{ path = $path; state = $state; hunks = $hunks }
    }
    return $report
}

function Invoke-Push {
    & git rev-parse --abbrev-ref --symbolic-full-name '@{u}' *> $null
    $push = if ($LASTEXITCODE -eq 0) { Invoke-Git push --quiet } else { Invoke-Git push --quiet -u $Remote $result.branch }
    if ($push.Code -ne 0) { Complete-Run 'error' 1 ("push failed: " + ($push.Output -join ' ')) }
    $result.pushed = $true
}

& git rev-parse --is-inside-work-tree *> $null
if ($LASTEXITCODE -ne 0) { Complete-Run 'error' 1 'not inside a git repository' }
Set-Location (git rev-parse --show-toplevel)

$result.branch = "$(git symbolic-ref --short -q HEAD)"
if (-not $result.branch) { Complete-Run 'blocked' 2 'detached HEAD; check out a branch first' }

if ($Abort) {
    if (-not (Test-MergeInProgress)) { Complete-Run 'blocked' 2 'no merge in progress' }
    $abortRun = Invoke-Git merge --abort
    if ($abortRun.Code -ne 0) { Complete-Run 'error' 1 ($abortRun.Output -join ' ') }
    Complete-Run 'aborted' 0
}

if ($Continue) {
    if (-not (Test-MergeInProgress)) { Complete-Run 'blocked' 2 'no merge in progress; nothing to continue' }
    $unmerged = Get-UnmergedPaths
    $candidates = @(@($unmerged) + @(git diff --name-only HEAD 2>$null) | Where-Object { $_ } | Sort-Object -Unique)
    if ($candidates.Count) {
        $result.markers = @(git grep -n -I -E '^(<<<<<<<|>>>>>>>)( |$)' -- @candidates 2>$null | Where-Object { $_ })
    }
    if ($result.markers.Count) {
        $result.conflicts = @(Get-ConflictReport)
        Complete-Run 'blocked' 2 'conflict markers remain'
    }
    foreach ($path in $unmerged) {
        if (Test-Path -LiteralPath $path) { & git add -- $path } else { & git rm --quiet --cached --ignore-unmatch -- $path }
    }
    if ((Get-UnmergedPaths).Count) {
        $result.conflicts = @(Get-ConflictReport)
        Complete-Run 'blocked' 2 'unmerged paths remain'
    }
    $commit = Invoke-Git commit --no-edit --quiet
    if ($commit.Code -ne 0) { Complete-Run 'error' 1 ("commit failed: " + ($commit.Output -join ' ')) }
    $result.files = @(git diff --name-only HEAD^1 HEAD 2>$null).Count
    if ($Push) { Invoke-Push }
    Complete-Run 'resolved' 0
}

if (Test-MergeInProgress) {
    $result.conflicts = @(Get-ConflictReport)
    Complete-Run 'conflicts' 2 'merge already in progress; resolve then run with -Continue, or -Abort'
}

$dirty = @(git status --porcelain=v1 --untracked-files=no 2>$null | Where-Object { $_ })
if ($dirty.Count) {
    $result.dirty = $dirty
    Complete-Run 'blocked' 2 'working tree has uncommitted changes; commit or stash first'
}

$fetch = Invoke-Git fetch --quiet $Remote $Base
if ($fetch.Code -ne 0) { Complete-Run 'error' 1 ("fetch failed: " + ($fetch.Output -join ' ')) }

$target = "$Remote/$Base"
$result.behind = [int](git rev-list --count "HEAD..$target")
$result.ahead = [int](git rev-list --count "$target..HEAD")
if ($result.behind -eq 0) { Complete-Run 'up-to-date' 0 }

$before = "$(git rev-parse HEAD)"
if ($result.branch -eq $Base) {
    $ff = Invoke-Git merge --ff-only --quiet $target
    if ($ff.Code -ne 0) { Complete-Run 'error' 1 "local $Base has diverged from $target; reconcile manually" }
    $result.files = @(git diff --name-only $before HEAD).Count
    if ($Push) { Invoke-Push }
    Complete-Run 'fast-forwarded' 0
}

$merge = Invoke-Git merge --no-edit --quiet $target
if ($merge.Code -eq 0) {
    $result.files = @(git diff --name-only $before HEAD).Count
    if ($Push) { Invoke-Push }
    Complete-Run 'merged' 0
}

if (-not (Test-MergeInProgress)) { Complete-Run 'error' 1 ("merge failed: " + ($merge.Output -join ' ')) }
$result.conflicts = @(Get-ConflictReport)
Complete-Run 'conflicts' 2 'resolve the listed hunks, then run with -Continue'
