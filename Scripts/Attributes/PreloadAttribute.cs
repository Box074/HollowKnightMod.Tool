
namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class PreloadAttribute : Attribute
{
    public PreloadAttribute(string scene, string obj)
    {
        sceneName = scene;
        objPath = obj;
    }
    public string sceneName;
    public string objPath;
}
