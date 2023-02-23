
namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
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
    public PreloadAttribute(string scene, string obj, bool throwExceptionOnMissing, bool setActive) : this(scene, obj, throwExceptionOnMissing)
    {
        this.setActive = setActive;
    }
    public bool setActive;
    public string sceneName;
    public string objPath;
    public bool throwExceptionOnMissing = false;
}
