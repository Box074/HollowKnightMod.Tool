
namespace HKTool.Utils.Compile;

public static class ReflectionHelperEx
{
    public static FieldInfo GetFieldSelf(string name) 
    {
        StackFrame fr = new StackFrame(1);
        return fr.GetMethod().DeclaringType.GetField(name, HReflectionHelper.All);
    }
    public static MethodBase GetMethodSelf(string name)
    {
        StackFrame fr = new StackFrame(1);
        return fr.GetMethod().DeclaringType.GetMethod(name, HReflectionHelper.All);
    }
    public static Type? FindType(string fullname)
    {
        var parts = fullname.Split('+');
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
    public static MethodBase FindMethodBase(string name)
    {
        var tn = name.Substring(0, name.IndexOf(':'));
        var fn = name.Substring(name.LastIndexOf(':') + 1);
        var type = FindType(tn);
        if(type == null) throw new MissingMethodException(tn, fn);
        return type.GetMethod(fn, HReflectionHelper.All) ?? throw new MissingMethodException(tn, fn);
    }
}
