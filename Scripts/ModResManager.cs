
namespace HKTool;

public static class ModResManager
{
    internal static void Init()
    {
        HKToolMod.logger.Log("Init ModResManager");
        On.System.Reflection.Assembly.GetManifestResourceStream_string += (orig, self, name) =>
        {
            HKToolMod.logger.LogFine($"GetManifestResourceStream: '{name}' in '{self.FullName}'");
            var stream = orig(self, name);
            if (self.IsDefined(typeof(EmbeddedResourceCompressionAttribute)) && stream is not null)
            {
                using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
                {

                    var ms = new MemoryStream();
                    gzip.CopyTo(ms);
                    ms.Position = 0;
                    HKToolMod.logger.LogFine($"GetManifestResourceStream Decompress({ms.Length} byte): '{name}' in '{self.FullName}'");
                    return ms;
                }
            }
            return stream;
        };
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
            HKToolMod.logger.LogFine($"GetManifestResourceBytes: {name}({result.Length} byte)");
            return result;
        }
    }
}
