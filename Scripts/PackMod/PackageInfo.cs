
namespace HKTool.PackMod;

[Serializable]
class PackageInfo
{
    public int? MinApiVersion = null;
    public string MinHKToolVersion = typeof(PackageInfo).Assembly.GetName().Version.ToString();
    public string MainModule = "main";
}
