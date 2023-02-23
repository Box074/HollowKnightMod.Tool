
namespace HKTool.Unsafe;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoReflectionType
{
    public MonoObject obj;
	public void* type;
}
