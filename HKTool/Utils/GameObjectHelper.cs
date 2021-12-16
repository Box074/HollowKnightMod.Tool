using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HKTool.Utils
{
    public static class GameObjectHelper
    {
        public static IEnumerable<GameObject> ForEachChildren(this GameObject parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            var t = parent.transform;
            var cc = t.childCount;
            for(int i = 0; i< cc; i++)
            {
                var c = t.GetChild(i).gameObject;
                yield return c;
                foreach (var v in c.ForEachChildren()) yield return v;
            }
        }
        public static IEnumerable<GameObject> ForEachGameObjects(this Scene scene)
        {
            foreach(var v in scene.GetRootGameObjects())
            {
                yield return v;
                foreach (var v2 in v.ForEachChildren()) yield return v2;
            }
        }
    }
}
