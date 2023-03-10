
namespace HKTool.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class PreloadAttribute : ModBase.PreloadAttribute
{
    public PreloadAttribute(string scene, string obj) : base(scene, obj)
    {
        
    }
    public PreloadAttribute(string scene, string obj, bool throwExceptionOnMissing) : base(scene, obj, throwExceptionOnMissing)
    {
        
    }
    public PreloadAttribute(string scene, string obj, bool throwExceptionOnMissing, bool setActive) : base(scene, obj, throwExceptionOnMissing, setActive)
    {
       
    }
}
