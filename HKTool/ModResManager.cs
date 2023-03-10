
namespace HKTool;

public static class ModResManager
{

    internal static void Init()
    {

    }

    public static byte[]? GetManifestResourceBytes(this Assembly ass, string name)
    {
        using (var s = ass.GetManifestResourceStream(name))
        {
            if (s is null) return null;
            byte[] result;

            if (s is MemoryStream ms)
            {
                result = ms.ToArray();
            }
            else
            {
                result = new byte[s.Length];
                s.Read(result, 0, result.Length);
            }
            return result;
        }
    }
}
