
namespace HKTool.Data;

public abstract class StructArrayProvider<T> : IReadOnlyList<T> where T : struct
{
    public abstract T this[int index] { get; }
    public abstract int Count { get; }
    public abstract IEnumerator<T> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator StructArrayProvider<T>(T[] array) => new SimpleStructArrayProvider<T>(array);
    public static implicit operator StructArrayProvider<T>(List<T> array) => new SimpleStructListProvider<T>(array);
    public static implicit operator StructArrayProvider<T>(NativeArray<T> array) => new NativeStructArrayProvider<T>(array);
}

internal class SimpleStructArrayProvider<T> : StructArrayProvider<T> where T : struct
{
    private T[] array;
    public SimpleStructArrayProvider(T[] array) => this.array = array;
    public SimpleStructArrayProvider(List<T> array) => this.array = array.ToArray();
    public SimpleStructArrayProvider(IEnumerable<T> ie) => this.array = ie.ToArray();
    public override T this[int index] => array[index];
    public override int Count => array.Length;
    public override IEnumerator<T> GetEnumerator() => (IEnumerator<T>)array.GetEnumerator();
}
internal class SimpleStructListProvider<T> : StructArrayProvider<T> where T : struct
{
    private List<T> array;
    public SimpleStructListProvider(List<T> array) => this.array = array;
    public override T this[int index] => array[index];
    public override int Count => array.Count;
    public override IEnumerator<T> GetEnumerator() => (IEnumerator<T>)array.GetEnumerator();
}
internal class NativeStructArrayProvider<T> : StructArrayProvider<T> where T : struct
{
    private NativeArray<T> array;
    public NativeStructArrayProvider(NativeArray<T> array) => this.array = array;
    public override T this[int index] => array[index];
    public override int Count => array.Length;
    public override IEnumerator<T> GetEnumerator() => array.GetEnumerator();
}
