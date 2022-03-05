
namespace HKTool;
public static class I18nHelper
{
    public static string Get(this string key)
    {
        return I18n.GlobalGet(key);
    }
    public static string GetFormat(this string key, params object[] args)
    {
        return string.Format(key.Get(), args);
    }
}

