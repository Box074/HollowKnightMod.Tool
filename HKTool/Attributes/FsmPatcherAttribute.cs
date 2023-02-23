
namespace HKTool.FSM;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class FsmPatcherAttribute : Attribute
{
    public string sceneName = "";
    public string objName = "";
    public string fsmName = "";
    public bool useRegex = false;
    public FsmPatcherAttribute(bool useRegex, string sceneName = "", string objName = "", string fsmName = "")
    {
        this.useRegex = useRegex;
        this.sceneName = sceneName;
        this.objName = objName;
        this.fsmName = fsmName;
    }
    public FsmPatcherAttribute(string sceneName = "", string objName = "", string fsmName = "")
    {
        this.sceneName = sceneName;
        this.objName = objName;
        this.fsmName = fsmName;
    }
}

