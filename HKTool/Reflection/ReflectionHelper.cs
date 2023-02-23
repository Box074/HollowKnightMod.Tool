

namespace HKTool.Reflection;
public static class ReflectionHelper
{
    public const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Instance | BindingFlags.Static;
    public const BindingFlags Instance = BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Instance;
    public const BindingFlags Static = BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Static;
    public static ReflectionObject CreateReflectionObject(this object obj) => new ReflectionObject(obj);
    public static ReflectionObject CreateReflectionObject(this Type type) => new ReflectionObject(type);
    public static ReflectionObject CreateReflectionObject(this ReflectionObject obj) => obj;
    private static Dictionary<string, Type> typemap = new();
    public static Type? FindType(string fullname)
    {
        if(fullname == null || fullname == "") return null;
        if(typemap.TryGetValue(fullname, out var ot) && ot != null) return ot;
        ot = Type.GetType(fullname);
        if(ot != null)
        {
            typemap[fullname] = ot;
            return ot;
        }
        foreach (var v in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = v.GetType(fullname);
            if (t == null) continue;
            typemap[fullname] = t;
            return t;
        }
        return null;
    }
    public static Type[] SafeGetTypes(this Assembly ass)
    {
        try
        {
            return ass.GetTypes();
        }
        catch(ReflectionTypeLoadException tle)
        {
            return tle.Types;
        }
    }
    public static object? FastGet(this object obj, string name) => obj.GetType().GetField(name, All).FastGet(obj);
    public static void FastSet(this object obj, string name, object val) => obj.GetType().GetField(name, All).FastSet(obj, val);
    public static object? FastInvoke(this MethodInfo m, object? @this, params object?[]? args) => FastReflection.CallMethod(@this, m, args);
    public static object? FastGet(this FieldInfo f, object? @this) => FastReflection.GetField(@this, f);
    public static void FastSet(this FieldInfo f, object? @this, object? val) => FastReflection.SetField(@this, f, val);
    public static object? FastGet(this PropertyInfo p, object? @this) => p.GetMethod.FastInvoke(@this);
    public static void FastSet(this PropertyInfo p, object? @this, object? val) => p.SetMethod.FastInvoke(@this, val);
}

