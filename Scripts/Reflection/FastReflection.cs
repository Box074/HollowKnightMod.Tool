
namespace HKTool.Reflection;

static class FastReflection
{
    public static Dictionary<FieldInfo, RD_SetField> fsetter = new();
    public static Dictionary<FieldInfo, RD_GetField> fgetter = new();
    public static Dictionary<FieldInfo, RT_GetFieldPtr> frefgetter = new();
    public static Dictionary<MethodInfo, FastReflectionDelegate> mcaller = new();
    public static RD_GetField GetGetter(FieldInfo field)
    {
        if (field is null) throw new ArgumentNullException(nameof(field));
        if (!fgetter.TryGetValue(field, out var getter))
        {
            DynamicMethod dm = new DynamicMethod("", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, typeof(object), new Type[]{
                        typeof(object)
                    }, (Type)field.DeclaringType, true);
            var il = dm.GetILGenerator();

            if (!field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                if (field.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, field.DeclaringType);
                }
                il.Emit(OpCodes.Ldfld, field);
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, field);
            }
            if (field.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Box, field.FieldType);
            }
            il.Emit(OpCodes.Ret);
            getter = (RD_GetField)dm.CreateDelegate(typeof(RD_GetField));
            fgetter[field] = getter;
        }
        return getter;
    }
    public static RD_SetField GetSetter(FieldInfo field)
    {
        if (field is null) throw new ArgumentNullException(nameof(field));
        if (!fsetter.TryGetValue(field, out var setter))
        {
            DynamicMethod dm = new DynamicMethod("", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, typeof(void), new Type[]{
                        typeof(object),
                        typeof(object)
                    }, (Type)field.DeclaringType, true);
            var il = dm.GetILGenerator();

            if (field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_1);
                if (field.FieldType.IsValueType) il.Emit(OpCodes.Unbox_Any, field.FieldType);
                il.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if (field.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, field.DeclaringType);
                }
                il.Emit(OpCodes.Ldarg_1);
                if (field.FieldType.IsValueType) il.Emit(OpCodes.Unbox_Any, field.FieldType);
                il.Emit(OpCodes.Stfld, field);
            }
            il.Emit(OpCodes.Ret);
            setter = (RD_SetField)dm.CreateDelegate(typeof(RD_SetField));
            fsetter[field] = setter;
        }
        return setter;
    }
    internal static object CallMethod(object? @this, MethodInfo method, params object?[]? args)
    {
        if (method is null) throw new ArgumentNullException(nameof(method));
        if (!mcaller.TryGetValue(method, out var caller))
        {
            caller = method.CreateFastDelegate(true);
            mcaller[method] = caller;
        }
        return caller(@this, args ?? new object?[] { null });
    }
    internal static object GetField(object? @this, FieldInfo field)
    {
        if (field is null) throw new ArgumentNullException(nameof(field));
        try
        {
            return GetGetter(field)(@this);
        }
        catch (Exception e)
        {
            HKToolMod.logger.LogError(e);
            return field.GetValue(@this);
        }
    }
    internal static IntPtr GetFieldRefEx(object? @this, FieldInfo field, ref RT_GetFieldPtr cache)
    {
        if (cache is not null) return cache.Invoke(@this);
        return GetFieldRef(@this, field, out cache);
    }
    internal static IntPtr GetFieldRef(object? @this, FieldInfo field, out RT_GetFieldPtr cache)
    {
        if (field is null) throw new ArgumentNullException(nameof(field));
        if (frefgetter.TryGetValue(field, out var getter))
        {
            cache = getter;
            return getter.Invoke(@this!);
        }
        if (field.IsStatic)
        {
            DynamicMethod dm = new("", MethodAttributes.Static | MethodAttributes.Public,
        CallingConventions.Standard, typeof(IntPtr), new Type[]{
                        typeof(object)
            }, (Type)field.DeclaringType, true);
            var il = dm.GetILGenerator();



            if (!field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                if (field.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, field.DeclaringType);
                }
                il.Emit(OpCodes.Ldflda, field);
            }
            else
            {
                il.Emit(OpCodes.Ldsflda, field);
            }
            il.Emit(OpCodes.Ret);
            getter = (RT_GetFieldPtr)dm.CreateDelegate(typeof(RT_GetFieldPtr));
        }
        else
        {
            int offset = UnsafeUtils.GetFieldOffset(field);
            getter = obj => IntPtr.Add(obj!.ToPointer(), offset);
        }
        frefgetter[field] = getter;
        cache = getter;
        return getter.Invoke(@this!);
    }

    internal static void SetField(object? @this, FieldInfo field, object? val)
    {
        if (field is null) throw new ArgumentNullException(nameof(field));
        try
        {
            GetSetter(field)(@this, val);
        }
        catch (Exception e)
        {
            HKToolMod.logger.LogError(e);
            field.SetValue(@this, val);
        }
    }
}

