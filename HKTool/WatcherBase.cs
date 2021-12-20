using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool
{
    public abstract class WatcherBase<T0, T, T2> : IWatcher<T, T2> where T0 : class where T : class where T2 : class
    {
        protected static List<WatcherBase<T0, T, T2>> watchers = new List<WatcherBase<T0, T, T2>>();
        protected static void Test(T obj)
        {
            foreach(var v in watchers)
            {
                v.Try(obj);
            }
        }
        public WatchHandler<T2> Handler { get; set; }
        public IFilter<T> Filter { get; set; }

        protected WatcherBase(IFilter<T> filter, WatchHandler<T2> handler)
        {
            Filter = filter;
            Handler = handler;
            watchers.Add(this);
        }

        public void RemoveWatcher()
        {
            watchers.Remove(this);
        }

        public void Try(T t)
        {
            if (Handler == null || Filter == null) return;
            try
            {
                if (Filter.Filter(t))
                {
                    CallHandler(t);
                }
            }
            catch (Exception e)
            {
                Modding.Logger.LogError(e);
            }
        }
        protected virtual void CallHandler(T obj)
        {
            Handler?.Invoke(obj as T2);
        }
    }
    public abstract class WatcherBase<T0, T> : WatcherBase<T0, T, T> , IWatcher<T> where T0 : class where T : class
    {
        protected WatcherBase(IFilter<T> filter, WatchHandler<T> handler) : base(filter, handler)
        {

        }
    }
}
