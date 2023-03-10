

namespace HKTool.Utils;
public class GameObjectWatcher : WatcherBase<GameObjectWatcher, GameObject>
{
    static GameObjectWatcher()
    {
        USceneManager.activeSceneChanged += SceneManager_sceneLoaded;
    }

    private static void SceneManager_sceneLoaded(Scene arg0, Scene arg1)
    {
        if (watchers.Count == 0) return;
        foreach (var v in arg1.ForEachGameObjects())
        {
            foreach (var v2 in watchers)
            {
                v2.Try(v);
            }
        }
    }

    public GameObjectWatcher(IFilter<GameObject> filter, WatchHandler<GameObject> handler) : base(filter, handler)
    {
    }
}

