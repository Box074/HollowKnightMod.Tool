
namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Class)]
class ModuleDefineAttribute : Attribute
{
    public ModuleDefineAttribute(string name, string version)
    {
        this.name = name.ToLower();
        ver = new Version(version);
    }
    public string name;
    public Version ver;
}
