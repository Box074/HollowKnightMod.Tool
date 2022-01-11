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
                return (T)f.FastGet(obj);
            }
            PropertyInfo p = objType.GetRuntimeProperty(name);
            if (p != null)
            {
                return (T)p.GetMethod.FastInvoke(obj, null);
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
                m.FastInvoke(obj, args);
                return default;
            }
            else
            {
                return (T)m.FastInvoke(obj, args);
            }
        }

        public void SetMemberData(string name, object data)
        {
            FieldInfo f = objType.GetRuntimeField(name);
            if (f != null)
            {
                f.FastSet(obj, data);
                return;
            }
            PropertyInfo p = objType.GetRuntimeProperty(name);
            if (p != null)
            {
                p.SetMethod.FastInvoke(obj, data);
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

        public ReflectionObject this[string name]
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
            return m.FastInvoke(obj, args);
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
