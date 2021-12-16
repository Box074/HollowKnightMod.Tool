using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HKTool.Utils
{
    public abstract class GameObjectFilter : IGameObjectFilter
    {
        public abstract bool Filter(GameObject go);
    }
}
