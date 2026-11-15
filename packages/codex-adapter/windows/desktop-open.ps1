param([Parameter(Mandatory = $true)][Guid]$ThreadId)
$ErrorActionPreference = 'Stop'
# Resolve the running original desktop from its authenticated pipe. Store app
# activation must target that process's application ID, not the default URI handler.
& (Join-Path $PSScriptRoot 'desktop-pipe.ps1') -Action open -ThreadId $ThreadId
if (-not $?) { exit 1 }
