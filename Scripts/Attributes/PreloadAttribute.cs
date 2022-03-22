
namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class PreloadAttribute : Attribute
{
    public PreloadAttribute(string scene, string obj)
    {
        sceneName = scene;
        objPath = obj;
    }
    public PreloadAttribute(string scene, string obj, bool throwExceptionOnMissing) : this(scene, obj)
    {
        this.throwExceptionOnMissing = throwExceptionOnMissing;
    }
    public string sceneName;
    public string objPath;
    public bool throwExceptionOnMissing = true;
}
