
namespace HKTool;

[ModuleDefine("HKTool.EmbeddedResHelper", "0.1")]
public static class EmbeddedResHelper
{
    private readonly static Dictionary<Assembly, Dictionary<string, byte[]>> caches = new();
    private static byte[] LoadCompression(Stream s)
    {
        using(var gzip = new GZipStream(s, CompressionMode.Decompress))
        {
            using(var ms = new MemoryStream())
            {
                gzip.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
    private static byte[] LoadIn(Assembly ass, string name, bool saveCache = true)
    {
        using (var stream = ass.GetManifestResourceStream(name))
        {
            if(stream == null) throw new FileNotFoundException($"\"{name}\" in {ass.FullName}");
            byte[] data;
            if (ass.IsDefined(typeof(EmbeddedResourceCompressionAttribute)))
            {
                data = LoadCompression(stream);
            }
            else
            {
                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
            }
            if(saveCache)
            {
                if(!caches.TryGetValue(ass, out var res))
                {
                    res = new();
                    caches.Add(ass, res);
                }
                res[name] = (byte[]) data.Clone();
            }
            return data;
        }
    }
    public static byte[] GetBytes(Assembly ass, string name, bool useCache = true)
    {
        if(useCache)
        {
            if(caches.TryGetValue(ass, out var res))
            {
                if(res.TryGetValue(name, out var d)) return (byte[])d.Clone();
            }
        }
        return LoadIn(ass, name, useCache);
    }
    public static Stream GetStream(Assembly ass, string name, bool useCache = true)
    {
        if(useCache)
        {
            if(caches.TryGetValue(ass, out var res))
            {
                if(res.TryGetValue(name, out var d)) return new MemoryStream(d, false);
            }
        }
        return new MemoryStream(LoadIn(ass, name, useCache), false);
    }
}
