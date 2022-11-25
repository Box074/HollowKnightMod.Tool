

namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public class ModResourcesListAttribute : Attribute
{
    public ModResourcesListAttribute(string[] names, int[] offset, int[] size, bool[]? compress, bool embedded)
    {
        this.names = names;
        this.offset = offset;
        this.size = size;
        this.compress = compress;
        this.embedded = embedded;
    }
    public ModResourcesListAttribute(string[] names, int[] offset, int[] size, bool[]? compress) : this(names, offset, size, compress, false) {}
    public ModResourcesListAttribute(string[] names, int[] offset, int[] size): this(names, offset, size, null) {}
    public readonly string[] names;
    public readonly int[] offset;
    public readonly int[] size;
    public readonly bool[]? compress;
    public readonly bool embedded;
}

