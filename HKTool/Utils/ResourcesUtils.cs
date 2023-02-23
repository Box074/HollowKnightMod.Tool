
namespace HKTool;

public static class ResourcesUtils
{
    private static Dictionary<Type, Dictionary<string, UObject>> cache = new();
    public static string GetMatchName(UObject obj)
    {
        {
            var ox = obj;
            var t = obj.GetType();
            if (t.IsSubclassOf(typeof(Component)))
            {
                obj = ((Component)ox).gameObject;
                t = typeof(GameObject);
            }
            if (t == typeof(GameObject))
            {
                var go = (GameObject)obj;
                if (go.transform.parent == null)
                {
                    return go.name;
                }
                else
                {
                    return go.GetPath();
                }
            }
            return ox.name;
        }
    }
    public static Dictionary<string, UObject> FindAssets(IEnumerable<string> names, Type type)
    {
        Dictionary<string, UObject> dict = new();
        List<string> missingAssets = new();
        var cacheT = cache.TryGetOrAddValue(type, () => new());
        foreach (var v in names)
        {
            if (cacheT.TryGetValue(v, out var obj) && obj != null) dict[v] = obj;
            else missingAssets.Add(v);
        }
        if (missingAssets.Count > 0)
        {
            IEnumerable<(UObject obj, string matchname)> assets = UnityEngine.Resources.FindObjectsOfTypeAll(type)
                                                                            .Where(x => (x?.GetInstanceID() ?? 0) > 0)
                                                                            .Select(x => (x, ResourcesUtils.GetMatchName(x)));
            foreach (var v in assets)
            {
                if (v.obj is GameObject go && go.scene.IsValid()) continue;
                if (missingAssets.Contains(v.matchname))
                {
                    dict[v.matchname] = v.obj;
                    cacheT[v.matchname] = v.obj;
                }
            }
        }
        return dict;
    }
    public static UObject? FindAsset(string name, Type type)
    {
        var cacheT = cache.TryGetOrAddValue(type, () => new());
        if (cacheT.TryGetValue(name, out var obj) && obj != null) return obj;
        obj = Resources.FindObjectsOfTypeAll(type).FirstOrDefault(x => x.GetInstanceID() > 0 && GetMatchName(x) == name);
        if (obj != null) cacheT[name] = obj;
        return obj;
    }
    public static T? FindAsset<T>(string name) where T : UObject => (T?)FindAsset(name, typeof(T));
}

