using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.CompilerAttributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FakeNameAttribute : Attribute
    {
        public string TargetName = "";
    }
}
