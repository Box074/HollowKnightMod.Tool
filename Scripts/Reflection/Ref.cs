
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
    public static Ref<T> From(ref T ptr) => GetRefPointer(ref ptr);
    public static implicit operator IntPtr(Ref<T> r)
    {
        return r.ptr;
    }
    private IntPtr ptr;
    public Ref<TCast> Cast<TCast>() => Ptr;
    public ref T Value 
    {
        get
        {
            return ref GetFieldRefFrom<T>(ptr);
        }
    }
    public IntPtr Ptr 
    {
        get => ptr;
        set => ptr = value;
    }

}
