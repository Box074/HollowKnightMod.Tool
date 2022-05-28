
namespace HKTool.Patcher;

[AttributeUsage(AttributeTargets.All)]
public class PatchCallerAttribute : Attribute
{
    public PatchCallerAttribute(Type type, string methodName) {}
}
