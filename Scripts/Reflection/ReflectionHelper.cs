

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
    public static Type? FindType(string fullname)
    {
        foreach (var v in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = v.GetType(fullname);
            if (t != null) return t;
        }
        return null;
    }
    public static object? FastGet(this object obj, string name) => obj.GetType().GetField(name, All).FastGet(obj);
    public static void FastSet(this object obj, string name, object val) => obj.GetType().GetField(name, All).FastSet(obj, val);
    public static object? FastInvoke(this MethodInfo m, object? @this, params object?[]? args) => FastReflection.CallMethod(@this, m, args);
    public static object? FastGet(this FieldInfo f, object? @this) => FastReflection.GetField(@this, f);
    public static void FastSet(this FieldInfo f, object? @this, object? val) => FastReflection.SetField(@this, f, val);
    public static object? FastGet(this PropertyInfo p, object? @this) => p.GetMethod.FastInvoke(@this);
    public static void FastSet(this PropertyInfo p, object? @this, object? val) => p.SetMethod.FastInvoke(@this, val);
    public static unsafe void SetSkipVisibility(MethodBase method)
    {
        var m = (MonoReflectionMethod*)method.UnsafeCast<MethodBase, IntPtr>();
        m->method->flag1 |= MonoMethod.Flag1.skip_visibility;
    }
    public static unsafe IntPtr GetInstanceField(IntPtr obj, FieldInfo field) => UnsafeUtils.GetInstanceField(obj, field);
    public static unsafe int GetFieldOffset(FieldInfo field) => UnsafeUtils.GetFieldOffset(field);
    public static ref TField GetInstanceFieldRef<TSelf, TField>(ref TSelf self, FieldInfo field)
    {
        return ref UnsafeUtils.ToRef<TField>(GetInstanceField(typeof(TSelf).IsValueType ? UnsafeUtils.ToPointer(ref self) : self!.ToPointer(), field));
    }
    
}

