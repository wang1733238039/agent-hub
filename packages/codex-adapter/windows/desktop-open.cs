using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

// Narrow packaged-app activation. No caller-supplied application ID, executable,
// arguments, model input or shell commands. The handle belongs to the verified
// current-user/session pipe owner and remains retained by desktop-pipe.ps1.
public static class CodexManageDesktopOpen {
    [DllImport("kernel32.dll")] static extern IntPtr GetCurrentProcess();
    [DllImport("kernel32.dll", SetLastError = true)] static extern IntPtr OpenProcess(uint access, bool inherit, uint pid);
    [DllImport("user32.dll")] static extern IntPtr GetShellWindow();
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window, out uint pid);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr handle);
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool GetTokenInformation(IntPtr token, int informationClass,
        out int value, int size, out int returned);
    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool DuplicateTokenEx(IntPtr token, uint access, IntPtr attributes,
        int impersonationLevel, int tokenType, out IntPtr primary);
    [DllImport("userenv.dll", SetLastError = true)]
    static extern bool CreateEnvironmentBlock(out IntPtr environment, IntPtr token, bool inherit);
    [DllImport("userenv.dll")] static extern bool DestroyEnvironmentBlock(IntPtr environment);
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct StartupInfo {
        public int size;
        public string reserved, desktop, title;
        public int x, y, xSize, ySize, xChars, yChars, fill, flags;
        public short show, reservedBytes;
        public IntPtr reservedPointer, stdin, stdout, stderr;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct ProcessInformation { public IntPtr process, thread; public uint pid, threadId; }
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool CreateProcessWithTokenW(IntPtr token, uint logonFlags,
        string application, StringBuilder commandLine, uint creationFlags,
        IntPtr environment, string directory, ref StartupInfo startup, out ProcessInformation process);
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool CreateProcessAsUserW(IntPtr token, string application,
        StringBuilder commandLine, IntPtr processAttributes, IntPtr threadAttributes,
        bool inheritHandles, uint creationFlags, IntPtr environment, string directory,
        ref StartupInfo startup, out ProcessInformation process);
    [DllImport("kernel32.dll")] static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    [DllImport("kernel32.dll")] static extern bool GetExitCodeProcess(IntPtr handle, out uint code);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern int GetApplicationUserModelId(IntPtr process, ref uint length, StringBuilder value);

    [ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IApplicationActivationManager {
        [PreserveSig]
        int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string appId,
            [MarshalAs(UnmanagedType.LPWStr)] string arguments, uint options, out uint processId);
        [PreserveSig]
        int ActivateForFile([MarshalAs(UnmanagedType.LPWStr)] string appId, IntPtr items,
            [MarshalAs(UnmanagedType.LPWStr)] string verb, out uint processId);
        [PreserveSig]
        int ActivateForProtocol([MarshalAs(UnmanagedType.LPWStr)] string appId,
            IntPtr items, out uint processId);
    }

    static InvalidOperationException Failure(string step) {
        return new InvalidOperationException(step + ":" + Marshal.GetLastWin32Error());
    }
    static bool Elevated(IntPtr process) {
        IntPtr token;
        if (!OpenProcessToken(process, 8, out token)) throw Failure("DESKTOP_OPEN_TOKEN_QUERY_FAILED");
        try {
            int value, returned;
            if (!GetTokenInformation(token, 20, out value, 4, out returned) || returned != 4)
                throw Failure("DESKTOP_OPEN_ELEVATION_QUERY_FAILED");
            return value != 0;
        } finally { CloseHandle(token); }
    }
    static void ActivateAtDesktopPrivilege(IntPtr desktop, Guid threadId, string helper) {
        // An elevated background service must not send activation to a separate
        // elevated Electron instance. Launch only this fixed navigation helper
        // at the authenticated original desktop's lower privilege. No UAC,
        // service stop, arbitrary command, credential or model input is involved.
        if (String.IsNullOrEmpty(helper) || !Path.IsPathRooted(helper) ||
            Path.GetFileName(helper) != "desktop-open.ps1" || !File.Exists(helper) ||
            helper.IndexOfAny(new[] { '"', '\r', '\n' }) >= 0)
            throw new ArgumentException("DESKTOP_OPEN_HELPER_INVALID");
        string executable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell", "v1.0", "powershell.exe");
        IntPtr token = IntPtr.Zero, primary = IntPtr.Zero, environment = IntPtr.Zero, shell = IntPtr.Zero;
        ProcessInformation child = new ProcessInformation();
        try {
            // Store app tokens may be package-restricted and cannot launch a
            // normal PowerShell child. Use only this same user's interactive
            // shell, discovered from the OS shell window, at ordinary privilege.
            uint shellPid;
            IntPtr window = GetShellWindow();
            if (window == IntPtr.Zero || GetWindowThreadProcessId(window, out shellPid) == 0)
                throw new InvalidOperationException("DESKTOP_OPEN_SHELL_UNAVAILABLE");
            using (var shellProcess = Process.GetProcessById((int)shellPid)) {
                if (shellProcess.SessionId != Process.GetCurrentProcess().SessionId)
                    throw new InvalidOperationException("DESKTOP_OPEN_SHELL_SESSION_MISMATCH");
            }
            shell = OpenProcess(0x1000, false, shellPid);
            if (shell == IntPtr.Zero || Elevated(shell) || !OpenProcessToken(shell, 10, out token))
                throw new InvalidOperationException("DESKTOP_OPEN_SHELL_TOKEN_UNAVAILABLE");
            using (var shellIdentity = new WindowsIdentity(token))
            using (var currentIdentity = WindowsIdentity.GetCurrent()) {
                if (shellIdentity.User == null || !shellIdentity.User.Equals(currentIdentity.User))
                    throw new InvalidOperationException("DESKTOP_OPEN_SHELL_USER_MISMATCH");
            }
            if (!DuplicateTokenEx(token, 0x02000000, IntPtr.Zero, 2, 1, out primary))
                throw Failure("DESKTOP_OPEN_TOKEN_DUPLICATION_FAILED");
            if (!CreateEnvironmentBlock(out environment, primary, false))
                throw Failure("DESKTOP_OPEN_ENVIRONMENT_FAILED");
            var startup = new StartupInfo { size = Marshal.SizeOf(typeof(StartupInfo)), flags = 1, show = 0 };
            var command = new StringBuilder("\"" + executable + "\" -NoProfile -NonInteractive -File \"" +
                helper + "\" -ThreadId " + threadId.ToString("D"));
            string arguments = command.ToString();
            if (!CreateProcessWithTokenW(primary, 1, executable, command, 0x08000400,
                environment, Path.GetDirectoryName(helper), ref startup, out child)) {
                // A false process-creation result proves no helper exists. Some
                // restricted desktop tokens require the same-user primary-token
                // API instead. Never try this after a successful/unknown launch.
                if (!CreateProcessAsUserW(primary, executable, new StringBuilder(arguments),
                    IntPtr.Zero, IntPtr.Zero, false, 0x08000400, environment,
                    Path.GetDirectoryName(helper), ref startup, out child))
                    throw Failure("DESKTOP_OPEN_USER_ACTIVATION_FAILED");
            }
            if (WaitForSingleObject(child.process, 8000) != 0)
                throw new InvalidOperationException("DESKTOP_OPEN_USER_ACTIVATION_UNKNOWN");
            uint code;
            if (!GetExitCodeProcess(child.process, out code) || code != 0)
                throw new InvalidOperationException("DESKTOP_OPEN_USER_ACTIVATION_FAILED");
        } finally {
            if (child.thread != IntPtr.Zero) CloseHandle(child.thread);
            if (child.process != IntPtr.Zero) CloseHandle(child.process);
            if (environment != IntPtr.Zero) DestroyEnvironmentBlock(environment);
            if (primary != IntPtr.Zero) CloseHandle(primary);
            if (token != IntPtr.Zero) CloseHandle(token);
            if (shell != IntPtr.Zero) CloseHandle(shell);
        }
    }

    public static bool TryActivate(IntPtr authenticatedProcess, Guid threadId, string helper = null) {
        if (threadId == Guid.Empty) throw new ArgumentException("DESKTOP_OPEN_TARGET_INVALID");
        if (authenticatedProcess == IntPtr.Zero || authenticatedProcess == new IntPtr(-1))
            throw new ArgumentException("DESKTOP_OPEN_PROCESS_INVALID");
        uint length = 0;
        int result = GetApplicationUserModelId(authenticatedProcess, ref length, null);
        // APPMODEL_ERROR_NO_APPLICATION: only a genuinely unpackaged owner may
        // fall back to the old registered protocol. Other failures stay unknown.
        if (result == 15703) return false;
        if (result != 122 || length < 2 || length > 4096)
            throw new InvalidOperationException("DESKTOP_OPEN_APPLICATION_UNAVAILABLE");
        var appId = new StringBuilder((int)length);
        if (GetApplicationUserModelId(authenticatedProcess, ref length, appId) != 0 || appId.Length == 0)
            throw new InvalidOperationException("DESKTOP_OPEN_APPLICATION_CHANGED");

        if (Elevated(GetCurrentProcess()) && !Elevated(authenticatedProcess)) {
            ActivateAtDesktopPrivilege(authenticatedProcess, threadId, helper);
            return true;
        }

        var manager = (IApplicationActivationManager)Activator.CreateInstance(
            Type.GetTypeFromCLSID(new Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")));
        try {
            uint dispatcherId;
            // AO_NOERRORUI prevents a background Host from leaving a modal
            // Windows error dialog. Activation is not an owner confirmation.
            result = manager.ActivateApplication(appId.ToString(),
                "codex://threads/" + threadId.ToString("D"), 2, out dispatcherId);
            Marshal.ThrowExceptionForHR(result);
            return true;
        } finally {
            Marshal.FinalReleaseComObject(manager);
        }
    }
}
