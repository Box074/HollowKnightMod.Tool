using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HKTool
{
    public class SingleMonoBegaviour<T> : MonoBehaviour where T : SingleMonoBegaviour<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if(_instance == null)
                {
                    var go = new GameObject($"SingleBehaviour {typeof(T).Name}");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<T>();
                }
                return _instance;
            }
        }
        void S()
        {
            StartCoroutine
        }
    }
}
