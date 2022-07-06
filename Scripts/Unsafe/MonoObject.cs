
namespace HKTool.Unsafe;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoObject
{
    public void* VTable;
    public void* Unknow;
}