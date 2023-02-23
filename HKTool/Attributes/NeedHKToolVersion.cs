
namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public class NeedHKToolVersionAttribute : Attribute
{
    [Obsolete("Please fill in version", true)]
    public NeedHKToolVersionAttribute()
    {

    }
    public NeedHKToolVersionAttribute(string ver)
    {
        version = ver;
    }
    public string version = ModBase.compileVersion;
}
