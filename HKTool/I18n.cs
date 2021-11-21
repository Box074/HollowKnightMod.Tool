using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Language;

namespace HKTool
{
    public class I18n
    {
        public static List<I18n> Instances { get; private set; } = new List<I18n>();
        public Dictionary<string, string> Current { get; private set; } = new Dictionary<string, string>();
        public Dictionary<LanguageCode, string> Languages { get; private set; } = new Dictionary<LanguageCode, string>();
        public I18n()
        {
            Instances.Add(this);
        }
        public bool ChangeLang(LanguageCode code)
        {
            if(Languages.TryGetValue(code,out var v))
            {
                Current.Clear();
                foreach(var l in v.Split('\n'))
                {
                    var l2 = l;
                    if (l2.StartsWith("#")) continue;
                    int s = l2.IndexOf('=');
                    if (s == -1) continue;
                    var n = l2.Substring(0, s);
                    var val = l2.Substring(s + 1);
                    Current.Add(n, val);
                    Modding.Logger.Log($"Parse Lang: {n} = {val}");
                }
                return true;
            }
            return false;
        }
        public string Get(string key)
        {
            if (Current.TryGetValue(key, out var v)) return v;
            return key;
        }
        
        public static string GlobalGet(string key)
        {
            foreach(var v in Instances)
            {
                if(v.Current.TryGetValue(key,out var s))
                {
                    return s;
                }
            }
            return key;
        }
        public void AddLang(LanguageCode code,string data)
        {
            Languages[code] = data;
        }
        public void AddLang(LanguageCode code, Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            byte[] b = new byte[stream.Length];
            stream.Read(b, 0, (int)stream.Length);
            stream.Close();
            AddLang(code, Encoding.UTF8.GetString(b));
        }

        public void AddLang(LanguageCode src,LanguageCode dst)
        {
            if(Languages.TryGetValue(src,out var v))
            {
                Languages[dst] = v;
            }
        }
        public void ChangeToDefault(LanguageCode defaultCode = LanguageCode.EN)
        {
            if (!ChangeLang(Language.Language.CurrentLanguage()))
            {
                ChangeLang(defaultCode);
            }
        }
    }
}
