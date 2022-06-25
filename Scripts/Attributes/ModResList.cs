

namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public class ModResourcesListAttribute : Attribute
{
    public ModResourcesListAttribute(string[] names, int[] offset, int[] size, bool[]? compress)
    {
        this.names = names;
        this.offset = offset;
        this.size = size;
        this.compress = compress;
    }
    public ModResourcesListAttribute(string[] names, int[] offset, int[] size): this(names, offset, size, null) {}
    public readonly string[] names;
    public readonly int[] offset;
    public readonly int[] size;
    public readonly bool[]? compress;
}

