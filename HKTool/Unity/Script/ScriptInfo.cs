
namespace HKTool.Unity;

public class RuntimeInitializeOnLoads
{
    public class RuntimeInitializeOnLoadsItem
    {
        public string assemblyName = "";
        public string nameSpace = "";
        public string className = "";
        public string methodName = "";
        public RuntimeInitializeLoadType loadTypes = RuntimeInitializeLoadType.AfterAssembliesLoaded;
        public bool isUnityClass = false;
    }
    public List<RuntimeInitializeOnLoadsItem> root = new();
    private static readonly string JSON_PATH = Path.Combine(Application.dataPath, "RuntimeInitializeOnLoads.json");
    public static RuntimeInitializeOnLoads Load()
    {
        return JsonConvert.DeserializeObject<RuntimeInitializeOnLoads>(File.ReadAllText(JSON_PATH))!;
    }
    public void Save()
    {
        File.WriteAllText(JSON_PATH, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
public class ScriptingAssemblies
{
    public enum ScriptingAssemblyType
    {
        UnityLibrary = 2,
        CustomAssembly = 16
    }
    public List<string> names = new();
    public List<ScriptingAssemblyType> types = new();
    public void AddAssembly(string path, ScriptingAssemblyType type = ScriptingAssemblyType.CustomAssembly)
    {
        path = Path.GetFullPath(path).Substring(Application.dataPath.Length + "/Managed/".Length);

        int id = names.IndexOf(path);
        if(id == -1)
        {
            names.Add(path);
            types.Add(type);
            return;
        }
        types[id] = type;
    }
    public void Remove(string path)
    {
        path = Path.GetFullPath(path).Substring(Application.dataPath.Length + "/Managed/".Length);
        int id = names.IndexOf(path);
        if(id == -1) return;
        types.RemoveAt(id);
        names.RemoveAt(id);
    }
    private static readonly string JSON_PATH = Path.Combine(Application.dataPath, "ScriptingAssemblies.json");
    public static ScriptingAssemblies Load()
    {
        return JsonConvert.DeserializeObject<ScriptingAssemblies>(File.ReadAllText(JSON_PATH))!;
    }
    public void Save()
    {
        File.WriteAllText(JSON_PATH, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
