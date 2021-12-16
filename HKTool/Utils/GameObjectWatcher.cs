using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HKTool.Utils
{
    public delegate void GameObjectCatchHandler(GameObject go);
    public class GameObjectWatcher : IWatcher<GameObject>
    {
        private readonly static List<GameObjectWatcher> watchers = new List<GameObjectWatcher>();
        static GameObjectWatcher()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (watchers.Count == 0) return;
            foreach(var v in arg0.ForEachGameObjects())
            {
                foreach(var v2 in watchers)
                {
                    v2.Try(v);
                }
            }
        }

        public IGameObjectFilter Filter { get; set; }
        public GameObjectCatchHandler Handler { get; set; }
        public GameObjectWatcher(IGameObjectFilter filter, GameObjectCatchHandler handler)
        {
            Filter = filter;
            Handler = handler;
            watchers.Add(this);
        }
        public void RemoveWatcher()
        {
            watchers.Remove(this);
        }
        public void Try(GameObject go)
        {
            try
            {
                if (Handler == null) return;
                if (Filter.Filter(go))
                {
                    Handler(go);
                }
            }catch(Exception e)
            {
                Modding.Logger.LogError(e);
            }
        }
    }
}
