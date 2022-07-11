
namespace HKTool;
public static class I18nHelper
{
    public static string Localize(this string key)
    {
        return I18n.GlobalGet(key);
    }
    public static string LocalizeFormat(this string key, params object[] args)
    {
        return string.Format(key.Localize(), args);
    }
    [Obsolete]
    public static string Get(string key)
    {
        return I18n.GlobalGet(key);
    }
    [Obsolete]
    public static string GetFormat(string key, params object[] args)
    {
        return string.Format(key.Localize(), args);
    }
}

