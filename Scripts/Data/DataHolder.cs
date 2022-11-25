
namespace HKTool.Data;

public struct DataHolder<T> where T : struct
{
    public T value { 
        get
        {
            return _commonHolder?.value ?? _value;
        }
    }
    private DataCommonHolder<T>? _commonHolder = null;
    private T _value;
    private Action<T, T>? _trigger;
    public DataHolder()
    {
        lastPos = Ref<DataHolder<T>>.From(ref this);
    }
    private Ref<DataHolder<T>> lastPos;
    private Action<T, DataHolder<T>>? _onUpdate;
    public event Action<T, DataHolder<T>> onUpdate
    {
        add
        {
            if(_commonHolder == null) return;
            Ref<DataHolder<T>> self = Ref<DataHolder<T>>.From(ref this);
            if(self.Ptr != lastPos.Ptr)
            {
                _trigger = null;
                _onUpdate = null;
            }
            if(_trigger == null)
            {
                _trigger = (orig, n) =>
                {
                    self.Value._onUpdate?.Invoke(orig, self.Value);
                };
                _commonHolder!.onUpdate += _trigger;
            }
            _onUpdate += value;
        }
        remove
        {
            if(_commonHolder == null) return;
            _onUpdate -= value;
        }
    }
    public static implicit operator DataHolder<T>(T value) => new(){ _value = value };
    public static implicit operator DataHolder<T>(DataCommonHolder<T> value) => new(){ _commonHolder = value };
    public static implicit operator T(DataHolder<T> holder) => holder.value;
}
public class DataCommonHolder<T> where T : struct
{
    private T _value;
    public T value
    {
        get
        {
            return _value;
        }
        set
        {
            var orig = _value;
            _value = value;
            onUpdate?.Invoke(orig, _value);
        }
    }
    public static implicit operator DataCommonHolder<T>(T value) => new(){ _value = value };
    public static implicit operator T(DataCommonHolder<T> holder) => holder.value;
    public event Action<T, T>? onUpdate;
}

