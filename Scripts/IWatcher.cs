

namespace HKTool;
public delegate void WatchHandler<in T>(T obj);
public interface IWatcher<T, T2> where T : class where T2 : class
{
    WatchHandler<T2> Handler { get; set; }
    IFilter<T> Filter { get; set; }
    void RemoveWatcher();
    void Try(T t);
}
public interface IWatcher<T> : IWatcher<T, T> where T : class
{

}

