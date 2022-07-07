
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

    public static int SizeOf<T>() => FIL.SizeOf<T>();

    public static ref TField GetStaticFieldRef<TField>(FieldInfo field)
    {
        return ref UnsafeUtils.ToRef<TField>(IntPtr.Add(UnsafeUtils.GetStaticFieldData(field.DeclaringType), GetFieldOffset(field)));
    }

    public static unsafe void SetSkipVisibility(MethodBase method)
    {
        var m = (MonoReflectionMethod*)method.ToPointer();
        m->method->flag1 |= MonoMethod.Flag1.skip_visibility;
    }
    public static unsafe IntPtr GetInstanceField(IntPtr obj, FieldInfo field)
    {
        return (IntPtr)((byte*)obj + GetFieldOffset(field));
    }
    public static unsafe int GetFieldOffset(FieldInfo field)
    {
        MonoClassField* monofield = (MonoClassField*)field.FieldHandle.Value;
        var isValueType = ((*(byte*)((byte*)monofield->parentType + sizeof(void*) * 3 + 2 + 1 + 4) >> 2) & 1) > 0;
        return monofield->offset + (isValueType ? sizeof(void*) * 2 /* Skip MonoObject */: 0);
    }

    public static unsafe IntPtr GetStaticFieldData(MonoObject* obj) => (IntPtr)mono_vtable_get_static_field_data(obj->VTable);
    public static unsafe IntPtr GetStaticFieldData(Type type) => (IntPtr)mono_vtable_get_static_field_data((void*)GetClassVTable(type));
    public static unsafe IntPtr GetClassVTable(Type type)
    {
        MonoError err;
        return (IntPtr)mono_class_vtable_full(
        mono_domain_get(), *(((void**)type.ToPointer()) + 1), &err
        );
    }
    static UnsafeUtils()
    {
        MonoMod.Utils.DynDll.ResolveDynDllImports(typeof(UnsafeUtils));
    }
    [MonoMod.Utils.DynDllImport("mono")]
    private static d_mono_vtable_get_static_field_data mono_vtable_get_static_field_data = null!;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate void* d_mono_vtable_get_static_field_data(void* vt);

    [MonoMod.Utils.DynDllImport("mono")]
    private static d_mono_class_vtable_full mono_class_vtable_full = null!;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate void* d_mono_class_vtable_full(void* domain, void* klass, void* error);
    [MonoMod.Utils.DynDllImport("mono")]
    private static d_mono_domain_get mono_domain_get = null!;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate void* d_mono_domain_get();
}
