
namespace System.Runtime.CompilerServices;

public static class RuntimeHelpers
{
    public static T[] GetSubArray<T>(T[] array, Range range)
    {
        if (array == null)
        {
            throw new ArgumentOutOfRangeException("array");
        }
        (int Offset, int Length) = range.GetOffsetAndLength(array.Length);
        T[] array2;
        if (typeof(T).IsValueType || typeof(T[]) == array.GetType())
        {
            if (Length == 0)
            {
                return Array.Empty<T>();
            }
            array2 = new T[Length];
        }
        else
        {
            array2 = UnsafeUtils.UnsafeCast<Array, T[]>(Array.CreateInstance(array.GetType().GetElementType(), Length));
        }
        StaticArray.FastCopy(array, Offset, array2, 0, Length);
        return array2;
    }
}
