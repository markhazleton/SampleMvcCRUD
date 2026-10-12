#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$PassThroughArgs
)

$engine = Join-Path (Split-Path $PSScriptRoot -Parent) 'context-projection.py'
& python3 $engine @PassThroughArgs
exit $LASTEXITCODE
