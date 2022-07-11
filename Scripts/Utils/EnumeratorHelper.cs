
namespace HKTool.Utils;

public static class EnumeratorHelper
{
    public static ResettableEnumerator AsResettable(this IEnumerator template) => new(template);
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
        if(_current == null)
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