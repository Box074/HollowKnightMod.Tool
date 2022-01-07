using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Class1
    {
        public static object A(object o1)
        {
            ref object o2 = ref o1;
            o2 = 0;
            return o2;
        }
    }
}
