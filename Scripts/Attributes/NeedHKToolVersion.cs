
namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public class NeedHKToolVersionAttribute : Attribute
{
    public string version = "";
}
