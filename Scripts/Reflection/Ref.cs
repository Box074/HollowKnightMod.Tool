
namespace HKTool.Reflection;
public class Ref<T>
{
    private Ref()
    {

    }
    public static implicit operator Ref<T>(IntPtr ptr)
    {
        return new()
        {
            ptr = ptr
        };
    }
    public static implicit operator IntPtr(Ref<T> r)
    {
        return r.ptr;
    }
    private IntPtr ptr;
    public ref T Value 
    {
        get
        {
            return ref GetFieldRefFrom<T>(ptr);
        }
    }

}
