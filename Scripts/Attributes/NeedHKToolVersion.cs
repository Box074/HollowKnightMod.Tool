
namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public class NeedHKToolVersionAttribute : Attribute
{
    public NeedHKToolVersionAttribute()
    {
        version = ModBase.compileVersion;
    }
    public NeedHKToolVersionAttribute(string ver)
    {
        version = ver;
    }
    public string version = "";
}
