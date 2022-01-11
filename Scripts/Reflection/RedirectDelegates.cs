using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.Reflection.Runtime
{
    public delegate object RD_CallMethod(object @this, object[] args, Type[] genTypes);
    public delegate object RD_GetField(object @this);
    public delegate void RD_SetField(object @this, object val);
    public delegate object RD_CreateInstance(object[] args);
}
