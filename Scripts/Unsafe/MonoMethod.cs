
namespace HKTool.Unsafe;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoReflectionMethod
{
    public static MonoReflectionMethod* From(MethodBase method) => (MonoReflectionMethod*)method.UnsafeCast<MethodBase, IntPtr>();
    public static MethodBase To(MonoReflectionMethod* method) => ((IntPtr)method).UnsafeCast<IntPtr, MethodBase>();
    public MonoObject obj;
    public MonoMethod* method;
    public byte* name;
    public void* reftype;
}
[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoMethod
{
    public enum Flag0 : byte
    {
        inline_info = 1,
        inline_failure = 2,
        string_ctor = 1 << 7
    }
    public enum Flag1 : byte
    {
        save_lmf = 1,
        dynamic = 2,
        sre_method = 4,
        is_generic = 8,
        is_inflated = 16,
        skip_visibility = 32,
        verification_success = 64
    }
    public ushort flags; //method flags
    public ushort iflags; //method implementation flags
    public uint token;
    public void* klass;
    public void* signature;
    public byte* name;
    public Flag0 flag0;
    public Flag1 flag1;
    public short slot;
}
