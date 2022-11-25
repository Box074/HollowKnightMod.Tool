
namespace HKTool.Unsafe;

public static class UnsafeUtils
{
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Ldarg0))]
    public static T GetSelf<T>() => throw new NotSupportedException();
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
    public static ref T ToRef<T>(this IntPtr ptr) => ref ToRef<T>(ptr);
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
    public static IntPtr ToPointer<T>(ref T ptr) => ToPointer<T>(ref ptr);
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
    public static IntPtr ToPointer(this object obj) => ToPointer(obj);
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
    public static TTo Cast<T, TTo>(T src) => Cast<T, TTo>(src);
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
    public static TTo UnsafeCast<T, TTo>(this T src) => src.UnsafeCast<T, TTo>();
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int SizeOf<T>() => FIL.SizeOf<T>();

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private unsafe static bool IsValueType(IntPtr kclass) => ((*(byte*)((byte*)kclass + sizeof(void*) * 3 + 2 + 1 + 4) >> 2) & 1) > 0;
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private unsafe static bool IsValueType(Type type)
    {
        var monotype = (byte*)type.TypeHandle.Value;
        return IsValueType((IntPtr)(*((void**)monotype)));
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetSkipVisibility(MethodBase method)
    {
        var m = (MonoReflectionMethod*)method.ToPointer();
        m->method->flag1 |= MonoMethod.Flag1.skip_visibility;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static unsafe IntPtr GetInstanceField(IntPtr obj, FieldInfo field)
    {
        return (IntPtr)((byte*)obj + GetFieldOffset(field));
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static unsafe IntPtr GetInstanceField(IntPtr obj, RuntimeFieldHandle field)
    {
        return (IntPtr)((byte*)obj + GetFieldOffset(field));
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static unsafe int GetFieldOffset(FieldInfo field) => GetFieldOffset(field.FieldHandle);
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static unsafe int GetFieldOffset(RuntimeFieldHandle field)
    {
        MonoClassField* monofield = (MonoClassField*)field.Value;
        return monofield->offset;
    }
}
