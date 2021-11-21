using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HKDebug.HitBox
{
    class HitBoxScript : MonoBehaviour
    {
        float t = 0;
        Material m = null;
        List<GameObject> lines = new List<GameObject>();
        GameObject lg = null;
        HitBoxColor hc = null;
        void Awake() => UpdateColor();
        void OnDestroy() => Destroy(lg);
        public void UpdateColor()
        {
            if (lg == null)
            {
                lg = new GameObject("Hit Box");
                lg.transform.parent = transform;
                lg.transform.SetPositionZ(0);
            }
            if (m == null)
            {
                m = new Material(Shader.Find("Diffuse"));
            }
            if (t < HitBoxCore.lastUpdate)
            {
                t = HitBoxCore.lastUpdate;
            }
            else
            {
                return;
            }
            string[] s = GetComponents<Component>().Select(x => x.GetType().FullName).ToArray();
            string[] fsms = GetComponents<PlayMakerFSM>().Select(x => x.FsmName).ToArray();
            hc = HitBoxCore.HitBoxConfig.colors.Where(
                x => (int)x.layer == gameObject.layer || x.layer == GlobalEnums.PhysLayers.DEFAULT
                ).Where(
                x => x.layer != 0 || (x.layer == 0 && (x.needComponents.Count != 0 || x.needPlayMakerFSMs.Count != 0))
                ).FirstOrDefault(
                x => (x.needComponents.Count == 0 || x.needComponents.All(x2 => s.Contains(x2))) &&
                    (x.needPlayMakerFSMs.Count == 0 || x.needPlayMakerFSMs.All(x2 => fsms.Contains(x2)))
                );
            if (hc == null)
            {
                lg?.SetActive(false);
                m.color = new Color(0, 0, 0, 0);
                return;
            }
            lg?.SetActive(true);
            m.color = new Color(hc.r, hc.g, hc.b);
        }
        public void UpdateHitBox()
        {
            foreach (var v in lines) Destroy(v);
            lines.Clear();
            if (!HitBoxCore.enableHitBox)
            {
                lg?.SetActive(false);
                return;
            }
            else
            {
                lg?.SetActive(true);
            }
            UpdateColor();
            if (hc == null) return;

            foreach (var v in gameObject.GetComponents<Collider2D>())
            {
                if (!v.enabled) continue;
                GameObject go = new GameObject("Hit Box");
                go.transform.parent = lg.transform;
                go.transform.position = transform.position;
                go.transform.SetPositionZ(0.01f);
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.sharedMaterial = m;
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lines.Add(go);
                HitBoxCore.SetupLineRenderer(v, m, lr);
            }
        }
        void FixedUpdate()
        {
            lg.transform.SetPositionZ(0);
            UpdateHitBox();
        }
    }
}
