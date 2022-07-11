
namespace HKTool.Utils;

public class Holder<T> 
{
    public T value = default(T)!;
    public static implicit operator T(Holder<T> holder) => holder.value;
}
