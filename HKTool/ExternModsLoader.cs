using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Modding;

namespace HKTool
{
    class ExternModsLoader
    {
        public static Type TModLoader = typeof(Mod).Assembly.GetType("Modding.ModLoader");
        public static MethodInfo MAddModInstance = TModLoader.GetMethod("AddModInstance", BindingFlags.Static | BindingFlags.NonPublic);
        public static Type TModInstance = TModLoader.GetNestedType("ModInstance");
        public static FieldInfo IMI_Mod = TModInstance.GetField("Mod", BindingFlags.Public | BindingFlags.Instance);
        public static FieldInfo IMI_Name = TModInstance.GetField("Name", BindingFlags.Public | BindingFlags.Instance);
        public static FieldInfo IMI_Error = TModInstance.GetField("Error", BindingFlags.Public | BindingFlags.Instance);
        public static FieldInfo IMI_Enabled = TModInstance.GetField("Enabled", BindingFlags.Public | BindingFlags.Instance);
        public static Type TModErrorState = TModLoader.GetNestedType("ModErrorState");
        public static Type TErrorC = typeof(Nullable<>).MakeGenericType(TModErrorState);
        public static void AddModInstance(Type type, Mod mod, bool enabled, string error, string name)
        {
            object mi = Activator.CreateInstance(TModInstance);
            IMI_Mod.SetValue(mi, mod);
            IMI_Name.SetValue(mi, name);
            IMI_Enabled.SetValue(mi, enabled);
            if (string.IsNullOrEmpty(error))
            {
                IMI_Error.SetValue(mi, Activator.CreateInstance(TErrorC, null));
            }
            else
            {
                IMI_Error.SetValue(mi, Activator.CreateInstance(TErrorC, Enum.Parse(TModErrorState, error)));
            }
            MAddModInstance.Invoke(null, new object[]
            {
                type, mi
            });
        }
        public static void LoadMod(Assembly ass)
        {
            foreach(var type in ass.GetTypes())
            {
                if(type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Mod)))
                {
					try
					{
						ConstructorInfo constructor = type.GetConstructor(new Type[0]);
                        if ((constructor?.Invoke(new object[0])) is Mod mod)
                        {
                            AddModInstance(type, mod, false, null, mod.GetName());
                        }
                    }
					catch (Exception e)
					{
                        Logger.LogError(e);
                        AddModInstance(type, null, false, "Construct", type.Name);
					}
				}
            }
        }
        public static void LoadMods(List<string> p)
        {
            List<Assembly> ass = new List<Assembly>();
            foreach(var v in p)
            {
                var v2 = Path.GetFullPath(v);
                if (!File.Exists(v2)) continue;
                try
                {
                    ass.Add(Assembly.LoadFrom(v2));
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
            }
            foreach(var v in ass)
            {
                LoadMod(v);
            }
        }
    }
}
