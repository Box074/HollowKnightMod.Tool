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
            foreach (var v in parent.GetComponentsInChildren<Transform>()) yield return v.gameObject;
        }
        public static GameObject FindChild(this GameObject parent, string name)
        {
            return parent.ForEachChildren().FirstOrDefault(x => x.name == name);
        }
        public static IEnumerable<GameObject> ForEachGameObjects(this Scene scene)
        {
            foreach(var v in scene.GetRootGameObjects())
            {
                yield return v;
                foreach (var v2 in v.ForEachChildren()) yield return v2;
            }
        }
        public static GameObject FindGameObject(this Scene scene, string name)
        {
            return scene.ForEachGameObjects().FirstOrDefault(
                x => x.name.Equals(name, StringComparison.Ordinal) || x.GetPath().Equals(name, StringComparison.Ordinal));
        }
        public static string GetPath(this GameObject go)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            while(go != null)
            {
                if (!first)
                {
                    sb.Insert(0, '/');
                }
                sb.Insert(0, go.name);
                go = go.transform.parent?.gameObject;
                first = false;
            }
            return sb.ToString();
        }
    }
}
