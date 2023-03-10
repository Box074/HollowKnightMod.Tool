
namespace HKTool.Utils.Compile;

[Obsolete]
public static class ReflectionHelperEx
{
    public static object GetSelf() => throw new NotSupportedException();

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
    public static FieldInfo FindFieldInfo(string name)
    {
        var tn = name.Substring(0, name.IndexOf(':'));
        var fn = name.Substring(name.LastIndexOf(':') + 1);
        var type = FindType(tn);
        if(type == null) throw new MissingFieldException(tn, fn);
        return type.GetField(fn, HReflectionHelper.All) ?? throw new MissingFieldException(tn, fn);
    }
    public static FieldInfo FindFieldInfo<TType>(string name)
    {
        return typeof(TType).GetField(name, HReflectionHelper.All) ?? throw new MissingFieldException(typeof(TType).FullName, name);
    }
    public static ref T GetFieldRefFrom<T>(IntPtr pointer)
    {
        return ref GetFieldRefFrom<T>(pointer);
    }

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
