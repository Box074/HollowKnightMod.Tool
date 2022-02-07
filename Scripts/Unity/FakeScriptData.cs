using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HKTool.Unity
{
    [CreateAssetMenu]
    public class FakeScriptData : ScriptableObject
    {
        public enum FieldType
        {
            GameObject, Component, ExternComponent, Int, Bool, Float, 
            String, Vector2, Vector3, Vector4, Quaternion, UnityObject
        }
        [System.Serializable]
        public class FieldData 
        {
            public string name;
            public FieldType fieldType;
            public string type;
        }
        public string scriptFullName;
        public FieldData[] fields;
    }
}
