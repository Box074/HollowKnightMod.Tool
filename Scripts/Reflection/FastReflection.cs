
using HKTool.Reflection.Runtime;

namespace HKTool.Reflection;

static class FastReflection
{
    public static Dictionary<FieldInfo, RD_SetField> fsetter = new Dictionary<FieldInfo, RD_SetField>();
    public static Dictionary<FieldInfo, RD_GetField> fgetter = new Dictionary<FieldInfo, RD_GetField>();
    public static Dictionary<MethodInfo, FastReflectionDelegate> mcaller =
        new Dictionary<MethodInfo, FastReflectionDelegate>();
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
                if(field.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, field.DeclaringType);
                }
                il.Emit(OpCodes.Ldfld, field);
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, field);
            }
            if(field.FieldType.IsValueType)
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
                if(field.FieldType.IsValueType) il.Emit(OpCodes.Unbox_Any, field.FieldType);
                il.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if(field.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, field.DeclaringType);
                }
                il.Emit(OpCodes.Ldarg_1);
                if(field.FieldType.IsValueType) il.Emit(OpCodes.Unbox_Any, field.FieldType);
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
        return caller(@this, args);
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

