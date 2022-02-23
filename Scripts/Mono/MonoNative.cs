
namespace HKTool.Mono;

public static class MonoNative
{
    public static bool isSupport => _current is not null;
    public static IMonoNative current
    {
        get
        {
            if (!isSupport) throw new NotSupportedException();
            return _current;
        }
    }
    public static IntPtr mono
    {
        get
        {
            if (!isSupport) throw new NotSupportedException();
            return _mono_handle;
        }
    }
    private static IMonoNative _current = null;
    private static IntPtr _mono_handle;
    static MonoNative()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                _current = new MonoNativeWin();
                _mono_handle = _current.LoadLibrary("mono-2.0-bdwgc.dll");
                break;
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.LinuxEditor:
                _current = new MonoNativeLinux();
                _mono_handle = _current.LoadLibrary("libmono.so");
                break;
            default:
                break;
        }
        Init();
    }
    #region Get Function
    public static bool TryGetFunction<T>(IntPtr handle, string name, out T d) where T : Delegate
    {
        if (handle == IntPtr.Zero) throw new ArgumentException(nameof(handle));
        try
        {
            GetFunction(handle, name, out d);
            return true;
        }
        catch (Exception)
        {
            d = null;
            return false;
        }
    }
    public static void GetFunction<T>(IntPtr handle, string name, out T d) where T : Delegate
    {
        if (handle == IntPtr.Zero) throw new ArgumentException(nameof(handle));
        var entry = current.GetFunction(handle, name);
        if (entry == IntPtr.Zero)
        {
            throw new EntryPointNotFoundException($"Entry point \"{name}\" cannot be found");
        }
        d = Marshal.GetDelegateForFunctionPointer<T>(entry);
    }
    public static bool TryGetMonoFunc<T>(string name, out T d) where T : Delegate
    {
        return TryGetFunction(_mono_handle, name, out d);
    }
    public static void GetMonoFunc<T>(string name, out T d) where T : Delegate
    {
        GetFunction(_mono_handle, name, out d);
    }
    #endregion


    private static void Init()
    {
        if(!isSupport) return;
    }
}
