

namespace HKTool;
public class SingleMonoBehaviour<T> : MonoBehaviour where T : SingleMonoBehaviour<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject($"SingleBehaviour {typeof(T).Name}");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<T>();
            }
            return _instance;
        }
    }
}

