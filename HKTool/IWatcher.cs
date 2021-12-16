using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool
{
    public interface IWatcher<T>
    {
        void RemoveWatcher();
        void Try(T t);
    }
}
