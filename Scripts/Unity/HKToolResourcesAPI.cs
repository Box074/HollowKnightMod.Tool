
namespace HKTool.Unity;
class HKToolResourcesAPI : ResourcesAPI
{
    public static void Init()
    {
        overrideAPI = new HKToolResourcesAPI();
    }
    protected override UObject Load(string path, Type systemTypeInstance)
    {
        var asset = base.Load(path, systemTypeInstance);
        if(asset != null) return asset;
        foreach(var v in UObject.FindObjectsOfType<AssetBundle>())
        {
            asset = v.LoadAsset(path, systemTypeInstance);
            if(asset != null)
            {
                return asset;
            }
        }
        return null;
    }
    protected override Shader FindShaderByName(string name)
    {
        var shader = base.FindShaderByName(name);
        if(shader!=null) return shader;
        foreach(var v in UObject.FindObjectsOfType<AssetBundle>())
        {
            shader = v.LoadAsset<Shader>(name);
            if(shader != null) return shader;
        }
        return null;
    }
    protected override UObject[] FindObjectsOfTypeAll(Type systemTypeInstance)
    {
        List<UObject> objs = new();
        foreach(var v in UObject.FindObjectsOfType<AssetBundle>())
        {
            objs.AddRange(v.LoadAllAssets(systemTypeInstance));
        }
        objs.AddRange(base.FindObjectsOfTypeAll(systemTypeInstance));
        return objs.ToArray();
    }
}
