#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Topic,
    [switch]$Json
)

$engine = Join-Path (Split-Path $PSScriptRoot -Parent) 'explain-context.py'
$engineArgs = @($Topic)
if ($Json) { $engineArgs += '--json' }
& python3 $engine @engineArgs
exit $LASTEXITCODE
