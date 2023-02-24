using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool
{
    public class FieldRefHolder<T>
    {
        private object _obj;
        private FieldInfo _fieldInfo;
        public FieldRefHolder(object obj, FieldInfo fieldInfo)
        {
            _obj = obj;
            _fieldInfo = fieldInfo;
        }
        public FieldRefHolder(object obj, string name) : this(obj, obj!.GetType().GetField(name, HReflectionHelper.All)) 
        {
        }

        public T Value
        {
            get => (T)_fieldInfo.FastGet(_obj)!;
            set => _fieldInfo.FastSet(_obj, value);
        }
    }
}
