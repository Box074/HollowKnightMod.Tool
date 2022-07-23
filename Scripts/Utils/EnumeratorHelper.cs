
namespace HKTool.Utils;

public static class EnumeratorHelper
{
    public static ResettableEnumerator AsResettable(this IEnumerator template) => new(template);
    public static CatchEnumerator<T> Catch<T>(this IEnumerator ie, Action<T> handler) where T : Exception => new(ie, handler);
}
public class FlatEnumerator : IEnumerator
{
    private IEnumerator _ie;
    internal FlatEnumerator(IEnumerator ie)
    {
        _ie = ie;
        executionStack.Push(_ie);
    }
    public bool MoveNext()
    {
        var result = CurrentIEnumerator.MoveNext();
        _current = CurrentIEnumerator.Current;
        if (_current is IEnumerator ie)
        {
            executionStack.Push(ie);
        }
        if(result == false)
        {
            if(executionStack.TryPop(out _))
            {
                return MoveNext();
            }
        }
        return result;
    }
    public IEnumerator CurrentIEnumerator => executionStack.Peek();
    private Stack<IEnumerator> executionStack = new();
    private object? _current;
    public object Current => _current!;
    public void Reset()
    {
        executionStack.Clear();
        executionStack.Push(_ie);
        _ie.Reset();
    }
}
public class CatchEnumerator<T> : IEnumerator where T : Exception
{
    private IEnumerator _ie;
    private Action<T> _handler;
    internal CatchEnumerator(IEnumerator ie, Action<T> handler)
    {
        _ie = ie;
        _handler = handler;
    }
    public bool MoveNext()
    {
        if (skip) return false;
        try
        {
            var result = _ie.MoveNext();
            _current = _ie.Current;
            if (_current is IEnumerator ie)
            {
                _current = ie.Catch<T>(ex =>
                {
                    skip = true;
                    _handler(ex);
                });
            }
            return result;
        }
        catch (T e)
        {
            _handler(e);
            return false;
        }
    }
    private bool skip = false;
    private object? _current;
    public object Current => _current!;
    public void Reset() => _ie.Reset();
}
public class ResettableEnumerator : IEnumerator
{
    private readonly IEnumerator _template;
    private IEnumerator? _current;
    internal ResettableEnumerator(IEnumerator template)
    {
        _template = template.MemberwiseClone();
        Reset();
    }
    private object _currentObj = null!;
    public bool MoveNext()
    {
        if (_current == null)
        {
            _current = _template.MemberwiseClone();
        }
        bool result = _current.MoveNext();
        _currentObj = _current.Current;
        return result;
    }
    public object Current => _currentObj;
    public void Reset()
    {
        _current = null;
    }
}