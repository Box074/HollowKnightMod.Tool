using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using MonoMod.Utils;

namespace HKTool.Reflection
{
    public class ReflectionObject
    {
        public static Dictionary<FieldInfo, Action<object, object>> fsetter = new Dictionary<FieldInfo, Action<object, object>>();
        public static Dictionary<FieldInfo, Func<object, object>> fgetter = new Dictionary<FieldInfo, Func<object, object>>();
        public static Dictionary<MethodInfo, FastReflectionDelegate> mcaller = 
            new Dictionary<MethodInfo, FastReflectionDelegate>();

        internal static object CallMethod(object @this, MethodInfo method, object[] args)
        {
            if(!mcaller.TryGetValue(method,out var caller))
            {
                caller = method.CreateFastDelegate(true);
                mcaller[method] = caller;
            }
            return caller(@this, args);
        }
        internal static object GetField(object @this,FieldInfo field)
        {
            if(!fgetter.TryGetValue(field,out var getter))
            {
                DynamicMethod dm = new DynamicMethod("", MethodAttributes.Static | MethodAttributes.Public,
                    CallingConventions.Standard, typeof(object), new Type[]{
                        typeof(object)
                        }, field.DeclaringType, true);
                var il = dm.GetILGenerator();
                
                if (field.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, field);
                }
                else
                {
                    il.Emit(OpCodes.Ldsfld, field);
                }
                il.Emit(OpCodes.Ret);
                getter = (Func<object, object>)dm.CreateDelegate(typeof(Func<object, object>));
            }
            return getter(@this);
        }
        internal static void SetField(object @this, FieldInfo field, object val)
        {
            if (!fsetter.TryGetValue(field, out var setter))
            {
                DynamicMethod dm = new DynamicMethod("", MethodAttributes.Static | MethodAttributes.Public,
                    CallingConventions.Standard, typeof(object), new Type[]{
                        typeof(object),
                        typeof(object)
                        }, field.DeclaringType, true);
                var il = dm.GetILGenerator();
                
                if (field.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stsfld, field);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stfld, field);
                }
                il.Emit(OpCodes.Ret);
                setter = (Action<object, object>)dm.CreateDelegate(typeof(Action<object, object>));
                fsetter[field] = setter;
            }
            setter(@this, val);
        }
        public Type objType = null;
        public object obj = null;
        public bool HasValue => !(obj is null);
        public ReflectionObject(object o)
        {
            if (o is null) throw new ArgumentNullException(nameof(o));
            objType = o.GetType();
            obj = o;
            
        }
        public ReflectionObject(Type t)
        {
            if (objType is null) throw new ArgumentNullException(nameof(t));
            obj = null;
            objType = t;
        }
        public T As<T>()
        {
            return (T)obj;
        }

        public ReflectionObject GetMemberData(string name)
        {
            return new ReflectionObject(GetMemberData<object>(name));
        }

        public T GetMemberData<T>(string name)
        {
            FieldInfo f = objType.GetRuntimeField(name);
            if (f != null)
            {
                return (T)GetField(obj, f);
            }
            PropertyInfo p = objType.GetRuntimeProperty(name);
            if (p != null)
            {
                return (T)CallMethod(obj, p.GetMethod, null);
            }
            throw new MissingMemberException(objType.FullName, name);
        }

        public object GetObject()
        {
            return obj;
        }

        public Type GetObjectType()
        {
            return objType;
        }

        private MethodInfo FindMethod(string name, params Type[] pt)
        {
            return objType.GetRuntimeMethod(name, pt) ?? throw new MissingMethodException(objType.FullName, name);
        }
        private MethodInfo FindMethod(string name, params object[] args)
        {
            return FindMethod(name, args.Select(x => x?.GetType() ?? typeof(object)).ToArray());
        }
        public T InvokeMethod<T>(string name, params object[] args)
        {
            var m = FindMethod(name, args);
            if (m.ReturnType == typeof(void))
            {
                CallMethod(obj, m, args);
                return default;
            }
            else
            {
                return (T)CallMethod(obj, m, args);
            }
        }

        public void SetMemberData(string name, object data)
        {
            FieldInfo f = objType.GetRuntimeField(name);
            if (f != null)
            {
                SetField(obj, f, data);
                return;
            }
            PropertyInfo p = objType.GetRuntimeProperty(name);
            if (p != null)
            {
                CallMethod(obj, p.SetMethod, new object[]{
                    data
                    });
                return;
            }
            throw new MissingMemberException(objType.FullName, name);
        }

        public void SetMemberData<T>(string name, T data)
        {
            SetMemberData(name, (object)data);
        }

        public void SetMemberData(string name, ReflectionObject data)
        {
            SetMemberData(name, data?.GetObject());
        }

        public ReflectionObject InvokeMethod(string name, params object[] args)
        {
            var r = InvokeMethod<object>(name, args);
            if(r == null)
            {
                return null;
            }
            else
            {
                return new ReflectionObject(r);
            }
        }
        public object InvokeMethod(string name, Type[] genTypes, params object[] args)
        {
            var m = FindMethod(name, args).MakeGenericMethod(genTypes);
            return CallMethod(obj, m, args);
        }
        public T InvokeMethod<T>(string name, Type[] genTypes, params object[] args)
        {
            return (T)InvokeMethod(name, genTypes, args);
        }

        public ReflectionObject CreateInstance(params object[] args)
        {
            return Activator.CreateInstance(objType, args).CreateReflectionObject();
        }
    }
}
