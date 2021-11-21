using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.FSM
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FsmPatchAttribute : Attribute
    {
        public string sceneName = "";
        public string objName = "";
        public string fsmName = "";
        public FsmPatchAttribute(string sceneName = "", string objName = "", string fsmName = "")
        {
            this.sceneName = sceneName;
            this.objName = objName;
            this.fsmName = fsmName;
        }
    }
}
