using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HKTool.Unity
{
    [System.Serializable]
    public class FakeScriptDataPairUnityObject
    {
        public string name;
        public UnityEngine.Object obj;
    }
    [System.Serializable]
    public class FakeScriptDataPairComponent
    {
        public string name;
        public Component component;
    }
    [System.Serializable]
    public class FakeScriptDataPairGameObject
    {
        public string name;
        public GameObject go;
    }
    [System.Serializable]
    public class FakeScriptDataPairFakeScript
    {
        public string name;
        public FakeScript script;
    }
    [System.Serializable]
    public class FakeScriptDataPairN
    {
        public string name;
        public byte[] data;
    }
    [System.Serializable]
    public class FakeScriptDataPairVector
    {
        public string name;
        public Vector4 data;
    }
    [System.Serializable]
    public class FakeScriptDataPairQuaternion
    {
        public string name;
        public Vector4 data;
    }
    public static class FakeScriptHelper
    {
        public static FakeScript FindScript(this GameObject go, string scriptName)
        {
            return go.GetComponents<FakeScript>().FirstOrDefault(x=>x.scriptData?.scriptFullName == scriptName);
        }
    }
    public class FakeScript : MonoBehaviour
    {
        //[HideInInspector]
        public List<FakeScriptDataPairComponent> components = new List<FakeScriptDataPairComponent>();
        public List<FakeScriptDataPairUnityObject> objects = new List<FakeScriptDataPairUnityObject>();
        //[HideInInspector]
        public List<FakeScriptDataPairGameObject> gameObjects = new List<FakeScriptDataPairGameObject>();
        //[HideInInspector]
        public List<FakeScriptDataPairFakeScript> scripts = new List<FakeScriptDataPairFakeScript>();
        public List<FakeScriptDataPairVector> vectors = new List<FakeScriptDataPairVector>();
        public List<FakeScriptDataPairQuaternion> quaternions = new List<FakeScriptDataPairQuaternion>();
        //[HideInInspector]
        public List<FakeScriptDataPairN> data = new List<FakeScriptDataPairN>();
        public Dictionary<string, object> editorData = new Dictionary<string, object>();
        private static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
        //[HideInInspector]
        public FakeScriptData scriptData;
        private Component _instance;
        public Component instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadData();
                }
                else
                {
                    throw new System.NotSupportedException();
                }
                return _instance;
            }
        }
        private void ReadData(Dictionary<string, object> dst)
        {
            foreach(var v in objects) dst[v.name] = v.obj;
            foreach (var v in gameObjects) dst[v.name] = v.go;
            foreach (var v in components) dst[v.name] = v.component;
            if(!Application.isEditor)
            {
                foreach(var v in scripts)
                {
                    dst[v.name] = v.script?.instance;
                }
            }
            foreach (var v in vectors)
            {
                var vt = scriptData.fields.FirstOrDefault(x => x.name == v.name);
                if (vt.fieldType == FakeScriptData.FieldType.Vector2)
                {
                    dst[v.name] = new Vector2(v.data.x, v.data.y);
                }
                else if (vt.fieldType == FakeScriptData.FieldType.Vector3)
                {
                    dst[v.name] = new Vector3(v.data.x, v.data.y, v.data.z);
                }
                else if (vt.fieldType == FakeScriptData.FieldType.Vector4)
                {
                    dst[v.name] = v.data;
                }
            }
            foreach (var v in quaternions)
            {
                dst[v.name] = new Quaternion(v.data.x, v.data.y, v.data.z, v.data.w);
            }
            foreach (var v in data)
            {
                var vt = scriptData.fields.FirstOrDefault(x => x.name == v.name);
                if (vt.fieldType == FakeScriptData.FieldType.String)
                {
                    dst[v.name] = Encoding.UTF8.GetString(v.data);
                }
                else if (vt.fieldType == FakeScriptData.FieldType.Int)
                {
                    dst[v.name] = BitConverter.ToInt32(v.data, 0);
                }
                else if (vt.fieldType == FakeScriptData.FieldType.Bool)
                {
                    dst[v.name] = BitConverter.ToBoolean(v.data, 0);
                }
                else if (vt.fieldType == FakeScriptData.FieldType.Float)
                {
                    dst[v.name] = BitConverter.ToSingle(v.data, 0);
                }
            }
        }
        private void LoadDataInEditor()
        {
            ReadData(editorData);
        }
        private void LoadData()
        {
            if (!typeCache.TryGetValue(scriptData.scriptFullName, out var type))
            {
                foreach (var v in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = v.GetType(scriptData.scriptFullName);
                    if (type != null) break;
                }
                if (type == null) throw new MissingReferenceException();
            }
            _instance = gameObject.AddComponent(type);
            Dictionary<string, object> sdata = new Dictionary<string, object>();
            ReadData(sdata);
            foreach (var v in type.GetRuntimeFields())
            {
                if (sdata.TryGetValue(v.Name, out var f))
                {
                    if (f == null)
                    {
                        if (!v.FieldType.IsValueType)
                        {
                            v.SetValue(_instance, f);
                        }
                        continue;
                    }
                    if (v.FieldType.IsAssignableFrom(f.GetType()))
                    {
                        v.SetValue(_instance, f);
                    }
                }
            }
        }
        public object GetData(string name)
        {
            if (Application.isEditor)
            {
                if (editorData.TryGetValue(name, out var v)) return v;
                return null;
            }
            else
            {
                return instance?.GetType()?.GetRuntimeField(name)?.GetValue(instance);
            }
        }
        public void SetData(string name, object data)
        {
            if (Application.isEditor)
            {
                editorData[name] = data;
            }
            else
            {
                instance.GetType().GetRuntimeField(name).SetValue(instance, data);
            }
        }
        private void Awake()
        {
            if (Application.isEditor)
            {
                LoadDataInEditor();
            }
            else
            {
                if (_instance == null) LoadData();
            }
        }
    }
}
