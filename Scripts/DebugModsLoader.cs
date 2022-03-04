
using System.IO;
using Modding;
using HKTool.Reflection;
using Mono;
using Mono.Cecil;
using static Modding.Logger;

namespace HKTool
{
    class DebugModsLoader
    {
        public static List<Mod> DebugMods { get; } = new List<Mod>();
        public static Type TModLoader = typeof(Mod).Assembly.GetType("Modding.ModLoader");
        public static MethodInfo MAddModInstance = TModLoader.GetMethod("AddModInstance", BindingFlags.Static | BindingFlags.NonPublic);
        public static Type TModInstance = TModLoader.GetNestedType("ModInstance");
        public static FieldInfo IMI_Mod = TModInstance.GetField("Mod", BindingFlags.Public | BindingFlags.Instance);
        public static FieldInfo IMI_Name = TModInstance.GetField("Name", BindingFlags.Public | BindingFlags.Instance);
        public static FieldInfo IMI_Error = TModInstance.GetField("Error", BindingFlags.Public | BindingFlags.Instance);
        public static FieldInfo IMI_Enabled = TModInstance.GetField("Enabled", BindingFlags.Public | BindingFlags.Instance);
        public static Type TModErrorState = TModLoader.GetNestedType("ModErrorState");
        public static Type TErrorC = typeof(Nullable<>).MakeGenericType(TModErrorState);

        private static byte[] ModifyAssembly(string path)
        {
            using(MemoryStream stream = new MemoryStream())
            {
                AssemblyDefinition ass = AssemblyDefinition.ReadAssembly(path);
                ass.Write(stream);
                return stream.ToArray();
            }
        }
        public static void AddModInstance(Type type, Mod mod, bool enabled, string error, string name)
        {
            object mi = Activator.CreateInstance(TModInstance);
            var rmi = mi.CreateReflectionObject();
            /*IMI_Mod.SetValue(mi, mod);
            IMI_Name.SetValue(mi, name);
            IMI_Enabled.SetValue(mi, enabled);
            if (string.IsNullOrEmpty(error))
            {
                IMI_Error.SetValue(mi, Activator.CreateInstance(TErrorC, null));
            }
            else
            {
                IMI_Error.SetValue(mi, Activator.CreateInstance(TErrorC, Enum.Parse(TModErrorState, error)));
            }*/
            rmi.SetMemberData("Mod", mod);
            rmi.SetMemberData("Name", name);
            rmi.SetMemberData("Enabled", enabled);
            if (string.IsNullOrEmpty(error))
            {
                rmi.SetMemberData("Error", Activator.CreateInstance(TErrorC, null));
            }
            else
            {
                rmi.SetMemberData("Error", Activator.CreateInstance(TErrorC, Enum.Parse(TModErrorState, error)));
            }
            MAddModInstance.FastInvoke(null, type, mi);
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
                            DebugMods.Add(mod);
                            AddModInstance(type, mod, false, null, mod.GetName());
                        }
                    }
					catch (Exception e)
					{
                        LogError(e);
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
                    ass.Add(Assembly.Load(ModifyAssembly(v2)));
                }
                catch (Exception e)
                {
                    LogError(e);
                }
            }
            foreach(var v in ass)
            {
                LoadMod(v);
            }
        }
    }
}
