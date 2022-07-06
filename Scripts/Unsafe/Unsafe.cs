
namespace HKTool.Unsafe;

public static class UnsafeUtils
{
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Ldarg0))]
    public static T GetSelf<T>() => throw new NotSupportedException();
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
    public static ref T ToRef<T>(IntPtr ptr) => ref ToRef<T>(ptr);
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
    public static IntPtr ToPointer<T>(ref T ptr) => ToPointer<T>(ref ptr);
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
    public static TTo Cast<T, TTo>(T src) => Cast<T, TTo>(src);
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
    public static TTo UnsafeCast<T, TTo>(this T src) => src.UnsafeCast<T, TTo>();
    
    public static int SizeOf<T>() => FIL.SizeOf<T>();
}
