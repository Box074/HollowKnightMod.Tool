
namespace HKTool;

public class I18n
{
    public static List<I18n> Instances { get; private set; } = new List<I18n>();
    internal static List<IBindI18n> waitBinds = new();
    public static void BeginBind(IBindI18n bind) => waitBinds.Add(bind);
    public static void EndBind(IBindI18n bind) => waitBinds.RemoveAll(x => ReferenceEquals(x, bind));
    public readonly string modName;
    public readonly string modDir;
    public event Action OnLanguageSwitch;
    public LanguageCode DefaultLanguage;
    public LanguageCode? CurrentCode { get; private set; }
    public Dictionary<string, string> Current
    { get; private set; } = new Dictionary<string, string>();
    public Dictionary<LanguageCode, string> Languages { get; private set; } = new Dictionary<LanguageCode, string>();
    public I18n(string modName = "", string modDir = "", LanguageCode defaultLanguage = LanguageCode.EN)
    {
        this.modName = modName;
        this.modDir = modDir;
        DefaultLanguage = defaultLanguage;
        Instances.Add(this);

        On.Language.Language.DoSwitch += (orig, lang) =>
        {
            orig(lang);
            TrySwitch();
        };
    }
    public bool UseLanguageHook
    {
        set
        {
            Modding.ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
            if (value)
            {
                Modding.ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
            }
        }
    }

    private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (TryGet(key, out var s))
        {
            return s;
        }
        return orig;
    }
    public bool ChangeLanguage(SupportedLanguages lang) => ChangeLanguage((LanguageCode)lang);
    public bool ChangeLanguage(LanguageCode code)
    {
        if(CurrentCode == code) return true;
        if (Languages.TryGetValue(code, out var v))
        {
            Current.Clear();
            foreach (var l in v.Split('\n'))
            {
                var l2 = l;
                if (l2.StartsWith("#")) continue;
                int s = l2.IndexOf('=');
                if (s == -1) continue;
                var n = l2.Substring(0, s);
                var val = l2.Substring(s + 1).Replace("\\n", "\n");
                Current.Add(n, val);
                HKToolMod.Instance.LogFine($"I18n: {n} = {val}");
            }
            CurrentCode = code;
            if (OnLanguageSwitch is not null)
            {
                foreach (var v2 in OnLanguageSwitch.GetInvocationList())
                {
                    try
                    {
                        v2.DynamicInvoke();
                    }
                    catch (Exception e)
                    {
                        HKToolMod.Instance.LogError(e);
                    }
                }
            }
            return true;
        }
        return false;
    }
    public string Get(string key)
    {
        if (Current.TryGetValue(key, out var v))
        {
            foreach (var v2 in waitBinds) v2.BindI18n(this);
            return v;
        }
        return key;
    }
    public bool TryGet(string key, out string val)
    {
        bool r = Current.TryGetValue(key, out val);
        if (r)
        {
            foreach (var v in waitBinds) v.BindI18n(this);
        }
        return r;
    }

    public static string GlobalGet(string key)
    {
        foreach (var v in Instances)
        {
            if (v.TryGet(key, out var s))
            {
                return s;
            }
        }
        return key;
    }
    public void AddLanguage(SupportedLanguages lang, string data)
    {
        AddLanguage((LanguageCode)lang, data);
    }
    public void AddLanguage(LanguageCode code, string data)
    {
        Languages[code] = data;
    }
    public void AddLanguage(SupportedLanguages lang, Stream stream, bool closeStream = true)
    {
        AddLanguage((LanguageCode)lang, stream, closeStream);
    }
    public void AddLanguage(LanguageCode code, Stream stream, bool closeStream = true)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        byte[] b = new byte[stream.Length];
        stream.Read(b, 0, (int)stream.Length);
        if (closeStream) stream.Close();
        AddLanguage(code, Encoding.UTF8.GetString(b));
    }

    public void AddLanguage(LanguageCode src, LanguageCode dst)
    {
        if (Languages.TryGetValue(src, out var v))
        {
            Languages[dst] = v;
        }
    }
    public void AddLanguage(SupportedLanguages src, SupportedLanguages dst)
    {
        AddLanguage((LanguageCode)src, (LanguageCode)dst);
    }
    public void UseGameLanguage(SupportedLanguages? defaultCode = null, bool allowExternLang = false)
    {
        UseGameLanguage((LanguageCode?)defaultCode, allowExternLang);
    }
    public void UseGameLanguage(LanguageCode? defaultCode = null, bool allowExternLang = false)
    {
        var current = Language.Language.CurrentLanguage();
        var dcode = defaultCode ?? DefaultLanguage;
        if (!ChangeLanguage(current))
        {
            if (allowExternLang)
            {
                var langName = $"{modName}.{current.ToString().ToLower()}.lang";
                var langPath = Path.Combine(modDir, langName);
                if (File.Exists(langPath))
                {
                    AddLanguage(current, File.ReadAllText(langPath));
                    ChangeLanguage(current);
                }
                else if (File.Exists(langName))
                {
                    AddLanguage(current, File.ReadAllText(langName));
                    ChangeLanguage(current);
                }
                else
                {
                    HKToolMod.Instance.LogWarn($"[{modName}]Missing Language: {current.ToString()}");
                    ChangeLanguage(dcode);
                }
                return;
            }
            HKToolMod.Instance.LogWarn($"[{modName}]Missing Language: {current.ToString()}");
            ChangeLanguage(dcode);
        }
    }
    public void TrySwitch()
    {
        UseGameLanguage(DefaultLanguage, true);
    }
}

