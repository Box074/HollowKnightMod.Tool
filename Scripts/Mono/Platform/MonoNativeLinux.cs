
namespace HKTool.Mono;

class MonoNativeLinux : IMonoNative
{
    [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "dlopen")]
    private static extern IntPtr dl_dlopen(string filename, int flags);

    [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "dlclose")]
    private static extern bool dl_dlclose(IntPtr handle);

    [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "dlsym")]
    private static extern IntPtr dl_dlsym(IntPtr handle, string symbol);

    [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "dlerror")]
    private static extern IntPtr dl_dlerror();

    public unsafe void ThrowError()
    {
        var err = dl_dlerror();
        if(err == IntPtr.Zero) return;
        var sb = new StringBuilder();
        byte* b = (byte*)err.ToPointer();
        char c;
        while((c = (char)*b) != 0)
        {
            sb.Append(c);
        }
        throw new SystemException(sb.ToString());
    }
    IntPtr IMonoNative.LoadLibrary(string path)
    {
        var result = dl_dlopen(path, 1);
        if(result == IntPtr.Zero)
        {
            ThrowError();
        }
        return result;
    }
    IntPtr IMonoNative.GetFunction(IntPtr handle, string name)
    {
        return dl_dlsym(handle, name);
    }
    void IMonoNative.CloseLibrary(IntPtr handle)
    {
        dl_dlclose(handle);
    }
}
