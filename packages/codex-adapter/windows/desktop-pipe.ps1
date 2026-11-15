param(
    [ValidateSet('inspect', 'bridge', 'trace', 'open')][string]$Action = 'inspect',
    [string]$ExpectedDesktop,
    [string]$ExpectedSha256,
    [Guid]$ThreadId
)
$ErrorActionPreference = 'Stop'
try {
    foreach ($name in @('Microsoft.PowerShell.Security', 'Microsoft.PowerShell.Utility', 'Microsoft.PowerShell.Management')) {
        Import-Module (Join-Path $PSHOME ('Modules\' + $name)) -ErrorAction Stop
    }
} catch {
    [Console]::Error.WriteLine('WINDOWS_POWERSHELL_MODULE_UNAVAILABLE')
    exit 1
}
Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
public static class CodexManagePipe {
    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern bool GetNamedPipeServerProcessId(SafePipeHandle pipe, out uint pid);
    [DllImport("kernel32.dll", SetLastError=true)]
    static extern IntPtr OpenProcess(uint access, bool inherit, uint pid);
    [DllImport("advapi32.dll", SetLastError=true)]
    static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr handle);
    public static string Owner(uint pid) {
        IntPtr process = OpenProcess(0x1000, false, pid), token = IntPtr.Zero;
        if (process == IntPtr.Zero) throw new Exception("PROCESS_QUERY_FAILED");
        try {
            if (!OpenProcessToken(process, 8, out token)) throw new Exception("TOKEN_QUERY_FAILED");
            using (var identity = new WindowsIdentity(token)) return identity.User.Value;
        } finally { if (token != IntPtr.Zero) CloseHandle(token); CloseHandle(process); }
    }
    public static void Bridge(NamedPipeClientStream pipe) {
        Task input = Console.OpenStandardInput().CopyToAsync(pipe);
        Task output = pipe.CopyToAsync(Console.OpenStandardOutput());
        Task.WhenAny(input, output).GetAwaiter().GetResult().GetAwaiter().GetResult();
    }
}
'@
$pipe = $null
try {
    # One retained handle: verify the server before any protocol bytes are forwarded.
    $pipe = [System.IO.Pipes.NamedPipeClientStream]::new('.', 'codex-ipc', [System.IO.Pipes.PipeDirection]::InOut, [System.IO.Pipes.PipeOptions]::Asynchronous)
    try { $pipe.Connect(10000) } catch [System.TimeoutException] { throw 'WINDOWS_DESKTOP_NOT_RUNNING' }
    [uint32]$serverProcessId = 0
    if (-not [CodexManagePipe]::GetNamedPipeServerProcessId($pipe.SafePipeHandle, [ref]$serverProcessId)) { throw 'PIPE_IDENTITY_FAILED' }
    $currentSid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
    if ([CodexManagePipe]::Owner($serverProcessId) -ne $currentSid) { throw 'WRONG_DESKTOP_USER' }
    $server = Get-Process -Id $serverProcessId
    if ($server.SessionId -ne [System.Diagnostics.Process]::GetCurrentProcess().SessionId) { throw 'WRONG_DESKTOP_SESSION' }
    $desktop = $server.Path
    if (-not $desktop) { throw 'DESKTOP_PATH_UNAVAILABLE' }
    $signature = Get-AuthenticodeSignature -LiteralPath $desktop
    $trusted = $signature.Status -eq 'Valid' -and $signature.SignerCertificate.Subject -match 'OpenAI'
    if (-not $trusted) {
        $trusted = @(Get-AppxPackage | Where-Object {
            $_.SignatureKind -eq 'Store' -and $_.Publisher -match 'OpenAI' -and
            $desktop.StartsWith($_.InstallLocation.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)
        }).Count -eq 1
    }
    if (-not $trusted) { throw 'UNTRUSTED_DESKTOP_PUBLISHER' }
    $digest = (Get-FileHash -LiteralPath $desktop -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($Action -eq 'bridge') {
        if (-not $ExpectedDesktop -or -not $ExpectedSha256 -or
            -not [string]::Equals($desktop, $ExpectedDesktop, [StringComparison]::OrdinalIgnoreCase) -or
            $digest -ne $ExpectedSha256) { throw 'DESKTOP_IDENTITY_CHANGED' }
        [Console]::Error.WriteLine('CODEX_MANAGE_PIPE_READY')
        [CodexManagePipe]::Bridge($pipe)
    } elseif ($Action -eq 'open') {
        if ($ThreadId -eq [Guid]::Empty) { throw 'DESKTOP_OPEN_TARGET_INVALID' }
        Add-Type -Path (Join-Path $PSScriptRoot 'desktop-open.cs')
        if (-not [CodexManageDesktopOpen]::TryActivate(
            $server.Handle,
            $ThreadId,
            (Join-Path $PSScriptRoot 'desktop-open.ps1')
        )) {
            Start-Process -FilePath ('codex://threads/' + $ThreadId.ToString('D'))
        }
    } elseif ($Action -eq 'trace') {
        # Retain this authenticated pipe while inspecting the exact main and
        # direct executors. Avoid multiple PowerShell processes per read.
        $started = $server.StartTime.ToUniversalTime().Ticks
        $trace = & (Join-Path $PSScriptRoot 'desktop-trace.ps1') -ExpectedPid $serverProcessId
        if (-not $? -or (Get-Process -Id $serverProcessId).StartTime.ToUniversalTime().Ticks -ne $started) { throw 'DESKTOP_IDENTITY_CHANGED' }
        $value = $trace | ConvertFrom-Json
        if ($value.pid -ne $serverProcessId -or $value.startedAtUnixMs -ne [DateTimeOffset]::new($server.StartTime.ToUniversalTime()).ToUnixTimeMilliseconds()) { throw 'DESKTOP_IDENTITY_CHANGED' }
        $trace
    } else {
        @{ pid = $serverProcessId; startedAtUnixMs = [DateTimeOffset]::new($server.StartTime.ToUniversalTime()).ToUnixTimeMilliseconds(); executable = $desktop; executableSha256 = $digest; userSid = $currentSid; sameUser = $true; sameSession = $true; trustedPublisher = $true } | ConvertTo-Json -Compress
    }
} catch {
    if ($Action -eq 'open' -and $env:HOST_AGENT_STATE_DIR) {
        $match = [regex]::Match($_.Exception.ToString(), 'DESKTOP_OPEN_[A-Z_]+(?::[0-9]+)?')
        if ($match.Success) {
            try {
                $logDirectory = Join-Path $env:HOST_AGENT_STATE_DIR 'diagnostics'
                $log = Join-Path $logDirectory 'desktop-open.jsonl'
                if ((Test-Path -LiteralPath $logDirectory -PathType Container) -and
                    -not ((Get-Item -LiteralPath $logDirectory).Attributes -band [IO.FileAttributes]::ReparsePoint) -and
                    (-not (Test-Path -LiteralPath $log) -or ((Get-Item -LiteralPath $log).Length -lt 65536 -and
                    -not ((Get-Item -LiteralPath $log).Attributes -band [IO.FileAttributes]::ReparsePoint)))) {
                    @{ at = [DateTime]::UtcNow.ToString('o'); scope = 'windows-desktop-open'; code = $match.Value } |
                        ConvertTo-Json -Compress | Add-Content -LiteralPath $log -Encoding utf8
                }
            } catch { }
        }
    }
    $known = @('WINDOWS_DESKTOP_NOT_RUNNING', 'PROCESS_QUERY_FAILED', 'TOKEN_QUERY_FAILED', 'PIPE_IDENTITY_FAILED', 'WRONG_DESKTOP_USER', 'WRONG_DESKTOP_SESSION', 'DESKTOP_PATH_UNAVAILABLE', 'UNTRUSTED_DESKTOP_PUBLISHER', 'DESKTOP_IDENTITY_CHANGED')
    $reason = if ($_.Exception.Message -in $known) { $_.Exception.Message } else { 'WINDOWS_DESKTOP_PIPE_REJECTED' }
    [Console]::Error.WriteLine($reason)
    exit 1
} finally {
    if ($null -ne $pipe) { $pipe.Dispose() }
}
