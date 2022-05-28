
namespace HKTool.Reflection;
public class Ref<T> where T : class
{
    private Ref()
    {

    }
    [NoModify]
    
    public static implicit operator Ref<T>(IntPtr ptr)
    {
        return new()
        {
            ptr = ptr
        };
    }
    [CustomPatcher(typeof(RefPatcher), nameof(RefPatcher.Patch0))]
    [return: NoModify]
    public static implicit operator Ref<T>(long ptr)
    {
        return new()
        {
            ptr = ptr
        };
    }
    [NoModify]
    public static implicit operator IntPtr(Ref<T> r)
    {
        return r.ptr;
    }
    [CustomPatcher(typeof(RefPatcher), nameof(RefPatcher.Patch1))]
    public static implicit operator long([NoModify] Ref<T> r)
    {
        return r.ptr;
    }
    private Ref<T> ptr = null!;
    public ref T Value 
    {
        [PatchCaller(typeof(InternalPatcher), nameof(InternalPatcher.Patch_Nop))]
        get
        {
            return ref GetFieldRefFrom<T>(ptr);
        }
    }

}

static class RefPatcher
{
    public static void Patch0(IMemberDefinition member)
    {
        if(member is MethodDefinition m)
        {
            m.Parameters[0].ParameterType = new ByReferenceType(m.DeclaringType.GenericParameters[0]);
        }
    }
    public static void Patch1(IMemberDefinition member)
    {
        if(member is MethodDefinition m)
        {
            m.ReturnType = new ByReferenceType(m.DeclaringType.GenericParameters[0]);
        }
    }
}

