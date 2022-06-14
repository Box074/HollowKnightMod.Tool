

namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public class ModResourcesListAttribute : Attribute
{
    public ModResourcesListAttribute(string[] names, int[] offset, int[] size)
    {
        this.names = names;
        this.offset = offset;
        this.size = size;
    }
    public readonly string[] names;
    public readonly int[] offset;
    public readonly int[] size;
}

