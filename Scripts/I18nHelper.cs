
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
    public static string GetA(string key)
    {
        return I18n.GlobalGet(key);
    }
    [Obsolete]
    public static string GetFormatA(string key, params object[] args)
    {
        return string.Format(key.Localize(), args);
    }
}

