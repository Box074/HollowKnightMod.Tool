
namespace HKTool;

[Obsolete]
public static class EmbeddedResHelper
{
    public static byte[] GetBytes(Assembly ass, string name, bool _1) => ass.GetManifestResourceBytes(name)!;
    public static Stream GetStream(Assembly ass, string name, bool _1) => ass.GetManifestResourceStream(name)!;
}
