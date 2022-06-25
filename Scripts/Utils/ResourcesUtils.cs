
namespace HKTool;

public static class ResourcesUtils
{
    private static Dictionary<Type, Dictionary<string, UObject>> cache = new();
    public static Dictionary<string, UObject> FindAssets(IEnumerable<string> names, Type type)
    {
        
        Dictionary<string, UObject> dict = new();
        List<string> missAssets = new();
        var cacheT = cache.TryGetOrAddValue(type, () => new());
        foreach(var v in names)
        {
            if(cacheT.TryGetValue(v, out var obj)) dict[v] = obj;
            else missAssets.Add(v);
        }
        var assets = Resources.FindObjectsOfTypeAll(type);
        foreach(var v in assets)
        {
            if(v.GetInstanceID() < 0) continue;
            if(v is GameObject go && (go.transform.parent != null || go.scene.IsValid())) continue;
            if(missAssets.Contains(v.name))
            {
                dict[v.name] = v;
                cacheT[v.name] = v;
            }
        }
        return dict;
    }
    public static UObject? FindAsset(string name, Type type)
    {
        var cacheT = cache.TryGetOrAddValue(type, () => new());
        if(cacheT.TryGetValue(name, out var obj)) return obj;
        obj = Resources.FindObjectsOfTypeAll(type).FirstOrDefault(x => x.name == name && x.GetInstanceID() > 0);
        if(obj != null) cacheT[name] = obj;
        return obj;
    }
}

