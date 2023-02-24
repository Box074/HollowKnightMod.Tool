
namespace HKTool.Utils.Compile;

public static class ReflectionHelperEx
{
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Ldarg0))]
    public static object GetSelf() => throw new NotSupportedException();

    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_FindType))]
    public static Type? FindType(string fullname)
    {
        var parts = fullname.Trim().Split('+');
        var parent = HReflectionHelper.FindType(parts[0]);
        if(parent == null) return null;
        for(int i = 1; i < parts.Length ; i++)
        {
            var name = parts[i];
            var t = parent.GetNestedType(name, HReflectionHelper.All);
            if(t == null) return null;
            parent = t;
        }
        return parent;
    }
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_FindFieldInfo))]
    public static FieldInfo FindFieldInfo(string name)
    {
        var tn = name.Substring(0, name.IndexOf(':'));
        var fn = name.Substring(name.LastIndexOf(':') + 1);
        var type = FindType(tn);
        if(type == null) throw new MissingFieldException(tn, fn);
        return type.GetField(fn, HReflectionHelper.All) ?? throw new MissingFieldException(tn, fn);
    }
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_FindFieldInfoEx))]
    public static FieldInfo FindFieldInfo<TType>(string name)
    {
        return typeof(TType).GetField(name, HReflectionHelper.All) ?? throw new MissingFieldException(typeof(TType).FullName, name);
    }
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
    public static ref T GetFieldRefFrom<T>(IntPtr pointer)
    {
        return ref GetFieldRefFrom<T>(pointer);
    }
    [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_FindMethodBase))]
    public static MethodBase FindMethodBase(string name)
    {
        var tn = name.Substring(0, name.IndexOf(':')).Trim();
        var fn = name.Substring(name.LastIndexOf(':') + 1).Trim();
        var type = FindType(tn);
        if(type == null) throw new MissingMemberException(tn, fn);

        if(fn == ".ctor" || fn == ".cctor")
        {
            return type.GetConstructors(HReflectionHelper.All)
                .FirstOrDefault(x => (fn == ".ctor" && !x.IsStatic) || (fn == ".cctor" && x.IsStatic)) ?? throw new MissingMethodException(tn, fn);
        }
        return type.GetMethod(fn, HReflectionHelper.All) ?? throw new MissingMethodException(tn, fn);
    }

}
