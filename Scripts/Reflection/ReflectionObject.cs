

namespace HKTool.Reflection;
[ModuleDefine("HKTool.ReflectionObject", "0.1")]
public class ReflectionObject
{

    public Type objType;
    public object? obj = null;
    private Dictionary<string, Func<object?>> getterCache = new();
    private Dictionary<string, Action<object?>> setterCache = new();
    public ReflectionObject(object o)
    {
        if (o is null) throw new ArgumentNullException(nameof(o));
        objType = o.GetType();
        obj = o;

    }
    public ReflectionObject(Type t)
    {
        if (t is null) throw new ArgumentNullException(nameof(t));
        obj = null;
        objType = t;
    }
    public T? As<T>()
    {
        return (T?)obj;
    }

    public ReflectionObject? GetMemberData(string name)
    {
        var r = GetMemberData<object>(name);
        return r is null ? null : new ReflectionObject(r);
    }

    public T? GetMemberData<T>(string name)
    {
        if(getterCache.TryGetValue(name, out var getter))
        {
            return (T?)getter();
        }
        if(objType is null) throw new InvalidOperationException();
        FieldInfo? f = objType.GetField(name, ReflectionHelper.All);
        if (f != null)
        {
            getterCache.Add(name, () => f.FastGet(obj));
            return (T?)f.FastGet(obj);
        }
        PropertyInfo? p = objType.GetProperty(name, ReflectionHelper.All);
        if (p != null)
        {
            getterCache.Add(name, () => p.GetMethod.FastInvoke(obj, null));
            return (T?)p.GetMethod.FastInvoke(obj, null);
        }
        throw new MissingMemberException(objType.FullName, name);
    }

    public object? GetObject()
    {
        return obj;
    }

    public Type GetObjectType()
    {
        return objType;
    }

    private MethodInfo FindMethod(string name, params Type[] pt)
    {
        if(objType is null) throw new InvalidOperationException();
        return objType.GetMethod(name, ReflectionHelper.All, null,
            CallingConventions.Any, pt, null) ?? throw new MissingMethodException(objType.FullName, name);
    }
    private MethodInfo FindMethod(string name, params object?[] args)
    {
        return FindMethod(name, args.Select(x => x?.GetType() ?? typeof(object)).ToArray());
    }
    public T? InvokeMethod<T>(string name, params object?[] args)
    {
        var m = FindMethod(name, args);
        if (m.ReturnType == typeof(void))
        {
            m.FastInvoke(obj, args);
            return default;
        }
        else
        {
            return (T?)m.FastInvoke(obj, args);
        }
    }

    public void SetMemberData(string name, object? data)
    {
        if(setterCache.TryGetValue(name, out var setter))
        {
            setter(data);
            return;
        }
        if(objType is null) throw new InvalidOperationException();
        FieldInfo f = objType.GetField(name, ReflectionHelper.All);
        if (f != null)
        {
            setterCache.Add(name, (val) => f.FastSet(obj, val));
            f.FastSet(obj, data);
            return;
        }
        PropertyInfo p = objType.GetProperty(name, ReflectionHelper.All);
        if (p != null)
        {
            setterCache.Add(name, (val) => p.SetMethod.FastInvoke(obj, val));
            p.SetMethod.FastInvoke(obj, data);
            return;
        }
        throw new MissingMemberException(objType.FullName, name);
    }

    public void SetMemberData<T>(string name, T? data)
    {
        SetMemberData(name, (object?)data);
    }

    public void SetMemberData(string name, ReflectionObject? data)
    {
        SetMemberData(name, data?.GetObject());
    }

    public ReflectionObject? this[string name]
    {
        get
        {
            return GetMemberData(name);
        }
        set
        {
            SetMemberData(name, value);
        }
    }
    public ReflectionObject? InvokeMethod(string name, params object?[] args)
    {
        var r = InvokeMethod<object>(name, args);
        if (r == null)
        {
            return null;
        }
        else
        {
            return new ReflectionObject(r);
        }
    }
    public object? InvokeMethod(string name, Type[] genTypes, params object?[] args)
    {
        var m = FindMethod(name, args).MakeGenericMethod(genTypes);
        return m.FastInvoke(obj, args);
    }
    public T? InvokeMethod<T>(string name, Type[] genTypes, params object?[] args)
    {
        return (T?)InvokeMethod(name, genTypes, args);
    }

    public ReflectionObject CreateInstance(params object?[] args)
    {
        return Activator.CreateInstance(objType, args).CreateReflectionObject();
    }
}

