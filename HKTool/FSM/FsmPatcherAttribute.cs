using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.FSM
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FsmPatcherAttribute : Attribute
    {
        public string sceneName = "";
        public string objName = "";
        public string fsmName = "";
        public FsmPatcherAttribute(string sceneName = "", string objName = "", string fsmName = "")
        {
            this.sceneName = sceneName;
            this.objName = objName;
            this.fsmName = fsmName;
        }
    }
}
