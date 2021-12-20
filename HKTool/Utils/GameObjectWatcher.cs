using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HKTool.Utils
{
    public class GameObjectWatcher : WatcherBase<GameObjectWatcher, GameObject>
    {
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

        public GameObjectWatcher(IFilter<GameObject> filter, WatchHandler<GameObject> handler) : base(filter, handler)
        {
        }
    }
}
