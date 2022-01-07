using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HKTool
{

    public class CoroutineInfo
    {
        public enum CoroutineState
        {
            Ready, Execute, Pause, Done, Exception
        }
        public GameObject AttendGameObject { get; private set; } = null;
        public Exception LastException { get; internal set; } = null;
        public bool IsAttendGameObject { get; private set; } = false;
        public bool IsFinished => State == CoroutineState.Done || State == CoroutineState.Exception;
        public IEnumerator Coroutine { get; internal set; } = null;
        public CoroutineState State { get; internal set; } = CoroutineState.Ready;
        public bool IsPause { get; set; }
        public void Pause() => IsPause = true;
        public void Continue() => IsPause = false;
        internal Coroutine _cor = null;
        public CoroutineInfo(IEnumerator coroutine, GameObject attendGameObject = null)
        {
            Coroutine = coroutine;
            if (attendGameObject != null)
            {
                IsAttendGameObject = true;
                AttendGameObject = attendGameObject;
            }
        }
    }
    public static class CoroutineHelper
    {
        public static CoroutineInfo CurrentCoroutine { get; private set; } = null;
        private class CoroutineHandler : SingleMonoBegaviour<CoroutineHandler>
        {
            public readonly static List<CoroutineInfo> coroutines = new();
            public readonly static List<CoroutineInfo> wait = new();
            public readonly static Queue<CoroutineInfo> queue = new();
            public void AddCor(CoroutineInfo info)
            {
                queue.Enqueue(info);
            }
            private IEnumerator CoroutineExecuter(CoroutineInfo info)
            {
                if(info.State == CoroutineInfo.CoroutineState.Ready || info.State == CoroutineInfo.CoroutineState.Pause)
                {
                    info.State = CoroutineInfo.CoroutineState.Execute;
                    var cor = info.Coroutine;
                    while(!info.IsFinished)
                    {
                        if(info.IsPause)
                        {
                            info.State = CoroutineInfo.CoroutineState.Pause;
                            yield break;
                        }
                        if(info.IsAttendGameObject)
                        {
                            if(info.AttendGameObject == null)
                            {
                                info.State = CoroutineInfo.CoroutineState.Done;
                                info._cor = null;
                                yield break;
                            }
                            else if(!info.AttendGameObject.activeInHierarchy)
                            {
                                info.State = CoroutineInfo.CoroutineState.Pause;
                                wait.Add(info);
                                info._cor = null;
                                yield break;
                            }
                        }
                        bool? result;
                        try
                        {
                            CurrentCoroutine = info;
                            result = cor.MoveNext();
                            CurrentCoroutine = null;
                            if (result == false) info.State = CoroutineInfo.CoroutineState.Done;
                        }
                        catch (Exception e)
                        {
                            CurrentCoroutine = null;
                            info.LastException = e;
                            info.State = CoroutineInfo.CoroutineState.Exception;
                            result = false;
                        }
                        if (result == null)
                        {
                            continue;
                        }
                        if(result == false)
                        {
                            info._cor = null;
                            yield break;
                        }
                        yield return cor.Current;
                    }
                    info._cor = null;
                }
            }
            private void CleanUp()
            {
                coroutines.RemoveAll(x => x.IsFinished
                 || (x.AttendGameObject && x.AttendGameObject == null));
            }
            private void StartCor(CoroutineInfo info)
            {
                if (info.IsAttendGameObject)
                {
                    if (!info.AttendGameObject.activeInHierarchy)
                    {
                        wait.Add(info);
                        return;
                    }
                }
                StartCoroutine(CoroutineExecuter(info));
            }
            private void OnEnable()
            {
                CleanUp();
                foreach(var v in coroutines)
                {
                    StartCor(v);
                }
            }
            private void OnDestroy()
            {
                OnDisable();
            }
            private void OnDisable()
            {
                foreach (var v in coroutines)
                {
                    if (v.State == CoroutineInfo.CoroutineState.Execute)
                    {
                        v.State = CoroutineInfo.CoroutineState.Pause;
                        if (v._cor != null)
                        {
                            StopCoroutine(v._cor);
                            v._cor = null;
                        }
                    }
                }
            }
            private void Update()
            {
                if(queue.Count > 0)
                {
                    foreach(var v in queue)
                    {
                        StartCor(v);
                    }
                    queue.Clear();
                }
                for(int i = 0;i<wait.Count;i++)
                {
                    var v = wait[i];
                    if(v.IsFinished || v.AttendGameObject == null)
                    {
                        wait.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if(v.AttendGameObject.activeInHierarchy)
                    {
                        wait.RemoveAt(i);
                        i--;
                        StartCor(v);
                    }
                }
                foreach(var v in coroutines)
                {
                    if(v.State == CoroutineInfo.CoroutineState.Pause)
                    {
                        if (v.IsPause) continue;
                        else
                        {
                            StartCor(v);
                        }
                    }
                }
            }
        }
        public static CoroutineInfo StartCoroutine(this GameObject go, IEnumerator cor)
        {
            if (cor == null) throw new ArgumentNullException(nameof(cor));
            var info = new CoroutineInfo(cor, go);
            CoroutineHandler.Instance.AddCor(info);
            return info;
        }
        public static CoroutineInfo StartCoroutine(this IEnumerator cor)
        {
            var info = new CoroutineInfo(cor);
            CoroutineHandler.Instance.AddCor(info);
            return info;
        }
    }
}
