param([Parameter(Mandatory=$true)][string]$Source)
$ErrorActionPreference='Stop'
Add-Type -Path $Source
$valid=[Guid]'00000000-0000-4000-8000-000000000001'
# These calls must reject before Windows activation, with no navigation or model input.
foreach ($handle in @([IntPtr]::Zero,[IntPtr]::new(-1))) {
    $rejected=$false
    try { [CodexManageDesktopOpen]::TryActivate($handle,$valid) | Out-Null }
    catch { $rejected=$_.Exception.ToString().Contains('DESKTOP_OPEN_PROCESS_INVALID') }
    if (-not $rejected) { throw 'Invalid process handle accepted' }
}
$process=[System.Diagnostics.Process]::GetProcessById($PID)
$rejected=$false
try { [CodexManageDesktopOpen]::TryActivate($process.Handle,[Guid]::Empty) | Out-Null }
catch { $rejected=$_.Exception.ToString().Contains('DESKTOP_OPEN_TARGET_INVALID') }
if (-not $rejected) { throw 'Empty thread accepted' }
# A real unpackaged PowerShell process must return false without activating an app.
if ([CodexManageDesktopOpen]::TryActivate($process.Handle,$valid)) { throw 'Unpackaged process activated' }
[Console]::WriteLine('NATIVE_DESKTOP_OPEN_BOUNDARIES_OK')
