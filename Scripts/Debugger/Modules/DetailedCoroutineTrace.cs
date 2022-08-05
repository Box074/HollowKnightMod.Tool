
namespace HKTool.Debugger.Modules;


public static class DCT
{
    public static void NewIEnumerator(IEnumerator self)
    {
        var info = DetailedCoroutineTrace.GetInfo(self);
        info.newCall = new StackTrace(1, true).ToString();
    }
}
internal class DetailedCoroutineTrace : DebugModule
{
    internal class CoroutineCallInfo
    {
        public string? newCall = null;
        public string? startCoroutineCall = null;
        public IEnumerator ie = null!;
        public CoroutineCallInfo? caller = null;
        public string PrintStackTrace(int spaceCount = 0)
        {
            var space = spaceCount > 0 ? new string(' ', spaceCount) : "";
            var rspace =  "\n" + space;
            StringBuilder sb = new();
            if(spaceCount == 0) sb.AppendLine();
            sb.AppendLine(space + $"Enumerator Type: {ie.GetType().AssemblyQualifiedName}");
            if (caller != null)
            {
                sb.AppendLine(space + "================ Where to yield return =================");
                sb.AppendLine(space + caller.PrintStackTrace(spaceCount + 4).Replace("\n", rspace));
                sb.AppendLine(space + "================ End =================");
            }
            if (newCall != null)
            {
                sb.AppendLine(space + "================ Where to create IEnumerator =================");
                sb.AppendLine(space + newCall.Replace("\n", rspace));
                sb.AppendLine(space + "================ End =================");
            }
            if (startCoroutineCall != null)
            {
                sb.AppendLine(space + "================= Where to start Coroutine =================");
                sb.AppendLine(space + startCoroutineCall.Replace("\n", rspace));
                sb.AppendLine(space + "================= End =================");
            }
            return sb.ToString();
        }
    }
    internal static ConditionalWeakTable<IEnumerator, CoroutineCallInfo> _coroutines = new();
    internal static ConditionalWeakTable<Coroutine, IEnumerator> _coroutineMap = new();
    internal static List<ILHook> hooks = new();
    public override void OnEnable()
    {
        On.UnityEngine.SetupCoroutine.InvokeMoveNext += PatchInvokeMoveNext;
        On.UnityEngine.MonoBehaviour.StartCoroutine_IEnumerator += PatchStartCoroutine_IEnumerator;
        On.UnityEngine.MonoBehaviour.StartCoroutine_string_object += PatchStartCoroutine_string_object;
        var newHandle = typeof(DCT).GetMethod(nameof(DCT.NewIEnumerator));
        foreach(var v in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach(var t in v.GetTypes().Where(x => typeof(IEnumerator).IsAssignableFrom(x) && !x.IsGenericType 
                && !x.IsGenericTypeDefinition && x.IsDefined(typeof(CompilerGeneratedAttribute))))
            {
                foreach(var c in t.GetConstructors(HReflectionHelper.All))
                {
                    if(c.IsStatic) continue;
                    try
                    {
                    hooks.Add(new(c, il =>
                    {
                        var ilp = il.Body.GetILProcessor();
                        var first = il.Instrs[0];
                        ilp.InsertAfter(first, Instruction.Create(MOpCodes.Call, il.Import(newHandle)));
                        ilp.InsertAfter(first, Instruction.Create(MOpCodes.Ldarg_0));
                    }));
                    }catch(Exception e)
                    {
                        HKToolMod.logger.LogError(e);
                    }
                }
            }
        }
    }
    public override void OnDisable()
    {
        On.UnityEngine.SetupCoroutine.InvokeMoveNext -= PatchInvokeMoveNext;
        On.UnityEngine.MonoBehaviour.StartCoroutine_IEnumerator -= PatchStartCoroutine_IEnumerator;
        On.UnityEngine.MonoBehaviour.StartCoroutine_string_object -= PatchStartCoroutine_string_object;
        foreach(var v in hooks)
        {
            v.Dispose();
        }
        hooks.Clear();
    }
    public override bool CanRuntimeEnabled => true;
    public override bool CanRuntimeDisabled => true;
    internal static CoroutineCallInfo GetInfo(IEnumerator enumerator)
    {
        if (!_coroutines.TryGetValue(enumerator, out var info))
        {
            info = new();
            info.ie = enumerator;
            _coroutines.Add(enumerator, info);
        }
        return info;
    }
    internal static CoroutineCallInfo? GetInfo(Coroutine coroutine)
    {
        if(!_coroutineMap.TryGetValue(coroutine, out var ie)) return null;
        return GetInfo(ie);
    }
    private Coroutine PatchStartCoroutine_string_object(On.UnityEngine.MonoBehaviour.orig_StartCoroutine_string_object orig, 
        global::UnityEngine.MonoBehaviour self, 
        string methodName, object value)
    {
        var ie = (IEnumerator)self.GetType().InvokeMember(methodName, HReflectionHelper.Instance | BindingFlags.InvokeMethod, null, self, value != null ? new object[]
        {
            value
        } : Array.Empty<object>(), null, null, null);
        return self.StartCoroutine(ie);
    }
    private Coroutine PatchStartCoroutine_IEnumerator(On.UnityEngine.MonoBehaviour.orig_StartCoroutine_IEnumerator orig, 
        global::UnityEngine.MonoBehaviour self, IEnumerator routine)
    {
        var cor = orig(self, routine);
        var info = GetInfo(routine);
        info.startCoroutineCall =  new StackTrace(1, true).ToString();

        if(cor != null) _coroutineMap.Add(cor, routine);
        return cor!;
    }
    private void PatchInvokeMoveNext(On.UnityEngine.SetupCoroutine.orig_InvokeMoveNext orig, IEnumerator enumerator, IntPtr returnValueAddress)
    {
        var info = GetInfo(enumerator);
        try
        {
            orig(enumerator, returnValueAddress);
            if (enumerator.Current is IEnumerator ie)
            {
                var cinfo = GetInfo(ie);
                cinfo.caller = info;
            }
            else if(enumerator.Current is Coroutine cor)
            {
                var cinfo = GetInfo(cor);
                if(cinfo != null)
                {
                    cinfo.caller = info;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString() + "\n" + info.PrintStackTrace());
        }
    }
}
