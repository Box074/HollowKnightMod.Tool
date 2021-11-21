using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool
{
    public static class I18nHelper
    {
        public static string Get(this string key)
        {
            return I18n.GlobalGet(key);
        }
    }
}
