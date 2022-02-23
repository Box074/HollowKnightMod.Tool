
namespace HKTool.Mono;

public interface IMonoNative
{
    IntPtr LoadLibrary(string path);
    IntPtr GetFunction(IntPtr handle, string name);
    void CloseLibrary(IntPtr handle);
}
