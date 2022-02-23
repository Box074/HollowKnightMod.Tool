
namespace HKTool.Mono;

class MonoNativeWin : IMonoNative
{
    [DllImport("kernel32", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32", SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hLibModule);

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    IntPtr IMonoNative.LoadLibrary(string path)
    {
        return LoadLibrary(path);
    }
    IntPtr IMonoNative.GetFunction(IntPtr handle, string name)
    {
        return GetProcAddress(handle, name);
    }
    void IMonoNative.CloseLibrary(IntPtr handle)
    {
        FreeLibrary(handle);
    }
}
