
namespace HKTool;

[System.Serializable]
public class MissingPreloadObjectException : System.Exception
{
    public MissingPreloadObjectException() { }
    public MissingPreloadObjectException(string sceneName, string objName) : 
        base("HKTool.MissingPreloadObjectException".GetFormat(sceneName, objName)) { }
    public MissingPreloadObjectException(string message) : base(message) { }
    public MissingPreloadObjectException(string message, System.Exception inner) : base(message, inner) { }
    protected MissingPreloadObjectException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
