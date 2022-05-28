
namespace HKTool.Patcher;

[AttributeUsage(AttributeTargets.All)]
public class CustomPatcherAttribute : Attribute
{
    public CustomPatcherAttribute(Type type, string methodName) {}
}
