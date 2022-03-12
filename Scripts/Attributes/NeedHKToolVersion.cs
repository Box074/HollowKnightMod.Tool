
namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public class NeedHKToolVersionAttribute : Attribute
{
    [Obsolete("Please fill in Version or use NeedHKToolVersionAttribute(default)", true)]
    public NeedHKToolVersionAttribute()
    {

    }
    public NeedHKToolVersionAttribute(string ver = ModBase.compileVersion)
    {
        version = ver;
    }
    public string version = ModBase.compileVersion;
}
